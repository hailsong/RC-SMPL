using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SetDebug : MonoBehaviour
{
    public Text m_Text;

    private void Start()
    {
        m_Text = GetComponent<Text>();
    }
    public void SetDebugFloat(float value)
    {

        m_Text.text = this.gameObject.name + " " + value.ToString() + "ms";

    }

    public void SetDebugConfidence(float value)
    {

        m_Text.text = this.gameObject.name + " " + value.ToString().Substring(0, 4);

    }
}
