using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Configura o EffectMesh para visualização correta da sala
/// Remove artefatos visuais (cores estranhas)
/// </summary>
[RequireComponent(typeof(MRUKAnchor))]
public class EffectMeshController : MonoBehaviour
{
    [Header("Material Settings")]
    public Material customMaterial; // Material customizado (wireframe, transparente, etc)
    public bool useWireframe = true;
    public Color wireframeColor = new Color(0, 1, 1, 0.3f); // Cyan transparente
    
    [Header("Visibility")]
    public bool showOnStart = false; // Mostra mesh ao iniciar
    public bool hideAfterDelay = true;
    public float hideDelay = 5f;
    
    [Header("Auto Setup")]
    public bool autoConfigureMaterial = true; // Configura material automaticamente
    
    private Renderer meshRenderer;
    private MRUKAnchor anchor;
    private Material originalMaterial;
    
    void Start()
    {
        // Obtém componentes
        meshRenderer = GetComponent<Renderer>();
        anchor = GetComponent<MRUKAnchor>();
        
        if (meshRenderer == null)
        {
            Debug.LogWarning($"⚠️ Renderer não encontrado em {gameObject.name}");
            return;
        }
        
        // Salva material original
        originalMaterial = meshRenderer.material;
        
        // Configura material
        if (autoConfigureMaterial)
        {
            ConfigureMaterial();
        }
        
        // Visibilidade inicial
        meshRenderer.enabled = showOnStart;
        
        // Esconde após delay se configurado
        if (showOnStart && hideAfterDelay)
        {
            Invoke(nameof(Hide), hideDelay);
        }
    }
    
    void ConfigureMaterial()
    {
        if (customMaterial != null)
        {
            // Usa material customizado
            meshRenderer.material = customMaterial;
            Debug.Log($"✓ Material customizado aplicado em {gameObject.name}");
        }
        else if (useWireframe)
        {
            // Cria material wireframe
            Material wireMat = CreateWireframeMaterial();
            meshRenderer.material = wireMat;
            Debug.Log($"✓ Material wireframe criado para {gameObject.name}");
        }
        else
        {
            // Material transparente simples
            Material transparentMat = CreateTransparentMaterial();
            meshRenderer.material = transparentMat;
            Debug.Log($"✓ Material transparente aplicado em {gameObject.name}");
        }
    }
    
    Material CreateWireframeMaterial()
    {
        // Cria material wireframe
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = wireframeColor;
        
        // Configura para ser transparente
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0); // Alpha
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        
        return mat;
    }
    
    Material CreateTransparentMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1, 1, 1, 0.1f); // Quase transparente
        
        // Transparente
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_Blend", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        
        return mat;
    }
    
    // Métodos públicos para controlar visibilidade
    
    public void Show()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
    }
    
    public void Hide()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
    }
    
    public void Toggle()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = !meshRenderer.enabled;
        }
    }
    
    public void SetVisibility(bool visible)
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }
    }
    
    public void SetColor(Color color)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = color;
        }
    }
    
    void OnDestroy()
    {
        // Limpa material criado
        if (meshRenderer != null && meshRenderer.material != originalMaterial)
        {
            Destroy(meshRenderer.material);
        }
    }
}
