using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinPanel : MonoBehaviour
{
    private InputField inputId = null;

    private void Start()
    {
        inputId = GetComponentInChildren<InputField>();
    }
    public InputField GetInputFieldId()
    {
        return inputId;
    }


}
