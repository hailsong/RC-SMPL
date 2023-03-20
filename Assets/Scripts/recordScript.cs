using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


using System.Linq;


[System.Serializable]
class SaveData
{

    [Serializable]
    public class SerializableVector3Array
    {
        public float[] x;
        public float[] y;
        public float[] z;

        public SerializableVector3Array(Vector3[] vecArray)
        {
            x = vecArray.Select(vec => vec.x).ToArray();
            y = vecArray.Select(vec => vec.y).ToArray();
            z = vecArray.Select(vec => vec.z).ToArray();
        }

        public Vector3[] ToVector3Array()
        {
            int length = x.Length;
            Vector3[] vecArray = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                vecArray[i] = new Vector3(x[i], y[i], z[i]);
            }
            return vecArray;
        }
    }


    [Serializable]
    public class SerializableColor32
    {
        public byte[] r;
        public byte[] g;
        public byte[] b;
        public byte[] a;

        public SerializableColor32(Color32[] colors)
        {
            r = colors.Select(c => c.r).ToArray();
            g = colors.Select(c => c.g).ToArray();
            b = colors.Select(c => c.b).ToArray();
            a = colors.Select(c => c.a).ToArray();
        }

        public Color32[] ToColor32Array()
        {
            int length = r.Length;
            Color32[] colors = new Color32[length];
            for (int i = 0; i < length; i++)
            {
                colors[i] = new Color32(r[i], g[i], b[i], a[i]);
            }
            return colors;
        }
    }


    [Serializable]
    public class SerializableQuaternion
    {
        public float[] x;
        public float[] y;
        public float[] z;
        public float[] w;

        public SerializableQuaternion(Quaternion quat)
        {
            x = new float[] { quat.x };
            y = new float[] { quat.y };
            z = new float[] { quat.z };
            w = new float[] { quat.w };
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x[0], y[0], z[0], w[0]);
        }
    }

    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(UnityEngine.Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }

        public UnityEngine.Vector3 ToUnityVector3()
        {
            return new UnityEngine.Vector3(x, y, z);
        }

        public static implicit operator Vector3(SerializableVector3 serializableVector3)
        {
            return new Vector3(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator SerializableVector3(Vector3 vector3)
        {
            return new SerializableVector3(vector3.x, vector3.y, vector3.z);
        }
    }


    [Serializable]
    public class SerializableBody
    {
        public SerializableVector3[] JointPositions3D;
        public SerializableQuaternion[] JointRotations;

        public SerializableBody(Body body)
        {
            JointPositions3D = new SerializableVector3[(int)JointId.Count];
            JointRotations = new SerializableQuaternion[(int)JointId.Count];

            for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
            {
                JointPositions3D[jointNum] = new SerializableVector3(new Vector3(body.JointPositions3D[jointNum].X, body.JointPositions3D[jointNum].Y, body.JointPositions3D[jointNum].Z));
                JointRotations[jointNum] = new SerializableQuaternion(new Quaternion(body.JointRotations[jointNum].X, body.JointRotations[jointNum].Y, body.JointRotations[jointNum].Z, body.JointRotations[jointNum].W));
            }
        }

        //public Body ToBody()
        //{
        //    Body body = new Body();

        //    for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
        //    {
        //        //body.JointPositions3D[jointNum] = JointPositions3D[jointNum].ToVector3();
        //        body.JointPositions3D[jointNum] = new System.Numerics.Vector3(JointPositions3D[jointNum].ToUnityVector3().x, JointPositions3D[jointNum].ToUnityVector3().y, JointPositions3D[jointNum].ToUnityVector3().z);
        //        //body.JointRotations[jointNum] = JointRotations[jointNum].ToQuaternion();
        //        body.JointRotations[jointNum] = new System.Numerics.Quaternion(JointRotations[jointNum].ToQuaternion().x, JointRotations[jointNum].ToQuaternion().y, JointRotations[jointNum].ToQuaternion().z, JointRotations[jointNum].ToQuaternion().w);
        //    }

        //    return body;
        //}
        public Body ToBody()
        {
            Body body = new Body();

            body.JointPositions3D = new System.Numerics.Vector3[(int)JointId.Count];
            body.JointRotations = new System.Numerics.Quaternion[(int)JointId.Count];

            for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
            {
                body.JointPositions3D[jointNum] = new System.Numerics.Vector3(JointPositions3D[jointNum].ToUnityVector3().x, JointPositions3D[jointNum].ToUnityVector3().y, JointPositions3D[jointNum].ToUnityVector3().z);
                body.JointRotations[jointNum] = new System.Numerics.Quaternion(JointRotations[jointNum].ToQuaternion().x, JointRotations[jointNum].ToQuaternion().y, JointRotations[jointNum].ToQuaternion().z, JointRotations[jointNum].ToQuaternion().w);
            }

            return body;
        }
    }

    //[Serializable]
    //public class SerializableBody
    //{
    //    public SerializableVector3[] JointPositions3D;
    //    public SerializableQuaternion[] JointRotations;

    //    public SerializableBody(Body body)
    //    {
    //        JointPositions3D = body.JointPositions3D.Select(v => new SerializableVector3(ConvertToUnityVector3(v))).ToArray();
    //        JointRotations = body.JointRotations.Select(q => new SerializableQuaternion(ConvertToUnityQuaternion(q))).ToArray();
    //    }

    //    public Body ToBody()
    //    {
    //        Body body = new Body();
    //        for (int i = 0; i < JointPositions3D.Length; i++)
    //        {
    //            body.JointPositions3D[i] = ConvertToNumericsVector3(JointPositions3D[i].ToVector3Array());
    //            body.JointRotations[i] = ConvertToNumericsQuaternion(JointRotations[i].ToQuaternion());
    //        }
    //        return body;
    //    }

    //    private static UnityEngine.Vector3 ConvertToUnityVector3(System.Numerics.Vector3 v)
    //    {
    //        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    //    }

    //    private static System.Numerics.Vector3 ConvertToNumericsVector3(UnityEngine.Vector3 v)
    //    {
    //        return new System.Numerics.Vector3(v.x, v.y, v.z);
    //    }

    //    private static UnityEngine.Quaternion ConvertToUnityQuaternion(System.Numerics.Quaternion q)
    //    {
    //        return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
    //    }

    //    private static System.Numerics.Quaternion ConvertToNumericsQuaternion(UnityEngine.Quaternion q)
    //    {
    //        return new System.Numerics.Quaternion(q.x, q.y, q.z, q.w);
    //    }
    //}

    //[Serializable]
    //public class SerializableQuaternion
    //{
    //    public float x;
    //    public float y;
    //    public float z;
    //    public float w;

    //    public SerializableQuaternion(Quaternion quaternion)
    //    {
    //        x = quaternion.x;
    //        y = quaternion.y;
    //        z = quaternion.z;
    //        w = quaternion.w;
    //    }

    //    public Quaternion ToQuaternion()
    //    {
    //        return new Quaternion(x, y, z, w);
    //    }
    //}


    public List<SerializableVector3Array> verticesSave;
    public List<SerializableColor32> colorSave;
    public List<int[]> indicesSave;
    public List<SerializableBody> bodySave;
}


