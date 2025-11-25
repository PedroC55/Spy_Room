using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;

/// <summary>
/// Helper para trabalhar com Anchor Points e Room Data
/// Use este script para aceder a informações da sala
/// </summary>
public class RoomDataHelper : MonoBehaviour
{
    [Header("References")]
    public MRSceneManager sceneManager;
    
    [Header("Anchor Visualization")]
    public bool visualizeAnchors = true;
    public GameObject anchorMarkerPrefab; // Opcional: prefab para marcar anchors
    public float markerSize = 0.1f;
    
    [Header("Anchor Filtering")]
    public bool findWalls = true;
    public bool findFloor = true;
    public bool findCeiling = true;
    public bool findFurniture = true;
    
    private List<GameObject> anchorMarkers = new List<GameObject>();
    
    void Start()
    {
        if (sceneManager == null)
        {
            sceneManager = FindObjectOfType<MRSceneManager>();
        }
        
        // Espera a cena carregar
        StartCoroutine(WaitForSceneAndVisualize());
    }
    
    System.Collections.IEnumerator WaitForSceneAndVisualize()
    {
        // Espera até a cena estar carregada
        while (sceneManager == null || !sceneManager.IsSceneLoaded())
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        Debug.Log("✓ Cena carregada! Processando anchors...");
        
        if (visualizeAnchors)
        {
            VisualizeAllAnchors();
        }
        
        // Exemplo de uso
        ExampleUsage();
    }
    
    void VisualizeAllAnchors()
    {
        var anchors = sceneManager.GetAllAnchors();
        
        if (anchors == null || anchors.Count == 0)
        {
            Debug.LogWarning("⚠️ Nenhum anchor encontrado");
            return;
        }
        
        Debug.Log($"📍 Visualizando {anchors.Count} anchors...");
        
        foreach (var anchor in anchors)
        {
            CreateAnchorMarker(anchor);
        }
    }
    
    void CreateAnchorMarker(MRUKAnchor anchor)
    {
        GameObject marker;
        
        if (anchorMarkerPrefab != null)
        {
            marker = Instantiate(anchorMarkerPrefab, anchor.transform.position, Quaternion.identity);
        }
        else
        {
            // Cria um cubo simples como marcador
            marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.position = anchor.transform.position;
            marker.transform.localScale = Vector3.one * markerSize;
            
            // Cor baseada no tipo
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = GetColorForLabel(anchor.Label);
                renderer.material = mat;
            }
        }
        
