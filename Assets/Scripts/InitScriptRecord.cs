using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitScriptRecord : MonoBehaviour
{
    public TextAsset ParmsJSON;
    [SerializeField, Tooltip("Input gender. male : 0, female : 1")]
    public bool female;
    [SerializeField, Tooltip("Record mode. record : 0, live : 1")]
    public bool isRecord;

    public GameObject malePrefab;
    public GameObject maleMaskPrefab;
    public GameObject femalePrefab;
    public GameObject femaleMaskPrefab;

    public SkinnedCollisionHelper maleCollisionHelper;
    public SkinnedCollisionHelper femaleCollisionHelper;

    GameObject avatarObject;
    GameObject maskObject;
    SkinnedCollisionHelper avatarCollisionHelper;

    [SerializeField]
    GameObject mainManager;

    private void Start()
    {

        switch (female)
        {
            case true:
                femalePrefab.SetActive(true);
                malePrefab.SetActive(false);
                avatarObject = femalePrefab;

                avatarCollisionHelper = femaleCollisionHelper;

                if (isRecord)
                {
                    femaleMaskPrefab.SetActive(true);
                    maleMaskPrefab.SetActive(false);
                    maskObject = femaleMaskPrefab;
                }

                break;

            case false:
                malePrefab.SetActive(true);
                femalePrefab.SetActive(false);
                avatarObject = malePrefab;

                avatarCollisionHelper = maleCollisionHelper;

                if (isRecord)
                {
                    maleMaskPrefab.SetActive(true);
                    femaleMaskPrefab.SetActive(false);
                    maskObject = maleMaskPrefab;
                }
        
                break;
        }

        switch (isRecord)
        {
            case true:
                mainManager.GetComponent<recordScript>().SMPL_Avatar = avatarObject;
                mainManager.GetComponent<recordScript>().SMPLXObject = avatarObject;
                mainManager.GetComponent<recordScript>().SMPLXMask = maskObject;
                mainManager.GetComponent<recordScript>().CollisionHelper = avatarCollisionHelper;
                break;
            case false:
                mainManager.GetComponent<main>().SMPLXObject = avatarObject;
                mainManager.GetComponent<main>().avatarOrigin = avatarObject.transform;
                break;
        }

        avatarObject.GetComponent<SMPLX>().SetHandPose(SMPLX.HandPose.Relaxed);
        avatarObject.GetComponent<SMPLX>().SetExpressions();


    }
    public float[] readShapeParms()
    {
        float[] betas = new float[10];

        SimpleJSON.JSONNode node = SimpleJSON.JSON.Parse(ParmsJSON.text);
        for (int bi = 0; bi < node["betas"][0].Count; bi++)
            //De bug.Log(node["betas"][0][bi].AsFloat);
            betas[bi] = node["betas"][0][bi].AsFloat;

        return betas;
    }
}