public class recordScript : MonoBehaviour
{

    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    public GameObject m_loader;
    public GameObject SMPL_Avatar;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public SkinnedCollisionHelper CollisionHelper;
    public BackgroundData m_lastFrameData = new BackgroundData();
    Transformation transformation;

    Texture2D kinectColorTexture;
    private float time = -5f;
    public float savePeriod;
    public string loadFilename;

    [Header("Pointcloud Filter")]
    [SerializeField]
    Transform avatarOrigin;
    [Range(0.0f, 3.0f)]
    public float filterThreshold;
    public bool ViewPointCloud;


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
    [SerializeField]
    public int startFrameOffset;
    public int endFrameOffset;
    public int startIndexOffset;
    public int endIndexOffset;

    public string maskDirectory;


    [SerializeField]
    UnityEngine.UI.RawImage ImageInput;

    int nowFilled = 0;

    static int textureWidth = 1024;
    static int textureHeight = 1024;


    public bool LogSaveScreenshots;

    public Camera maskCamera;
    public Camera virtualCamera;
    

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public bool[,] istexturefilled = new bool[1024, 1024];

    static int kinectWidth = 640;
    static int kinectHeight = 576;

    public bool rayCasting = false;

    public bool useBaseTexture;

    int brushCounter = 0, MAX_BRUSH_COUNT = 368640; //To avoid having millions of brushes
    bool saving = false; //Flag to check if we are saving the texture
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
    Body body;

    public int updateNumber = 0;
    //public Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);

    List<SaveData.SerializableVector3Array> verticesList = new List<SaveData.SerializableVector3Array>();
    List<SaveData.SerializableColor32> colorList = new List<SaveData.SerializableColor32>();
    List<int[]> indicesList = new List<int[]>();
    List<SaveData.SerializableBody> bodyList = new List<SaveData.SerializableBody>();



    bool nowRecording = false;
    bool nowLoading = false;
    SaveData mySaveData = new SaveData();
    string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");

    int frameNum = 0;


