using UnityEngine;
using Meta.XR.MRUtilityKit;

[RequireComponent(typeof(EffectMesh))]

public class ChangeRoomLayer : MonoBehaviour
{
    [SerializeField]
    private LayerMask terrainLayer = -1;

    private EffectMesh effectMesh;

    void Start()
    {
        effectMesh = GetComponent<EffectMesh>();

        if (Utils.HasMultipleLayersOrNull(terrainLayer))
        {
            Debug.LogError("Please assign a single layer to the terrainLayer field.");
        }
        else
        {
            effectMesh.Layer = Utils.LayerMaskToLayer(terrainLayer);
            //MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomReady);
        }
    }

    void OnDestroy()
    {
        if(MRUK.Instance != null)
            MRUK.Instance.RoomCreatedEvent.RemoveListener(OnRoomReady);
    }

    private void OnRoomReady(MRUKRoom room)
    {
        if (room != null)
        {
            Utils.SetLayerRecursive(room.gameObject, Utils.LayerMaskToLayer(terrainLayer));
            /*List<MRUKAnchor> anchors = room.Anchors;
            foreach (MRUKAnchor anchor in anchors)
            {
                if (anchor != null)
                {
                    anchor.gameObject.layer = Utils.LayerMaskToLayer(terrainLayer);
                }
            }*/
        }
        else
        {
            Debug.LogWarning("No active MRUKRoom found.");
        }
    }
}
