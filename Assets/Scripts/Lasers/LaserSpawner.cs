using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Animations.Rigging;

public class LaserSpawner : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int numberOfLasers = 2;
    [SerializeField] private float laserWidth = 0.02f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private LayerMask sceneMeshLayer;

    [Header("Objective Settings")]
    [SerializeField] private int maxInteractions = 20;
    [SerializeField] private float objectiveWidth = 0.3f;
    [SerializeField] private Color objectiveColor = Color.yellow;



    [SerializeField] private OVRCameraRig ovrCameraRig;

    private List<GameObject> activeLasers = new List<GameObject>();

    private MRUKRoom currentRoom;

    void Start()
    {
        // Wait for MRUK to be ready
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance not found!");
            return;
        }

        MRUK.Instance.RoomCreatedEvent.AddListener(OnSceneLoaded);
    }

    private void OnSceneLoaded(MRUKRoom room)
    {
        currentRoom = room;

        SpawnLasers();

        StartCoroutine(WaitEndOfFrameObjective());
    }

    private void SpawnLasers()
    {
        for (int i = 0; i < numberOfLasers; i++)
        {
            StartCoroutine(WaitEndOfFrame());
        }
    }

    private IEnumerator WaitEndOfFrameObjective()
    {
        yield return new WaitForEndOfFrame();
        SpawnObjective();
    }

    private IEnumerator WaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        SpawnCellingLaser();
    }

    private void SpawnObjective()
    {
        GameObject objective = RoomSpawnPosition.Instance.SpawnObjective(laserPrefab, ovrCameraRig.transform, currentRoom, maxInteractions, out var spawnPostion, out var spawnNormal);
        if (objective == null)
        {
            return;
        }
        if (Physics.Raycast(new Ray(spawnPostion, spawnNormal), out var hit, Mathf.Infinity, sceneMeshLayer))
        {
            LineRenderer line = objective.GetComponent<LineRenderer>();
            line.startColor = objectiveColor;
            line.endColor = objectiveColor;
            line.startWidth = objectiveWidth;
            line.endWidth = objectiveWidth;

            Physics.Raycast(new Ray(spawnPostion, -spawnNormal), out var hitOrigin, Mathf.Infinity, sceneMeshLayer);

            line.SetPosition(0, hitOrigin.point);
            line.SetPosition(1, hit.point);

            CapsuleCollider col = objective.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = objectiveWidth;
        }
        else
        {
            Debug.LogWarning("Raycast did not hit Terrain");
        }
    }

    private void SpawnCellingLaser()
    {
        GameObject laser = RoomSpawnPosition.Instance.TryToSpawn(laserPrefab, currentRoom, RoomSpawnPosition.SpawnLocation.HangingDown, out var spawnPostion, out var spawnNormal);
        if (laser == null)
        {
            return;
        }

        if (Physics.Raycast(new Ray(spawnPostion, spawnNormal), out var hit, Mathf.Infinity, sceneMeshLayer))
        {
            LineRenderer line = laser.GetComponent<LineRenderer>();
            line.startColor = laserColor;
            line.endColor = laserColor;
            line.startWidth = laserWidth;
            line.endWidth = laserWidth;

            Physics.Raycast(new Ray(spawnPostion, -spawnNormal), out var hitOrigin, Mathf.Infinity, sceneMeshLayer);

            line.SetPosition(0, hitOrigin.point);
            line.SetPosition(1, hit.point);

            CapsuleCollider col = laser.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.radius = laserWidth;
        }
        else
        {
            Debug.LogWarning("Raycast did not hit Terrain");
        }
    }

    private void CreateLaser(Vector3 topPosition, float height)
    {
        GameObject laser = new GameObject("VerticalLaser");
        laser.transform.position = topPosition;
        laser.transform.parent = transform;

        // Add LineRenderer component
        LineRenderer lineRenderer = laser.AddComponent<LineRenderer>();
        
        // Configure LineRenderer
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
        lineRenderer.positionCount = 2;
        
        // Set positions (from ceiling to floor)
        Vector3 bottomPosition = topPosition;
        bottomPosition.y -= height;
        
        lineRenderer.SetPosition(0, topPosition);
        lineRenderer.SetPosition(1, bottomPosition);

        // Add collider for detection
        CapsuleCollider laserCollider = laser.AddComponent<CapsuleCollider>();
        laserCollider.isTrigger = true;
        laserCollider.radius = laserWidth;
        laserCollider.height = height;
        laserCollider.direction = 1; // Y-axis
        laserCollider.center = new Vector3(0, -height / 2, 0);

        // Add laser behavior script
        LaserBehavior laserBehavior = laser.AddComponent<LaserBehavior>();

        activeLasers.Add(laser);
        
        Debug.Log($"Laser created at position: {topPosition}");
    }

    // Optional: Method to clear all lasers
    public void ClearLasers()
    {
        foreach (GameObject laser in activeLasers)
        {
            Destroy(laser);
        }
        activeLasers.Clear();
    }

    // Optional: Method to respawn lasers
    public void RespawnLasers()
    {
        ClearLasers();
        SpawnLasers();
    }
}
