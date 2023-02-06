using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System.Linq;

[System.Serializable]
class SaveData
{
    // public List<Vector3[]> verticesSave;
    public List<Color32[]> colorSave;
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

    List<Vector3[]> verticesList = new List<Vector3[]>();
    List<Color32[]> colorList = new List<Color32[]>();
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
                    colorList.Add(colors);
                    indicesList.Add(indices);
                    // bodyList.Add(body);

                }
                

                Debug.Log($"{verticesList.Count}, {colorList.Count}, {indicesList.Count}, {bodyList.Count}");

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

        // mySaveData.verticesSave = verticesList;
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
}
