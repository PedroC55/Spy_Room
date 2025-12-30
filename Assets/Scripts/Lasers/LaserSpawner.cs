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
            StartCoroutine(WaitEndOfFrame());
        }
    }

    private void SpawnNewLaser()
    {
        //COLOCAR AQUI OS CALCULOS PROBABILISTICOS DE ONDE SPAWNAR O LASER
        SpawnCellingLaser();
    }

    private IEnumerator WaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        SpawnCellingLaser();
    }

    private void SpawnCellingLaser()
    {
        GameObject laser = RoomSpawnPosition.Instance.TryToSpawn(laserPrefab, currentRoom, RoomSpawnPosition.SpawnLocation.HangingDown, out var spawnPostion, out var spawnNormal);
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
