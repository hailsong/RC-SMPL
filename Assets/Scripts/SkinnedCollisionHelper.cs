
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class SkinnedCollisionHelper : MonoBehaviour
{
    class CVertexWeight
    {
        public int index;
        public Vector3 localPosition;
        public float weight;

        public CVertexWeight(int i, Vector3 p, float w)
        {
            index = i;
            localPosition = p;
            weight = w;
        }
    }

    class CWeightList
    {
        public Transform transform;
        public ArrayList weights;
        public CWeightList()
        {
            weights = new ArrayList();
        }
    }

    public bool forceUpdate;
    public bool updateOncePerFrame = true;

    private CWeightList[] nodeWeights; // one per node

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;


    /// <summary>
    ///  This basically translates the information about the skinned mesh into
    /// data that we can internally use to quickly update the collision mesh.
    /// </summary>
    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (meshCollider != null && skinnedMeshRenderer != null)
        {
            // Cache used values rather than accessing straight from the mesh on the loop below
            Vector3[] cachedVertices = skinnedMeshRenderer.sharedMesh.vertices;
            Matrix4x4[] cachedBindposes = skinnedMeshRenderer.sharedMesh.bindposes;
            BoneWeight[] cachedBoneWeights = skinnedMeshRenderer.sharedMesh.boneWeights;

            // Make a CWeightList for each bone in the skinned mesh
            nodeWeights = new CWeightList[skinnedMeshRenderer.bones.Length];
            for (int i = 0; i < skinnedMeshRenderer.bones.Length; i++)
            {
                nodeWeights[i] = new CWeightList();
                nodeWeights[i].transform = skinnedMeshRenderer.bones[i];
            }

            // Create a bone weight list for each bone, ready for quick calculation during an update...
            for (int i = 0; i < cachedVertices.Length; i++)
            {
                BoneWeight bw = cachedBoneWeights[i];
                if (bw.weight0 != 0.0f)
                {
                    Vector3 localPt = cachedBindposes[bw.boneIndex0].MultiplyPoint3x4(cachedVertices[i]);
                    nodeWeights[bw.boneIndex0].weights.Add(new CVertexWeight(i, localPt, bw.weight0));
                }
                if (bw.weight1 != 0.0f)
                {
                    Vector3 localPt = cachedBindposes[bw.boneIndex1].MultiplyPoint3x4(cachedVertices[i]);
                    nodeWeights[bw.boneIndex1].weights.Add(new CVertexWeight(i, localPt, bw.weight1));
                }
                if (bw.weight2 != 0.0f)
                {
                    Vector3 localPt = cachedBindposes[bw.boneIndex2].MultiplyPoint3x4(cachedVertices[i]);
                    nodeWeights[bw.boneIndex2].weights.Add(new CVertexWeight(i, localPt, bw.weight2));
                }
                if (bw.weight3 != 0.0f)
                {
                    Vector3 localPt = cachedBindposes[bw.boneIndex3].MultiplyPoint3x4(cachedVertices[i]);
                    nodeWeights[bw.boneIndex3].weights.Add(new CVertexWeight(i, localPt, bw.weight3));
                }
            }

            UpdateCollisionMesh();
        }
        else
        {
            Debug.LogError("[SkinnedCollisionHelper] " + gameObject.name + " is missing SkinnedMeshRenderer or MeshCollider!");
        }

    }

    /// <summary>
    /// Manually recalculates the collision mesh of the skinned mesh on this object.
    /// </summary>
    public void UpdateCollisionMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] newVert = new Vector3[skinnedMeshRenderer.sharedMesh.vertices.Length];

        // Now get the local positions of all weighted indices...
        foreach (CWeightList wList in nodeWeights)
        {
            foreach (CVertexWeight vw in wList.weights)
            {
                newVert[vw.index] += wList.transform.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
            }
        }

        // Now convert each point into local coordinates of this object.
        for (int i = 0; i < newVert.Length; i++)
        {
            newVert[i] = transform.InverseTransformPoint(newVert[i]);
        }

        // Update the mesh ( collider) with the updated vertices
        mesh.vertices = newVert;
        mesh.uv = skinnedMeshRenderer.sharedMesh.uv; // is this even needed here?
        mesh.triangles = skinnedMeshRenderer.sharedMesh.triangles;
        mesh.RecalculateBounds();
        mesh.MarkDynamic(); // says it should improve performance, but I couldn't see it happening
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// If the 'forceUpdate' flag is set, updates the collision mesh for the skinned mesh on this object
    /// </summary>
    void Update()
    {
        UpdateCollisionMesh();
        //if (forceUpdate)
        //{
        //    if (updateOncePerFrame) forceUpdate = false;
        //    UpdateCollisionMesh();
        //}
    }


}