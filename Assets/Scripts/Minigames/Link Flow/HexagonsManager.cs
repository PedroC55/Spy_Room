using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HexagonsManager : MonoBehaviour
{
    [SerializeField]
    private HexagonController _selectedHex;

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
            //GameEvents.LinkFlowMinigameComplete();
        }
    }

    public void HexagonLeftCorrectPostion()
    {
        _correctlyPlacedHexagons--;
    }
}
