using LearnXR.Core.Utilities;
using Meta.XR.MRUtilityKit;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LasersSpawner : MonoBehaviour
{

    [SerializeField] private MRUK mruk;
    [SerializeField] private OVRInput.Controller controller;
    [SerializeField] private GameObject laserPrefab;

    private bool sceneHasBeenLoaded;
    private MRUKRoom currentRoom;

    private List<GameObject> objectsCreated = new();
    private bool SceneAndRoomAvailable => currentRoom != null && sceneHasBeenLoaded;


    private void OnEnable()
    {
        mruk.RoomCreatedEvent.AddListener(BindRoomInfo);
    }

    private void OnDisable()
    {
        mruk.RoomCreatedEvent.RemoveListener(BindRoomInfo);
    }

    public void EnableMRUKScene()
    {
        sceneHasBeenLoaded = true;
        SpatialLogger.Instance.LogInfo($"{nameof(LasersSpawner)} has been enable due to scene availability");
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller) && SceneAndRoomAvailable)
        {
            Debug.Log("Primiu botao");
            if (objectsCreated.Count == 0)
            {
                foreach (var wallAnchor in currentRoom.WallAnchors)
                {
                    var createdWallObject = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity, wallAnchor.transform);
                    createdWallObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    objectsCreated.Add(createdWallObject);
                    SpatialLogger.Instance.LogInfo($"{nameof(LasersSpawner)} laser added to the wall");
                }
                SpatialLogger.Instance.LogInfo($"{nameof(LasersSpawner)} all lasers added");
            }
            else
            {
                foreach (var wallObject in objectsCreated)
                {
                    Destroy(wallObject);
                }
                objectsCreated.Clear();
                SpatialLogger.Instance.LogInfo($"{nameof(LasersSpawner)} lasers were deleted");

            }
        }
    }

    private void BindRoomInfo(MRUKRoom room)
    {
        currentRoom = room;
        SpatialLogger.Instance.LogInfo($"{nameof(LasersSpawner)} room was bound to current room");
    }
}
