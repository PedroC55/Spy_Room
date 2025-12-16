using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform ikTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public void Map()
    {
        ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class IKTargetFollowVRRig : MonoBehaviour
{
    [Range(0,1)]
    public float turnSmoothness = 0.1f;
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Vector3 headBodyPositionOffset;
    public float headBodyYawOffset;

    [Header("ResizeModel")]
    [SerializeField]
    private float modelEyeHeight = 1.75f;
    [SerializeField]
    private OVRCameraRig ovrCameraRig;

    private bool executeOnce = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float userEyeHeight = ovrCameraRig.trackerAnchor.position.y;
        float heightRatio = userEyeHeight / modelEyeHeight;
        Debug.Log($"User eye height: {userEyeHeight}, Height Ratio: {heightRatio}");
        if (!executeOnce)
        {
            if (ovrCameraRig != null)
            {
                transform.localScale = new Vector3(heightRatio, heightRatio, heightRatio);
                headBodyPositionOffset *= heightRatio;
            }
            else
            {
                Debug.Log("não achou camera rig");
            }

            executeOnce = true;
        }
        

        transform.position = head.ikTarget.position + headBodyPositionOffset;
        float yaw = head.vrTarget.eulerAngles.y;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z),turnSmoothness);

        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
