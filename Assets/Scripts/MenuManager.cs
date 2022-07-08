using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	private float fps;
	
	[SerializeField] private Dropdown _drpCamera;
	[SerializeField] private Dropdown _drpLight;

	[SerializeField] Canvas _menuCanvas;
	[SerializeField] Canvas _winCanvas;
	[SerializeField] Canvas _failCanvas;

	public static bool MenuActive = true;

	bool _winMenuActive = false;

	public static bool FirstLoad = true;

	[SerializeField] PlayerMove _player;

	public static MenuManager Instance;

	[SerializeField] private PlaytimeSettings _defaultSettings;
	[SerializeField] private PlaytimeSettings _playtimeSettings;

	private void Awake()
	{
		//if (Instance == null)
		//{
			Instance = this;
		//	DontDestroyOnLoad(gameObject);
		//}
		//else
		//	Destroy(gameObject);
	}

	void Start()
	{
		Time.timeScale = FirstLoad ? 0f : 1f;
		MenuActive = FirstLoad;

		_winCanvas.gameObject.SetActive(false);
	}

    void Update()
    {
		if (!_player.Dead && Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleMenuActive();
		}
	}

	public void ToggleWinMenuActive()
	{
		_winMenuActive = !_winMenuActive;
		_winCanvas.gameObject.SetActive(_winMenuActive);
		MenuActive = _winMenuActive;
	}

	void OnGUI()
	{
		//float newFPS = 1.0f / Time.smoothDeltaTime;
		fps = 1.0f / Time.smoothDeltaTime;  //Mathf.Lerp(fps, newFPS, 0.0005f);
		GUI.Label(new Rect(0, 0, 200, 100), "FPS: " + ((int)fps).ToString());
	}
	public void NextCameraDropdownValue()
	{
		if (_drpCamera.value >= _drpCamera.options.Count - 1)
			_drpCamera.value = 0;
		else
			_drpCamera.value++;
	}
	public void NextLightDropdownValue()
	{
		if (_drpLight.value >= _drpLight.options.Count - 1)
			_drpLight.value = 0;
		else
			_drpLight.value++;
	}

	public void ToggleMenuActive()
	{
		MenuActive = !MenuActive;
		_menuCanvas.gameObject.SetActive(MenuActive);
		Time.timeScale = MenuActive ? 0f : 1f;
	}

	public void HideFailMenu()
	{
		_failCanvas.gameObject.SetActive(false);
		// два следующих присвоения будут не нужны, поскольку переделываем архитектуру
		//MenuActive = false;
		//_dead = false;
	}

	public void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // работает только в редакторе
#else
			Application.Quit(); // работает только в билде
#endif
	}
}
