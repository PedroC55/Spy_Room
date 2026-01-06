using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class HexagonsManager : MonoBehaviour
{
    [SerializeField]
    private HexagonController _selectedHex;

    [SerializeField]
    private VisualEffect _visualEffect;

    private List<HexagonController> _hexagonsList = new ();

    private int _totalHexagons = 0;
    private int _correctlyPlacedHexagons = 0;

    private void Start()
    {
        if(_selectedHex == null)
        {
            Debug.LogError("Hex Manager has no first selected Hex");
        }
    }
    
    public void RegisterHexagon(HexagonController hex)
    {
        if(!_hexagonsList.Contains(hex))
        {
            _hexagonsList.Add(hex);
            _totalHexagons++;
        }
    }

    public void HexagonSelected(HexagonController selectedHex)
    {
        if(!_visualEffect.isActiveAndEnabled)
            _visualEffect.enabled = true;

        Vector3 vfxPosition = new (selectedHex.transform.position.x, selectedHex.transform.position.y, _visualEffect.transform.position.z);
        _visualEffect.transform.position = vfxPosition;

        _selectedHex = selectedHex;
        Debug.Log($"Hexagon selected: {selectedHex.gameObject.name}");
    }

    public void SpinSelectedHex(int value)
    {
        if (_selectedHex == null) return;

        if(value < 0)
            _selectedHex.SpinClockWise(value);
        else
            _selectedHex.SpinCounterClockWise(value);
    }

    public void HexagonPlacedCorrectly()
    {
        _correctlyPlacedHexagons++;
        if(_correctlyPlacedHexagons == _totalHexagons)
        {
            Debug.Log("All hexagons placed correctly! Minigame complete.");
            GameEvents.MinigameCompleted();
            Destroy(transform.parent.gameObject);
        }
    }

    public void HexagonLeftCorrectPostion()
    {
        _correctlyPlacedHexagons--;
    }
}
