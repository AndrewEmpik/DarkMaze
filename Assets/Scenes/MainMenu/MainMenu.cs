using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] RawImage _imageForDarkEffect;
	[SerializeField] float _darkEffectDuration = 0.5f;
	[SerializeField] AudioSource _music;
	float _startMusicVolume;

	Color _imageForDarkEffectStartColor = new Color(35f/255, 31f / 255, 32f / 255, 0);

	void Start()
    {
		_startMusicVolume = _music.volume;
		StartCoroutine(DarkOutCoroutine(false));
	}

	public void LoadSceneByIndex(int index)
	{
		StartCoroutine(LoadSceneByIndexCoroutine(index));
	}

	private IEnumerator LoadSceneByIndexCoroutine(int index)
	{
		yield return StartCoroutine(DarkOutCoroutine(true));
		SceneManager.LoadScene(index);
	}

	IEnumerator DarkOutCoroutine(bool on)
	{
		if (on)
			_imageForDarkEffect.gameObject.SetActive(true);

		float ratio;

		for (float t = 0; t <= _darkEffectDuration; t += Time.deltaTime)
		{
			ratio = t / _darkEffectDuration;
			if (!on)
				ratio = 1 - ratio;

			_imageForDarkEffect.color = _imageForDarkEffectStartColor + new Color(0, 0, 0, ratio);
			if (on)
				_music.volume = _startMusicVolume * (1-ratio);
			yield return null;
		}

		if (on)
			_imageForDarkEffect.color = _imageForDarkEffectStartColor + Color.black;

		if (!on)
			_imageForDarkEffect.gameObject.SetActive(false);
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
