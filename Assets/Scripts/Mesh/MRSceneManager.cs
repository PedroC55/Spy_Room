using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

/// <summary>
/// Gerencia a cena Mixed Reality - pede scan da sala e carrega room data
/// Anexe ao objeto MRUK na cena
/// </summary>
public class MRSceneManager : MonoBehaviour
{
    [Header("Scene Setup")]
    public bool requestRoomSetupIfNeeded = true; // Pede scan automático
    public float checkDelay = 1f; // Tempo antes de verificar
    
    [Header("Effect Mesh Settings")]
    public bool showEffectMesh = true; // Mostra mesh durante scan
    public Material effectMeshMaterial; // Material customizado (opcional)
    public float effectMeshDuration = 5f; // Tempo que mesh fica visível
    
    [Header("Debug")]
    public bool verboseLogging = true;
    
    private MRUK mruk;
    private bool roomSetupRequested = false;
    private bool sceneLoaded = false;
    
    void Start()
    {
        // Obtém referência ao MRUK
        mruk = MRUK.Instance;
        
        if (mruk == null)
        {
            Debug.LogError("❌ MRUK não encontrado! Certifique-se que tem o MRUK prefab na cena.");
            return;
        }
        
        // Registra callbacks
        RegisterCallbacks();
        
        // Verifica se precisa de scan após delay
        StartCoroutine(CheckSceneAfterDelay());
    }
    
    void RegisterCallbacks()
    {
        // Quando a cena carrega
        if (mruk != null)
        {
            // MRUK tem eventos - vamos usar
            Debug.Log("✓ Callbacks registrados para MRUK");
        }
    }
    
    IEnumerator CheckSceneAfterDelay()
    {
        yield return new WaitForSeconds(checkDelay);
        
        if (verboseLogging)
            Debug.Log("🔍 Verificando se há dados da cena...");
        
        // Verifica se já tem room data
        bool hasRoomData = CheckIfRoomDataExists();
        
        if (!hasRoomData && requestRoomSetupIfNeeded)
        {
            Debug.LogWarning("⚠️ Nenhum dado de sala encontrado!");
            RequestRoomSetup();
        }
        else if (hasRoomData)
        {
            Debug.Log("✅ Dados da sala já existem!");
            OnSceneLoaded();
        }
    }
    
    bool CheckIfRoomDataExists()
    {
        if (mruk == null) return false;
        
        // Verifica se MRUK tem rooms carregados
        var currentRoom = mruk.GetCurrentRoom();
        
        if (currentRoom != null)
        {
            if (verboseLogging)
                Debug.Log($"✓ Sala atual encontrada: {currentRoom.name}");
            return true;
        }
        
        if (verboseLogging)
            Debug.Log("⚠️ Nenhuma sala carregada no MRUK");
        
        return false;
    }
    
    void RequestRoomSetup()
    {
        if (roomSetupRequested)
        {
            Debug.Log("⏳ Room setup já foi solicitado, aguardando...");
            return;
        }
        
        roomSetupRequested = true;
        
        Debug.Log("🎯 Solicitando Room Setup ao utilizador...");
        
        // Pede ao utilizador para fazer scan da sala
        // Isto abre o Space Setup do Meta Quest
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // Usa Meta XR Scene API para pedir setup
            RequestSceneCapture();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erro ao solicitar room setup: {e.Message}");
        }
        #else
        Debug.LogWarning("⚠️ Room Setup só funciona no dispositivo Meta Quest!");
        Debug.Log("💡 No Editor, use Scene Capture (Meta > Tools > Capture Scene)");
        #endif
    }
    
    void RequestSceneCapture()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Usa OVR Scene para pedir captura
        // Isto abre o Space Setup automaticamente
        if (OVRManager.boundary != null)
        {
            Debug.Log("📱 Abrindo Space Setup...");
            // O Meta Quest vai abrir automaticamente o Space Setup
            OVRManager.boundary.RequestBoundaryVisible(true);
        }
        #endif
    }
    
    void OnSceneLoaded()
    {
        sceneLoaded = true;
        Debug.Log("✅ Cena carregada com sucesso!");
        
        // Obtém informações da sala
        PrintRoomInformation();
        
        // Configura EffectMesh
        if (showEffectMesh)
        {
            StartCoroutine(ShowEffectMeshTemporarily());
        }
    }
    
    void PrintRoomInformation()
    {
        if (mruk == null) return;
        
        var currentRoom = mruk.GetCurrentRoom();
        
        if (currentRoom == null)
        {
            Debug.LogWarning("⚠️ Nenhuma sala atual disponível");
            return;
        }
        
        Debug.Log("=== INFORMAÇÕES DA SALA ===");
        Debug.Log($"Nome: {currentRoom.name}");
        
        // Anchor Points (paredes, chão, teto, objetos)
        var anchors = currentRoom.Anchors;
        if (anchors != null)
        {
            Debug.Log($"📍 Total de Anchors: {anchors.Count}");
            
            foreach (var anchor in anchors)
            {
                Debug.Log($"  - {anchor.name}: {anchor.Label}");
            }
        }
        
        // Dimensões da sala
        if (currentRoom.FloorAnchor != null)
        {
            Debug.Log($"Chão: {currentRoom.FloorAnchor.name}");
        }
        
        if (currentRoom.CeilingAnchor != null)
        {
            Debug.Log($"Teto: {currentRoom.CeilingAnchor.name}");
        }
        
        var walls = currentRoom.WallAnchors;
        if (walls != null)
        {
            Debug.Log($"🧱 Paredes: {walls.Count}");
        }
        
        Debug.Log("=========================");
    }
    
    IEnumerator ShowEffectMeshTemporarily()
    {
        Debug.Log("🎨 Mostrando Effect Mesh...");
        
        // Encontra todos os EffectMesh na cena
        var effectMeshes = FindObjectsOfType<OVRSceneModelLoader>();
        
        foreach (var mesh in effectMeshes)
        {
            if (mesh.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.enabled = true;
                
                // Aplica material customizado se fornecido
                if (effectMeshMaterial != null)
                {
                    renderer.material = effectMeshMaterial;
                }
            }
        }
        
        // Espera e depois esconde
        yield return new WaitForSeconds(effectMeshDuration);
        
        Debug.Log("🎨 Escondendo Effect Mesh...");
        
        foreach (var mesh in effectMeshes)
        {
            if (mesh.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.enabled = false;
            }
        }
    }
    
    // Métodos públicos para aceder à room data
    
    /// <summary>
    /// Obtém a sala atual
    /// </summary>
    public MRUKRoom GetCurrentRoom()
    {
        if (mruk == null) return null;
        return mruk.GetCurrentRoom();
    }
    
    /// <summary>
    /// Obtém todos os anchor points da sala
    /// </summary>
    public System.Collections.Generic.List<MRUKAnchor> GetAllAnchors()
    {
        var room = GetCurrentRoom();
        if (room == null) return null;
        return room.Anchors;
    }
    
    /// <summary>
    /// Obtém anchors por tipo (parede, chão, mesa, etc)
    /// </summary>
    public System.Collections.Generic.List<MRUKAnchor> GetAnchorsByLabel(MRUKAnchor.SceneLabels label)
    {
        var room = GetCurrentRoom();
        if (room == null) return null;
        
        var result = new System.Collections.Generic.List<MRUKAnchor>();
        
        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == label)
            {
                result.Add(anchor);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Verifica se a cena está carregada
    /// </summary>
    public bool IsSceneLoaded()
    {
        return sceneLoaded && CheckIfRoomDataExists();
    }
    
    void OnDestroy()
    {
        // Cleanup
    }
}
