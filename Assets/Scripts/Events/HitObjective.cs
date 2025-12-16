using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitObjective : MonoBehaviour
{
    public delegate void HitHandler();
    public static event HitHandler OnHit;

    public static void GetHit()
    {
        OnHit?.Invoke();
    }
}