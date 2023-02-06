using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

using System.Collections;
using System.Collections.Generic;

using System.IO;

using System.Linq;






public class main : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
    Transformation transformation;
    public GameObject InitManager;
    public GameObject SMPLXObject;

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
    float angleTreshold;
    [SerializeField]
    float completeThreshold;
    [SerializeField]
    Color brushColor = Color.red;
    public Transform rayTarget;
    public GameObject brushContainer;
    public Sprite cursorPaint, cursorDecal; // Cursor for the differen functions 
    public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    public Material baseMaterial;
    public Material normalMaterial;
    [SerializeField]
    int brushWindowSize; //The size of our brush
    public float UpdatePeriod;
    public bool useNormalmap = true;
    public bool useDebug;
    float poseConfidence = 0;
    public float poseConfidenceThreshold;

    [Header("Log UI")]
    [SerializeField]
    public GameObject confidenceDebug;
    [SerializeField]
    public GameObject debugSaveTex;
    [SerializeField]
    UnityEngine.UI.RawImage TextureDebug;
    [SerializeField]
    UnityEngine.UI.RawImage NormalDebug;
    [SerializeField]
    UnityEngine.UI.RawImage ImageInput;
    [SerializeField]
    RectTransform progressBar;
    [SerializeField]
    RectTransform progressBarContainer;

    public bool LogSaveScreenshots;

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();



    bool saving = false; //Flag to check if we are saving the texture
    int brushCounter = 0, MAX_BRUSH_COUNT = 368640; //To avoid having millions of brushes

    // Using Queue

    int nowFilled = 0;

    static int textureWidth = 1024;
    static int textureHeight = 1024;

    public bool[,] istexturefilled = new bool[1024, 1024];

    static int kinectWidth = 640;
    static int kinectHeight = 576;

    public bool rayCasting = false;
    List<int> log_isfilled = new List<int>() { 0 };

    //pt 수
    int num = kinectWidth * kinectHeight;

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


        kinectColorTexture = new Texture2D(kinectWidth, kinectHeight);

        Coroutine projectCoroutine = StartCoroutine(RayCastPTsLoop());


        // mesh를 MeshFilter에 적용
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

        watch.Start();

        float[] betas = InitManager.GetComponent<JsonScript>().readShapeParms();
        

        setShapeParms(betas);





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

                for (int i = 0; i < m_lastFrameData.Bodies[0].JointPrecisions.Length; i++)
                {
                    poseConfidence += (int)m_lastFrameData.Bodies[0].JointPrecisions[i];
                }
                poseConfidence /= m_lastFrameData.Bodies[0].JointPrecisions.Length;

                confidenceDebug.GetComponent<SetDebug>().SetDebugConfidence(poseConfidence);


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

    void setShapeParms(float[] betas)
    {
        SMPLXObject.GetComponent<SMPLX>().betas = betas;
        SMPLXObject.GetComponent<SMPLX>().SetBetaShapes();

        //for (int i = 0; i < 10; i++)
        //{
        //    Debug.Log(betas[i]);
        //}
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
        watch.Stop();
        // debugRaycast.GetComponent<SetDebug>().SetDebugFloat(watch.ElapsedMilliseconds);

        // Debug.Log($"brushCounter : {brushCounter}");
        // Invoke("SaveTexture", 0.1f);


    }

    IEnumerator RayCastPTsLoop()
    {
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        Texture2D normalMap = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        // tex.LoadImage(InitManager.GetComponent<TextureLoader>().initTexture.GetRawTextureData());

        while (true)
        {
            brushCounter = 0;

            //if (updateNumber > 0)
            //{
            //    tex = (Texture2D)baseMaterial.mainTexture;
            //}



            for (int index = 0; index < num; index = index + 1)
            {
                Vector3 vertex = vertices[index];

                if (saving)
                    yield return 0;
                if (vertex.z < 2.0f && poseConfidence > poseConfidenceThreshold)
                {
                    Color32 color = colors[index];
                    float hitAngle = 0;
                    Vector3 hitNormal = Vector3.zero;

                    Vector3 uvWorldPosition = Vector3.zero;
                    if (HitTestUVPosition(ref uvWorldPosition, ref vertex, ref hitAngle, ref hitNormal))
                    {
                        int x_coord = (int)(uvWorldPosition.x * textureWidth);
                        int y_coord = (int)(uvWorldPosition.y * textureHeight);

                        // Debug.Log($"{x_coord}, {y_coord}");

                        if (true)
                        {
                            //SetPixel with Circle + Filter
                            if (istexturefilled[x_coord, y_coord] == false)
                            {
                                istexturefilled[x_coord, y_coord] = true;
                                nowFilled++;
                            }
                            for (int i = -Mathf.RoundToInt(brushWindowSize / 2); i < Mathf.RoundToInt(brushWindowSize / 2); i++)
                            {
                                for (int j = -Mathf.RoundToInt(brushWindowSize / 2); j < Mathf.RoundToInt(brushWindowSize / 2); j++)
                                {
                                    Color32 color_origin = tex.GetPixel(x_coord + i, y_coord + j);
                                    Vector2 pointRelativePosition = new Vector2(i, j);
                                    float distanceFromCenter = pointRelativePosition.magnitude;
                                    // Color32 brushColor = Color.Lerp(color, color_origin, distanceFromCenter) ;
                                    if (i == 0 && j == 0)
                                    {
                                        tex.SetPixel(x_coord + i, y_coord + j, color);
                                    }
                                    else if (istexturefilled[x_coord + i, y_coord + j] == true)
                                    {
                                        Color32 brushColor = Color32.Lerp(color, color_origin, distanceFromCenter / brushWindowSize);
                                        tex.SetPixel(x_coord + i, y_coord + j, brushColor);
                                    }
                                    else
                                    {
                                        tex.SetPixel(x_coord + i, y_coord + j, color);
                                    }
                                    

                                    //if (x_coord + i >= 0 && y_coord + j >= 0 && x_coord + i <= 1024 || y_coord + j <= 1024)
                                    //    istexturefilled[x_coord + i, y_coord + j] = true;

                                    //if (index % 1000 == 0)
                                    //{
                                    //    // Debug.Log($"{brushColor}, {color}, {color_origin}, {distanceFromCenter / brushWindowSize}, {distanceFromCenter}, {i}{j}");
                                    //    Debug.Log(hitAngle);
                                    //}

                                }
                            }

                            //SetPixel with cross
                            //tex.SetPixel(x_coord, y_coord, color);
                            //tex.SetPixel(x_coord - 1, y_coord, color);
                            //tex.SetPixel(x_coord + 1, y_coord, color);
                            //tex.SetPixel(x_coord, y_coord - 1, color);
                            //tex.SetPixel(x_coord, y_coord + 1, color);


                            // SetPixel with square
                            //Color[] colors = Enumerable.Repeat<Color>(color, brushWindowSize * brushWindowSize).ToArray<Color>();
                            //tex.SetPixels(x_coord, y_coord, brushWindowSize, brushWindowSize, colors);

                            //if (istexturefilled[x_coord, y_coord] == false)
                            //{
                            //    istexturefilled[x_coord, y_coord] = true;
                            //    nowFilled++;
                            //}
                           



                            brushCounter++;
                        }

                        if (useNormalmap)
                        {
                            if (!(index % kinectWidth == 0 || index % kinectWidth == kinectWidth - 1 || index < kinectWidth || index > num - kinectWidth))
                            {
                                Vector3 normal = calculateNormal(vertex, vertices[index + 1], vertices[index - kinectWidth], vertices[index - 1], vertices[index + kinectWidth]);
                                //if (index % 1000 == 0)
                                //{
                                //    Debug.Log(normal.normalized.ToString());
                                //    Debug.Log(hitNormal.magnitude); 
                                //}
                                Color normalColor = normalToColor((normal.normalized - hitNormal).normalized);
                                Color[] colors = Enumerable.Repeat<Color>(normalColor, brushWindowSize * brushWindowSize).ToArray<Color>();
                                normalMap.SetPixels(x_coord, y_coord, brushWindowSize, brushWindowSize, colors);
                            }
                        }
                    }
                }
            }
            tex.Apply();
            normalMap.Apply();
            // RenderTexture.active = null;
            baseMaterial.mainTexture = tex; //Put the painted texture as the base

            if (useDebug)
            {
                TextureDebug.canvasRenderer.SetTexture(tex);
                NormalDebug.canvasRenderer.SetTexture(normalMap);
            }

            updateNumber += 1;
            rayCasting = false;

            if (nowFilled > textureWidth * textureHeight * completeThreshold)
            {
                normalMaterial.mainTexture = normalMap;
                SaveTextureToFile();
                Debug.Log(nowFilled.ToString());
                yield break;
            }
            else
            {
                Debug.Log(nowFilled.ToString() + " / " + (textureWidth * textureHeight * completeThreshold).ToString());
            }

            // Log how filled?

            //for (int i = 0; i < 1024; i++)
            //{
            //    for (int j = 0; j < 1024; j++)
            //    {
            //        if (istexturefilled[i, j] == true)
            //        {
            //            nowFilled++;
            //        }
            //    }
            //}
            // Debug.Log(nowFilled);
            // log_isfilled.Add(nowFilled);

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

    bool HitTestUVPosition(ref Vector3 uvWorldPosition, ref Vector3 rayTargetTransform, ref float hitAngle, ref Vector3 hitNormal)
    {

        RaycastHit hit;
        Vector3 rayDirection = rayTargetTransform - CameraOrigin.position;
        if (Physics.Raycast(CameraOrigin.position, rayDirection, out hit, 100))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            hitNormal = hit.normal;
            hitAngle = GetAngle(hitNormal, rayDirection);
            

            if (meshCollider == null || meshCollider.sharedMesh == null)
                return false;
            if (Vector3.Magnitude(hit.point - rayTargetTransform) > distanceThreshold || hitAngle > angleTreshold)
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

    public static float GetAngle(Vector3 N, Vector3 A)
    {   
        return Mathf.Atan(-Vector2.Dot(N,A)/(N.magnitude * A.magnitude)) * Mathf.Rad2Deg;
    }

    static Vector3 calculateNormal(Vector3 point, Vector3 point_right, Vector3 point_up, Vector3 point_left, Vector3 point_down)
    {
        /*
                      point_up
           . (normal2)   ｜    . (normal1)  
                         ｜
        point_left――――――point――――――point_right
                         ｜        
           . (normal3)   ｜    . (normal4)  
                     point_down

        return normal at point, by averaging normal of 4 triangle composed with (point, right, up), (point, up, left), (point, left, down), (point, down, right)
         */
        Vector3 normal1 = Vector3.Cross(point_right - point, point_up - point);
        Vector3 normal2 = Vector3.Cross(point_up - point, point_left - point);
        Vector3 normal3 = Vector3.Cross(point_left - point, point_down - point);
        Vector3 normal4 = Vector3.Cross(point_down - point, point_right - point);

        Vector3 normal = (normal1 + normal2 + normal3 + normal4) / 4;

        return normal;
    }

    static Color normalToColor(Vector3 normal)
    {
        /*
        ref : https://docs.unity3d.com/Manual/StandardShaderMaterialParameterNormalMap.html
        
         */

        Color normalMapColor = new Color(normal.x / 2 + 0.5f, normal.y / 2 + 0.5f, normal.z / 2 + 0.5f, 1f);
        return normalMapColor;
    }

    Texture2D postProcessing(Texture2D inputTexture)
    {
        Texture2D resultTexture = inputTexture;
        for (int i = 0; i < textureWidth; i++)
        {
            for (int j = 0; j < textureHeight; j++)
            {
                if (!(istexturefilled[i, j])) // && (i + j)%10 == 0)
                {
                    int squareSize = 1;

                    while (squareSize < textureWidth)
                    {
                        /*
                    
                    2――――――1
                    ｜     ｜
                    ｜ i,j ｜
                    ｜     ｜ 
                    3――――――4
                    squareSize = (1 to 2) / 2
                     */

                        int w1_pointer = Mathf.Min(textureWidth - 1, i + squareSize);
                        int w2_pointer = Mathf.Max(0, i - squareSize);
                        int h1_pointer = Mathf.Min(textureHeight - 1, j + squareSize);
                        int h2_pointer = Mathf.Max(0, j - squareSize);

                        List<Color> colorList = new List<Color>();

                        for (int w_pointer = w2_pointer; w_pointer < w1_pointer; w_pointer++)
                        {
                            // 3->4
                            if (istexturefilled[w_pointer, h1_pointer])
                            {
                                colorList.Add(inputTexture.GetPixel(w_pointer, h1_pointer));
                            }
                            if (istexturefilled[w_pointer, h2_pointer])
                            {
                                colorList.Add(inputTexture.GetPixel(w_pointer, h2_pointer));
                            }
                        }
                        for (int h_pointer = h2_pointer; h_pointer < h1_pointer; h_pointer++)
                        {
                            if (istexturefilled[w1_pointer, h_pointer])
                            {
                                colorList.Add(inputTexture.GetPixel(w1_pointer, h_pointer));
                            }
                            if (istexturefilled[w2_pointer, h_pointer])
                            {
                                colorList.Add(inputTexture.GetPixel(w2_pointer, h_pointer));
                            }
                        }
                        if (colorList.Count > 0)
                        {
                            Vector4 colorVector = Vector4.zero;
                            foreach (Color colorCandidate in colorList)
                            {
                                colorVector += (Vector4)colorCandidate;
                            }
                            Color colorValue = (Color)(colorVector / colorList.Count);
                            resultTexture.SetPixel(i, j, colorValue);
                            break;
                        }
                        else
                        {
                            squareSize++;
                        }
                    }
                }
            }
        }

        return resultTexture;
    }


    public void SaveTextureToFile()
    {
        Texture2D texture = postProcessing((Texture2D)baseMaterial.mainTexture);
        byte[] bytes = texture.EncodeToPNG();
        string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        var dirPath = Application.dataPath + "/SaveTextures/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "Image_" + now + "_texture.png", bytes);
        Debug.Log($"Saved to {dirPath + "Image_" + now + "_texture.png"}");

        if (useNormalmap)
        {
            Texture2D normalMap = postProcessing((Texture2D)normalMaterial.mainTexture);
            byte[] bytes_normal = normalMap.EncodeToPNG();

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + "Image_" + now + "_normal.png", bytes_normal);
            Debug.Log($"Saved to {dirPath + "Image_" + now + "_normal.png"}");
        }
    }

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


    public void ScreenshotToggle(bool value)
    {
        LogSaveScreenshots = !LogSaveScreenshots;
        // Debug.Log(LogSaveScreenshots);
    }

}