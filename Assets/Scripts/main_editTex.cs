using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

using System.Collections;
using System.Collections.Generic;

using System.IO;

using System.Threading.Tasks;
using System.Threading;



public class main_editTex : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    Transformation transformation;


    Texture2D kinectColorTexture;

    [SerializeField]
    UnityEngine.UI.RawImage rawColorImg;

    [Header("Pointcloud Filter")]
    [SerializeField]
    Transform avatarOrigin;
    [Range(0.0f, 3.0f)]
    public float filterThreshold;
    public bool ViewPointCloud = false;

    [Header("Raycast Setting")]
    [SerializeField]
    Transform CameraOrigin;
    public Camera sceneCamera, canvasCam;
    [SerializeField]
    float distanceThreshold = 0.1f;
    [SerializeField]
    Color brushColor = Color.red;
    public Transform rayTarget;
    public GameObject brushContainer;
    public Sprite cursorPaint, cursorDecal; // Cursor for the differen functions 
    public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    public Material baseMaterial;
    [SerializeField]
    float brushSize = 0.13f; //The size of our brush
    public bool UseThread = true;


    [Header("Log UI")]
    [SerializeField]
    public GameObject debugRaycast;
    [SerializeField]
    public GameObject debugSaveTex;

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


    Vector3 rayDirection;
    bool saving = false; //Flag to check if we are saving the texture
    int brushCounter = 0;


    public int textureWidth = 1024;
    public int textureHeight = 1024;


    //pt 수
    int num = 368640;

    Mesh mesh;


    // pc 좌표
    Vector3[] vertices;

    // pc 색 정보
    Color32[] colors;

    // pc index
    int[] indices;

    public int updateNumber = 0;
    //public Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);

    void Start()
    {
        //tracker ids needed for when there are two trackers
        const int TRACKER_ID = 0;
        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);

        // mesh 인스턴스화
        mesh = new Mesh();

        // 점 갯수 65535 이상으로!
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];

        // PointColoud 배열 번호 기록
        for (int i = 0; i < num; i++)
        {
            indices[i] = i;
        }
        // 점의 좌표랑 색상 mesh로 전달

        if (ViewPointCloud)
        {
            mesh.vertices = vertices;
            mesh.colors32 = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            GetComponent<MeshRenderer>().enabled = true;
        }




        // mesh를 MeshFilter에 적용
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        watch.Start();

        //thread = new Thread(TextureStream);
        //thread.Start();

    }

    void Update()
    {
        if (m_skeletalTrackingProvider.IsRunning)
        {
            if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
            {
                if (m_lastFrameData.NumOfBodies != 0)
                {
                    m_tracker.GetComponent<TrackerHandler>().updateTracker(m_lastFrameData);
                }
                vertices = m_tracker.GetComponent<TrackerHandler>().getVerticesData(m_lastFrameData);
                colors = m_tracker.GetComponent<TrackerHandler>().getColorsData(m_lastFrameData);
                indices = m_tracker.GetComponent<TrackerHandler>().getIndicesData(m_lastFrameData);

                // Debug.Log("Colors : " + colors[1000].r.ToString());
                // Debug.Log("Vertices : " + vertices[1000].x.ToString());

                //for (int i = 0; i < num; i++)
                //{
                //    if (Vector3.Magnitude(avatarOrigin.transform.position - vertices[i]) > filterThreshold)
                //    {
                //        colors[i].r = 255;
                //        colors[i].g = 0;
                //        colors[i].b = 0;
                //    }
                //}

                // mesh에 전달
                if (ViewPointCloud)
                {
                    mesh.vertices = vertices;
                    mesh.colors32 = colors;
                    mesh.RecalculateBounds();
                }

                

                //Color32[] pixels = colors;
                //for (int i = 0; i < pixels.Length; i++)
                //{
                //    var d = pixels[i].b;
                //    var k = pixels[i].r;
                //    pixels[i].r = d;
                //    pixels[i].b = k;
                //}

                //kinectColorTexture.SetPixels32(pixels);
                //kinectColorTexture.Apply();

                //rawColorImg.texture = kinectColorTexture;

                //Capture capture = m_tracker.GetComponent<TrackerHandler>().getCaptureData(m_lastFrameData);
                //Debug.Log("Capture");
                //Image colorImage = transformation.ColorImageToDepthCamera(capture);
                //BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();
                //Debug.Log(colorArray.ToString());
            }


        }
    }

    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }


    public void DoAction()
    {
        watch.Restart();

        RayCastPTs();
        watch.Stop();
        debugRaycast.GetComponent<SetDebug>().SetDebugFloat(watch.ElapsedMilliseconds);

        Debug.Log($"brushCounter : {brushCounter}");
        // Invoke("SaveTexture", 0.1f);


    }

    void RayCastPTs()
    {
        brushCounter = 0;
        System.DateTime date = System.DateTime.Now;
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

        if (updateNumber > 0)
        {
            tex = (Texture2D)baseMaterial.mainTexture;
        }
            for (int index = 0; index < num; index = index + 1)
        {
            Vector3 vertex = vertices[index];

            Color32 color = colors[index];
            if (saving)
                return;
            if (vertex.z < 2.0f)
            {
                Vector3 uvWorldPosition = Vector3.zero;
                if (HitTestUVPosition(ref uvWorldPosition, vertex))
                {
                    int x_coord = (int) (uvWorldPosition.x * textureWidth);
                    int y_coord = (int) (uvWorldPosition.y * textureHeight);
                    // Debug.Log($"{x_coord}, {y_coord}");
                    
                    tex.SetPixel(x_coord, y_coord, color);
                    //if (index % 100 == 0)
                    //{
                    //    Debug.Log($"{x_coord}, {y_coord}, {color}");
                    //}


                    brushCounter++;
                }
            }
        }
        tex.Apply();
        // RenderTexture.active = null;
        baseMaterial.mainTexture = tex; //Put the painted texture as the base

        rawColorImg.canvasRenderer.SetTexture(tex);
        updateNumber += 1;
    }

    public Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        dest.Apply(false);
        Graphics.CopyTexture(rTex, dest);
        return dest;
    }

    bool HitTestUVPosition(ref Vector3 uvWorldPosition, Vector3 rayTargetTransform)
    {
        //RaycastHit hit;
        //Vector3 cursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        //Ray cursorRay = sceneCamera.ScreenPointToRay(cursorPos);
        //if (Physics.Raycast(cursorRay, out hit, 200))
        //{
        //    MeshCollider meshCollider = hit.collider as MeshCollider;
        //    if (meshCollider == null || meshCollider.sharedMesh == null)
        //        return false;
        //    Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
        //    uvWorldPosition.x = pixelUV.x - canvasCam.orthographicSize;//To center the UV on X
        //    uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y
        //    uvWorldPosition.z = 0.0f;
        //    return true;
        //}
        //else
        //{
        //    return false;
        //}

        RaycastHit hit;
        rayDirection = rayTargetTransform - CameraOrigin.position;
        if (Physics.Raycast(CameraOrigin.position, rayDirection, out hit, 100))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null || meshCollider.sharedMesh == null)
                return false;
            if (Vector3.Magnitude(hit.point - rayTargetTransform) > distanceThreshold)
            {
                return false;
            }
            Vector2 pixelUV = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
            //Debug.Log($"{canvasCam.orthographicSize.ToString()}");
            //uvWorldPosition.x = pixelUV.x - canvasCam.orthographicSize;//To center the UV on X
            //uvWorldPosition.y = pixelUV.y - canvasCam.orthographicSize;//To center the UV on Y
            uvWorldPosition.x = pixelUV.x;
            uvWorldPosition.y = pixelUV.y;
            uvWorldPosition.z = 0.0f;
            return true;
        }
        else
        {
            return false;
        }

    }

    void SaveTexture()
    {
        Debug.Log("SaveTexture");
        watch.Restart();

        brushCounter = 0;
        System.DateTime date = System.DateTime.Now;
        RenderTexture.active = canvasTexture;
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        baseMaterial.mainTexture = tex; //Put the painted texture as the base
        foreach (Transform child in brushContainer.transform)
        {//Clear brushes
            Destroy(child.gameObject);
        }
        //StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
        // Invoke("ShowCursor", 0.1f);
        watch.Stop();
        debugSaveTex.GetComponent<SetDebug>().SetDebugFloat(watch.ElapsedMilliseconds);

    }

    //private void TextureStream()
    //{
    //    while (true)
    //    {
    //        Debug.Log("TextureStream");
    //        DoAction();
    //        Thread.Sleep(1000);
    //    }
    //}

    public void TextureStream()
    {
        Thread.Sleep(5000);

        while (true)
        {
            Debug.LogFormat("Thread#{0}: 시작", Thread.CurrentThread.ManagedThreadId);

            // Debug.Log(vertices.Length.ToString())
            // DoAction();

            Debug.LogFormat("Thread#{0}: 종료", Thread.CurrentThread.ManagedThreadId);
        }

    }



}
