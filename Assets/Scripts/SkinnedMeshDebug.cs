using UnityEngine;

public class SkinnedMeshDebug : MonoBehaviour
{
    void Start()
    {
        SkinnedMesh mesh = GetComponent<SkinnedMesh>();
        mesh.OnResultsReady += DrawVertices;
    }
    void DrawVertices(SkinnedMesh mesh)
    {
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 position = mesh.vertices[i];
            Vector3 normal = mesh.normals[i];

            Color color = Color.green;
            // Debug.DrawWireCube(position, 0.1f * Vector3.one, color);
            Debug.DrawRay(position, normal, color);
        }
    }
}