        marker.name = $"Marker_{anchor.Label}_{anchor.name}";
        anchorMarkers.Add(marker);
    }
    
    Color GetColorForLabel(MRUKAnchor.SceneLabels label)
    {
        switch (label)
        {
            case MRUKAnchor.SceneLabels.FLOOR:
                return Color.green;
            case MRUKAnchor.SceneLabels.CEILING:
                return Color.cyan;
            case MRUKAnchor.SceneLabels.WALL_FACE:
                return Color.gray;
            case MRUKAnchor.SceneLabels.TABLE:
                return Color.yellow;
            case MRUKAnchor.SceneLabels.COUCH:
                return Color.magenta;
            case MRUKAnchor.SceneLabels.WINDOW_FRAME:
                return Color.blue;
            case MRUKAnchor.SceneLabels.DOOR_FRAME:
                return new Color(0.6f, 0.3f, 0);
            default:
                return Color.white;
        }
    }
    
    // ===== MÉTODOS DE EXEMPLO =====
    
    void ExampleUsage()
    {
        Debug.Log("=== EXEMPLOS DE USO ===");
        
        // 1. Obter todas as paredes
        if (findWalls)
        {
            var walls = sceneManager.GetAnchorsByLabel(MRUKAnchor.SceneLabels.WALL_FACE);
            Debug.Log($"🧱 Encontradas {walls?.Count ?? 0} paredes");
            
            if (walls != null)
            {
                foreach (var wall in walls)
                {
                    Debug.Log($"  Parede: {wall.name} em {wall.transform.position}");
                }
            }
        }
        
        // 2. Obter o chão
        if (findFloor)
        {
            var room = sceneManager.GetCurrentRoom();
            if (room != null && room.FloorAnchor != null)
            {
                Debug.Log($"🟩 Chão: {room.FloorAnchor.transform.position}");
                Debug.Log($"   Tamanho: {room.FloorAnchor.VolumeBounds.Value.size}");
            }
        }
        
        // 3. Obter o teto
        if (findCeiling)
        {
            var room = sceneManager.GetCurrentRoom();
            if (room != null && room.CeilingAnchor != null)
            {
                Debug.Log($"⬜ Teto: {room.CeilingAnchor.transform.position}");
            }
        }
        
        // 4. Obter móveis (mesas, sofás, etc)
        if (findFurniture)
        {
            var tables = sceneManager.GetAnchorsByLabel(MRUKAnchor.SceneLabels.TABLE);
            Debug.Log($"🪑 Mesas: {tables?.Count ?? 0}");
            
            var couches = sceneManager.GetAnchorsByLabel(MRUKAnchor.SceneLabels.COUCH);
            Debug.Log($"🛋️ Sofás: {couches?.Count ?? 0}");
        }
        
        Debug.Log("========================");
    }
    
    // ===== MÉTODOS PÚBLICOS ÚTEIS =====
    
    /// <summary>
    /// Encontra o anchor mais próximo de uma posição
    /// </summary>
    public MRUKAnchor FindNearestAnchor(Vector3 position, MRUKAnchor.SceneLabels? filterByLabel = null)
    {
        var anchors = sceneManager.GetAllAnchors();
        if (anchors == null || anchors.Count == 0) return null;
        
        MRUKAnchor nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var anchor in anchors)
        {
            // Filtro por label se especificado
            if (filterByLabel.HasValue && anchor.Label != filterByLabel.Value)
                continue;
            
            float distance = Vector3.Distance(position, anchor.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = anchor;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Verifica se uma posição está dentro da sala
    /// </summary>
    public bool IsPositionInRoom(Vector3 position)
    {
        var room = sceneManager.GetCurrentRoom();
        if (room == null) return false;
        
        // Verifica altura (entre chão e teto)
        if (room.FloorAnchor != null && room.CeilingAnchor != null)
        {
            float floorY = room.FloorAnchor.transform.position.y;
            float ceilingY = room.CeilingAnchor.transform.position.y;
            
            if (position.y < floorY || position.y > ceilingY)
                return false;
        }
        
        // TODO: Verificar se está dentro das paredes (polygon check)
        
        return true;
    }
    
    /// <summary>
    /// Obtém a posição no chão mais próxima
    /// </summary>
    public Vector3? GetFloorPosition(Vector3 worldPosition)
    {
        var room = sceneManager.GetCurrentRoom();
        if (room == null || room.FloorAnchor == null) return null;
        
        Vector3 floorPos = worldPosition;
        floorPos.y = room.FloorAnchor.transform.position.y;
        
        return floorPos;
    }
    
    /// <summary>
    /// Obtém todas as superfícies planas (mesas, chão, etc)
    /// </summary>
    public List<MRUKAnchor> GetAllPlanarSurfaces()
    {
        var anchors = sceneManager.GetAllAnchors();
        if (anchors == null) return null;
        
        var surfaces = new List<MRUKAnchor>();
        
        foreach (var anchor in anchors)
        {
            // Verifica se é uma superfície plana
            if (anchor.Label == MRUKAnchor.SceneLabels.FLOOR ||
                anchor.Label == MRUKAnchor.SceneLabels.TABLE ||
                anchor.Label == MRUKAnchor.SceneLabels.COUCH ||
                anchor.VolumeBounds.HasValue)
            {
                surfaces.Add(anchor);
            }
        }
        
        return surfaces;
    }
    
    /// <summary>
    /// Limpa os marcadores visuais
    /// </summary>
    public void ClearAnchorMarkers()
    {
        foreach (var marker in anchorMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        anchorMarkers.Clear();
    }
    
    void OnDestroy()
    {
        ClearAnchorMarkers();
    }
}
