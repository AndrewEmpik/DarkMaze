using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleText : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Text>().text = "<b>" + Application.productName + "</b> " + Application.version;
	}
}
