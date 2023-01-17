using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    // Start is called before the first frame update

    public void ToKinectScene()
    {
        SceneManager.LoadScene("Kinect4AzureSampleScene");
    }

    public void ToMainScene()
    {
        SceneManager.LoadScene("main");
    }
}
