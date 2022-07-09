using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
	[SerializeField] private PlaytimeSettings _defaultSettings;
	[SerializeField] private PlaytimeSettings _playtimeSettings;

	[SerializeField] private MenuManager _menuManager;

	public GameObject WallPrefab;
	public GameObject ExitWallPrefab;
	private Material _curMaterial;
	public Vector3 MazeCenter = new Vector3(40f, 0f, 10f);
	public int MazeSize = 5;

	public Camera MainCamera;
	public Camera PlayerCamera;

	private float _cellSize = 3;

	public List<GameObject> Walls;
	private GameObject _newWall;
	private GameObject _exitWall;

	public Text MazeSizeText;

	private float _curWallHeight = 1f;
	private int _curTorchType = 3;
	private float _curDayTime = 0.05f;

	[SerializeField]
	private GameObject _globalLight;

	[SerializeField]
	private Toggle _tglAddLight;

	public List<GameObject> Torches;
	public GameObject TorchPrefab;
	private GameObject _newTorch;

	public int TorchProbability = 50;

	private Vector3 _mazeZeroPoint = Vector3.zero;

	[SerializeField]
	private Matchbox _matchboxPrefab;
	[SerializeField]
	private int _matchboxCount = 10;

	[SerializeField] GameObject PostProcessVolume;
	[SerializeField] GameObject MenuCanvas;

	private bool _postEffectsOn = true;
	private bool _addLightOn = true;
	[SerializeField] Toggle _postEffectsToggle;
	[SerializeField] Toggle _addLightToggle;
	[SerializeField] Dropdown _torchTypeDropdown;
	[SerializeField] AudioSource _clickSound;

	[SerializeField] Slider _sliderLight;
	[SerializeField] Slider _sliderHeight;

	int _cameraPosition = 0;

	public enum PinnedPosition
	{
		Center,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		HalfTopLeft,
		HalfTopRight,
		HalfBottomLeft,
		HalfBottomRight,
		Exit
	}

	void Start()
	{
		PostProcessVolume.SetActive(true);
		if (MenuManager.FirstLoad)
		{
			ApplySettings(_defaultSettings);

			MenuCanvas.SetActive(true);
			MenuManager.FirstLoad = false;
		}
		else
		{
			ApplySettings(_playtimeSettings);
		}

		List<List<int>> _mazeMapList;

		_mazeMapList = _generateMaze(MazeSize);

		float _mazeZeroPointSingle = (Mathf.Floor(MazeSize / 2f)) * _cellSize;
		_mazeZeroPoint = MazeCenter - new Vector3(_mazeZeroPointSingle, 0f, -_mazeZeroPointSingle);

		// j - горизонталь, i - вертикаль
		for (int i = 0; i < MazeSize; i++)
			for (int j = 0; j < MazeSize; j++)
			{

				if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 1) != 0)
				{
					_newWall = Instantiate(WallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, -90f, 0f));
					Walls.Add(_newWall);

					for (int c = 1; c <= 2; c++)
					{
						if (Random.Range(1, TorchProbability) == 1)
						{
							_newTorch = Instantiate(TorchPrefab, _newWall.transform.GetChild(c).position, _newWall.transform.GetChild(c).rotation);
							_newTorch.SetActive(_tglAddLight.isOn);
							Torches.Add(_newTorch);
						}
					}

				}

				if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 2) != 0)
				{
					_newWall = Instantiate(WallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
					Walls.Add(_newWall);

					for (int c = 1; c <= 2; c++)
					{
						if (Random.Range(1, TorchProbability) == 1)
						{
							_newTorch = Instantiate(TorchPrefab, _newWall.transform.GetChild(c).position, _newWall.transform.GetChild(c).rotation);
							_newTorch.SetActive(_tglAddLight.isOn);
							Torches.Add(_newTorch);
						}
					}

				}


				// ставим "выход"
				if (i == 0 && j >= 1 && _mazeMapList[i][j] == 0)
				{
					_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
				}
				if (i == MazeSize-1 && j >= 1 && _mazeMapList[i][j] <= 1) // 0 или 1
				{
					_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
				}
				if (j == 0 && i >= 1 && _mazeMapList[i][j] == 0)
				{
					_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, -90f, 0f));
				}
				if (j == MazeSize-1 && i >= 1 && (_mazeMapList[i][j] == 0 || _mazeMapList[i][j] == 2)) // 0 или 2, лень делать бинарное "или", уже и так выше его применял
				{
					_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, -90f, 0f));
				}
			}

		ApplyRestOfSettings();

		for (int i = 0; i < _matchboxCount; i++)
		{
			Vector2Int matchBoxCellAddress = new Vector2Int(Random.Range(0, MazeSize), Random.Range(0, MazeSize));
			Vector3 matchBoxCoords = PositionByCellAddress(matchBoxCellAddress.x, matchBoxCellAddress.y);
			matchBoxCoords += (Vector3.right * Random.Range(-1f, 1f) +
								Vector3.forward * Random.Range(-1f, 1f)) * _cellSize / 2 * 0.9f;
			Instantiate(_matchboxPrefab, matchBoxCoords, Quaternion.Euler(0f, Random.Range(0,360), 0f)); 
		}

	}

	public void ApplySettings(PlaytimeSettings settings)
	{
		MazeSize = settings.MazeSize;
		_curWallHeight = settings.WallHeight;
		_curDayTime = settings.LightIntensity;
		_addLightOn = settings.AdditionLightOn;
		_curTorchType = settings.TypeOfAddLight;
		_cameraPosition = settings.CameraPosition;
		_postEffectsOn = settings.PostEffectsOn;
		_curMaterial = settings.WallMaterial;

		SetDayTime(_curDayTime);
		MainCamera.GetComponent<CameraPosition>().SetCameraPosition(_cameraPosition);
		_menuManager.SetCameraDropdownValue(_cameraPosition);
		MazeSizeText.text = (MazeSize - 1).ToString();
		_sliderLight.value = _curDayTime;
		_sliderHeight.value = _curWallHeight;
		_postEffectsToggle.isOn = _postEffectsOn;
		_addLightToggle.isOn = _addLightOn;
		_clickSound.Stop(); // нужно, чтобы на предыдущих шагах звук не срабатывал
		//PostProcessVolume.gameObject.SetActive(_postEffectsOn);
	}

	public void ApplyRestOfSettings()
	{
		SetWallsHeight(_curWallHeight);
		_torchTypeDropdown.value = _curTorchType;

		if (_curMaterial != null)
			SetMaterial(_curMaterial);
	}

	public void SavePlaytimeSettings()
	{
		_playtimeSettings.MazeSize = MazeSize;
		_playtimeSettings.WallHeight = _curWallHeight;
		_playtimeSettings.LightIntensity = _curDayTime;
		_playtimeSettings.AdditionLightOn = _addLightOn;
		_playtimeSettings.TypeOfAddLight = _curTorchType;
		_playtimeSettings.CameraPosition = _cameraPosition;
		_playtimeSettings.PostEffectsOn = _postEffectsOn;
		_playtimeSettings.WallMaterial = _curMaterial;
	}

	public Vector3 PositionByCellAddress(PinnedPosition value)
	{
		switch (value)
		{
			case PinnedPosition.Center:
				return Vector3.zero;
			case PinnedPosition.TopLeft:
				return PositionByCellAddress(0, 0);
			case PinnedPosition.TopRight:
				return PositionByCellAddress(MazeSize, 0);
			case PinnedPosition.BottomLeft:
				return PositionByCellAddress(0, MazeSize);
			case PinnedPosition.BottomRight:
				return PositionByCellAddress(MazeSize, MazeSize);
			case PinnedPosition.HalfTopLeft:
				return PositionByCellAddress(MazeSize / 4, MazeSize / 4);
			case PinnedPosition.HalfTopRight:
				return PositionByCellAddress(MazeSize * 3/4, MazeSize / 4);
			case PinnedPosition.HalfBottomLeft:
				return PositionByCellAddress(MazeSize / 4, MazeSize * 3/4);
			case PinnedPosition.HalfBottomRight:
				return PositionByCellAddress(MazeSize * 3/4, MazeSize * 3/4);
			case PinnedPosition.Exit:
			// TODO
				return PositionByCellAddress(0, 0);
			default:
				return Vector3.zero;
		}
	}

	public Vector3 PositionByCellAddress(int x,int y)
	{
		return _mazeZeroPoint + new Vector3( Mathf.Clamp(x+1,1,MazeSize-1) * _cellSize, 0, -Mathf.Clamp(y+1, 1, MazeSize-1) * _cellSize);
	}

	public void RebuildMaze()
	{
		//_playtimeSettings.CheckString = "Changed " + MazeSize.ToString();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);

	}

	public void ToggleTorch(bool val)
	{
		foreach (GameObject T in Torches)
			T.SetActive(val);
		_addLightOn = val;
	}
	public void TogglePostEffectsValue(bool val)
	{
		_postEffectsOn = val;
	}
	public void SetCameraPositionValue(int value)
	{
		_cameraPosition = value;
	}

	public void SetWallsHeight(float val)
	{
		foreach (GameObject W in Walls)
			W.transform.localScale = new Vector3(W.transform.localScale.x, val, W.transform.localScale.z);
		if (_exitWall)
			_exitWall.transform.localScale = new Vector3(_exitWall.transform.localScale.x, val, _exitWall.transform.localScale.z);

		foreach (GameObject T in Torches)
		{
			try
			{
				T.transform.position = new Vector3(T.transform.position.x, Walls[0].transform.GetChild(1).position.y, T.transform.position.z);
			}
			catch
			{
				Debug.LogWarning("Баг с факелом! Надо исправить!");
			}
		}
		
		_curWallHeight = val;

		_playtimeSettings.WallHeight = _curWallHeight;
	}

	public void SetMaterial(Material material)
	{
		_curMaterial = material;
		Renderer rend;
		foreach (GameObject W in Walls)
		{
			//Debug.Log(W.);
			rend = W.transform.GetChild(0).GetComponent<Renderer>();
			rend.material = material;
		}
	}

	public void SetTorchType(int index)
	{
		switch (index)
		{
			case 0: // обычная лампа
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
				}
				break;
			case 1: // красный фонарик
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
				}
				break;
			case 2: // синий фонарик
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
				}
				break;
			case 3: // огонь
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 3; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(4).gameObject.SetActive(true);
				};
				break;
			case 4: // RANDOM
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(Random.Range(1, 5)).gameObject.SetActive(true);
				};
				break;
		}
		_curTorchType = index;
	}

	public void SetDayTime(float val)
	{
		_globalLight.transform.rotation = Quaternion.Euler(Mathf.Lerp(-30, 50, val), -30, 0f);

		if (Mathf.Lerp(-30, 50, val) < -5f)
			_globalLight.SetActive(false);
		else
			_globalLight.SetActive(true);
		//_globalLight.GetComponent<Light>().intensity = val;

		RenderSettings.ambientIntensity = val;
		RenderSettings.reflectionIntensity = val;

		_curDayTime = val;
	}

	public void ChangeSize(int val)
	{
		int res = MazeSize + val;
		res = Mathf.Clamp(res, 2, 51);
		MazeSize = res;
		MazeSizeText.text = (res - 1).ToString();
	}


	private int manageWall(int where, int which, bool toAdd = true) // 1 пр, 2 лев, 3 всё
	{
		if (where < 0 || where > 3) where = 0;
		return (toAdd ? where | which : where & ~which);
	}

	private List<List<int>> _generateMaze(int _mazeSize)
	{
		List<List<int>> _mazeMapList = new List<List<int>>();

		for (int i = 0; i < _mazeSize; i++)
		{
			_mazeMapList.Add(new List<int>());
			for (int j = 0; j < _mazeSize; j++)
			{
				_mazeMapList[i].Add(-1);
			}
		}

		// границы
		_mazeMapList[0][0] = 0;
		for (int i = 1; i < _mazeSize; i++)
		{
			_mazeMapList[i][0] = 1;
			_mazeMapList[i][_mazeSize - 1] = 1;
		}
		for (int j = 1; j < _mazeSize; j++)
		{
			_mazeMapList[0][j] = 2;
			_mazeMapList[_mazeSize - 1][j] = 2;
		}
		_mazeMapList[_mazeSize - 1][_mazeSize - 1] = 3;

		// непосредственно генерация при помощи алгоритма Эллера
		List<int> rowSets = new List<int>();
		int curCnt = 0;
		bool curHasExit = false;
		for (int i = 0; i < _mazeSize; i++) rowSets.Add(i); // создание множеств
		for (int i = 1; i < _mazeSize - 1; i++)
		{ // создание стенок справа
			if (Random.Range(0, 2) == 1)
				_mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 1, true);
			else
				rowSets[i + 1] = rowSets[i];
		};
		for (int i = 1; i < _mazeSize; i++)
		{ // создание стенок снизу
			if (rowSets[i] != rowSets[i - 1])
			{
				if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
					continue;

				curCnt = 0;
				curHasExit = false;
			}
			curCnt++;

			if (!curHasExit && (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1]))
				continue;

			if (Random.Range(0, 2) == 1)
				_mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 2, true);
			else
				curHasExit = true;
		};

		// следующие строки
		for (int r = 2; r < _mazeSize; r++)
		{

			for (int i = 1; i < _mazeSize; i++)
				if (_mazeMapList[r - 1][i] == 2 || _mazeMapList[r - 1][i] == 3)
					rowSets[i] = i + _mazeSize * r; // присваиваем уникальное множество тем ячейкам, у которых была граница снизу

			for (int i = 1; i < _mazeSize - 1; i++)
			{ // создание стенок справа
				if (rowSets[i] == rowSets[i + 1] || Random.Range(0, 2) == 1)
					_mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 1, true);
				else
					rowSets[i + 1] = rowSets[i];
			};

			for (int i = 1; i < _mazeSize; i++)
			{ // создание стенок снизу
				if (rowSets[i] != rowSets[i - 1])
				{
					if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
					{
						//Debug.Log("Не строим стенку для rowSets[" + i + "], = " + rowSets[i]);
						continue;
					}

					curCnt = 0;
					curHasExit = false;
				}
				curCnt++;

				if (!curHasExit && (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1]))
					continue;

				if (Random.Range(0, 4) > 1)// || curHasExit) 
					_mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 2, true);
				else
					curHasExit = true;
			};
		};

		// последняя строка
		bool allTheSameSet = false;
		while (!allTheSameSet)
		{
			for (int i = 1; i < _mazeSize - 1; i++)
			{
				if (rowSets[i + 1] != rowSets[i])
				{
					_mazeMapList[_mazeSize - 1][i] = manageWall(_mazeMapList[_mazeSize - 1][i], 1, false);
					//int s;
					int formerSet = rowSets[i + 1];
					//while (rowSets[i + 2 + s] == rowSets[i];)
					rowSets[i + 1] = rowSets[i];
					for (int s = i + 1; s < _mazeSize - 1; s++)
						if (rowSets[s] == formerSet)
							rowSets[s] = rowSets[i];
				}
			}

			allTheSameSet = true; // *пока не доказано обратное
			for (int i = 1; i < _mazeSize - 1; i++)
			{
				if (rowSets[i + 1] != rowSets[i])
					allTheSameSet = false;
			}
		}

		// выход
		int exitAt = Random.Range(0, (_mazeSize - 1) * 4);
		int exitAtOffset = (exitAt % (_mazeSize - 1)) + 1;
		switch (exitAt / (_mazeSize - 1))
		{
			case 0:
				_mazeMapList[0][exitAtOffset] = 0;
				break;
			case 1:
				_mazeMapList[exitAtOffset][_mazeSize - 1] =
					manageWall(_mazeMapList[exitAtOffset][_mazeSize - 1], 1, false);
				// _mazeMapList[exitAtOffset][_mazeSize - 1].intManageWall(1, false);
				break;
			case 2:
				_mazeMapList[_mazeSize - 1][exitAtOffset] =
					manageWall(_mazeMapList[_mazeSize - 1][exitAtOffset], 2, false);
				// _mazeMapList[_mazeSize - 1][exitAtOffset].intManageWall(2, false);
				break;
			case 3:
				_mazeMapList[exitAtOffset][0] = 0;
				break;
		}

		return _mazeMapList;
	}

	private void OnDestroy()
	{
		SavePlaytimeSettings();
	}

}

