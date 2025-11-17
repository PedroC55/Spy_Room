using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Remove automaticamente artefatos visuais (objetos coloridos estranhos)
/// Anexe ao MRUK ou a um GameObject na cena
/// </summary>
public class ArtifactCleaner : MonoBehaviour
{
    [Header("Cleanup Settings")]
    public bool cleanOnStart = true;
    public bool cleanContinuously = false;
    public float cleanInterval = 2f; // Segundos entre limpezas
    
    [Header("Detection Settings")]
    public bool removeMagenta = true; // Rosa brilhante
    public bool removeGreen = true; // Verde brilhante
    public bool removeBlue = true; // Azul brilhante
    public bool removeCyan = true; // Cyan brilhante
    public bool removeDefaultMaterials = true;
    
    [Header("Action")]
    public CleanupAction action = CleanupAction.Hide; // Hide ou Delete
    
    [Header("Whitelist (não tocar nestes)")]
    public List<string> ignoredObjectNames = new List<string>()
    {
        "OVRCameraRig",
        "Main Camera",
        "MRUK",
        "TourManager"
    };
    
    [Header("Debug")]
    public bool verboseLogging = false;
    
    public enum CleanupAction
    {
        Hide,      // Esconde (desabilita renderer)
        Delete,    // Delete completamente
        Transparent // Torna transparente
    }
    
    private float cleanTimer = 0f;
    private int totalCleaned = 0;
    
    void Start()
    {
        if (cleanOnStart)
        {
            CleanArtifacts();
        }
    }
    
    void Update()
    {
        if (cleanContinuously)
        {
            cleanTimer += Time.deltaTime;
            
            if (cleanTimer >= cleanInterval)
            {
                CleanArtifacts();
                cleanTimer = 0f;
            }
        }
    }
    
    [ContextMenu("Clean Artifacts Now")]
    public void CleanArtifacts()
    {
        Debug.Log("🧹 Iniciando limpeza de artefatos...");
        
        int cleaned = 0;
        
        // Encontra todos os renderers na cena
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            // Verifica whitelist
            if (IsInWhitelist(renderer.gameObject))
            {
                if (verboseLogging)
                    Debug.Log($"⚪ Ignorando (whitelist): {renderer.gameObject.name}");
                continue;
            }
            
            // Verifica se é um artefato
            if (IsArtifact(renderer))
            {
                ProcessArtifact(renderer);
                cleaned++;
            }
        }
        
        totalCleaned += cleaned;
        
        if (cleaned > 0)
        {
            Debug.Log($"✅ Limpeza completa! {cleaned} artefatos processados (Total: {totalCleaned})");
        }
        else
        {
            if (verboseLogging)
                Debug.Log("✓ Nenhum artefato encontrado");
        }
    }
    
    bool IsInWhitelist(GameObject obj)
    {
        foreach (string name in ignoredObjectNames)
        {
            if (obj.name.Contains(name))
                return true;
        }
        return false;
    }
    
    bool IsArtifact(Renderer renderer)
    {
        if (renderer.material == null)
            return false;
        
        Color color = renderer.material.color;
        string materialName = renderer.material.name.ToLower();
        
        // Detecta cores específicas de artefatos
        if (removeMagenta && IsColorClose(color, Color.magenta))
        {
            if (verboseLogging)
                Debug.Log($"🔴 Artefato magenta detectado: {renderer.gameObject.name}");
            return true;
        }
        
        if (removeGreen && IsColorClose(color, Color.green) && color.a > 0.9f)
        {
            if (verboseLogging)
                Debug.Log($"🟢 Artefato verde detectado: {renderer.gameObject.name}");
            return true;
        }
        
        if (removeBlue && IsColorClose(color, Color.blue) && color.a > 0.9f)
        {
            if (verboseLogging)
                Debug.Log($"🔵 Artefato azul detectado: {renderer.gameObject.name}");
            return true;
        }
        
        if (removeCyan && IsColorClose(color, Color.cyan) && color.a > 0.9f)
        {
            if (verboseLogging)
                Debug.Log($"🔷 Artefato cyan detectado: {renderer.gameObject.name}");
            return true;
        }
        
        // Detecta materiais default do Unity
        if (removeDefaultMaterials && 
            (materialName.Contains("default") || 
             materialName.Contains("material") && materialName.Length < 20))
        {
            if (verboseLogging)
                Debug.Log($"📦 Material default detectado: {renderer.gameObject.name}");
            return true;
        }
        
        return false;
    }
    
    bool IsColorClose(Color a, Color b, float threshold = 0.2f)
    {
        float rDiff = Mathf.Abs(a.r - b.r);
        float gDiff = Mathf.Abs(a.g - b.g);
        float bDiff = Mathf.Abs(a.b - b.b);
        
        return (rDiff + gDiff + bDiff) / 3f < threshold;
    }
    
    void ProcessArtifact(Renderer renderer)
    {
        string objName = renderer.gameObject.name;
        
        switch (action)
        {
            case CleanupAction.Hide:
                renderer.enabled = false;
                if (verboseLogging)
                    Debug.Log($"👁️ Escondido: {objName}");
                break;
                
            case CleanupAction.Delete:
                Destroy(renderer.gameObject);
                if (verboseLogging)
                    Debug.Log($"🗑️ Deletado: {objName}");
                break;
                
            case CleanupAction.Transparent:
                Color transparent = renderer.material.color;
                transparent.a = 0.05f;
                renderer.material.color = transparent;
                if (verboseLogging)
                    Debug.Log($"👻 Transparente: {objName}");
                break;
        }
    }
    
    // Método público para adicionar à whitelist
    public void AddToWhitelist(string objectName)
    {
        if (!ignoredObjectNames.Contains(objectName))
        {
            ignoredObjectNames.Add(objectName);
            Debug.Log($"✓ Adicionado à whitelist: {objectName}");
        }
    }
    
    // Método público para forçar limpeza
    public void ForceClean()
    {
        CleanArtifacts();
    }
    
    // Estatísticas
    public int GetTotalCleaned()
    {
        return totalCleaned;
    }
}
