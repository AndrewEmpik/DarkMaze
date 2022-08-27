using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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

	[SerializeField] DarkOutEffect darkOutEffect;

	public static bool MenuActive = true;

	bool _winMenuActive = false;
	bool _failMenuActive = false;

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
		SetMenuOff();
		Debug.Log("Стартуем darkOutEffect.ShowDarkOutEffect(false)");
		darkOutEffect.ShowDarkOutEffect(false);

		// для удобства, если в редакторе они были включены, в игре выключить
		if (_winCanvas.gameObject.activeInHierarchy)
			_winCanvas.gameObject.SetActive(false);
		if (_failCanvas.gameObject.activeInHierarchy)
			_failCanvas.gameObject.SetActive(false);
	}

    void Update()
	{
		Cursor.visible = MenuActive;
		Cursor.lockState = MenuActive ? CursorLockMode.None : CursorLockMode.Locked;

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!_winMenuActive && !_failMenuActive)
				ToggleMenuActive();
		}
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

	public void SetCameraDropdownValue(int value)
	{
		_drpCamera.value = value;
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
		if (MenuActive)
			SetMenuOn();
		else
			SetMenuOff();
	}

	public void SetMenuOn()
	{
		MenuActive = true;
		_menuCanvas.gameObject.SetActive(true);
		Time.timeScale = 0f;
	}

	public void SetMenuOff()
	{
		MenuActive = false;
		_menuCanvas.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	public void Win()
	{
		_winMenuActive = true;
		_winCanvas.gameObject.SetActive(true);
		MenuActive = true;
	}
	public void HideWinMenu()
	{
		_winMenuActive = false;
		_winCanvas.gameObject.SetActive(false);
		MenuActive = false;
	}

	public void Lose()
	{
		_failMenuActive = true;
		_failCanvas.gameObject.SetActive(true);
		MenuActive = true;
	}

	public void RestartSceneWithDarkOut()
	{
		StartCoroutine(RestartSceneWithDarkOutCoroutine());
	}

	private IEnumerator RestartSceneWithDarkOutCoroutine()
	{
		darkOutEffect.gameObject.SetActive(true);
		yield return StartCoroutine(darkOutEffect.ShowDarkOutEffectAsCoroutine(true));
		//_loadingBadge.SetActive(true);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	public void LoadSceneMainMenuWithDarkOut()
	{
		StartCoroutine(LoadSceneMainMenuWithDarkOutCoroutine());
	}

	private IEnumerator LoadSceneMainMenuWithDarkOutCoroutine()
	{
		darkOutEffect.gameObject.SetActive(true);
		yield return StartCoroutine(darkOutEffect.ShowDarkOutEffectAsCoroutine(true));
		//_loadingBadge.SetActive(true);
		LoadSceneMainMenu();
	}

	public void LoadSceneMainMenu()
	{
		SceneManager.LoadScene(0);
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
