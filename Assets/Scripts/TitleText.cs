using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleText : MonoBehaviour
{
	MazeGenerator _main;

	private void Start()
	{
		_main = FindObjectOfType<MazeGenerator>();
		//string gameTitle = "HorrorMaze";
		//string gameVersion = "alpha 0.3.1_i1";
		GetComponent<Text>().text = "<b>" + _main.GameName + "</b> " + _main.GameVersion;
	}
     
}
