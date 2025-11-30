using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

public class LaserSpawner : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private int numberOfLasers = 2;
    [SerializeField] private float laserWidth = 0.02f;
    [SerializeField] private Color laserColor = Color.red;
    
    private MRUKRoom currentRoom;
    private List<GameObject> activeLasers = new List<GameObject>();

    void Start()
    {
        // Wait for MRUK to be ready
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance not found!");
            return;
        }

        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    private void OnSceneLoaded()
    {
        currentRoom = MRUK.Instance.GetCurrentRoom();
        
        if (currentRoom == null)
        {
            Debug.LogError("No room found!");
            return;
        }

        SpawnLasers();
    }

    private void SpawnLasers()
    {
        // Get ceiling and floor anchors using properties
        MRUKAnchor ceiling = currentRoom.CeilingAnchor;
        MRUKAnchor floor = currentRoom.FloorAnchor;

        if (ceiling == null || floor == null)
        {
            Debug.LogError("Ceiling or floor not found!");
            return;
        }

        // Get ceiling and floor positions
        Vector3 ceilingCenter = ceiling.transform.position;
        Vector3 floorCenter = floor.transform.position;

        // Calculate height
        float roomHeight = Mathf.Abs(ceilingCenter.y - floorCenter.y);

        Debug.Log($"Room height: {roomHeight}m");

        // Spawn lasers at different positions
        for (int i = 0; i < numberOfLasers; i++)
        {
            Vector3 spawnPosition = GetRandomPositionInRoom(ceilingCenter, floor);
            CreateLaser(spawnPosition, roomHeight);
        }
    }

    private Vector3 GetRandomPositionInRoom(Vector3 ceilingCenter, MRUKAnchor floor)
    {
        // Get the floor plane rect bounds
        Rect floorBounds = floor.PlaneRect.Value;
        
        // Generate random position within bounds
        float randomX = Random.Range(floorBounds.xMin, floorBounds.xMax);
        float randomZ = Random.Range(floorBounds.yMin, floorBounds.yMax);
        
        // Convert to world position at ceiling height
        Vector3 localPosition = new Vector3(randomX, 0, randomZ);
        Vector3 worldPosition = floor.transform.TransformPoint(localPosition);
        worldPosition.y = ceilingCenter.y;
        
        return worldPosition;
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
