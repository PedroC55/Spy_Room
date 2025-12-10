using UnityEngine;

public class LaserBehavior : MonoBehaviour
{
    [SerializeField]
    private GameObject laserBeamAudioObject;

    private OVRCameraRig ovrCameraRig;

    void Update()
    {
        if(ovrCameraRig == null) return;

        Vector3 vectorToPlayerHead = ovrCameraRig.centerEyeAnchor.position - transform.position;
        Vector3 projectedPoint = Vector3.Project(vectorToPlayerHead, transform.up);

        laserBeamAudioObject.transform.position = transform.position + projectedPoint;
    }

    public void SetOVRCameraRig(OVRCameraRig camera)
    {
        ovrCameraRig = camera;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            Debug.LogError("Player has touched the laser!");
            OnLaserTouched(other.gameObject);
        }
    }

    private void OnLaserTouched(GameObject player)
    {       
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.DecreaseScore(10);
        }
    }
}
