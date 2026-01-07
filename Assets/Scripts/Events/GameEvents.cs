using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public delegate void DiamondGrabHandler();
    public static event DiamondGrabHandler OnDiamondGrab;

    public delegate void HitLaserHandler();
    public static event HitLaserHandler OnHitLaser;

    public delegate void SpawnMinigameHandler();
    public static event SpawnMinigameHandler OnSpawnMinigame;

    public delegate void MinigaemCompletedHandler();
    public static event MinigaemCompletedHandler OnMinigameCompleted;

    public static void DiamongGrab()
    {
        OnDiamondGrab?.Invoke();
    }

    public static void HitLaser()
    {
        OnHitLaser?.Invoke();
    }

    public static void SpawnMinigame()
    {
        OnSpawnMinigame?.Invoke();
    }

    public static void MinigameCompleted()
    {
        OnMinigameCompleted?.Invoke();
    }
}
