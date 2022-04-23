using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCanvas : MonoBehaviour
{
    void Start()
    {
		gameObject.SetActive(false);
    }

	bool _winMenuActive = false;

	public void ToggleWinMenuActive()
	{
		_winMenuActive = !_winMenuActive;
		gameObject.SetActive(_winMenuActive);
		FindObjectOfType<PlayerMove>().MenuActive = _winMenuActive;
	}

}
