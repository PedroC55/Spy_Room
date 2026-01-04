using UnityEngine;
using UnityEngine.VFX;

public class LaserBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject laserBeamAudioObject;
    [SerializeField]
    private GameObject laserHitAudioObject;
    [SerializeField]
    private VisualEffect laserVisualEffect;
    [SerializeField] private LayerMask sceneMeshLayer;

    private OVRCameraRig ovrCameraRig;

    void Update()
    {
        if(ovrCameraRig == null) return;

        Vector3 vectorToPlayerHead = ovrCameraRig.centerEyeAnchor.position - transform.position;
        Vector3 projectedPoint = Vector3.Project(vectorToPlayerHead, transform.up);

        laserBeamAudioObject.transform.position = transform.position + projectedPoint;

        if (laserVisualEffect.GetBool("Hit"))
        {
            if (Physics.Raycast(new Ray(transform.position, transform.up), out var hit, Mathf.Infinity, sceneMeshLayer))
            {
                laserHitAudioObject.SetActive(true);
                laserHitAudioObject.transform.position = hit.point;
            }
        }
        else laserHitAudioObject.SetActive(false);
    }

    public void SetOVRCameraRig(OVRCameraRig camera)
    {
        ovrCameraRig = camera;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.LogWarning("Player has touched the laser!");
            GameEvents.HitLaser();
        }
    }
}
