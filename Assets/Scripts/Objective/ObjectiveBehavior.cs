using Oculus.Interaction;
using System.Collections;
using UnityEngine;

public class ObjectiveBehavior : MonoBehaviour
{
    [SerializeField] private Grabbable grabbable;
    private GameObject diamondObject;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isGrabbed = false;

    private void Start()
    {
        diamondObject = grabbable.gameObject;

        initialPosition = diamondObject.transform.localPosition;
        initialRotation = diamondObject.transform.localRotation;
        Debug.Log($"Initial Position: {initialPosition}, Initial Rotation: {initialRotation}");
    }

    public void OnDiamondGrab()
    {
        if (isGrabbed) return;

        isGrabbed = true;
        grabbable.enabled = false;
        GameEvents.DiamongGrab();
    }

    public void OnDiamondRelease() 
    {
        StartCoroutine(WaitEndOfFrame());
    }

    private IEnumerator WaitEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        diamondObject.transform.SetLocalPositionAndRotation(initialPosition, initialRotation);
        grabbable.enabled = true;
        isGrabbed = false;
        Debug.Log("Soltou");
    }
}
