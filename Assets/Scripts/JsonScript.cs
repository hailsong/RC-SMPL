using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonScript : MonoBehaviour
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

    GameObject avatarObject;
    GameObject maskObject;

    [SerializeField]
    GameObject mainManager;

    private void Start()
    {
        
        switch (female)
        {
            case true:
                femalePrefab.SetActive(true);
                femaleMaskPrefab.SetActive(true);
                malePrefab.SetActive(false);
                maleMaskPrefab.SetActive(false);
                avatarObject = femalePrefab;
                maskObject = femaleMaskPrefab;
                break;
            case false:
                malePrefab.SetActive(true);
                maleMaskPrefab.SetActive(true);
                femalePrefab.SetActive(false);
                femaleMaskPrefab.SetActive(false);
                avatarObject = malePrefab;
                maskObject = maleMaskPrefab;
                break;
        }

        switch (isRecord)
        {
            case true:
                mainManager.GetComponent<recordScript>().SMPL_Avatar = avatarObject;
                mainManager.GetComponent<recordScript>().SMPLXObject = avatarObject;
                mainManager.GetComponent<recordScript>().SMPLXMask = maskObject;
                break;
            case false:
                mainManager.GetComponent<main>().SMPLXObject = avatarObject;
                break;
        }
        
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
