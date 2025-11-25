using UnityEngine;

public class Utils : MonoBehaviour
{
    public static bool HasMultipleLayersOrNull(LayerMask mask)
    {
        int v = mask.value;

        return v == 0 || (v != 0 && (v & (v - 1)) != 0);
    }

    public static int LayerMaskToLayer(LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    public static void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            Utils.SetLayerRecursive(child.gameObject, newLayer);
        }
    }
}
