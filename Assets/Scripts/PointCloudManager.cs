using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

using System.IO;

using System.Threading.Tasks;

public class PointCloudManager : MonoBehaviour
{
    //pt 수
    public int num;

    public Mesh mesh;

    // pc 좌표
    public Vector3[] vertices;

    // pc 색 정보
    public Color32[] colors;

    // pc index
    public int[] indices;

    // RGB visualization
    public Texture2D kinectColorTexture;

    [SerializeField]
    UnityEngine.UI.RawImage rawColorImg;

    public void setVertices(Vector3[] vertices_in)
    {
        vertices = vertices_in;
    }

    public void setColors(Color32[] colors_in)
    {
        colors = colors_in;
    }
    
}
