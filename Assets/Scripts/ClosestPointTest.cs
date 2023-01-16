using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPointTest : MonoBehaviour
{
    public GameObject pointSample;
    public GameObject pointOnMesh;


    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //var collider = GetComponent<Collider>();

        //if (!collider)
        //{
        //    return; // nothing to do without a collider
        //}

        //Vector3 location = pointSample.transform.position;
        //Vector3 closestPoint = collider.ClosestPoint(location);

        ////Debug.Log(closestPoint.x);

        //pointOnMesh.transform.position = closestPoint;

        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        Vector3 location = pointSample.transform.position;
        Vector3 closestPoint = meshCollider.ClosestPointOnBounds(location);

        //Vector3 point = new Vector3(0,0,0);
        //Physics.ClosestPoint(point, meshCollider, location, V);

        //Debug.Log(closestPoint.x);

        pointOnMesh.transform.position = closestPoint;

    }
}
