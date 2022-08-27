using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField] AudioSource _music;
	[SerializeField] GameObject _musicCrossOut;
	float _startMusicVolume;
	bool _musicIsOn = true;
	[SerializeField] DarkOutEffect darkOutEffect;
	[SerializeField] GameObject _loadingBadge;

	Color _imageForDarkEffectStartColor = new Color(35f/255, 31f / 255, 32f / 255, 0);

	void Start()
    {
		Time.timeScale = 1;
		darkOutEffect.ShowDarkOutEffect(false);
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

	// потом унифицировать где-то, чтобы не плодить код
	public void QuitApplication()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false; // работает только в редакторе
#else
			Application.Quit(); // работает только в билде
#endif
	}

}
