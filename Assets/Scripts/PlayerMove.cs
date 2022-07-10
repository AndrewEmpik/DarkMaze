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
	private Camera _playerCamera;
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
	[SerializeField] GameObject MatchInHandObject;
	[SerializeField] Transform BurntPartOfMatch;
	[SerializeField] Transform MatchFlame;
	[SerializeField] GameObject BurnedMatchPrefab;
	[SerializeField] Light Flashlight;
	public int MatchesCount = 10;


	[SerializeField] private List<float> _playerRotationsList = new List<float>();
	[SerializeField] private int _playerRotationsListLength = 30;

	private bool _hasFlashlight = false;
	[SerializeField] TMP_Text _flashLightUI;

	private void Start()
	{
		for (int i = 0; i < _playerRotationsListLength; i++)
			_playerRotationsList.Add(0f);

		MatchesUIUpdate();
		MatchesProgressBar.transform.localScale = new Vector3(0, 1, 1);
		FootstepsAudio.Play();
		FootstepsAudio.Pause();
		//FootstepsAudio.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 1f);
		_playerCamera = PointOfView.GetComponent<Camera>();
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
				else if (!MatchInHandObject.activeInHierarchy && TryChangeMatchesCount(-1))
					LightMatch();
			}

			if (Input.GetKeyDown(KeyCode.L))
			{
				if (_hasFlashlight)
					ToggleFlashlight();
			}
		}

		//

		// Create ray from center of the screen
		var ray = _playerCamera.ViewportPointToRay(Vector3.one * 0.5f);
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

			if (!_hasFlashlight)
			{
				Flashlight flashlight = hit.transform.GetComponent<Flashlight>();
				if (flashlight)
				{
					PickUIText.gameObject.SetActive(true);
					if (Input.GetKeyDown(KeyCode.E))
						PickFlashlight(flashlight);
				}
			}
		}

		if (Input.GetMouseButton(1))
			_playerCamera.fieldOfView = 30;
		else
			_playerCamera.fieldOfView = 60;
	}

	public void PickMatchbox(Matchbox matchbox)
	{
		Destroy(matchbox.gameObject);
		TryChangeMatchesCount(35);
		GatherItemAudio.Play();
	}
	public void PickFlashlight(Flashlight flashlight)
	{
		_hasFlashlight = true;
		Destroy(flashlight.gameObject);
		GatherItemAudio.Play();
		ToggleFlashlight();
		_flashLightUI.gameObject.SetActive(true);
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
		Vector3 FlameTopPosition = new Vector3(0.133499995f, -0.1180999546f, 0.362199992f);
		Vector3 FlameBottomPosition = new Vector3(0.164199993f, -0.2053f, 0.338400006f);

		for (float t = baseTime; t >= 0; t -= Time.deltaTime * (1 + Rigidbody.velocity.magnitude/moveSpeed) )
		{
			MatchesProgressBar.transform.localScale = new Vector3(t / baseTime, 1, 1);
			BurntPartOfMatch.localScale = new Vector3(1, 0.31f * ((baseTime-t) / baseTime), 1);
			MatchFlame.localPosition = new Vector3(	Mathf.Lerp(FlameTopPosition.x, FlameBottomPosition.x, (baseTime - t) / baseTime),
													Mathf.Lerp(FlameTopPosition.y, FlameBottomPosition.y, (baseTime - t) / baseTime),
													Mathf.Lerp(FlameTopPosition.z, FlameBottomPosition.z, (baseTime - t) / baseTime));
			yield return null;
		}
		PutOutMatch(); // переделать на выключение
	}

	IEnumerator RaiseInHandsCoroutine(bool boolValue)
	{
		float baseTime = 0.2f;
		float yMax = 0f;
		float yMin = -0.23f;

		if (boolValue)
			MatchInHandObject.SetActive(true);

		for (float t = 0; t <= baseTime; t += Time.deltaTime)
		{
			MatchInHandObject.transform.localPosition = new Vector3(0,Mathf.Lerp(boolValue ? yMin : yMax, boolValue ? yMax : yMin, t/baseTime),0);
			yield return null;
		}

		if (!boolValue)
		{
			MatchInHandObject.SetActive(false);
			Instantiate(BurnedMatchPrefab, transform.localPosition, Quaternion.Euler(0f,Random.Range(0,360),0f));
		}
	}

	public void ToggleMenuActive()
	{
		MenuActive = !MenuActive;
		Menu.gameObject.SetActive(MenuActive);
	}

	public void ToggleFlashlight()
	{
		Flashlight.gameObject.SetActive(!Flashlight.gameObject.activeInHierarchy);
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
		StartCoroutine(RaiseInHandsCoroutine(true));
		//MatchInHandObject.SetActive(true);
	}

	public void PutOutMatch()
	{
		_matchActive = false;
		Match.SetActive(_matchActive);
		MatchesProgressBar.SetActive(false);
		StopCoroutine(matchLifeCoroutine);
		StartCoroutine(RaiseInHandsCoroutine(false));
		MatchLightingAudio.Stop(); // актуально, если очень быстро погасить после зажигания
		//MatchInHandObject.SetActive(false);
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

	float rotationImpulse = 0f;

	void FixedUpdate()
	{
		if (!MenuActive)
			KeyboardMovement();
		else
		{
			Rigidbody.velocity = Vector3.zero;
			FootstepsAudio.Pause();
		}

		/*float y = Input.GetAxis("Mouse X") * turnSpeed;
		_playerRotationsList.Add(y);
		_playerRotationsList.RemoveAt(0);
		float sumY = 0f;
		foreach (float s in _playerRotationsList)
			sumY += s;
		for (int i = 0; i < _playerRotationsListLength; i++)
		{
			sumY += _playerRotationsList[i]* (((float)i)/_playerRotationsListLength);
		}

		rotationImpulse = sumY / _playerRotationsListLength;*/

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
		
		//Debug.Log(rotX);
		float _sin = Mathf.Sin(Mathf.Abs(rotX) * Mathf.Deg2Rad);
		float angleCoef = 0.585f * _sin * _sin;

		MatchFlame.transform.localScale = new Vector3(1,1 - angleCoef /*(90+45)*/ ,1);

		_playerRotationsList.Add(y);
		_playerRotationsList.RemoveAt(0);
		float sumY = 0f;
		foreach (float s in _playerRotationsList)
			sumY += s;
		float rotationImpulse = sumY / _playerRotationsListLength;

		MatchFlame.localEulerAngles = new Vector3(0f, 0f, rotationImpulse*4);

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