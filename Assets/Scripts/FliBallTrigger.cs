using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FliBallTrigger : MonoBehaviour
{

	public PlayerMove Player;
	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Player.Die();
		}
	}
}
