using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] AudioSource _music;
	[SerializeField] GameObject _musicCrossOut;
	float _startMusicVolume;
	[SerializeField] DarkOutEffect darkOutEffect;
	[SerializeField] GameObject _loadingBadge;

	[SerializeField] Slider _masterVolumeSlider;
	[SerializeField] Slider _musicVolumeSlider;

	Color _imageForDarkEffectStartColor = new Color(35f/255, 31f / 255, 32f / 255, 0); // maybe change it later, looks not good

	public static bool MusicIsOn = true;
	public static float MusicVolume = 0.5f;

	void Start()
    {
		Time.timeScale = 1;
		darkOutEffect.ShowDarkOutEffect(false);
		_masterVolumeSlider.value = AudioListener.volume;
		_musicVolumeSlider.value = MusicVolume;
		if (MusicIsOn)
			SetMusicOn();
		else
			SetMusicOff();
	}

	public void LoadSceneByIndex(int index)
	{
		StartCoroutine(LoadSceneByIndexCoroutine(index));
	}

	private IEnumerator LoadSceneByIndexCoroutine(int index)
	{
		darkOutEffect.gameObject.SetActive(true);
		yield return StartCoroutine(darkOutEffect.ShowDarkOutEffectAsCoroutine(true));
		_loadingBadge.SetActive(true);
		SceneManager.LoadScene(index);
	}

	public void ToggleMusic()
	{
		MusicIsOn = !MusicIsOn;
		if (MusicIsOn)
			SetMusicOn();
		else
			SetMusicOff();
	}
	public void SetMusicOn()
	{
		MusicIsOn = true;
		_music.enabled = MusicIsOn;
		_musicCrossOut.SetActive(!MusicIsOn);
	}
	public void SetMusicOff()
	{
		MusicIsOn = false;
		_music.enabled = MusicIsOn;
		_musicCrossOut.SetActive(!MusicIsOn);
	}

	public void SetMasterVolume(float value)
	{
		AudioListener.volume = value;
	}
	public void SetMusicVolume(float value)
	{
		MusicVolume = value;
		_music.volume = MusicVolume;
	}

	public void QuitApplicationWithEffect()
	{
		StartCoroutine(QuitApplicationWithEffectCoroutine());
	}
	private IEnumerator QuitApplicationWithEffectCoroutine()
	{
		darkOutEffect.gameObject.SetActive(true);
		yield return StartCoroutine(darkOutEffect.ShowDarkOutEffectAsCoroutine(true));
		QuitApplication();
	}

	// need to unificate somewhere not to duplicate the code
	public void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // works in editor only
#else
			Application.Quit(); // works in build only
#endif
	}

}
