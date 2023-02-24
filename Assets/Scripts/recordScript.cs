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
    public class SerializableVector3
    {
        public float[] x;
        public float[] y;
        public float[] z;

        public SerializableVector3(Vector3[] vectors)
        {
            x = vectors.Select(v => v.x).ToArray();
            y = vectors.Select(v => v.y).ToArray();
            z = vectors.Select(v => v.z).ToArray();
        }

        public SerializableVector3(Vector3 vector3)
        {
            Vector3 = vector3;
        }

        public Vector3 Vector3 { get; }

        public Vector3[] ToVector3Array()
        {
            int length = x.Length;
            Vector3[] vectors = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                vectors[i] = new Vector3(x[i], y[i], z[i]);
            }
            return vectors;
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


    public List<SerializableVector3> verticesSave;
    public List<SerializableColor32> colorSave;
    public List<int[]> indicesSave;
    // public List<Body> bodySave;
}


public class recordScript : MonoBehaviour
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

    float poseConfidence = 0;
    public float poseConfidenceThreshold;

    [SerializeField]
    UnityEngine.UI.RawImage ImageInput;

    public bool LogSaveScreenshots;

    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

    public bool[,] istexturefilled = new bool[1024, 1024];

    static int kinectWidth = 640;
    static int kinectHeight = 576;

    public bool rayCasting = false;

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

    List<SaveData.SerializableVector3> verticesList = new List<SaveData.SerializableVector3>();
    List<SaveData.SerializableColor32> colorList = new List<SaveData.SerializableColor32>();
    List<int[]> indicesList = new List<int[]>();
    List<Body> bodyList = new List<Body>();



    bool nowRecording = false;
    SaveData mySaveData = new SaveData();

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


        // mesh를 MeshFilter에 적용
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        

        watch.Start();

    }

    void FixedUpdate()
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
                body = m_tracker.GetComponent<TrackerHandler>().getBodyData(m_lastFrameData);

                for (int i = 0; i < m_lastFrameData.Bodies[0].JointPrecisions.Length; i++)
                {
                    poseConfidence += (int)m_lastFrameData.Bodies[0].JointPrecisions[i];
                }
                poseConfidence /= m_lastFrameData.Bodies[0].JointPrecisions.Length;

                // print(poseConfidence);

                kinectColorTexture.SetPixels32(colors);
                kinectColorTexture.Apply();

                ImageInput.canvasRenderer.SetTexture(kinectColorTexture);

                if (nowRecording)
                {
                    // verticesList.Add(vertices);
                    SaveData.SerializableVector3 serializableVector = new SaveData.SerializableVector3(vertices);
                    SaveData.SerializableColor32 serializableColor = new SaveData.SerializableColor32(colors);
                    verticesList.Add(serializableVector);
                    colorList.Add(serializableColor);
                    indicesList.Add(indices);
                    // bodyList.Add(body);
                    Debug.Log($"{verticesList.Count}, {colorList.Count}, {indicesList.Count}, {bodyList.Count}");
                }
                

                

                if (verticesList.Count > 500)
                {
                    Application.Quit();
                }

                // Save {vertices, colors, indices, 
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

        string now = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = Application.dataPath + "/SaveStream/" + now + "_SaveStream.dat";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(fileName, FileMode.Create);

        mySaveData.verticesSave = verticesList;
        mySaveData.colorSave = colorList;
        mySaveData.indicesSave = indicesList;
        // mySaveData.bodySave = bodyList;

        bf.Serialize(fs, mySaveData);
        fs.Close();

        Application.Quit();

    }

    public void triggerLoad()
    {
        BinaryFormatter bf = new BinaryFormatter();

        string fileName = Application.dataPath + "/SaveStream/" + "20230206164520_SaveStream.dat";
        FileStream fs = new FileStream(fileName, FileMode.Open);

        SaveData myLoadData = bf.Deserialize(fs) as SaveData;

        Debug.Log(myLoadData.colorSave);
        // Debug.Log(myLoadData.bodySave);
        Debug.Log(myLoadData.indicesSave);



    }
    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }
}
