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
	bool _musicIsOn = true;
	[SerializeField] DarkOutEffect darkOutEffect;
	[SerializeField] GameObject _loadingBadge;

	[SerializeField] Slider _masterVolumeSlider;
	[SerializeField] Slider _musicVolumeSlider;

	Color _imageForDarkEffectStartColor = new Color(35f/255, 31f / 255, 32f / 255, 0);

	public static float MusicVolume = 0.5f;


	void Start()
    {
		Time.timeScale = 1;
		darkOutEffect.ShowDarkOutEffect(false);
		_masterVolumeSlider.value = AudioListener.volume;
		_musicVolumeSlider.value = MusicVolume;
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
		_musicIsOn = !_musicIsOn;
		_music.enabled = _musicIsOn;
		_musicCrossOut.SetActive(!_musicIsOn);
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

	// ����� ������������� ���-��, ����� �� ������� ���
	public void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // �������� ������ � ���������
#else
			Application.Quit(); // �������� ������ � �����
#endif
	}

}
