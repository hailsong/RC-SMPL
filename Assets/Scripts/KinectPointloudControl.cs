using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Kinect.Sensor;

using System.Threading.Tasks;

public class KinectPointloudControl : MonoBehaviour
{
    Device kinect;

    //pt ��
    int num;

    Mesh mesh;

    // pc ��ǥ
    Vector3[] vertices;

    // pc �� ����
    Color32[] colors;

    // pc index
    int[] indices;

    Transformation transformation;

    private void Start()
    {
        InitKinect();
        //PointCloud �غ�
        InitMesh();
        //Kinect ������ ��������
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

        // ��ǥ ��ȯ ���� Color <-> Depth
        transformation = kinect.GetCalibration().CreateTransformation();
    }

    // PC �غ�
    private void InitMesh()
    {
        // ���� W, H
        int width = kinect.GetCalibration().DepthCameraCalibration.ResolutionWidth;
        int height = kinect.GetCalibration().DepthCameraCalibration.ResolutionHeight;
        num = width * height;

        // mesh �ν��Ͻ�ȭ
        mesh = new Mesh();

        // �� ���� 65535 �̻�����!
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Depth �̹��� �� �ȼ� ����ŭ�� ���� ���� Ȯ��
        vertices = new Vector3[num];
        colors = new Color32[num];
        indices = new int[num];

        // PointColoud �迭 ��ȣ ���
        for (int i=0; i<num; i++)
        {
            indices[i] = i;
        }
        // ���� ��ǥ�� ���� mesh�� ����
        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        // mesh�� MeshFilter�� ����
        gameObject.GetComponent<MeshFilter>().mesh = mesh;

    }
       
    // Kinect ������ ��������
    private async Task KinectLoop()
    {
        while (true)
        {
            Debug.Log("LoopTest");
            //GetCapture���� Kinect ������ �˻�
            using (Capture capture = await Task.Run(() => kinect.GetCapture()).ConfigureAwait(true))
            {
                //Depth �̹��� ȹ��
                Image colorImage = transformation.ColorImageToDepthCamera(capture);
                // ���� ������ �迭��
                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();

                // Depth �̹����� xyz��
                Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth);
                // ��ȭ�� �����Ϳ��� ���� ��ǥ�� �迭��
                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();

                // Kinect���� ����� ��� ���� ��ǥ, ������ ����
                for (int i=0; i < num; i++)
                {
                    // vertices ��ǥ ����
                    vertices[i].x = xyzArray[i].X * 0.001f;
                    vertices[i].y = -xyzArray[i].Y * 0.001f;
                    vertices[i].z = xyzArray[i].Z * 0.001f;
                    // ���� �Ҵ�
                    colors[i].b = colorArray[i].B;
                    colors[i].g = colorArray[i].G;
                    colors[i].r = colorArray[i].R;

                    colors[i].a = 255;
                }
                // mesh�� ����
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
