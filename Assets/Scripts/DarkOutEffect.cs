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
	
	static Color _imageForDarkEffectStartColor = new Color(35f / 255, 31f / 255, 32f / 255, 0);

	private void Awake()
	{
		_imageForDarkEffect = this.GetComponent<RawImage>();
		_startMusicVolume = _music.volume;
	}

	public void ShowDarkOutEffect(bool _in)
	{
		//Debug.Log("Внутри ShowDarkOutEffect");
		StartCoroutine(DarkOutCoroutine(_in));
	}

	public IEnumerator ShowDarkOutEffectAsCoroutine(bool _in)
	{
		yield return StartCoroutine(DarkOutCoroutine(true));
	}

	IEnumerator DarkOutCoroutine(bool _in)
	{
		//Debug.Log("Внутри DarkOutCoroutine");
		//Debug.Log("Time.timeScale = " + Time.timeScale);
		//Debug.Log("Time.unscaledDeltaTime = " + Time.unscaledDeltaTime);
		//Debug.Log("Time.deltaTime = " + Time.deltaTime);
		if (_in)
			_imageForDarkEffect.gameObject.SetActive(true);
		else
			_imageForDarkEffect.color = _imageForDarkEffectStartColor + new Color(0, 0, 0, 1); // чтобы не мелькало, например

		float ratio;

		float tmpTime = Time.unscaledDeltaTime;

		bool skipOneFrame = true;
		while (true)
		{
			if (skipOneFrame)
			{
				skipOneFrame = false;
				//Debug.Log("Пытаемся пропустить ван фрейм");
				//Debug.Log("Time.unscaledDeltaTime = " + Time.unscaledDeltaTime);
				//Debug.Log("Time.deltaTime = " + Time.deltaTime);
				yield return new WaitForEndOfFrame(); //WaitForFixedUpdate();//
			}
			else
			{
				//Debug.Log("А тут уже без ван фрейма");
				//Debug.Log("Time.unscaledDeltaTime = " + Time.unscaledDeltaTime);
				//Debug.Log("Time.deltaTime = " + Time.deltaTime);
				break;
			}
		}

		for (float t = 0; t <= _darkEffectDuration; t += Time.timeScale == 0f ? Time.unscaledDeltaTime : Time.deltaTime)
		{
			//Debug.Log("t = " + t + ", tmpTime = " + tmpTime + ", _darkEffectDuration = " + _darkEffectDuration + ", Time.unscaledDeltaTime = " + Time.unscaledDeltaTime + ", Time.deltaTime = " + Time.deltaTime);
			ratio = t / _darkEffectDuration;
			if (!_in)
				ratio = 1 - ratio;

			_imageForDarkEffect.color = _imageForDarkEffectStartColor + new Color(0, 0, 0, ratio);
			if (_in)
				_music.volume = _startMusicVolume * (1 - ratio);
			yield return null;
		}
		//Debug.Log("Закончилось, Time.unscaledDeltaTime = " + Time.unscaledDeltaTime);

		if (_in)
			_imageForDarkEffect.color = _imageForDarkEffectStartColor + Color.black;

		if (!_in)
		{
			_imageForDarkEffect.gameObject.SetActive(false);
			_imageForDarkEffectStartColor = Color.clear;
		}
	}
}
