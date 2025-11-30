using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int startingScore = 100;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Canvas scoreCanvas;
    
    private int currentScore;
    private Camera mainCamera;

    void Start()
    {
        currentScore = startingScore;
        
        // Find the XR camera automatically
        FindXRCamera();
        
        UpdateScoreDisplay();
    }

    private void FindXRCamera()
    {
        // Try to find the main camera (usually the XR camera is tagged as MainCamera)
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            // If not found, search for any camera using the new API
            mainCamera = FindFirstObjectByType<Camera>();
        }

        // Set the canvas event camera if canvas exists and is in World Space
        if (scoreCanvas != null && mainCamera != null)
        {
            if (scoreCanvas.renderMode == RenderMode.WorldSpace)
            {
                scoreCanvas.worldCamera = mainCamera;
                Debug.Log($"Canvas camera set to: {mainCamera.name}");
            }
        }
    }

    public void DecreaseScore(int amount)
    {
        currentScore -= amount;
        if (currentScore < 0) currentScore = 0;
        
        UpdateScoreDisplay();
        Debug.Log($"Score decreased by {amount}. Current score: {currentScore}");
    }

    public void IncreaseScore(int amount)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = startingScore;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
}
