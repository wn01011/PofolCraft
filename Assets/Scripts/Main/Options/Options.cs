using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Options : MonoBehaviour
{

    [SerializeField]
    private AimController aimController = null;
    [SerializeField]
    private Text senseText = null;

    private void Update()
    {
        senseText.text = "Senesitivity\nXSense : " + aimController.xSensitivity.ToString() + "\nYSense" + aimController.ySensitivity.ToString() + "\nTotalSense" + aimController.totalSensitivity.ToString();
    }
    private void XSensitivity(float _value)
    {
        aimController.xSensitivity = _value;
    }

    private void YSensitivity(float _value)
    {
        aimController.ySensitivity = _value;
    }

    private void TotalSensitivity(float _value)
    {
        aimController.totalSensitivity = _value;
    }
}
