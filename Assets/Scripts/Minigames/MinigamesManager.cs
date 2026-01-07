using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MinigamesManager : MonoBehaviour
{
    [SerializeField]
    private GameObject minigamePrefab;

    [SerializeField] private OVRCameraRig ovrCameraRig;

    private int spawnCount = 11;

    private MRUKRoom currentRoom;

    private void OnEnable()
    {
        GameEvents.OnDiamondGrab += DecreaseSpawnCounter;
    }

    private void OnDisable()
    {
        GameEvents.OnDiamondGrab -= DecreaseSpawnCounter;
    }

    void Start()
    {
        // Wait for MRUK to be ready
        if (MRUK.Instance == null)
        {
            Debug.LogError("MRUK Instance not found!");
            return;
        }

        MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomLoaded);
    }

    private void OnRoomLoaded(MRUKRoom room)
    {
        currentRoom = room;
    }

    private void DecreaseSpawnCounter()
    {
        spawnCount--;

        if (spawnCount == 0)
        {
            SpawnMinigame();
            spawnCount = 11;
        }
    }

    private void SpawnMinigame()
    {
        GameObject minigame = RoomSpawnPosition.Instance.TryToSpawn(minigamePrefab, currentRoom, ovrCameraRig.centerEyeAnchor, RoomSpawnPosition.SpawnLocation.VerticalSurfaces, out var spawnPostion, out var spawnNormal);

        float headHeightOffset = -0.2f;

        minigame.transform.position = new Vector3(minigame.transform.position.x, ovrCameraRig.centerEyeAnchor.position.y + headHeightOffset, minigame.transform.position.z);
        minigame.transform.forward = -spawnNormal.normalized;

        if (minigame == null)
        {
            Debug.LogWarning("Failed to spawn minigame laser after validation");
            return;
        }
    }

}
