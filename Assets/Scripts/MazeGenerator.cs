using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum direction
{
	up,
	right,
	down,
	left,
	randomOrUnknown
};

public class MazeGenerator : MonoBehaviour
{
	[SerializeField] private PlaytimeSettings _defaultSettings;
	[SerializeField] private PlaytimeSettings _playtimeSettings;

	[SerializeField] private MenuManager _menuManager;

	public GameObject WallPrefab;
	public GameObject CubeWallPrefab;
	public GameObject ExitWallPrefab;
	private Material _curMaterial;
	public Vector3 MazeCenter = new Vector3(40f, 0f, 10f);
	public int MazeSize = 5;

	public Camera MainCamera;
	public Camera PlayerCamera;

	public float CellSize = 3;

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

	public int TorchProbability = 20;
	public int LatticeProbability = 60;

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
	[SerializeField] ToggleGroup _materialsToggleGroup;

	public static int _curMaterialsToggleGroupIndex = 0;

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

	void Awake()
	{
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

		bool generateNewMaze = true;

		_mazeMapList = _generateMaze2(MazeSize);

		//Vector3 _prevPoint = _mazeZeroPoint-new Vector3(10,0,-10);
		if (generateNewMaze)
		{
			// j - горизонталь, i - вертикаль
			for (int i = 0; i < MazeSize; i++)
				for (int j = 0; j < MazeSize; j++)
				{
					if (_mazeMapList[i][j] == 1 || _mazeMapList[i][j] == 2)
					{
						_newWall = Instantiate(CubeWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.identity);
						Walls.Add(_newWall);
					}

				}

		}

		else
		{

			float _mazeZeroPointSingle = (Mathf.Floor(MazeSize / 2f)) * CellSize;
			_mazeZeroPoint = MazeCenter - new Vector3(_mazeZeroPointSingle, 0f, -_mazeZeroPointSingle);

			void RandomizeTorchAndLatticeHere()
			{
				bool torchPlaced = false;
				for (int c = 1; c <= 2; c++)
				{
					if (Random.Range(1, TorchProbability) == 1)
					{
						_newTorch = Instantiate(TorchPrefab, _newWall.transform.GetChild(c).position, _newWall.transform.GetChild(c).rotation);
						_newTorch.SetActive(_tglAddLight.isOn);
						Torches.Add(_newTorch);
						torchPlaced = true;
					}
				}
				if (!torchPlaced)
				{
					if (Random.Range(1, LatticeProbability) == 1)
					{
						_newWall.GetComponent<WallInternal>().ActivateLattice();
					}

				}
			}

			// j - горизонталь, i - вертикаль
			for (int i = 0; i < MazeSize; i++)
				for (int j = 0; j < MazeSize; j++)
				{

					if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 1) != 0)
					{
						_newWall = Instantiate(WallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, -90f, 0f));
						Walls.Add(_newWall);

						RandomizeTorchAndLatticeHere();

					}

