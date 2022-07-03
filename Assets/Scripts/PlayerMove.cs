using System.Collections;
using System.Collections.Generic;
using TMPro;
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
	public AudioSource GatherItemAudio;
	public AudioSource MatchLightingAudio;

	public Canvas FailMenu;

	private bool _dead = false;

	[SerializeField] GameObject Match;
	bool _matchActive = false;

	[SerializeField] TMP_Text MatchesUIText;
	[SerializeField] TMP_Text PickUIText;
	[SerializeField] GameObject MatchesProgressBar;
	public int MatchesCount = 10;

	private void Start()
	{
		MatchesUIUpdate();
		MatchesProgressBar.transform.localScale = new Vector3(0, 1, 1);
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
				if (_matchActive)
					PutOutMatch();
				else if (TryChangeMatchesCount(-1))
					LightMatch();
			}
		}

		//

		// Create ray from center of the screen
		// TODO cache camera !!
		var ray = PointOfView.GetComponent<Camera>().ViewportPointToRay(Vector3.one * 0.5f);
		RaycastHit hit;
		// Shot ray to find object to pick
		Debug.DrawLine(ray.origin, ray.origin+ray.direction, Color.blue);

		PickUIText.gameObject.SetActive(false);

		if (Physics.Raycast(ray, out hit, 2f))
		{
			Debug.DrawLine(ray.origin, hit.point, Color.yellow);
			// Check if object is pickable
			Matchbox matchbox = hit.transform.GetComponent<Matchbox>();
			// If object has PickableItem class
			if (matchbox)
			{
				PickUIText.gameObject.SetActive(true);
				if (Input.GetKeyDown(KeyCode.E))
					PickMatchbox(matchbox);
			}
		}
	}

	public void PickMatchbox(Matchbox matchbox)
	{
		Destroy(matchbox.gameObject);
		TryChangeMatchesCount(35);
		GatherItemAudio.Play();
	}

	public void MatchesUIUpdate()
	{
		MatchesUIText.text = MatchesCount.ToString();
	}

	public bool TryChangeMatchesCount(int value)
	{
		bool result = true;
		MatchesCount += value;
		if (MatchesCount < 0)
		{
			MatchesCount = 0;
			result = false;
		}
		MatchesUIUpdate();
		return result;
	}

	IEnumerator MatchLifeCoroutine()
	{
		float baseTime = 20f;

		for (float t = baseTime; t >= 0; t -= Time.deltaTime * (1 + Rigidbody.velocity.magnitude/moveSpeed) )
		{
			MatchesProgressBar.transform.localScale = new Vector3(t / baseTime, 1, 1);
			yield return null;
		}
		PutOutMatch(); // переделать на выключение
	}

	public void ToggleMenuActive()
	{
		MenuActive = !MenuActive;
		Menu.gameObject.SetActive(MenuActive);
	}

	Coroutine matchLifeCoroutine;

	public void LightMatch()
	{
		_matchActive = true;
		Match.SetActive(true);
		MatchesProgressBar.SetActive(true);
		matchLifeCoroutine = StartCoroutine(MatchLifeCoroutine());
		MatchLightingAudio.pitch = Random.Range(0.7f, 1.1f);
		MatchLightingAudio.Play();
	}

	public void PutOutMatch()
	{
		_matchActive = false;
		Match.SetActive(_matchActive);
		MatchesProgressBar.SetActive(false);
		StopCoroutine(matchLifeCoroutine);
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