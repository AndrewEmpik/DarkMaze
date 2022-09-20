using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{


	private bool _localeChangingInProgress = false;

	// 0 = English,
	// 1 = Ukrainian,
	// 2 = Russian,
	// 3 = Polish
	public void ChangeLocale(int _locale)
	{
		if (_localeChangingInProgress)
			return;
		StartCoroutine(SetLocale(_locale));
	}

	IEnumerator SetLocale(int _locale)
	{
		_localeChangingInProgress = true;
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_locale];
		_localeChangingInProgress = false;
	}
}
