using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	public Camera MainCamera;
	public Camera PlayerCamera;

	public MazeGenerator MazeGenerator;
	int _curCameraPosition = 0;

	[SerializeField] GameObject inGameUI;

	public void SetCameraPosition (int index)
	{
		_curCameraPosition = index;
		float coefficient = (MazeGenerator.MazeSize - 2) / 49f; // depending on the Maze size

		if (index == 0) // player
		{
			SetActiveCamera(1);
		}
		else if (index == 1) // angled
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(	Mathf.Lerp(-2.56f, -71.2f, coefficient),
													Mathf.Lerp(5.23f, 26.8f, coefficient),
													Mathf.Lerp(-3.1f, -87.5f, coefficient));
			this.transform.rotation = Quaternion.Euler(35.1774979f, 42f, -1.04453579e-06f);
		}
		else if (index == 2) // from above
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(	0,
													Mathf.Lerp(6f, 135.67f, coefficient),
													Mathf.Lerp(0f, -1.84f, coefficient));
			this.transform.rotation = Quaternion.Euler(90, 0, 0);
		}
	}

	public void ResetCameraPosition()
	{
		SetCameraPosition(_curCameraPosition);
	}

	void SetActiveCamera(int index)
	{
		bool isPlayerCamera = index == 1 ? true : false;

		MainCamera.enabled = !isPlayerCamera;
		MainCamera.gameObject.SetActive( !isPlayerCamera );

		PlayerCamera.enabled = isPlayerCamera;
		PlayerCamera.gameObject.SetActive(isPlayerCamera);

		RenderSettings.fog = isPlayerCamera;
		inGameUI.SetActive(isPlayerCamera);
	}
}
