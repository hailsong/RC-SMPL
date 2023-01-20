using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonScript : MonoBehaviour
{
    public TextAsset ParmsJSON;

    private void Start()
    {
        // For debugging
        // readShapeParms();
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
