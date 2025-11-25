using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GenerateMeshOnLineRenderer : MonoBehaviour
{
    void Start()
    {
        GenerateColliderMesh();
        GenerateKinematicRigidbody();
    }

    private void GenerateColliderMesh(){
        MeshCollider collider = GetComponent<MeshCollider>();

        if(collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }

        Mesh mesh = new();
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.BakeMesh(mesh, true);
        collider.sharedMesh = mesh;

        collider.convex = true;
        collider.isTrigger = true;
    }

    private void GenerateKinematicRigidbody(){
        Rigidbody rb = GetComponent<Rigidbody>();

        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.gameObject.tag);
    }
}
