using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class CaptureScript : MonoBehaviour
{
    public Button button;
    public Camera virtualCamera;
    int kinectWidth = 1024;
    int kinectHeight = 1024;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void capture()
    {
        Texture2D VirtualTexture2D = RTImage(virtualCamera);
        byte[] bytes_render = VirtualTexture2D.EncodeToPNG();

        string fileName = "capture";
        var dirPath = Application.dataPath + "/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }


        File.WriteAllBytes(dirPath + "Image_capture.png", bytes_render);
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
}
