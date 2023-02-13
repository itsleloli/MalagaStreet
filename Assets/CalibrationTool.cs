using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationTool : MonoBehaviour
{
    public Slider xSlider;
    public Slider ySlider;
    public Slider zSlider;

    public TextMeshProUGUI xValue;
    public TextMeshProUGUI yValue;
    public TextMeshProUGUI zValue;

    public Toggle xInvert;
    public Toggle yInvert;
    public Toggle zInvert;

    public bool xinv;
    public bool yinv;
    public bool zinv;

    private void Update()
    {
        xValue.text = xSlider.value.ToString();
        yValue.text = ySlider.value.ToString();
        zValue.text = zSlider.value.ToString();

        xinv = xInvert.isOn;
        yinv = yInvert.isOn;
        zinv = zInvert.isOn;

    }
}
