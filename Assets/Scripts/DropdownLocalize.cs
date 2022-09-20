using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class DropdownLocalize : MonoBehaviour
{
	[SerializeField] string[] _localizeKeys;
	string _stringTableName = "StringTable"; // constant here

    void Start()
    {
		if (_localizeKeys.Length > 0)
		{
			Dropdown dropdown = this.GetComponent<Dropdown>();
			if (dropdown)
			{
				for (int i = 0; i < Mathf.Min(_localizeKeys.Length, dropdown.options.Count); i++)
				{
					dropdown.options[i].text = LocalizationSettings.StringDatabase.GetLocalizedString(_stringTableName, _localizeKeys[i]);
				}
			}
		}
	}
}
