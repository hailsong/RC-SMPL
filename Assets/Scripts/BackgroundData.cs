using System;
using System.Runtime.Serialization;

using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

// Class which contains all data sent from background thread to main thread.
[Serializable]
public class BackgroundData : ISerializable
{
    // Timestamp of current data
    public float TimestampInMs { get; set; }

    // Depth image frame. 
    public byte[] DepthImage { get; set; }

    public int DepthImageWidth { get; set; }
    public int DepthImageHeight { get; set; }
    public int DepthImageSize { get; set; }

    // Number of detected bodies.
    public ulong NumOfBodies { get; set; }

    // vertices, colors, indices
    public Vector3[] vertices { get; set; }
    public Color32[] colors{ get; set; }
    public int[] indices{ get; set; }
    public Image colorImg { get; set; }

    // List of all bodies in current frame, each body is list of Body objects.
    public Body[] Bodies { get; set; }

    public BackgroundData(int maxDepthImageSize = 1024 * 1024 * 3, int maxBodiesCount = 20, int maxJointsSize = 100)
    {
        DepthImage = new byte[maxDepthImageSize];

        Bodies = new Body[maxBodiesCount];
        for (int i = 0; i < maxBodiesCount; i++)
        {
            Bodies[i] = new Body(maxJointsSize);
        }

        int num = DepthImageWidth * DepthImageHeight;



    }

    public BackgroundData(SerializationInfo info, StreamingContext context)
    {
        TimestampInMs = (float)info.GetValue("TimestampInMs", typeof(float));
        DepthImageWidth = (int)info.GetValue("DepthImageWidth", typeof(int));
        DepthImageHeight = (int)info.GetValue("DepthImageHeight", typeof(int));
        DepthImageSize = (int)info.GetValue("DepthImageSize", typeof(int));
        NumOfBodies = (ulong)info.GetValue("NumOfBodies", typeof(ulong));
        Bodies = (Body[])info.GetValue("Bodies", typeof(Body[]));
        DepthImage = (byte[])info.GetValue("DepthImage", typeof(byte[]));
        vertices = (Vector3[])info.GetValue("vertices", typeof(Vector3[]));
        colors = (Color32[])info.GetValue("colors", typeof(Color32[]));
        indices = (int[])info.GetValue("indices", typeof(int[]));
        colorImg = (Image)info.GetValue("colorImage", typeof(Image));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        // Writing only relevant data to serialized stream, without the placeholder data
        // (the real depthimage size is not maxdepthimagesize, but smaller).
        info.AddValue("TimestampInMs", TimestampInMs, typeof(float));
        info.AddValue("DepthImageWidth", DepthImageWidth, typeof(int));
        info.AddValue("DepthImageHeight", DepthImageHeight, typeof(int));
        info.AddValue("DepthImageSize", DepthImageSize, typeof(int));
        info.AddValue("NumOfBodies", NumOfBodies, typeof(ulong));
        Body[] ValidBodies = new Body[NumOfBodies];
        for (int i = 0; i < (int)NumOfBodies; i ++)
        {
            ValidBodies[i] = Bodies[i];
        }
        info.AddValue("Bodies", ValidBodies, typeof(Body[]));
        byte[] ValidDepthImage = new byte[DepthImageSize];
        for (int i = 0; i < DepthImageSize; i++)
        {
            ValidDepthImage[i] = DepthImage[i];
        }
        info.AddValue("DepthImage", ValidDepthImage, typeof(byte[]));
        info.AddValue("vertices", vertices, typeof(Vector3[]));
        info.AddValue("colors", colors, typeof(Color32[]));
        info.AddValue("indices", vertices, typeof(int[]));
        info.AddValue("colorImg", colorImg, typeof(Image));

    }
}

