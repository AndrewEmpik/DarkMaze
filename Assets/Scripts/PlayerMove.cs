using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float turnSpeed = 4.0f;
    public float moveSpeed = 2.0f;
    public float minTurnAngle = -90.0f;
    public float maxTurnAngle = 90.0f;
    private float rotX;

	public Transform PointOfView;
	public Rigidbody Rigidbody;

	public bool MenuActive = true;
	public Canvas Menu;

	public AudioSource FootstepsAudio;

	public Canvas FailMenu;

	private bool _dead = false;

	[SerializeField] GameObject Match;
	bool _matchActive = false;

	private void Start()
	{
		FootstepsAudio.Play();
		FootstepsAudio.Pause();
		//FootstepsAudio.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1f);
	}

	void Update()
    {
		Cursor.visible = MenuActive;
		Cursor.lockState = MenuActive ? CursorLockMode.None : CursorLockMode.Locked;

		if (!_dead && Input.GetKeyDown(KeyCode.Escape))	
		{
			ToggleMenuActive();
		}

		if (!MenuActive)
		{
			MouseAiming();

			if (Input.GetKeyDown(KeyCode.F))
			{
				ToggleMatch();
			}

		}


    }

	public void ToggleMenuActive()
	{
		MenuActive = !MenuActive;
		Menu.gameObject.SetActive(MenuActive);
	}

	public void ToggleMatch()
	{
		_matchActive = !_matchActive;
		Match.SetActive(_matchActive);
	}

	public void Die()
	{
		FailMenu.gameObject.SetActive(true);
		MenuActive = true;
		_dead = true;
	}

	public void HideFailMenu()
	{
		FailMenu.gameObject.SetActive(false);
		MenuActive = false;
		_dead = false;
	}

	void FixedUpdate()
	{
		if (!MenuActive)
			KeyboardMovement();
		else
		{
			Rigidbody.velocity = Vector3.zero;
			FootstepsAudio.Pause();
		}
			
	}
	void MouseAiming()
    {
		//Cursor.visible = false;
		//Cursor.lockState = CursorLockMode.Locked;

        // get the mouse inputs
        float y = Input.GetAxis("Mouse X") * turnSpeed;
        rotX += Input.GetAxis("Mouse Y") * turnSpeed;
        // clamp the vertical rotation
        rotX = Mathf.Clamp(rotX, minTurnAngle, maxTurnAngle);
		// rotate the camera
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + y, 0);
		PointOfView.eulerAngles = new Vector3(-rotX, PointOfView.eulerAngles.y, 0);
		//PointOfView.eulerAngles = new Vector3(0, transform.eulerAngles.y + y, 0);
	}
    void KeyboardMovement()
    {
		float boost = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;


		Vector3 dir = new Vector3(0, 0, 0);
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");
		//transform.Translate(dir * moveSpeed);// * Time.deltaTime);
		Rigidbody.velocity = (dir.z * moveSpeed * transform.forward
							+ dir.x * moveSpeed * transform.right)
							 * boost;

		//Debug.Log(1f / boost);


		FootstepsAudio.pitch = boost;
		//FootstepsAudio.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1f / boost);

		if (Rigidbody.velocity.magnitude > 0.5f)
			FootstepsAudio.UnPause();
		else
			FootstepsAudio.Pause();

	}

	//void CameraBobbing(val);
}