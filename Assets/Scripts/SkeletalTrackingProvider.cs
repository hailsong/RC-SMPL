using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class SkeletalTrackingProvider : BackgroundDataProvider
{
    bool readFirstFrame = false;
    TimeSpan initialTimestamp;

    //pt 수
    int num;

    Mesh mesh;

    // pc 좌표
    public Vector3[] vertices;

    // pc 색 정보
    public Color32[] colors;

    // pc index
    public int[] indices;

    public Capture bodyFrameCapture;

    Transformation transformation;


    public SkeletalTrackingProvider(int id) : base(id)
    {
        Debug.Log("in the skeleton provider constructor");
    }

    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter { get; set; } = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

    public Stream RawDataLoggingFile = null;

    protected override void RunBackgroundThreadAsync(int id, CancellationToken token)
    {
        try
        {
            UnityEngine.Debug.Log("Starting body tracker background thread.");

            // Buffer allocations.
            BackgroundData currentFrameData = new BackgroundData();
            // Open device.
            using (Device device = Device.Open(id))
            {
                device.StartCameras(new DeviceConfiguration()
                {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R720p,
                    CameraFPS = FPS.FPS30,
                    // ColorResolution = ColorResolution.Off,
                    DepthMode = DepthMode.NFOV_Unbinned,
                    // DepthMode = DepthMode.NFOV_2x2Binned,
                    SynchronizedImagesOnly = true,
                    // WiredSyncMode = WiredSyncMode.Standalone,
                });

                UnityEngine.Debug.Log("Open K4A device successful. id " + id + "sn:" + device.SerialNum);

                var deviceCalibration = device.GetCalibration();
                transformation = deviceCalibration.CreateTransformation();

                int width = device.GetCalibration().DepthCameraCalibration.ResolutionWidth;
                int height = device.GetCalibration().DepthCameraCalibration.ResolutionHeight;
                num = width * height;
                Debug.Log($"{width}, {height}");
                Debug.Log("num" + num.ToString());

                vertices = new Vector3[num];
                colors = new Color32[num];
                indices = new int[num];

                for (int i = 0; i < num; i++)
                {
                    indices[i] = i;
                }

                using (Tracker tracker = Tracker.Create(deviceCalibration, new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Cuda, SensorOrientation = SensorOrientation.Default }))
                {
                    UnityEngine.Debug.Log("Body tracker created.");
                    while (!token.IsCancellationRequested)

                    {
                        using (Capture sensorCapture = device.GetCapture())
                        {
                            // Queue latest frame from the sensor.
                            tracker.EnqueueCapture(sensorCapture);
                        }

                        // Try getting latest tracker frame.
                        using (Frame frame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                        {
                            if (frame == null)
                            {
                                UnityEngine.Debug.Log("Pop result from tracker timeout!");
                            }
                            else
                            {
                                IsRunning = true;
                                // Get number of bodies in the current frame.
                                currentFrameData.NumOfBodies = frame.NumberOfBodies;

                                // Copy bodies.
                                for (uint i = 0; i < currentFrameData.NumOfBodies; i++)
                                {
                                    currentFrameData.Bodies[i].CopyFromBodyTrackingSdk(frame.GetBody(i), deviceCalibration);
                                }

                                // Store depth image.
                                Capture bodyFrameCapture = frame.Capture;
                                Image depthImage = bodyFrameCapture.Depth;

                                ////Depth 이미지 획득

                                //// Color 이미지 획득
                                Image colorImage = transformation.ColorImageToDepthCamera(bodyFrameCapture);
                                // Image colorImage = bodyFrameCapture.Color;
                                //// 색상 정보를 배열로
                                BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray();
                                // Debug.Log("color : " + colorArray[1000].G.ToString());

                                currentFrameData.colorImg= colorImage;

                                // 포인트클라우드 visualization
                                Image xyzImage = transformation.DepthImageToPointCloud(depthImage);
                                Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray();
                                // Debug.Log("xyzArr : " + xyzArray[1000].Y.ToString());

                                // Kinect에서 취득한 모든 점의 좌표, 색상을 대입
                                for (int i = 0; i < num; i++)
                                {
                                    if (xyzArray[i].Z * 0.001f < 2)
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
                                }

                                // Debug.Log("vertices : " + vertices[1000].y.ToString());


                                //// mesh에 전달
                                ////scenemanager.GetComponent<main>().mesh.vertices = vertices;
                                ////scenemanager.GetComponent<main>().mesh.colors32 = colors;
                                ////scenemanager.GetComponent<main>().mesh.RecalculateBounds();
                                //mesh.vertices = vertices;
                                //mesh.colors32 = colors;
                                //mesh.RecalculateBounds();

                                //currentFrameData.Bodyframecapture = bodyFrameCapture;

                                if (!readFirstFrame)
                                {
                                    readFirstFrame = true;
                                    initialTimestamp = depthImage.DeviceTimestamp;
                                }
                                currentFrameData.TimestampInMs = (float)(depthImage.DeviceTimestamp - initialTimestamp).TotalMilliseconds;
                                currentFrameData.DepthImageWidth = depthImage.WidthPixels;
                                currentFrameData.DepthImageHeight = depthImage.HeightPixels;

                                currentFrameData.vertices = vertices;
                                currentFrameData.colors = colors;
                                currentFrameData.indices = indices;

                                // Read image data from the SDK.
                                var depthFrame = MemoryMarshal.Cast<byte, ushort>(depthImage.Memory.Span);

                                // Repack data and store image data.
                                int byteCounter = 0;
                                currentFrameData.DepthImageSize = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight * 3;

                                for (int it = currentFrameData.DepthImageWidth * currentFrameData.DepthImageHeight - 1; it > 0; it--)
                                {
                                    byte b = (byte)(depthFrame[it] / (ConfigLoader.Instance.Configs.SkeletalTracking.MaximumDisplayedDepthInMillimeters) * 255);
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                    currentFrameData.DepthImage[byteCounter++] = b;
                                }

                                if (RawDataLoggingFile != null && RawDataLoggingFile.CanWrite)
                                {
                                    binaryFormatter.Serialize(RawDataLoggingFile, currentFrameData);
                                }

                                // Update data variable that is being read in the UI thread.
                                SetCurrentFrameData(ref currentFrameData);
                            }

                        }
                    }
                    Debug.Log("dispose of tracker now!!!!!");
                    tracker.Dispose();
                }
                device.Dispose();
            }
            if (RawDataLoggingFile != null)
            {
                RawDataLoggingFile.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log($"catching exception for background thread {e.Message}");
            token.ThrowIfCancellationRequested();
        }
    }
}