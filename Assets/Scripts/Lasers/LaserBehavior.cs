using UnityEngine;

public class LaserBehavior : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    
    private LineRenderer lineRenderer;
    private Color originalColor;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            originalColor = lineRenderer.startColor;
        }
    }

    void Update()
    {
        if (enablePulse && lineRenderer != null)
        {
            // Create pulsing effect
            float pulse = Mathf.Lerp(minIntensity, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            Color pulsedColor = originalColor * pulse;
            pulsedColor.a = originalColor.a;
            
            lineRenderer.startColor = pulsedColor;
            lineRenderer.endColor = pulsedColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Laser triggered by: {other.gameObject.name}");
        // Check if player touched the laser
        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            OnLaserTouched(other.gameObject);
        }
    }

    private void OnLaserTouched(GameObject player)
    {
        Debug.Log($"Laser touched by: {player.name}");
        
        // Find the score manager and decrease points
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.DecreaseScore(10);
        }

        // Visual feedback
        StartCoroutine(FlashLaser());
    }

    private System.Collections.IEnumerator FlashLaser()
    {
        if (lineRenderer == null) yield break;

        Color flashColor = Color.white;
        lineRenderer.startColor = flashColor;
        lineRenderer.endColor = flashColor;

        yield return new WaitForSeconds(0.1f);

        lineRenderer.startColor = originalColor;
        lineRenderer.endColor = originalColor;
    }
}
