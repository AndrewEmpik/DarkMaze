using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
	WinCanvas WinCanvas;

	public MazeGenerator Main;

	private void Start()
	{
		Main = FindObjectOfType<MazeGenerator>();

		WinCanvas = Main.WinCanvas;
		//WinCanvas.gameObject.SetActive(false);


	}

	bool _winMenuActive = false;

	public void ToggleWinMenuActive()
	{
		//_winMenuActive = !_winMenuActive;
		//WinCanvas.gameObject.SetActive(_winMenuActive);
		//FindObjectOfType<PlayerMove>().MenuActive = _winMenuActive;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			WinCanvas.ToggleWinMenuActive();
		}
	}
}
