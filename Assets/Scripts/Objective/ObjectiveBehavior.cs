using Oculus.Interaction;
using UnityEngine;

public class ObjectiveBehavior : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minIntensity = 0.5f;

    private LaserSpawner laserSpawner;


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Objective touched by: {other.name}");
        HitObjective.GetHit();
        laserSpawner.SpawnCellingLaser();
    }

    public void SetLaserSpawner(LaserSpawner laserSpawner)
    {
        this.laserSpawner = laserSpawner;
    }

}
