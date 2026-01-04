using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(InteractableUnityEventWrapper))]
public class HexagonController : MonoBehaviour
{
    [SerializeField]
    private GameObject _visualGO;

    [SerializeField]
    [Range(0, 5)]
    private int _correctPosition = 5;
    
    //This value goes from 0 to 5;
    private int _currentPosition = 0;

    private bool _isCorrectlyPlaced = false;

    private InteractableUnityEventWrapper _interactableWrapper;

    private HexagonsManager _hexagonsManager;

    private void Start()
    {
        _hexagonsManager = GetComponentInParent<HexagonsManager>();
        _interactableWrapper = GetComponent<InteractableUnityEventWrapper>();
        StartRotation();

        if (_hexagonsManager != null)
        {
            _hexagonsManager.RegisterHexagon(this);
            _interactableWrapper.WhenSelect.AddListener(HexSelected);
        }
        else
        {
            Debug.LogError("HexagonController failed to find HexagonManager in parent.");
        }
    }

    private void OnDestroy()
    {
        if(_hexagonsManager != null)
        {
            _interactableWrapper.WhenSelect.RemoveListener(HexSelected);
        }
    }

    public void SpinCounterClockWise(int spinValue)
    {
        if(_isCorrectlyPlaced)
            LeftCorrectPosition();

        _visualGO.transform.Rotate(0, 0, spinValue);

        _currentPosition--;
        if(_currentPosition < 0)
            _currentPosition = 5;

        if(_currentPosition == _correctPosition)
            CorrectPosition();
    }

    public void SpinClockWise(int spinValue)
    {
        if (_isCorrectlyPlaced)
            LeftCorrectPosition();

        _visualGO.transform.Rotate(0, 0, spinValue);

        _currentPosition++;
        if (_currentPosition > 5)
            _currentPosition = 0;

        if (_currentPosition == _correctPosition)
            CorrectPosition();
    }

    private void StartRotation()
    {
        int randomValue = Random.Range(0, 6);
        if (randomValue == _correctPosition)
        {
            StartRotation();
        }
        else
        {
            _currentPosition = randomValue;
            _visualGO.transform.Rotate(0, 0, _currentPosition * -60f);
        }
    }

    private void HexSelected()
    {
        _hexagonsManager.HexagonSelected(this);
    }

    private void CorrectPosition()
    {
        _isCorrectlyPlaced = true;
        _hexagonsManager.HexagonPlacedCorrectly();
    }

    private void LeftCorrectPosition()
    {
        _isCorrectlyPlaced = false;
        _hexagonsManager.HexagonLeftCorrectPostion();
    }
}
