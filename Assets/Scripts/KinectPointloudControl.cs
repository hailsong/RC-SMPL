using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Kinect.Sensor;

using System.Threading.Tasks;

public class KinectPointloudControl : MonoBehaviour
{
    Device kinect;

    //pt 수
    int num;

    Mesh mesh;

    // pc 좌표
    Vector3[] vertices;

    // pc 색 정보
    Color32[] colors;

    // pc index
    int[] indices;

    Transformation transformation;

    private void Start()
    {
        InitKinect();
        //PointCloud 준비
        InitMesh();
        //Kinect 데이터 가져오기
        Task t = KinectLoop();

    }
    private void InitKinect()
    {
        kinect = Device.Open(0);

        kinect.StartCameras(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        });

        // 좌표 변환 정보 Color <-> Depth
        transformation = kinect.GetCalibration().CreateTransformation();
    }

    // PC 준비
    private void InitMesh()
    {
        // 뎁스 W, H
        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        num = width * height;

        // mesh 인스턴스화
        mesh = new Mesh();

        // 점 갯수 65535 이상으로!
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Depth 이미지 총 픽셀 수만큼의 저장 공간 확보
        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];

        // PointColoud 배열 번호 기록
        for (int i=0; i<num; i++)
        {
            indices[i] = i;
        }
        // 점의 좌표랑 색상 mesh로 전달
        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        // mesh를 MeshFilter에 적용
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

    }
       
    // Kinect 데이터 가져오기
    private async Task KinectLoop()
    {
        while (true)
        {
            Debug.Log("LoopTest");
            //GetCapture에서 Kinect 데이터 검색
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                //Depth 이미지 획득
                Image colorImage = transformation.ColorImageToDepthCamera(capture);
                // 색상 정보를 배열로
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                // Depth 이미지를 xyz로
                Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth);
                // 변화된 데이터에서 점의 좌표를 배열로
                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                // Kinect에서 취득한 모든 점의 좌표, 색상을 대입
                for (int i=0; i < num; i++)
                {
                    // vertices 좌표 대입
                    vertices[i].x = xyzArray[i].X * 0.001f;
                    vertices[i].y = -xyzArray[i].Y * 0.001f;
                    vertices[i].z = xyzArray[i].Z * 0.001f;
                    // 색상 할당
                    colors[i].b = colorArray[i].B;
                    colors[i].g = colorArray[i].G;
                    colors[i].r = colorArray[i].R;

                    colors[i].a = 255;
                }
                // mesh에 전달
                mesh.vertices = vertices;
                mesh.colors32 = colors;
                mesh.RecalculateBounds();
            }
        }
    }
    private void OnDestroy()
    {
        kinect.StopCameras();
    }
}
