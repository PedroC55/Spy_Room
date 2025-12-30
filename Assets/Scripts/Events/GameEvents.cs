using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public delegate void DiamondGrabHandler();
    public static event DiamondGrabHandler OnDiamondGrab;

    public delegate void HitLaserHandler();
    public static event HitLaserHandler OnHitLaser;

    public static void DiamongGrab()
    {
        OnDiamondGrab?.Invoke();
    }

    public static void HitLaser()
    {
        OnHitLaser?.Invoke();
    }
}
