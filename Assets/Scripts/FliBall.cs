using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FliBall : MonoBehaviour
{
	public float Speed = 7;
	public Rigidbody Rigidbody;
	public AudioSource HitSound1;
	public AudioSource HitSound2;
	private AudioSource _curHitSound;
	public AudioClip ClapClip;

	public List<Vector3> CollisionsList;
	private int CollisionsListLength = 7;

	Vector3 _prevPos;

	public MazeGenerator Main;

	public int _collisionsAfterCorrection = 0; 

    void Start()
    {
		Respawn();
		_prevPos = transform.position;
		Rigidbody.velocity = new Vector3(1, 0, 1).normalized * Speed;
	}

    void FixedUpdate()
    {
		// was needed for debug
		//if (Input.GetKey(KeyCode.Space))
			//transform.position = Main.PositionByCellAddress(MazeGenerator.PinnedPosition.Center) + new Vector3(0, 1.52f, 0);
			//CorrectDirection(1f);
			//Rigidbody.velocity = new Vector3(1, 0, 1).normalized * Speed;

		Rigidbody.velocity = Rigidbody.velocity.normalized * Speed;
	}

	private void Update()
	{
		// it is seen in the editor only
		Debug.DrawLine(_prevPos, transform.position,Color.blue,1000);
		_prevPos = transform.position;
	}

	void CorrectDirection(float value)
	{
		Rigidbody.velocity = new Vector3(Rigidbody.velocity.x + Random.Range(value/2,value)*Mathf.Sign(Random.Range(-1f,1f)), 0, 
										 Rigidbody.velocity.z + Random.Range(value/2,value)*Mathf.Sign(Random.Range(-1f,1f)));

		_collisionsAfterCorrection = 0;
	}

	public void Respawn()
	{
		transform.position = Main.PositionByCellAddress((MazeGenerator.PinnedPosition)Random.Range(5, 9)) + new Vector3(0, 1.52f, 0);
	}

	private void OnCollisionEnter(Collision collision)
	{
		//collision.GetContacts(CollisionsList);
		if (CollisionsList.Contains(collision.contacts[0].point))
		{
			Debug.Log("Fliball stuck!");
			CorrectDirection(2f);
		}
		CollisionsList.Add(collision.contacts[0].point);
		if (CollisionsList.Count > CollisionsListLength)
			CollisionsList.RemoveAt(0);
		
		_curHitSound = _curHitSound == HitSound1 ? HitSound2 : HitSound1;

		_curHitSound.volume = collision.impulse.magnitude / 14f;
		_curHitSound.pitch = Mathf.Lerp(0.8f, 1.2f, Random.value);
		_curHitSound.Play();

		//AudioSource.PlayClipAtPoint(ClapClip, collision.contacts[0].point /*transform.position*/,collision.impulse.magnitude/14f);

		_collisionsAfterCorrection++;
		if (_collisionsAfterCorrection > 100)
		{
			Debug.Log("Profilactical correction");
			CorrectDirection(2f);
		}
	}
}
