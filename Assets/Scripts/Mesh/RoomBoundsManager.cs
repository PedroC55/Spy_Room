using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Usa as paredes escaneadas da sala como limites do jogo
/// Em vez do Guardian, o jogo usa a geometria real da sala
/// </summary>
public class RoomBoundsManager : MonoBehaviour
{
    [Header("Boundary Visualization")]
    [SerializeField] private bool showWallBoundaries = true;
    [SerializeField] private Material boundaryMaterial;
    [SerializeField] private float boundaryHeight = 2.5f;
    [SerializeField] private float boundaryThickness = 0.02f;
    
    [Header("Player Warning")]
    [SerializeField] private bool warnPlayerNearWall = true;
    [SerializeField] private float warningDistance = 0.5f; // Metros da parede
    [SerializeField] private GameObject warningUI;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Camera/Head
    
    private MRUKRoom currentRoom;
    private bool isNearWall = false;

    void Start()
    {
        // Se não definido, usa a câmera principal
        if (playerTransform == null)
        {
            playerTransform = Camera.main.transform;
        }

        // Aguarda o room scan estar pronto
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnRoomLoaded);
        }
    }

    void OnRoomLoaded()
    {
        currentRoom = MRUK.Instance.GetCurrentRoom();
        
        if (currentRoom == null)
        {
            Debug.LogWarning("RoomBoundsManager: Nenhuma sala encontrada!");
            return;
        }

        Debug.Log("✅ Sala carregada! Configurando limites...");
        
        if (showWallBoundaries)
        {
            VisualizeRoomBoundaries();
        }
    }

    void Update()
    {
        if (currentRoom == null || playerTransform == null) return;

        if (warnPlayerNearWall)
        {
            CheckPlayerProximityToWalls();
        }
    }

    void VisualizeRoomBoundaries()
    {
        if (currentRoom == null) return;

        // Obtém todas as paredes
        var walls = currentRoom.WallAnchors;
        
        if (walls == null || walls.Count == 0)
        {
            Debug.LogWarning("Nenhuma parede encontrada!");
            return;
        }

        Debug.Log($"Visualizando {walls.Count} paredes da sala...");

        // Cria visualização para cada parede
        foreach (var wall in walls)
        {
            CreateWallBoundaryVisualization(wall);
        }
    }

    void CreateWallBoundaryVisualization(MRUKAnchor wallAnchor)
    {
        // Cria um objeto visual para a parede
        GameObject boundaryVis = new GameObject($"Boundary_{wallAnchor.name}");
        boundaryVis.transform.parent = wallAnchor.transform;
        boundaryVis.transform.localPosition = Vector3.zero;
        boundaryVis.transform.localRotation = Quaternion.identity;

        // Adiciona LineRenderer para mostrar o limite
        LineRenderer line = boundaryVis.AddComponent<LineRenderer>();
        
        // Configurar LineRenderer
        if (boundaryMaterial != null)
        {
            line.material = boundaryMaterial;
        }
        else
        {
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(1f, 0f, 0f, 0.5f); // Vermelho semi-transparente
            line.endColor = new Color(1f, 0f, 0f, 0.5f);
        }
        
        line.startWidth = boundaryThickness;
        line.endWidth = boundaryThickness;
        line.positionCount = 5; // 4 cantos + volta ao início

        // Obtém os cantos da parede
        if (wallAnchor.PlaneRect.HasValue)
        {
            Rect rect = wallAnchor.PlaneRect.Value;
            
            Vector3[] corners = new Vector3[5];
            corners[0] = wallAnchor.transform.TransformPoint(new Vector3(rect.xMin, 0, rect.yMin));
            corners[1] = wallAnchor.transform.TransformPoint(new Vector3(rect.xMax, 0, rect.yMin));
            corners[2] = wallAnchor.transform.TransformPoint(new Vector3(rect.xMax, 0, rect.yMax));
            corners[3] = wallAnchor.transform.TransformPoint(new Vector3(rect.xMin, 0, rect.yMax));
            corners[4] = corners[0]; // Volta ao início
            
            line.SetPositions(corners);
        }
    }

    void CheckPlayerProximityToWalls()
    {
        if (currentRoom == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        bool nearWall = false;

        // Verifica distância a cada parede
        var walls = currentRoom.WallAnchors;
        if (walls != null)
        {
            foreach (var wall in walls)
            {
                if (wall.PlaneRect.HasValue)
                {
                    // Calcula distância aproximada à parede
                    float distance = Vector3.Distance(playerPos, wall.transform.position);
                    
                    if (distance < warningDistance)
                    {
                        nearWall = true;
                        break;
                    }
                }
            }
        }

        // Atualiza estado
        if (nearWall != isNearWall)
        {
            isNearWall = nearWall;
            
            if (isNearWall)
            {
                OnPlayerNearWall();
            }
            else
            {
                OnPlayerInSafeArea();
            }
        }

        // Atualiza UI de aviso
        if (warningUI != null)
        {
            warningUI.SetActive(isNearWall);
        }
    }

    void OnPlayerNearWall()
    {
        Debug.LogWarning("⚠️ Jogador está perto da parede!");
        
        // Notifica outros scripts
        BroadcastMessage("OnPlayerNearBoundary", SendMessageOptions.DontRequireReceiver);
    }

    void OnPlayerInSafeArea()
    {
        Debug.Log("✅ Jogador voltou para área segura");
        
        // Notifica outros scripts
        BroadcastMessage("OnPlayerInSafeArea", SendMessageOptions.DontRequireReceiver);
    }

    // Método público para verificar se está perto da parede
    public bool IsPlayerNearWall()
    {
        return isNearWall;
    }

    // Método para obter a sala atual
    public MRUKRoom GetCurrentRoom()
    {
        return currentRoom;
    }

    // Método para obter distância à parede mais próxima
    public float GetDistanceToNearestWall()
    {
        if (currentRoom == null || playerTransform == null) return float.MaxValue;

        float minDistance = float.MaxValue;
        Vector3 playerPos = playerTransform.position;

        var walls = currentRoom.WallAnchors;
        if (walls != null)
        {
            foreach (var wall in walls)
            {
                float distance = Vector3.Distance(playerPos, wall.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
        }

        return minDistance;
    }

    // Visualização no Editor
    void OnDrawGizmos()
    {
        if (currentRoom == null || !showWallBoundaries) return;

        Gizmos.color = Color.red;
        
        var walls = currentRoom.WallAnchors;
        if (walls != null)
        {
            foreach (var wall in walls)
            {
                if (wall.PlaneRect.HasValue)
                {
                    Gizmos.DrawWireCube(wall.transform.position, Vector3.one * 0.5f);
                }
            }
        }

        // Mostra warning distance
        if (playerTransform != null && warnPlayerNearWall)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, warningDistance);
        }
    }
}
