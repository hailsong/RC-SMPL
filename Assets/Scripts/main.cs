using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

using System.Collections;
using System.Collections.Generic;

using System.IO;

using System.Threading.Tasks;
using System.Threading;

using System.Linq;





public class main : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    Transformation transformation;

    Texture2D kinectColorTexture;



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
    float distanceThreshold;
    [SerializeField]
    Color brushColor = Color.red;
    public Transform rayTarget;
    public GameObject brushContainer;
    public Sprite cursorPaint, cursorDecal; // Cursor for the differen functions 
    public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    public Material baseMaterial;
    [SerializeField]
    int brushWindowSize; //The size of our brush
    public bool UseThread = true;
    public float UpdatePeriod;




    [Header("Log UI")]
    [SerializeField]
    public GameObject debugRaycast;
    [SerializeField]
    public GameObject debugSaveTex;
    [SerializeField]
    UnityEngine.UI.RawImage TextureDebug;
    [SerializeField]
    UnityEngine.UI.RawImage ImageInput;

    public bool LogSaveScreenshots;

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();


    Vector3 rayDirection;
    bool saving = false; //Flag to check if we are saving the texture
    int brushCounter = 0, MAX_BRUSH_COUNT = 368640; //To avoid having millions of brushes

    // Using Queue
    private Thread thread;
    private Queue<Texture2D> textureQueue = new Queue<Texture2D>();

    public int textureWidth = 1024;
    public int textureHeight = 1024;

    public bool[,] istexturefilled = new bool[1024, 1024];

    public bool rayCasting = false;
    List<int> log_isfilled = new List<int>() { 0 };

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


        kinectColorTexture = new Texture2D(640, 576);

        StartCoroutine(RayCastPTsLoop());


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

                // mesh에 전달
                //if (ViewPointCloud)
                //{
                //    mesh.vertices = vertices;
                //    mesh.colors32 = colors;
                //    mesh.RecalculateBounds();
                //}
                //if (!rayCasting)
                //{
                //    DoAction();
                //}
                // DoAction();
                //Color32[] pixels = colors;
                //for (int i = 0; i < pixels.Length; i++)
                //{
                //    var d = pixels[i].b;
                //    var k = pixels[i].r;
                //    pixels[i].r = d;
                //    pixels[i].b = k;
                //}
                //Debug.Log($"pixels : {pixels.Length}");
                kinectColorTexture.SetPixels32(colors);
                kinectColorTexture.Apply();

                ImageInput.canvasRenderer.SetTexture(kinectColorTexture);

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
        rayCasting = true;
        watch.Restart();

        // StartCoroutine(RayCastPTs());
        // RayCastPTs_thread();
        watch.Stop();
        debugRaycast.GetComponent<SetDebug>().SetDebugFloat(watch.ElapsedMilliseconds);

        // Debug.Log($"brushCounter : {brushCounter}");
        // Invoke("SaveTexture", 0.1f);


    }

    //IEnumerator RayCastPTs()
    //{
    //    brushCounter = 0;
    //    Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

    //    if (updateNumber > 0)
    //    {
    //        tex = (Texture2D)baseMaterial.mainTexture;
    //    }
    //    for (int index = 0; index < num; index = index + 1)
    //    {
    //        Vector3 vertex = vertices[index];

    //        Color32 color = colors[index];
    //        if (saving)
    //            yield return 0;
    //        if (vertex.z < 2.0f)
    //        {
    //            Vector3 uvWorldPosition = Vector3.zero;
    //            if (HitTestUVPosition(ref uvWorldPosition, vertex))
    //            {
    //                int x_coord = (int)(uvWorldPosition.x * textureWidth);
    //                int y_coord = (int)(uvWorldPosition.y * textureHeight);
    //                // Debug.Log($"{x_coord}, {y_coord}");

    //                tex.SetPixel(x_coord, y_coord, color);
    //                tex.SetPixel(x_coord - 1, y_coord, color);
    //                tex.SetPixel(x_coord + 1, y_coord, color);
    //                tex.SetPixel(x_coord, y_coord - 1, color);
    //                tex.SetPixel(x_coord, y_coord + 1, color);

    //                //if (index % 100 == 0)
    //                //{
    //                //    Debug.Log($"{x_coord}, {y_coord}, {color}");
    //                //}


    //                brushCounter++;
    //            }
    //        }
    //    }
    //    tex.Apply();
    //    // RenderTexture.active = null;
    //    baseMaterial.mainTexture = tex; //Put the painted texture as the base

    //    TextureDebug.canvasRenderer.SetTexture(tex);
    //    updateNumber += 1;
    //    rayCasting = false;

    //    ScreenCapture.CaptureScreenshot("Screenshot.png");///Screenshot_{updateNumber}.png");
    //}

    IEnumerator RayCastPTsLoop()
    {
        while (true)
        {
            brushCounter = 0;
            Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

            if (updateNumber > 0)
            {
                tex = (Texture2D)baseMaterial.mainTexture;
            }
            for (int index = 0; index < num; index = index + 2)
            {
                Vector3 vertex = vertices[index];

                Color32 color = colors[index];
                if (saving)
                    yield return 0;
                if (vertex.z < 2.0f)
                {
                    Vector3 uvWorldPosition = Vector3.zero;
                    if (HitTestUVPosition(ref uvWorldPosition, vertex))
                    {
                        int x_coord = (int)(uvWorldPosition.x * textureWidth);
                        int y_coord = (int)(uvWorldPosition.y * textureHeight);
                        // Debug.Log($"{x_coord}, {y_coord}");

                        //for (int i = -Mathf.RoundToInt(brushWindowSize / 2); i < Mathf.RoundToInt(brushWindowSize / 2); i++)
                        //{
                        //    for (int j = -Mathf.RoundToInt(brushWindowSize / 2); j < Mathf.RoundToInt(brushWindowSize / 2); j++)
                        //    {
                        //        Color32 color_origin = tex.GetPixel(x_coord + i, y_coord + j);
                        //        Vector2 pointRelativePosition = new Vector2(i, j);
                        //        float distanceFromCenter = pointRelativePosition.magnitude;
                        //        // Color32 brushColor = Color.Lerp(color, color_origin, distanceFromCenter) ;
                        //        Color32 brushColor = Color32.Lerp(color, color_origin, distanceFromCenter / brushWindowSize);
                        //        tex.SetPixel(x_coord + i, y_coord + j, brushColor);

                        //        if (x_coord + i >= 0 && y_coord + j >= 0 && x_coord + i <= 1024 || y_coord + j <= 1024)
                        //            istexturefilled[x_coord + i, y_coord + j] = true;

                        //        if (index % 1000 == 0)
                        //        {
                        //            Debug.Log($"{brushColor}, {color}, {color_origin}, {distanceFromCenter / brushWindowSize}, {distanceFromCenter}, {i}{j}");
                        //        }
                        //    }
                        //}
                        //tex.SetPixel(x_coord, y_coord, color);
                        //tex.SetPixel(x_coord - 1, y_coord, color);
                        //tex.SetPixel(x_coord + 1, y_coord, color);
                        //tex.SetPixel(x_coord, y_coord - 1, color);
                        //tex.SetPixel(x_coord, y_coord + 1, color);

                        Color[] colors = Enumerable.Repeat<Color>(color, brushWindowSize * brushWindowSize).ToArray<Color>();

                        tex.SetPixels(x_coord, y_coord, brushWindowSize, brushWindowSize, colors);

                        brushCounter++;
                    }
                }
            }
            tex.Apply();
            // RenderTexture.active = null;
            baseMaterial.mainTexture = tex; //Put the painted texture as the base

            TextureDebug.canvasRenderer.SetTexture(tex);
            updateNumber += 1;
            rayCasting = false;


            int nowFilled = 0;
            for (int i = 0; i < 1024; i++)
            {
                for (int j = 0; j < 1024; j++)
                {
                    if (istexturefilled[i, j] == true)
                    {
                        nowFilled++;
                    }
                }
            }
            Debug.Log(nowFilled);
            log_isfilled.Add(nowFilled);

            //if (updateNumber % 3 == 0 && LogSaveScreenshots)
            //{
            //    SaveScreenshot();
            //    SaveTextureToFile();



            //}


            yield return new WaitForSeconds(UpdatePeriod);
        }

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


    public void SaveTextureToFile()
    {
        Texture2D texture = (Texture2D)baseMaterial.mainTexture;
        byte[] bytes = texture.EncodeToPNG();
        string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var dirPath = Application.dataPath + "/SaveTextures/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image_" + now + ".png", bytes);
        Debug.Log($"Saved to {dirPath + "Image_" + now + ".png"}");
    }

    //LOAD TEXTURE

    public void LoadTextureFromFile()
    {
        Texture2D texture = (Texture2D)baseMaterial.mainTexture;
        string filename = "TextureMapSave.png";

        System.IO.FileStream fileLoad;
        byte[] bytes;
        bytes = System.IO.File.ReadAllBytes(Application.dataPath + "/Textures/" + filename);
        Texture2D loadTexture = new Texture2D(1, 1);
        loadTexture.LoadImage(bytes);
        baseMaterial.mainTexture = loadTexture;

    }

    public void saveLog()
    {
        string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        using (System.IO.StreamWriter file = new System.IO.StreamWriter("./Assets/SaveLog/test_" + now + ".csv"))

        {
            file.WriteLine("filled");

            foreach (int i in log_isfilled)
            {
                file.WriteLine(i.ToString());
            }

        }
    }

    public void SaveScreenshot()
    {
        Texture2D screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        Rect area = new Rect(0f, 0f, Screen.width, Screen.height);
        screenTex.ReadPixels(area, 0, 0);
        string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var dirPath = Application.dataPath + "/../Screenshots/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Screenshot_" + now + ".png", screenTex.EncodeToPNG());
        Debug.Log($"Saved to {dirPath + "Screenshot_" + now + ".png"}");

    }
    public void TextureStream()
    {
        Thread.Sleep(5000);

        while (true)
        {
            Debug.LogFormat("Thread#{0}: 시작", Thread.CurrentThread.ManagedThreadId);

            // Debug.Log(vertices.Length.ToString())
            DoAction();
            Thread.Sleep(5000);

            Debug.LogFormat("Thread#{0}: 종료", Thread.CurrentThread.ManagedThreadId);
        }

    }

    public void ScreenshotToggle(bool value)
    {
        LogSaveScreenshots = !LogSaveScreenshots;
        // Debug.Log(LogSaveScreenshots);
    }




}