					if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 2) != 0)
					{
						_newWall = Instantiate(WallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, 0f, 0f));
						Walls.Add(_newWall);

						RandomizeTorchAndLatticeHere();

					}


					// ставим "выход"
					if (i == 0 && j >= 1 && _mazeMapList[i][j] == 0)
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, 0f, 0f));
					}
					if (i == MazeSize - 1 && j >= 1 && _mazeMapList[i][j] <= 1) // 0 или 1
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, 0f, 0f));
					}
					if (j == 0 && i >= 1 && _mazeMapList[i][j] == 0)
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, -90f, 0f));
					}
					if (j == MazeSize - 1 && i >= 1 && (_mazeMapList[i][j] == 0 || _mazeMapList[i][j] == 2)) // 0 или 2, лень делать бинарное "или", уже и так выше его применял
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, -90f, 0f));
					}
				}

		}

		ApplyRestOfSettings();

		for (int i = 0; i < _matchboxCount; i++)
		{
			Vector2Int matchBoxCellAddress = new Vector2Int(Random.Range(0, MazeSize), Random.Range(0, MazeSize));
			Vector3 matchBoxCoords = PositionByCellAddress(matchBoxCellAddress.x, matchBoxCellAddress.y);
			matchBoxCoords += (Vector3.right * Random.Range(-1f, 1f) +
								Vector3.forward * Random.Range(-1f, 1f)) * CellSize / 2 * 0.9f;
			Instantiate(_matchboxPrefab, matchBoxCoords, Quaternion.Euler(0f, Random.Range(0,360), 0f)); 
		}

	}

	private void Update()
	{
		RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.4f);
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
		DefineCurMaterialsToggleGroupIndex();

		SetDayTime(_curDayTime);
		MainCamera.GetComponent<CameraPosition>().SetCameraPosition(_cameraPosition);
		_menuManager.SetCameraDropdownValue(_cameraPosition);
		MazeSizeText.text = (MazeSize - 1).ToString();
		_sliderLight.value = _curDayTime;
		_sliderHeight.value = _curWallHeight;
		_postEffectsToggle.isOn = _postEffectsOn;
		PostProcessVolume.SetActive(_postEffectsOn);
		_addLightToggle.isOn = _addLightOn;
		_materialToggles[_curMaterialsToggleGroupIndex].isOn = true;
		_clickSound.Stop(); // нужно, чтобы на предыдущих шагах звук не срабатывал
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

	void DefineCurMaterialsToggleGroupIndex()
	{
		for (int i = 0; i < _materialsForToggles.Length; i++)
		{
			if (_materialsForToggles[i] == _curMaterial)
			{
				_curMaterialsToggleGroupIndex = i;
				break;
			}
		}
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
		return _mazeZeroPoint + new Vector3( Mathf.Clamp(x+1,1,MazeSize-1) * CellSize, 0, -Mathf.Clamp(y+1, 1, MazeSize-1) * CellSize);
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

	[SerializeField] Toggle[] _materialToggles = new Toggle[3];
	[SerializeField] Material[] _materialsForToggles = new Material[3];

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

	// // //
	// CATACOMB GENERATION
	// // // 
	private List<List<int>> _generateMaze2(int _mazeSize)
	{

		List<List<int>> _mazeMapList = new List<List<int>>();

		for (int i = 0; i < _mazeSize; i++)
		{
			_mazeMapList.Add(new List<int>());
			for (int j = 0; j < _mazeSize; j++)
			{
				if (i == 0 || i == _mazeSize - 1 || j == 0 || j == _mazeSize - 1) // внешняя граница
					_mazeMapList[i].Add(2);
				else
					_mazeMapList[i].Add(1);
			}
		}

		List<Vector2Int> activePaths = new List<Vector2Int>();

		activePaths.Add(new Vector2Int(_mazeSize / 2, _mazeSize / 2));
		_mazeMapList[activePaths[0].y][activePaths[0].x] = 0;
		//bool 

		int testCnt = 0;
		Vector2Int previousP = new Vector2Int(); // test
		while (activePaths.Count > 0 && testCnt < 1000)
		{
			testCnt++;
			foreach (Vector2Int P in activePaths.ToList()) // ToList - для того, чтобы удалять внутри foreach
			{
				//_mazeMapList[P.y][P.x] = 0;
				Vector2Int? nextCell = StepFromExistingCell(P);
				if (nextCell == null)
					activePaths.Remove(P);
				else
				{
					if (testCnt == 3)
						previousP = P;

					activePaths[activePaths.IndexOf(P)] = (Vector2Int)nextCell;
					_mazeMapList[((Vector2Int)nextCell).y][((Vector2Int)nextCell).x] = 0;

					if (testCnt == 3)
						activePaths.Add(previousP);
				}

			}
		}
		if (testCnt >= 1000)
			Debug.LogError("Зациклились, абортнулись");

		//StepAtPathContinuous(_mazeSize/2, _mazeSize/2);


		/*string dbgStr = "";
		for (int i = 0; i < _mazeSize; i++)
		{
			dbgStr = "";
			for (int j = 0; j < _mazeSize; j++)
				dbgStr += _mazeMapList[i][j] + " ";
			Debug.Log(dbgStr);
		};*/


		return _mazeMapList;


		//////////////
		Vector2Int? StepFromExistingCell(Vector2Int currentCell) //, direction direction = direction.random)
		{
			List<direction> directionsAvailable = new List<direction>();

			for (int i = 0; i < 4; i++)
			{
				direction direction = (direction)i;
				if (CheckCellForPath(GetNeighbourCell(currentCell, direction), direction))
					directionsAvailable.Add(direction);
			}

			direction furtherDirection = direction.randomOrUnknown;

			if (directionsAvailable.Count == 0)
				return null;
			else if (directionsAvailable.Count == 1)
				furtherDirection = directionsAvailable[0];
			else if (directionsAvailable.Count >= 2)
				furtherDirection = directionsAvailable[Random.Range(0, directionsAvailable.Count)];

			Vector2Int newCell = currentCell;

			switch (furtherDirection)
			{
				case direction.left: newCell.x--; break;
				case direction.up: newCell.y--; break;
				case direction.right: newCell.x++; break;
				case direction.down: newCell.y++; break;
			}

			return newCell;
		}

		void CreatePathFromCell(int y, int x, direction direction = direction.randomOrUnknown)
		{
			if (direction == direction.randomOrUnknown)
				direction = (direction)Random.Range(0, 4);
			//Debug.Log(direction);

			if (_mazeMapList[y][x] == 2)
				Debug.LogError("I am on \"2\", don't want to! y = " + y + ", x = " + x);
			_mazeMapList[y][x] = 0;

			bool goFurther = true;
			///
			Vector2Int currentCell = new Vector2Int(x, y);
			List<direction> directionsAvailable = new List<direction>();

			if (CheckCellForPath(GetNeighbourCell(currentCell, directionToLeft(direction)), directionToLeft(direction)))
				directionsAvailable.Add(directionToLeft(direction));
			if (CheckCellForPath(GetNeighbourCell(currentCell, direction), direction))
				directionsAvailable.Add(direction);
			if (CheckCellForPath(GetNeighbourCell(currentCell, directionToRight(direction)), directionToRight(direction)))
				directionsAvailable.Add(directionToRight(direction));

			Debug.Log("At y:" + y + " x:" + x + ", direction: " + direction + "| directionsAvailable:");
			foreach (direction D in directionsAvailable)
				Debug.Log(D);

			direction furtherDirection = direction;

			if (directionsAvailable.Count == 0)
				return;
			else if (directionsAvailable.Count == 1)
				furtherDirection = directionsAvailable[0];
			else if (directionsAvailable.Count >= 2)
				furtherDirection = directionsAvailable[Random.Range(0, directionsAvailable.Count)];

			Debug.Log("furtherDirection: " + furtherDirection);
			//int furtherBase = Random.Range(-1,/*2*/3);
			/*switch (furtherBase)
			{
				case -1:
					furtherDirection = directionToLeft(direction);
					break;
				case 0:
					//furtherDirection = direction;
					break;
				case 1:
					furtherDirection = directionToRight(direction);
					break;
				//for test
				default:
					goFurther = false;
					break;
			}*/


			switch (furtherDirection)
			{
				case direction.left: x--; break;
				case direction.up: y--; break;
				case direction.right: x++; break;
				case direction.down: y++; break;
			}

			if (goFurther) CreatePathFromCell(y, x, furtherDirection);

		}

		bool CheckCellForPath(Vector2Int cell, direction direction)
		{
			try
			{
				if (_mazeMapList[cell.y][cell.x] == 2)
				{
					Debug.Log("Detected \"2\": y = " + cell.y + ", x = " + cell.x);
					return false;
				}

			}
			catch
			{
				Debug.LogError("Exception with coords: y = " + cell.y + ", x = " + cell.x);
				return false;
			}

			Vector2Int cellToCheck = new Vector2Int();
			cellToCheck = GetNeighbourCell(cell, directionToLeft(direction));
			if (_mazeMapList[cellToCheck.y][cellToCheck.x] == 0) return false;
			cellToCheck = GetNeighbourCell(cell, direction);
			if (_mazeMapList[cellToCheck.y][cellToCheck.x] == 0) return false;
			cellToCheck = GetNeighbourCell(cell, directionToRight(direction));
			if (_mazeMapList[cellToCheck.y][cellToCheck.x] == 0) return false;

			return true;
		};

		Vector2Int GetNeighbourCell(Vector2Int cell, direction direction)
		{
			return new Vector2Int(
				cell.x + (direction == direction.left ? -1 :
							direction == direction.right ? 1
															: 0),
				cell.y + (direction == direction.up ? -1 :
							direction == direction.down ? 1
															: 0)
															);
		}

		//Vector2Int cell = new Vector2Int();

		direction directionToRight(direction direction)
		{
			direction directionToRight = direction + 1;
			if ((int)directionToRight > 3)
				directionToRight = 0;
			return directionToRight;
		}
		direction directionToLeft(direction direction)
		{
			direction directionToLeft = direction - 1;
			if ((int)directionToLeft < 0)
				directionToLeft = (direction)3;
			return directionToLeft;
		}

	}



	private void OnDestroy()
	{
		SavePlaytimeSettings();
	}

}

