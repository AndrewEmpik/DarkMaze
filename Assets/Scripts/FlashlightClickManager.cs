using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightClickManager : MonoBehaviour
{
	[SerializeField] AudioSource[] ClickSounds;

	public void PlayClickSound()
	{
		if (ClickSounds.Length > 0)
		{
			int randomIndex = Random.Range(0, ClickSounds.Length);
			ClickSounds[randomIndex].pitch = Random.Range(0.9f, 1.1f);
			ClickSounds[randomIndex].Play();
		}
		else
			Debug.LogWarning("No click sounds found");
	}
	
}
