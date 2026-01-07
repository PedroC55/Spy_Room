using UnityEngine;

public class LinkFlowManager : MonoBehaviour
{
    [SerializeField]
    private HexagonsManager _hexagonsManager;

    [SerializeField]
    private OneHandRotationValve _valveManager;

    private float _previousAngle = 0f;
    private bool _isValveBeingGrabbed = false;

    private void Start()
    {
        if(_hexagonsManager == null)
            Debug.LogError("Hexagons Manager is not assigned in the inspector.");

        if (_valveManager == null)
        {
            Debug.LogError("Valve Manager is not assigned in the inspector.");
        }
        else
        {
            _valveManager.OnValveGrabbed.AddListener(ValveGrabbed);
            _valveManager.OnValveReleased.AddListener(ValveReleased);
        }
    }

    private void OnDestroy()
    {
        if (_valveManager != null)
        {
            _valveManager.OnValveGrabbed.RemoveListener(ValveGrabbed);
            _valveManager.OnValveReleased.RemoveListener(ValveReleased);
        }
    }

    private void Update()
    {
        if (_hexagonsManager == null || _valveManager == null) return;

        if (_isValveBeingGrabbed)
        {
            float spinAngle = _valveManager.CurrentAngle - _previousAngle;

            if(spinAngle > _valveManager.ValveSideAngle)
            {
                //Clockwise Rotation
                Debug.Log("Clockwise Rotation");
                //Anges needs to be the inversed to spin the right way in Unity
                _hexagonsManager.SpinSelectedHex(-(int)_valveManager.ValveSideAngle);
                _previousAngle += _valveManager.ValveSideAngle;
            }
            else if (spinAngle < -_valveManager.ValveSideAngle)
            {
                //CounterClockWise Rotation
                Debug.Log("Counter Clockwise Rotation");
                //Anges needs to be the inversed to spin the right way in Unity
                _hexagonsManager.SpinSelectedHex((int)_valveManager.ValveSideAngle);
                _previousAngle -= _valveManager.ValveSideAngle;
            }
        }
    }

    private void ValveGrabbed()
    {
        _isValveBeingGrabbed = true;
        _previousAngle = _valveManager.StartAngle;
    }

    private void ValveReleased()
    {
        _isValveBeingGrabbed = false;
    }
}