    BinaryFormatter bf;
    string fileName;
    FileStream fs;
    SaveData myLoadData;
    Texture2D tex;
    Texture2D normalMap;
    int index_load = 0;

    Texture2D mask_all;
    Texture2D mask_except;
    Texture2D base_texture;

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

        if (ViewPointCloud && !nowLoading)
        {
            mesh.vertices = vertices;
            mesh.colors32 = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            GetComponent<MeshRenderer>().enabled = true;
        }


        kinectColorTexture = new Texture2D(kinectWidth, kinectHeight);


        // mesh를 MeshFilter에 적용
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        

        watch.Start();




        bf = new BinaryFormatter();

        fileName = Application.dataPath + "/SaveStream/" + loadFilename;


        fs = new FileStream(fileName, FileMode.Open);

        myLoadData = bf.Deserialize(fs) as SaveData;

        Debug.Log(myLoadData.indicesSave);

        tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        if (useBaseTexture)
        {
            tex = base_texture;
        }
        normalMap = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);

        index_load = startFrameOffset;

        loadMasks(maskDirectory);
        Debug.Log($"Mask Dimension, {mask_all.dimension}");

        makeDir("Assets/SaveStream/");

    }

    void FixedUpdate()
    {
        

    }

    private void Update()
    {
        if (nowLoading)
        {
            int[] indexLoaded = myLoadData.indicesSave[index_load];
            Color32[] colors = myLoadData.colorSave[index_load].ToColor32Array();
            Vector3[] vertices = myLoadData.verticesSave[index_load].ToVector3Array();
            Body bodyLoaded = myLoadData.bodySave[index_load].ToBody();

            // Including renderSkeleton function
            Quaternion[] jointRotation = m_loader.GetComponent<TrackerLoader>().returnJointRotationsSkeleton(bodyLoaded);
            SMPL_Avatar.GetComponent<AvatarLoader>().AnimationUpdate(jointRotation);

            CollisionHelper.UpdateCollisionMesh();


            if (ViewPointCloud && nowLoading)
            {
                mesh.vertices = vertices;
                mesh.colors32 = colors;
                mesh.SetIndices(indexLoaded, MeshTopology.Points, 0);
                GetComponent<MeshRenderer>().enabled = true;

            }

            // Debug.Log(bodyLoaded.JointPositions3D[1].X);
            Debug.Log($"{index_load} / {myLoadData.indicesSave.Count} ");

            brushCounter = 0;

            //if (updateNumber > 0)
            //{
            //    tex = (Texture2D)baseMaterial.mainTexture;
            //}


            for (int index = startIndexOffset; index < num - endIndexOffset; index = index + 1)
            {
                Vector3 vertex = vertices[index];

                if (vertex.z < 2.0f) // && poseConfidence > poseConfidenceThreshold)
                {
                    Color32 color = colors[index];
                    float hitAngle = 0;
                    Vector3 hitNormal = Vector3.zero;

                    Vector3 uvWorldPosition = Vector3.zero;

                    if (HitTestUVPosition(ref uvWorldPosition, ref vertex, ref hitAngle, ref hitNormal))
                    {
                        int x_coord = (int)(uvWorldPosition.x * textureWidth);
                        int y_coord = (int)(uvWorldPosition.y * textureHeight);

                        if (mask_except.GetPixel(x_coord, y_coord) == Color.white)
                        {
                            continue;
                        }


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
                                color = normalColor;

                                for (int i = -Mathf.RoundToInt(brushWindowSize / 2); i < Mathf.RoundToInt(brushWindowSize / 2); i++)
                                {
                                    for (int j = -Mathf.RoundToInt(brushWindowSize / 2); j < Mathf.RoundToInt(brushWindowSize / 2); j++)
                                    {
                                        Color32 color_origin = normalMap.GetPixel(x_coord + i, y_coord + j);
                                        Vector2 pointRelativePosition = new Vector2(i, j);
                                        float distanceFromCenter = pointRelativePosition.magnitude;
                                        // Color32 brushColor = Color.Lerp(color, color_origin, distanceFromCenter) ;
                                        if (i == 0 && j == 0)
                                        {
                                            normalMap.SetPixel(x_coord + i, y_coord + j, color);
                                        }
                                        else if (istexturefilled[x_coord + i, y_coord + j] == true)
                                        {
                                            Color32 brushColor = Color32.Lerp(color, color_origin, distanceFromCenter / brushWindowSize);
                                            normalMap.SetPixel(x_coord + i, y_coord + j, brushColor);
                                        }
                                        else
                                        {
                                            normalMap.SetPixel(x_coord + i, y_coord + j, color);
                                        }


                                        //if (x_coord + i >= 0 && y_coord + j >= 0 && x_coord + i <= 1024 || y_coord + j <= 1024)
                                        //    istexturefilled[x_coord + i, y_coord + j] = true;

                                    }
                                }
                            }
                        }
                    }
                }
            }
            tex.Apply();
            normalMap.Apply();
            // RenderTexture.active = null;
            baseMaterial.mainTexture = tex; //Put the painted texture as the base

            rayCasting = false;

            //if (nowFilled > textureWidth * textureHeight * completeThreshold)
            //{
            //    normalMaterial.mainTexture = normalMap;
            //    SaveTextureToFile();
            //    Debug.Log(nowFilled.ToString());
            //    Application.Quit();
            //}
            //else
            //{
            //    Debug.Log(nowFilled.ToString() + " / " + (textureWidth * textureHeight * completeThreshold).ToString());
            //}

            Debug.Log(nowFilled.ToString() + " / " + (textureWidth * textureHeight * completeThreshold).ToString());


            Texture2D VirtualTexture2D = RTImage(virtualCamera);
            byte[] bytes_render = VirtualTexture2D.EncodeToPNG();

            string directoryName = loadFilename.Substring(0, loadFilename.Length - 4);
            var dirPath = Application.dataPath + "/SaveStream/" + directoryName + "/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            
            File.WriteAllBytes(dirPath + "Image_" + index_load + "_render.png", bytes_render);



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
            Debug.Log(nowFilled);
            log_isfilled.Add(nowFilled);

            index_load += 1;

            //if (updateNumber % 3 == 0 && LogSaveScreenshots)
            //{
            //    SaveScreenshot();
            //    SaveTextureToFile();

            //}




            if (index_load >= myLoadData.indicesSave.Count - endFrameOffset)
            {
                normalMaterial.mainTexture = normalMap;
                SaveTextureToFile();
                saveLog();
                Debug.Log(nowFilled.ToString());
                Application.Quit();
                nowLoading = false;
            }

        }
    
        if (nowRecording)
        {
            time += Time.deltaTime;

            if (ViewPointCloud)
            {
                mesh.vertices = vertices;
                mesh.colors32 = colors;
                mesh.RecalculateBounds();
            }

            if (m_skeletalTrackingProvider.IsRunning)
            {
                if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
                {
                    if (m_lastFrameData.NumOfBodies != 0)
                    {
                        if (!nowLoading)
                        {
                            m_tracker.GetComponent<TrackerHandler>().updateTracker(m_lastFrameData);
                        }

                        body = m_tracker.GetComponent<TrackerHandler>().getBodyData(m_lastFrameData);
                        vertices = m_tracker.GetComponent<TrackerHandler>().getVerticesData(m_lastFrameData);
                        colors = m_tracker.GetComponent<TrackerHandler>().getColorsData(m_lastFrameData);
                        indices = m_tracker.GetComponent<TrackerHandler>().getIndicesData(m_lastFrameData);

                        // Debug.Log($"{body.Length}, {vertices.Length}, {colors.Length}, {indices.Length}");

                        for (int i = 0; i < m_lastFrameData.Bodies[0].JointPrecisions.Length; i++)
                        {
                            poseConfidence += (int)m_lastFrameData.Bodies[0].JointPrecisions[i];
                        }
                        poseConfidence /= m_lastFrameData.Bodies[0].JointPrecisions.Length;

                        // print(poseConfidence);

                        kinectColorTexture.SetPixels32(colors);
                        kinectColorTexture.Apply();

                        ImageInput.canvasRenderer.SetTexture(kinectColorTexture);





                        if (nowRecording && savePeriod < time)
                        {
                            // verticesList.Add(vertices);
                            SaveData.SerializableVector3Array serializableVector3Arr = new SaveData.SerializableVector3Array(vertices);
                            SaveData.SerializableColor32 serializableColor = new SaveData.SerializableColor32(colors);
                            SaveData.SerializableBody serializableBody = new SaveData.SerializableBody(body);
                            bodyList.Add(serializableBody);
                            verticesList.Add(serializableVector3Arr);
                            colorList.Add(serializableColor);
                            indicesList.Add(indices);

                            Debug.Log($"{verticesList.Count}, {colorList.Count}, {indicesList.Count}, {bodyList.Count}");

                            byte[] bytes = kinectColorTexture.EncodeToPNG();

                            Texture2D MaskTexture2D = RTImage(maskCamera);
                            byte[] bytes_mask = MaskTexture2D.EncodeToPNG();

                            var dirPath = Application.dataPath + "/SaveStream/" + now + "/";
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(dirPath);
                            }
                            File.WriteAllBytes(dirPath + "InputImage_" + frameNum + ".png", bytes);
                            File.WriteAllBytes(dirPath + "InputMask_" + frameNum + ".png", bytes_mask);




                            time = 0f;
                            frameNum += 1;


                        }
                    }






                    if (verticesList.Count > 500)
                    {
                        Application.Quit();
                    }

                    // Save {vertices, colors, indices, 
                }


            }
        }
    }

    public void triggerRecord()
    {
        nowRecording = true;
    }

    public void triggerSave()
    {
        nowRecording = false;
        Debug.Log($"Saving {verticesList.Count}, {colorList.Count}, {indicesList.Count}, {bodyList.Count} Frames");

        // Save 함수 만들기


        string fileName = Application.dataPath + "/SaveStream/" + now + "_SaveStream.dat";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(fileName, FileMode.Create);

        mySaveData.verticesSave = verticesList;
        mySaveData.colorSave = colorList;
        mySaveData.indicesSave = indicesList;
        mySaveData.bodySave = bodyList;

        bf.Serialize(fs, mySaveData);
        fs.Close();

        Application.Quit();

    }

    private Texture2D RTImage(Camera targetCamera)
    {
        Rect rect = new Rect(0, 0, kinectWidth, kinectHeight);
        RenderTexture renderTexture = new RenderTexture(kinectWidth, kinectHeight, 24);
        Texture2D screenShot = new Texture2D(kinectWidth, kinectHeight, TextureFormat.RGBA32, false);

        targetCamera.targetTexture = renderTexture;
        targetCamera.Render();

        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);

        targetCamera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(renderTexture);
        renderTexture = null;
        return screenShot;
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
        return Mathf.Atan(-Vector2.Dot(N, A) / (N.magnitude * A.magnitude)) * Mathf.Rad2Deg;
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

                if (mask_except.GetPixel(i, j) == Color.white || mask_all.GetPixel(i, j) == Color.black)
                {
                    continue;
                }

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


    public void triggerLoad()
    {
        if (nowRecording)
        {
            Debug.Log("Cannot load .dat file while recording!");
            Application.Quit();
        }

        nowRecording = false;
        nowLoading = true;




    }

    public void SaveTextureToFile()
    {
        Texture2D texture = postProcessing((Texture2D)baseMaterial.mainTexture);
        byte[] bytes = texture.EncodeToPNG();
        

        string directoryName = loadFilename.Substring(0, loadFilename.Length - 4);
        var dirPath = Application.dataPath + "/SaveStream/" + directoryName + "/";
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

    public void saveLog()
    {
        string directoryName = loadFilename.Substring(0, loadFilename.Length - 4);
        var dirPath = Application.dataPath + "/SaveStream/" + directoryName + "/";

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(dirPath + "Log_" + now + ".csv"))

        {
            file.WriteLine("filled");

            foreach (int i in log_isfilled)
            {
                file.WriteLine(i.ToString());
            }

        }
    }

    
    Texture2D[] loadMasks(string filepath)
    {

        Texture2D mask = null;
        byte[] fileData;

        if (System.IO.File.Exists(Application.dataPath + "/" + filepath + "/mask_all.png"))
        {
            fileData = System.IO.File.ReadAllBytes(Application.dataPath + "/" + filepath + "/mask_all.png");
            mask_all = new Texture2D(textureWidth, textureHeight);
            mask_all.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        else
        {
            Debug.Log("There's no mask_all.png");
        }

        if (System.IO.File.Exists(Application.dataPath + "/" + filepath + "/mask_except.png"))
        { 
            fileData = System.IO.File.ReadAllBytes(Application.dataPath + "/" + filepath + "/mask_except.png");
            mask_except = new Texture2D(textureWidth, textureHeight);
            mask_except.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        else
        {
            
            Debug.Log("There's no mask_except.png");
            
        }

        if (System.IO.File.Exists(Application.dataPath + "/" + filepath + "/base_texture.png"))
        {
            fileData = System.IO.File.ReadAllBytes(Application.dataPath + "/" + filepath + "/base_texture.png");
            base_texture = new Texture2D(textureWidth, textureHeight);
            base_texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        else
        {

            Debug.Log("There's no mask_except.png");

        }

        Texture2D[] loaded_mask = new Texture2D[] { mask_all, mask_except, base_texture };

        return loaded_mask;
    }


    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }

    private void makeDir(string filepath)
    {
        string directoryName = loadFilename.Substring(0, loadFilename.Length - 4);
        if (!Directory.Exists(filepath + directoryName))
        {
            Directory.CreateDirectory(filepath + directoryName);
        }
    }
}
