using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DarkOutEffect : MonoBehaviour
{
	[SerializeField] float _darkEffectDuration = 0.5f;
	[SerializeField] AudioSource _music;
	float _startMusicVolume;
	RawImage _imageForDarkEffect;
	
	static Color _imageForDarkEffectStartColor = new Color(35f / 255, 31f / 255, 32f / 255, 0); // maybe change it later, looks not good

	private void Awake()
	{
		_imageForDarkEffect = this.GetComponent<RawImage>();
		_startMusicVolume = _music.volume;
	}

	public void ShowDarkOutEffect(bool _in)
	{
		StartCoroutine(DarkOutCoroutine(_in));
	}

	public IEnumerator ShowDarkOutEffectAsCoroutine(bool _in)
	{
		yield return StartCoroutine(DarkOutCoroutine(true));
	}

	IEnumerator DarkOutCoroutine(bool _in)
	{
		if (_in)
			_imageForDarkEffect.gameObject.SetActive(true);
		else
			_imageForDarkEffect.color = _imageForDarkEffectStartColor + new Color(0, 0, 0, 1); // for it not to blink

		float ratio;

		float tmpTime = Time.unscaledDeltaTime;

		bool skipOneFrame = true;
		while (true)
		{
			if (skipOneFrame)
			{
				skipOneFrame = false;
				yield return new WaitForEndOfFrame(); //WaitForFixedUpdate();
			}
			else
			{
				break;
			}
		}

		for (float t = 0; t <= _darkEffectDuration; t += Time.timeScale == 0f ? Time.unscaledDeltaTime : Time.deltaTime)
		{
			ratio = t / _darkEffectDuration;
			if (!_in)
				ratio = 1 - ratio;

			_imageForDarkEffect.color = _imageForDarkEffectStartColor + new Color(0, 0, 0, ratio);
			if (_in)
				_music.volume = _startMusicVolume * (1 - ratio);
			yield return null;
		}

		if (_in)
			_imageForDarkEffect.color = _imageForDarkEffectStartColor + Color.black;

		if (!_in)
		{
			_imageForDarkEffect.gameObject.SetActive(false);
			_imageForDarkEffectStartColor = Color.clear;
		}
	}
}
