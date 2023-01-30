using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public bool isResultScene;

    public void ToKinectScene()
    {
        SceneManager.LoadScene("Kinect4AzureSampleScene");
    }

    public void ToSMPLKinectScene()
    {
        SceneManager.LoadScene("SMPL_Kinect");
    }

    public void ToMainScene()
    {
        SceneManager.LoadScene("main");
    }

    private void Start()
    {
        if (isResultScene)
        {
            
        }
    }
}
