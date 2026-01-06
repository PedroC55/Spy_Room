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

    [Header("Laser Type Probabilities")]
    [SerializeField][Range(0f, 1f)] private float horizontalLaserChance = 0.3f; // 30% horizontal

    [Header("Horizontal Laser Settings")]
    [SerializeField] private float minHeightAboveGround = 1.2f; // Altura mínima acima do chão
    [SerializeField] private float maxHeightAboveGround = 2.0f; // Altura máxima acima do chão // Offset em relação à altura da cabeça
    [SerializeField] private float maxLaserLength = 300f; // Comprimento máximo do laser horizontal

    [SerializeField] private OVRCameraRig ovrCameraRig;

    private List<GameObject> activeLasers = new List<GameObject>();

    private MRUKRoom currentRoom;

    private void OnEnable()
    {
        GameEvents.OnDiamondGrab += SpawnNewLaser;
    }

    private void OnDisable()
    {
        GameEvents.OnDiamondGrab -= SpawnNewLaser;
    }

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

        SpawnInitialLasers();
    }

    private void SpawnInitialLasers()
    {
        for (int i = 0; i < numberOfLasers; i++)
        {
            SpawnNewLaser();
        }
    }

    private void SpawnNewLaser()
    {
        // Decide aleatoriamente entre horizontal e vertical
        float randomValue = Random.Range(0f, 1f);

        if (randomValue < horizontalLaserChance)
        {
            Debug.Log("Spawning Horizontal Laser");
            // 30% chance - Spawn horizontal laser
            StartCoroutine(WaitEndOfFrameHorizontal());
        }
        else
        {
            Debug.Log("Spawning Vertical Laser");
            // 70% chance - Spawn vertical laser
            StartCoroutine(WaitEndOfFrame());
        }
    }

    private IEnumerator WaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        SpawnCellingLaser();
    }

    private IEnumerator WaitEndOfFrameHorizontal()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnHorizontalLaser();
    }


    private void SpawnCellingLaser()
    {
        GameObject laser = RoomSpawnPosition.Instance.TryToSpawn(laserPrefab, currentRoom, ovrCameraRig.centerEyeAnchor, RoomSpawnPosition.SpawnLocation.HangingDown, out var spawnPostion, out var spawnNormal);
        if (laser == null)
        {
            return;
        }

        laser.GetComponent<LaserBehavior>().SetOVRCameraRig(ovrCameraRig);

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
        activeLasers.Add(laser);
    }

    private void SpawnHorizontalLaser()
    {
        // Usa o novo método TryToSpawnHorizontalLaser para validar tudo antes de spawnar
        Debug.Log("Altura do jogador:" + ovrCameraRig.centerEyeAnchor.position.y);
        //GameObject laser = RoomSpawnPosition.Instance.TryToSpawnHorizontalLaser(laserPrefab,currentRoom, ovrCameraRig.centerEyeAnchor, minHeightAboveGround,maxHeightAboveGround,headHeightOffset,sceneMeshLayer,out Vector3 spawnPosition,out Vector3 laserDirection,out Vector3 startPoint,out Vector3 endPoint);

        GameObject laser = RoomSpawnPosition.Instance.TryToSpawn(laserPrefab, currentRoom, ovrCameraRig.centerEyeAnchor, RoomSpawnPosition.SpawnLocation.VerticalSurfaces, out var spawnPostion, out var spawnNormal);
  
        float headHeightOffset = Random.Range(-0.2f, 0.2f);

        laser.transform.position = new Vector3(laser.transform.position.x, ovrCameraRig.centerEyeAnchor.position.y + headHeightOffset, laser.transform.position.z);
        Debug.Log($"laser x: {laser.transform.position.x}, y: {laser.transform.position.y},z: {laser.transform.position.z}");

        if (laser == null)
        {
            Debug.LogWarning("Failed to spawn horizontal laser after validation");
            return;
        }

        // Configure LaserBehavior
        laser.GetComponent<LaserBehavior>().SetOVRCameraRig(ovrCameraRig);

        spawnPostion = laser.transform.position;

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
        activeLasers.Add(laser);
    }
    // Optional: Method to clear all lasers
    private void ClearLasers()
    {
        foreach (GameObject laser in activeLasers)
        {
            Destroy(laser);
        }
        activeLasers.Clear();
    }

    // Optional: Method to respawn lasers
    private void RespawnLasers()
    {
        ClearLasers();
        SpawnInitialLasers();
    }
}
