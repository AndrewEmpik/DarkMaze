using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	public Camera MainCamera;
	public Camera PlayerCamera;

	public void SetCameraPosition (int index)
	{
		if (index == 0) // игрок
		{
			SetActiveCamera(1);
			this.transform.position = new Vector3(0, 1, -10);
			this.transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		else if (index == 1) // под углом
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(-24.8999996f, 13.3999996f, -31.1000004f);
			this.transform.rotation = Quaternion.Euler(35.1774979f, 42f, -1.04453579e-06f);
		}
		else if (index == 2) // сверху
		{
			SetActiveCamera(0);
			this.transform.position = new Vector3(0, 53.79f, 0);
			this.transform.rotation = Quaternion.Euler(90, 0, 0);
		}
	}

	void SetActiveCamera(int index)
	{
		MainCamera.enabled = (index == 0 ? true : false);
		MainCamera.gameObject.SetActive(index == 0 ? true : false);

		PlayerCamera.enabled = (index == 1 ? true : false);
		PlayerCamera.gameObject.SetActive(index == 1 ? true : false);
	}
}
