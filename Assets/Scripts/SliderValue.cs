using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    public Text Text;
    public Slider Slider;
	public float Multiplier = 1.0f;

    private void Start()
    {
        SetText(Slider.value);
    }
    public void SetText(float value)
    {
		// Multiplier - the value that the user sees
		Text.text = (value * Multiplier).ToString("0.0"); 
    }
}
