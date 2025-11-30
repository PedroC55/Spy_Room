using UnityEngine;

public class ForcePassthrough : MonoBehaviour
{
    void Start()
    {
        // Força passthrough de TODAS as formas possíveis
        Invoke(nameof(EnableEverything), 0.5f);
    }
    
    void EnableEverything()
    {
        // 1. Configura câmera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            Color bg = Color.black;
            bg.a = 0;
            cam.backgroundColor = bg;
            Debug.Log("✅ Câmera configurada");
        }
        
        // 2. Ativa todos os Passthrough Layers
        var layers = FindObjectsByType<OVRPassthroughLayer>(FindObjectsSortMode.None);
        Debug.Log($"Encontrados {layers.Length} passthrough layers");
        
        foreach (var layer in layers)
        {
            layer.enabled = true;
            layer.gameObject.SetActive(true);
            Debug.Log($"✅ Ativado: {layer.gameObject.name}");
        }
        
        // 3. Força OVRManager
        if (OVRManager.instance != null)
        {
            Debug.Log("✅ OVRManager encontrado");
        }
    }
}
