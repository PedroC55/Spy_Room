using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.XR.ARSubsystems;



public class RoomSpawnPosition : MonoBehaviour
{
    public static RoomSpawnPosition Instance { get; private set; }

    /// <summary>
    /// Maximum number of times to attempt spawning/moving an object before giving up.
    /// </summary>
    [Tooltip("Maximum number of times to attempt spawning/moving an object before giving up.")]
    public int MaxIterations = 1000;

    /// <summary>
    /// Defines possible locations where objects can be spawned.
    /// </summary>
    public enum SpawnLocation
    {
        /// Spawn on any surface (i.e. a combination of all 3 options below)
        AnySurface,
        /// Spawn only on vertical surfaces such as walls, windows, wall art, doors, etc...
        VerticalSurfaces,
        /// Spawn on surfaces facing upwards such as ground, top of tables, beds, couches, etc...
        OnTopOfSurfaces,
        /// Spawn on surfaces facing downwards such as the ceiling
        HangingDown
    }

    /// <summary>
    /// When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.
    /// </summary>
    [Tooltip("When using surface spawning, use this to filter which anchor labels should be included. Eg, spawn only on TABLE or OTHER.")]
    public MRUKAnchor.SceneLabels Labels = ~(MRUKAnchor.SceneLabels)0;

    /// <summary>
    /// If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.
    /// </summary>
    [Tooltip("If enabled then the spawn position will be checked to make sure there is no overlap with physics colliders including themselves.")]
    public bool CheckOverlaps = false;

    /// <summary>
    /// Required free space for the object (Set negative to auto-detect using GetPrefabBounds)
    /// default to auto-detect. This value represents the extents of the bounding box
    /// </summary>
    [Tooltip("Required free space for the object (Set negative to auto-detect using GetPrefabBounds)")]
    public float OverrideBounds = -1;

    /// <summary>
    /// Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.
    /// </summary>
    [Tooltip("Set the layer(s) for the physics bounding box checks, collisions will be avoided with these layers.")]
    public LayerMask LayerMask = -1;

    /// <summary>
    /// The clearance distance required in front of the surface in order for it to be considered a valid spawn position
    /// </summary>
    [Tooltip("The clearance distance required in front of the surface in order for it to be considered a valid spawn position")]
    public float SurfaceClearanceDistance = 0.1f;

    private List<GameObject> spawnedObjects = new();

    /// <summary>
    /// The list containing all the objects instantiated by this component
    /// </summary>
    public IReadOnlyList<GameObject> SpawnedObjects => spawnedObjects;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            //Debug.Assert(condition: false, "There should be only one instance of MRUK!");
            Debug.LogError("There should be only one instance of RoomSpawnPosition!");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Starts the spawning process for a specific room. A maximum of <see cref="MaxIterations"/> attempts will be made to find a valid spawn position.
    /// <see cref="MRUKRoom.GenerateRandomPositionInRoom"/> and <see cref="MRUKRoom.GenerateRandomPositionOnSurface"/> are used to generate the positions.
    /// </summary>
    /// <param name="room">The room to spawn objects in.</param>
    public GameObject TryToSpawn(GameObject objectToSpawn, MRUKRoom room, SpawnLocation spawnLocation, out Vector3 spawnPosition, out Vector3 spawnNormal)
    {
        spawnPosition = Vector3.zero;
        spawnNormal = Vector3.zero;
        var prefabBounds = Utilities.GetPrefabBounds(objectToSpawn);
        var minRadius = 0.0f;
        const float clearanceDistance = 0.01f;
        var baseOffset = -prefabBounds?.min.y ?? 0.0f;
        var centerOffset = prefabBounds?.center.y ?? 0.0f;
        Bounds adjustedBounds = new();

        if (prefabBounds.HasValue)
        {
            minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
            if (minRadius < 0f)
            {
                minRadius = 0f;
            }

            var min = prefabBounds.Value.min;
            var max = prefabBounds.Value.max;
            min.y += clearanceDistance;
            if (max.y < min.y)
            {
                max.y = min.y;
            }

            adjustedBounds.SetMinMax(min, max);
            if (OverrideBounds > 0)
            {
                var center = new Vector3(0f, clearanceDistance, 0f);
                var size = new Vector3(OverrideBounds * 2f, clearanceDistance * 2f,
                    OverrideBounds * 2f); // OverrideBounds represents the extents, not the size
                adjustedBounds = new Bounds(center, size);
            }
        }

        for (var j = 0; j < MaxIterations; ++j)
        {
            spawnPosition = Vector3.zero;
            spawnNormal = Vector3.zero;
            MRUK.SurfaceType surfaceType = 0;
            switch (spawnLocation)
            {
                case SpawnLocation.AnySurface:
                    surfaceType |= MRUK.SurfaceType.FACING_UP;
                    surfaceType |= MRUK.SurfaceType.VERTICAL;
                    surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                    break;
                case SpawnLocation.VerticalSurfaces:
                    surfaceType |= MRUK.SurfaceType.VERTICAL;
                    break;
                case SpawnLocation.OnTopOfSurfaces:
                    surfaceType |= MRUK.SurfaceType.FACING_UP;
                    break;
                case SpawnLocation.HangingDown:
                    surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                    break;
            }

            if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
            {
                
                spawnPosition = pos + normal * baseOffset;
                spawnNormal = normal;
                var center = spawnPosition + normal * centerOffset;
                // In some cases, surfaces may protrude through walls and end up outside the room
                // check to make sure the center of the prefab will spawn inside the room
                if (!room.IsPositionInRoom(center))
                {
                    continue;
                }

                // Ensure the center of the prefab will not spawn inside a scene volume
                if (room.IsPositionInSceneVolume(center))
                {
                    continue;
                }

                // Also make sure there is nothing close to the surface that would obstruct it
                if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                {
                    continue;
                }

                Debug.LogWarning("LEMBRAR DE COLOCAR MAIS VERIFICAÇÕES NESTA PARTE DO CODIGO");
            }
            else
            {
                Debug.LogError("Failed to generate random position on surface.");
                return null;
            }

            var spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
            if (CheckOverlaps && prefabBounds.HasValue)
            {
                if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
            }

            var item = Instantiate(objectToSpawn, spawnPosition, spawnRotation, transform);
            spawnedObjects.Add(item);

            return item;
        }

        Debug.LogWarning($"Failed to find valid spawn position after {MaxIterations} iterations.");
        return null;
    }

