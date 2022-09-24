using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	public Camera MainCamera;
	public Camera PlayerCamera;

	public MazeGenerator MazeGenerator;
	int _curCameraPosition = 0;

	public void SetCameraPosition (int index)
	{
		_curCameraPosition = index;
		float coef = (MazeGenerator.MazeSize - 2) / 49f;

		if (index == 0) // игрок
		{
			SetActiveCamera(1);
			//this.transform.position = new Vector3(0, 1, -10);
			//this.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		else if (index == 1) // под углом
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(	Mathf.Lerp(-2.56f, -71.2f, coef),
													Mathf.Lerp(5.23f, 26.8f, coef),
													Mathf.Lerp(-3.1f, -87.5f, coef));
			this.transform.rotation = Quaternion.Euler(35.1774979f, 42f, -1.04453579e-06f);
		}
		else if (index == 2) // сверху
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(	0,
													Mathf.Lerp(6f, 135.67f, coef),
													Mathf.Lerp(0f, -1.84f, coef));
			this.transform.rotation = Quaternion.Euler(90, 0, 0);
		}
	}

	public void ResetCameraPosition()
	{
		SetCameraPosition(_curCameraPosition);
	}

	void SetActiveCamera(int index)
	{
		MainCamera.enabled = (index == 0 ? true : false);
		MainCamera.gameObject.SetActive(index == 0 ? true : false);

		PlayerCamera.enabled = (index == 1 ? true : false);
		PlayerCamera.gameObject.SetActive(index == 1 ? true : false);

		RenderSettings.fog = (index == 1 ? true : false);
	}
}
