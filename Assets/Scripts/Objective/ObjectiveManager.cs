using Meta.XR.MRUtilityKit;
using System.Collections;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [Header("Objective Settings")]
    [SerializeField] private GameObject objectivePrefab;
    [SerializeField] private int maxInteractions = 20;

    [SerializeField] private OVRCameraRig ovrCameraRig;

    private GameObject objective;

    private MRUKRoom currentRoom;

    private void OnEnable()
    {
        GameEvents.OnDiamondGrab += MoveObjective;
    }

    private void OnDisable()
    {
        GameEvents.OnDiamondGrab -= MoveObjective;
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

        StartCoroutine(WaitEndOfFrame());
    }

    private IEnumerator WaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        SpawnObjective();
    }

    private void SpawnObjective()
    {
        var spawnPosition = Vector3.zero;
        var spawnNormal = Vector3.zero;
        
        objective = RoomSpawnPosition.Instance.SpawnObjective(objectivePrefab, ovrCameraRig.transform, currentRoom, maxInteractions, out spawnPosition, out spawnNormal);

        if (objective == null)
        {
            Debug.LogError("Could not spawn objective in room.");
        }
    }

    private void MoveObjective()
    {
        if(objective != null)
        {
            var spawnPosition = Vector3.zero;
            var spawnNormal = Vector3.zero;
            var aux = RoomSpawnPosition.Instance.SpawnObjective(objective, ovrCameraRig.transform, currentRoom, maxInteractions, out spawnPosition, out spawnNormal);

            if (aux == null)
            {
                Debug.LogError("Could not move objective to a new position.");
            }
        }
    }

    //public void RemoveObjective()
    //{
    //    GameObject objective = GameObject.FindWithTag("Objective");
    //    if (objective != null)
    //    {
    //        Destroy(objective);
    //    }
    //}
}