    public GameObject SpawnObjective(GameObject objectToSpawn,Transform playerPosition, MRUKRoom room, int maxInteractions, out Vector3 spawnPosition, out Vector3 spawnNormal)
    {
        spawnPosition = Vector3.zero;
        spawnNormal = Vector3.zero;
        var prefabBounds = Utilities.GetPrefabBounds(objectToSpawn);
        var minRadius = 0.0f;
        const float clearanceDistance = 0.01f;
        var baseOffset = -prefabBounds?.min.y ?? 0.0f;
        var centerOffset = prefabBounds?.center.y ?? 0.0f;
        float currentDistance = 0f;
        Vector3 currentVectorPostion = Vector3.zero;
        Quaternion currentVectorRotation = Quaternion.identity;
        bool sucess = false;
        Bounds adjustedBounds = new();

        if (prefabBounds.HasValue)
        {
            minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
            if (minRadius < 0f)
            {
                minRadius = 0f;
            }

            var min = prefabBounds.Value.min;
            var max = prefabBounds.Value.max;
            min.y += clearanceDistance;
            if (max.y < min.y)
            {
                max.y = min.y;
            }

            adjustedBounds.SetMinMax(min, max);
            if (OverrideBounds > 0)
            {
                var center = new Vector3(0f, clearanceDistance, 0f);
                var size = new Vector3(OverrideBounds * 2f, clearanceDistance * 2f,
                    OverrideBounds * 2f); // OverrideBounds represents the extents, not the size
                adjustedBounds = new Bounds(center, size);
            }
        }

        for (var j = 0; j < MaxIterations; ++j)
        {
            Debug.Log($"Remaining interactions to find farthest point: {maxInteractions}");
            spawnPosition = Vector3.zero;
            spawnNormal = Vector3.zero;
            MRUK.SurfaceType surfaceType = 0;
            surfaceType |= MRUK.SurfaceType.FACING_UP;
            

            if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, new LabelFilter(Labels), out var pos, out var normal))
            {

                spawnPosition = pos + normal * baseOffset;
                spawnNormal = -normal;
                Debug.Log("Spawn Normal Candidate: " + spawnNormal);
                var center = spawnPosition + normal * centerOffset;
                // In some cases, surfaces may protrude through walls and end up outside the room
                // check to make sure the center of the prefab will spawn inside the room
                if (!room.IsPositionInRoom(center))
                {
                    continue;
                }

                // Ensure the center of the prefab will not spawn inside a scene volume
                if (room.IsPositionInSceneVolume(center))
                {
                    continue;
                }

                // Also make sure there is nothing close to the surface that would obstruct it
                if (room.Raycast(new Ray(pos, normal), SurfaceClearanceDistance, out _))
                {
                    continue;
                }

                Debug.LogWarning("LEMBRAR DE COLOCAR MAIS VERIFICAÇÕES NESTA PARTE DO CODIGO");
            }
            else
            {
                Debug.LogError("Failed to generate random position on surface.");
                return null;
            }

            var spawnRotation = Quaternion.FromToRotation(Vector3.up, spawnNormal);
            if (CheckOverlaps && prefabBounds.HasValue)
            {
                if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, adjustedBounds.extents, spawnRotation, LayerMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
            }


            float distance = Vector3.Distance(playerPosition.position, spawnPosition);
            if (distance > currentDistance)
            {
                currentDistance = distance;
                currentVectorPostion = spawnPosition;
                currentVectorRotation = spawnRotation;
            }

            maxInteractions--;
            if (maxInteractions == 0)
            {
                sucess = true;
                break;
            }
                
            
        }
        if (sucess)
        { 
            if (objectToSpawn.gameObject.scene.path == null)
            {
                var item = Instantiate(objectToSpawn, currentVectorPostion, currentVectorRotation, transform);
                spawnedObjects.Add(item);
                return item;
            }
            else
            {
                objectToSpawn.transform.position = currentVectorPostion;
                objectToSpawn.transform.rotation = currentVectorRotation;
                return objectToSpawn; // ignore SpawnAmount once we have a successful move of existing object in the scene
            }


            
        }
        
        
        return null;
    }



    /// <summary>
    /// Destroys all the game objects instantiated and clears the <see cref="SpawnedObjects"/> list.
    /// </summary>
    public void ClearSpawnedPrefabs()
    {
        for (var i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            var spawnedObject = spawnedObjects[i];
            if (spawnedObject)
            {
                Destroy(spawnedObject);
            }
        }

        spawnedObjects.Clear();
    }
}