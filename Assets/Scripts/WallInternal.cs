using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallInternal : MonoBehaviour
{
	[SerializeField] GameObject[] Children;

    public void ActivateLattice()
    {
		for (int i = 0; i < Children.Length - 1; i++)
			Children[i].SetActive(false);
		Children[Children.Length - 1].SetActive(true);
	}

}
