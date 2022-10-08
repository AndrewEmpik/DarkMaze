using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <remarks>
/// WARNING!
/// This is an extremely large file, 
/// it is strongly needed to divide it by several particular scripts in future.
/// So:
/// TODO rewrite this file!
/// </remarks>

public enum direction
{
	up,
	right,
	down,
	left,
	randomOrUnknown
};

public enum LevelType
{
	ClassicMaze,
	CatacombMaze,
	DemoLevel,
	Other
}

public struct NavigateCell
{
	public Vector2Int cell;
	public direction direction;
	//bool mainDirection;

	public NavigateCell (Vector2Int cell, direction direction)
	{
		this.cell = cell;
		this.direction = direction;
	}

	public NavigateCell turnRight()
	{
		NavigateCell navCell = this;
		navCell.direction = navCell.direction.directionToRight();
		return navCell;
	}
	public NavigateCell turnLeft()
	{
		NavigateCell navCell = this;
		navCell.direction = navCell.direction.directionToLeft();
		return navCell;
	}
}

static class Extensions
{
	public static direction directionToRight(this direction direction)
	{
		if (direction == direction.randomOrUnknown)
			return direction.randomOrUnknown;
		direction directionToRight = direction + 1;
		if ((int)directionToRight > 3)
			directionToRight = 0;
		return directionToRight;
	}

	public static direction directionToLeft(this direction direction)
	{
		if (direction == direction.randomOrUnknown)
			return direction.randomOrUnknown;
		direction directionToLeft = direction - 1;
		if ((int)directionToLeft < 0)
			directionToLeft = (direction)3;
		return directionToLeft;
	}

	public static Vector3 toVector3(this direction direction)
	{
		switch (direction)
		{
			case direction.up:
				return Vector3.forward;
			case direction.right:
				return Vector3.right;
			case direction.down:
				return Vector3.back;
			case direction.left:
				return Vector3.left;
		}
		return Vector3.zero;
	}

	public static float toQuatornianDegrees(this direction direction)
	{
		switch (direction)
		{
			case direction.up:
				return 0f;
			case direction.right:
				return 90f;
			case direction.down:
				return 180f;
			case direction.left:
				return -90f;
		}
		return 0f;
	}
}


public class MazeGenerator : MonoBehaviour
{
	[SerializeField] private PlaytimeSettings _defaultSettings;
	[SerializeField] private PlaytimeSettings _playtimeSettings;

	[SerializeField] private MenuManager _menuManager;

	public LevelType LevelType;

	public GameObject WallPrefab;
	public GameObject CubeWallPrefab;
	public GameObject ExitWallPrefab;
	public GameObject LatticePrefab;
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
	private bool _reflectionsOn = false;
	private bool _crosshairOn = true;
	private bool _addLightOn = true;
	[SerializeField] Toggle _postEffectsToggle;
	[SerializeField] Toggle _reflectionsToggle;
	[SerializeField] Toggle _crosshairToggle;
	[SerializeField] Toggle _addLightToggle;
	[SerializeField] Dropdown _torchTypeDropdown;
	[SerializeField] AudioSource _clickSound;

	[SerializeField] Slider _sliderLight;
	[SerializeField] Slider _sliderHeight;

	int _cameraPosition = 0;
	[SerializeField] ToggleGroup _materialsToggleGroup;

	[SerializeField] Material _darkSkybox;
	[SerializeField] Material _defaultSkybox;

	[SerializeField] GameObject _additionalCeilingForWEBGL;

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
			RestoreDefaultSettings();

			ApplySettings(_defaultSettings);

			MenuCanvas.SetActive(true);
			MenuManager.FirstLoad = false;
		}
		else
		{
			ApplySettings(_playtimeSettings);
		}

		List<List<int>> _mazeMapList;

		//Vector3 _prevPoint = _mazeZeroPoint-new Vector3(10,0,-10);

		float _mazeZeroPointSingle = (Mathf.Floor(MazeSize / 2f)) * CellSize;
		_mazeZeroPoint = MazeCenter - new Vector3(_mazeZeroPointSingle, 0f, -_mazeZeroPointSingle);

		if (LevelType == LevelType.CatacombMaze)
		{
			_mazeMapList = null;
			while (_mazeMapList == null)
				_mazeMapList = _generateCatacombMaze(MazeSize);

			// j - horizontal, i - vertical
			for (int i = 0; i < MazeSize; i++)
				for (int j = 0; j < MazeSize; j++)
				{
					switch (_mazeMapList[i][j])
					{
						case (int)CatacombMazeMap.Legend.Wall:
						case (int)CatacombMazeMap.Legend.EdgeOfTheMaze:

							_newWall = Instantiate(CubeWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.identity);
							Walls.Add(_newWall);
							break;

						case (int)CatacombMazeMap.Legend.ExitHorizontal:

							Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, (-i+0.5f) * CellSize),
								Quaternion.Euler(0f, 0f, 0f));
							break;

						case (int)CatacombMazeMap.Legend.ExitVertical:

							Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3((j - 0.5f) * CellSize, 0, -i * CellSize),
								Quaternion.Euler(0f, -90f, 0f));
							break;

						case (int)CatacombMazeMap.Legend.LatticeHorizontal:

							Instantiate(LatticePrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize),
								Quaternion.Euler(0f, 0f, 0f));
							break;

						case (int)CatacombMazeMap.Legend.LatticeVertical:

							Instantiate(LatticePrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize),
								Quaternion.Euler(0f, -90f, 0f));
							break;
					}
				}

			// placing torches
			List<Vector2Int> freeCells = catacombMazeMap.GetFreeCells;
			// first one we place in the start point
			try
			{
				PlaceTorchInCell(PathProcess.allPathProcesses[0].PathOrigin);
			}
			catch (System.ArgumentOutOfRangeException e)
			{
				Debug.LogError("Failed PlaceTorchInCell (in start): " + e.ToString() 
					+ "; " + e.ActualValue 
					+ "; allPathProcesses.Count = " 
					+ PathProcess.allPathProcesses.Count 
					+ "; PathProcess.allPathProcesses[0].PathOrigin = " + PathProcess.allPathProcesses[0].PathOrigin.ToString());
			}
			freeCells.Remove(PathProcess.allPathProcesses[0].PathOrigin);

			for (int i = 0; i < 75; i++)
			{
				if (freeCells.Count > 0)
				{
					int rnd = Random.Range(0, freeCells.Count);
					try
					{
						PlaceTorchInCell(freeCells[rnd]);
					}
					catch (System.ArgumentOutOfRangeException e)
					{
						Debug.LogError("Failed PlaceTorchInCell: " + e.ToString() + "; " + e.ActualValue + "; freeCells.Count = " + freeCells.Count + "; rnd (index) = " + rnd);
					}
					freeCells.RemoveAt(rnd);
				}
				else break;
			}

			List<Vector2Int> cellsForMatchBox = catacombMazeMap.GetFreeCells;
			for (int i = 0; i < _matchboxCount; i++)
			{
				if (cellsForMatchBox.Count > 0)
				{
					try
					{
						Vector2Int matchBoxCellAddress = cellsForMatchBox[Random.Range(0, cellsForMatchBox.Count)];
						cellsForMatchBox.Remove(matchBoxCellAddress);
						Vector3 matchBoxCoords = PositionByCellAddress(matchBoxCellAddress);
						matchBoxCoords += (Vector3.right * Random.Range(-1f, 1f) +
											Vector3.forward * Random.Range(-1f, 1f)) * CellSize / 2 * 0.9f;
						Instantiate(_matchboxPrefab, matchBoxCoords, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
					}
					catch (System.ArgumentOutOfRangeException e)
					{
						Debug.LogError("Failed cellsForMatchBox: " + e.ToString() + "; " + e.ActualValue + "; cellsForMatchBox.Count = " + cellsForMatchBox.Count);
					}
				}
				else break;

			}
		}

		else if (LevelType == LevelType.ClassicMaze)
		{
			_mazeMapList = _generateClassicMaze(MazeSize);

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

			// j - horizontal, i - vertical
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


					// placing "exit"
					if (i == 0 && j >= 1 && _mazeMapList[i][j] == 0)
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, 0f, 0f));
					}
					if (i == MazeSize - 1 && j >= 1 && _mazeMapList[i][j] <= 1) // 0 or 1
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, 0f, 0f));
					}
					if (j == 0 && i >= 1 && _mazeMapList[i][j] == 0)
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, -90f, 0f));
					}
					if (j == MazeSize - 1 && i >= 1 && (_mazeMapList[i][j] == 0 || _mazeMapList[i][j] == 2)) // 0 or 2, don't want to do binary "or" here, made it upwards already
					{
						_exitWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * CellSize, 0, -i * CellSize), Quaternion.Euler(0f, -90f, 0f));
					}
				}

			for (int i = 0; i < _matchboxCount; i++)
			{
				Vector2Int matchBoxCellAddress = new Vector2Int(Random.Range(0, MazeSize), Random.Range(0, MazeSize));
				Vector3 matchBoxCoords = PositionByCellAddress(matchBoxCellAddress);
				matchBoxCoords += (Vector3.right * Random.Range(-1f, 1f) +
									Vector3.forward * Random.Range(-1f, 1f)) * CellSize / 2 * 0.9f;
				Instantiate(_matchboxPrefab, matchBoxCoords, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
			}

		}

		ApplyRestOfSettings();

#if UNITY_WEBGL
		if (LevelType == LevelType.CatacombMaze)
		// a workaround for fixing lighting issues
			_additionalCeilingForWEBGL.SetActive(true);
#endif

	}

	private void Update()
	{
		RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.4f);
	}

	public void ApplySettings(PlaytimeSettings settings)
	{
		MazeSize = LevelType == LevelType.ClassicMaze ? settings.Classic_MazeSize : settings.Catacombs_MazeSize;
		_curWallHeight = settings.Classic_WallHeight;
		_curDayTime = settings.Classic_LightIntensity;
		_addLightOn = settings.AdditionLightOn;
		_curTorchType = settings.TypeOfAddLight;
		_cameraPosition = settings.CameraPosition;
		_postEffectsOn = settings.PostEffectsOn;
		_reflectionsOn = settings.ReflectionsOn;
		_crosshairOn = settings.CrosshairOn;
		_curMaterial = settings.Classic_WallMaterial;
		DefineCurMaterialsToggleGroupIndex();

		if (LevelType == LevelType.ClassicMaze)
			SetDayTime(_curDayTime);

		MainCamera.GetComponent<CameraPosition>().SetCameraPosition(_cameraPosition);
		_menuManager.SetCameraDropdownValue(_cameraPosition);
		MazeSizeText.text = (MazeSize - 1).ToString();
		_sliderLight.value = _curDayTime;
		_sliderHeight.value = _curWallHeight;
		_postEffectsToggle.isOn = _postEffectsOn;
		_reflectionsToggle.isOn = _reflectionsOn;
		_crosshairToggle.isOn = _crosshairOn;
		ToggleCrosshair(_crosshairOn);
		PostProcessVolume.SetActive(_postEffectsOn);
		_addLightToggle.isOn = _addLightOn;
		_materialToggles[_curMaterialsToggleGroupIndex].isOn = true;
		_clickSound.Stop(); // it is needed for the click sound did not play at the previous steps
	}

	public void ApplyRestOfSettings()
	{
		if (LevelType == LevelType.ClassicMaze)
			SetWallsHeight(_curWallHeight);
	
		_torchTypeDropdown.value = _curTorchType;

		if (_curMaterial != null && LevelType == LevelType.ClassicMaze)
			SetMaterial(_curMaterial);
	}

	public void SavePlaytimeSettings()
	{
		Debug.Log("Saving playtime settings...");
		switch (LevelType)
		{
			case LevelType.ClassicMaze:
				_playtimeSettings.Classic_MazeSize = MazeSize;
				_playtimeSettings.Classic_WallHeight = _curWallHeight;
				_playtimeSettings.Classic_LightIntensity = _curDayTime;
				_playtimeSettings.Classic_WallMaterial = _curMaterial;
				break;
			case LevelType.CatacombMaze:
				_playtimeSettings.Catacombs_MazeSize = MazeSize;
				break;
		}
		_playtimeSettings.AdditionLightOn = _addLightOn;
		_playtimeSettings.TypeOfAddLight = _curTorchType;
		_playtimeSettings.CameraPosition = _cameraPosition;
		_playtimeSettings.PostEffectsOn = _postEffectsOn;
		_playtimeSettings.ReflectionsOn = _reflectionsOn;
		_playtimeSettings.CrosshairOn = _crosshairOn;
	}

	private void RestoreDefaultSettings()
	{
		_playtimeSettings.Classic_MazeSize			= _defaultSettings.Classic_MazeSize;
		_playtimeSettings.Catacombs_MazeSize		= _defaultSettings.Catacombs_MazeSize;
		_playtimeSettings.Classic_WallHeight		= _defaultSettings.Classic_WallHeight;
		_playtimeSettings.Classic_LightIntensity	= _defaultSettings.Classic_LightIntensity;
		_playtimeSettings.Classic_WallMaterial		= _defaultSettings.Classic_WallMaterial;
		_playtimeSettings.AdditionLightOn			= _defaultSettings.AdditionLightOn;
		_playtimeSettings.TypeOfAddLight			= _defaultSettings.TypeOfAddLight;
		_playtimeSettings.CameraPosition			= _defaultSettings.CameraPosition;
		_playtimeSettings.PostEffectsOn				= _defaultSettings.PostEffectsOn;
		_playtimeSettings.ReflectionsOn				= _defaultSettings.ReflectionsOn;
		_playtimeSettings.CrosshairOn				= _defaultSettings.CrosshairOn;
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
		int customOffset = LevelType == LevelType.ClassicMaze ? 1 : 0;
		return _mazeZeroPoint + new Vector3(Mathf.Clamp(x + customOffset, customOffset, MazeSize - 1) * CellSize, 
											0, 
											-Mathf.Clamp(y + customOffset, customOffset, MazeSize - 1) * CellSize);
	}

	public Vector3 PositionByCellAddress(Vector2Int cell)
	{
		return PositionByCellAddress(cell.x, cell.y);
	}

	public direction middleOfThree(direction dir1, direction dir2, direction dir3)
	{
		if (dir1 == dir2 || dir2 == dir3 || dir3 == dir1)
			throw new System.ArgumentException("Directions should not duplicate!");
		if (dir1 == direction.randomOrUnknown || dir2 == direction.randomOrUnknown || dir3 == direction.randomOrUnknown)
			throw new System.ArgumentException("Passed the randomOrUnknown value");

		List<direction> threeDirections = new List<direction> { dir1, dir2, dir3 };
		for (direction tmpdir = direction.up; tmpdir != direction.left; tmpdir = tmpdir.directionToRight())
		{
			if (!threeDirections.Contains(tmpdir)) return tmpdir.directionToRight().directionToRight();
		}

		return direction.right;
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
	public void ToggleReflections(bool val)
	{
		_reflectionsOn = val;
		ReflectionProbe[] reflectionProbes = FindObjectsOfType<ReflectionProbe>();
		Debug.Log(reflectionProbes.Length);
		foreach (ReflectionProbe RP in FindObjectsOfType<ReflectionProbe>())
		{
			RP.enabled = val;
			if (val) RP.RenderProbe();
		}
	}
	public void ToggleCrosshair(bool val)
	{
		_crosshairOn = val;
		_menuManager.ToggleCrosshair(val);
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
				Debug.LogWarning("Torch bug, need to fix it!");
			}
		}
		
		_curWallHeight = val;

		_playtimeSettings.Classic_WallHeight = _curWallHeight;
	}

	[SerializeField] Toggle[] _materialToggles = new Toggle[3];
	[SerializeField] Material[] _materialsForToggles = new Material[3];

	public void SetMaterial(Material material)
	{
		_curMaterial = material;
		Renderer rend;
		foreach (GameObject W in Walls)
		{
			rend = W.transform.GetChild(0).GetComponent<Renderer>();
			rend.material = material;
		}
	}

	public void SetTorchType(int index)
	{
		switch (index)
		{
			case 0: // matte lamp
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
				}
				break;
			case 1: // red
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
				}
				break;
			case 2: // blue
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
				}
				break;
			case 3: // fire
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
		{
			RenderSettings.skybox = _darkSkybox;
			_globalLight.SetActive(false);
		}
		else
		{
			RenderSettings.skybox = _defaultSkybox;
			_globalLight.SetActive(true);
		}
		//_globalLight.GetComponent<Light>().intensity = val;

		RenderSettings.ambientIntensity = val;
		RenderSettings.reflectionIntensity = val;

		_curDayTime = val;
	}

	public void ChangeSize(int val)
	{
		int res = MazeSize + val;
		res = Mathf.Clamp(res, LevelType == LevelType.CatacombMaze ? 21 : 4, 51);
		MazeSize = res;
		MazeSizeText.text = (res - 1).ToString();
	}

	private int manageWall(int where, int which, bool toAdd = true) // 1 right, 2 left, 3 all
	{
		if (where < 0 || where > 3) where = 0;
		return (toAdd ? where | which : where & ~which);
	}

	private List<List<int>> _generateClassicMaze(int _mazeSize)
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

		// borders
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

		// direct generation using the Eller algorithm
		List<int> rowSets = new List<int>();
		int curCnt = 0;
		bool curHasExit = false;
		for (int i = 0; i < _mazeSize; i++) rowSets.Add(i); // creating sets
		for (int i = 1; i < _mazeSize - 1; i++)
		{ // creating right walls
			if (Random.Range(0, 2) == 1)
				_mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 1, true);
			else
				rowSets[i + 1] = rowSets[i];
		};
		for (int i = 1; i < _mazeSize; i++)
		{ // creating bottom walls
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

		// next rows
		for (int r = 2; r < _mazeSize; r++)
		{

			for (int i = 1; i < _mazeSize; i++)
				if (_mazeMapList[r - 1][i] == 2 || _mazeMapList[r - 1][i] == 3)
					rowSets[i] = i + _mazeSize * r; // assigning a unique set to those cells that had a bottom border

			for (int i = 1; i < _mazeSize - 1; i++)
			{ // creating right walls
				if (rowSets[i] == rowSets[i + 1] || Random.Range(0, 2) == 1)
					_mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 1, true);
				else
					rowSets[i + 1] = rowSets[i];
			};

			for (int i = 1; i < _mazeSize; i++)
			{ // creating bottom walls
				if (rowSets[i] != rowSets[i - 1])
				{
					if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
					{
						//Debug.Log("Do not build the wall for rowSets[" + i + "], = " + rowSets[i]);
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

		// the last row
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

			allTheSameSet = true; // *until proven otherwise
			for (int i = 1; i < _mazeSize - 1; i++)
			{
				if (rowSets[i + 1] != rowSets[i])
					allTheSameSet = false;
			}
		}

		// exit
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

	void PlaceTorchInCell(Vector2Int cell)
	{
		List<direction> availableWalls = catacombMazeMap.GetWallsInCell(cell);
		if (availableWalls.Count == 0) return;

		direction chosenWall = direction.randomOrUnknown;

		if (availableWalls.Count == 1)
		{
			chosenWall = availableWalls[0];
		}
		else if (availableWalls.Count == 2)
		{
			chosenWall = availableWalls[Random.Range(0,2)];
		}
		else if (availableWalls.Count == 3)
		{
			chosenWall = middleOfThree(availableWalls[0], availableWalls[1], availableWalls[2]);
		}

		GameObject _newTorch = Instantiate(TorchPrefab, 
											PositionByCellAddress(cell)+chosenWall.toVector3()*(CellSize/2)+Vector3.up*1.15f,
											Quaternion.Euler(0f, chosenWall.toQuatornianDegrees(), 0f)
								);
		_newTorch.SetActive(_tglAddLight.isOn);
		Torches.Add(_newTorch);
	}


	// // //
	// CATACOMB GENERATION
	// // // 

	public class CatacombMazeMap
	{
		public enum Legend
		{
			FreeCell,
			Wall,
			EdgeOfTheMaze,
			ExitHorizontal,
			ExitVertical,
			LatticeHorizontal,
			LatticeVertical
		}

		List<List<int>> mazeMap = new List<List<int>>();
		int mazeSize;

		List<Vector2Int> freeCells = new List<Vector2Int>();
		List<Vector2Int> thinWalls = new List<Vector2Int>();

		public int MazeSize
		{
			get => mazeSize;
		}

		public List<List<int>> MazeMap
		{
			get => mazeMap;
		}

		public List<Vector2Int> GetFreeCells {
			get
			{
				if (freeCells.Count == 0)
				{
					for (int i = 0; i < mazeSize; i++)
						for (int j = 0; j < mazeSize; j++)
						{
							if (mazeMap[i][j] == (int)Legend.FreeCell)
								freeCells.Add(new Vector2Int(j, i));
						}
				}

				return freeCells;
			}
		}

		public List<Vector2Int> GetThinWalls
		{
			// only if 2x2, and only an alternation
			get
			{
				if (thinWalls.Count == 0)
				{
					for (int i = 0; i < mazeSize; i++)
						for (int j = 0; j < mazeSize; j++)
						{
							if ( (mazeMap[i][j] == (int)Legend.Wall || mazeMap[i][j] == (int)Legend.EdgeOfTheMaze)
								&&
								(
									(
											(i != 0				&& (mazeMap[i - 1][j] == (int)Legend.Wall || mazeMap[i - 1][j] == (int)Legend.EdgeOfTheMaze))
										&&	(i != mazeSize-1	&& (mazeMap[i + 1][j] == (int)Legend.Wall || mazeMap[i + 1][j] == (int)Legend.EdgeOfTheMaze))
										&&	(j == 0				|| mazeMap[i][j - 1] == (int)Legend.FreeCell)
										&&  (j == mazeSize-1	|| mazeMap[i][j + 1] == (int)Legend.FreeCell)
									)
									||
									(
											(i == 0				|| mazeMap[i - 1][j] == (int)Legend.FreeCell)
										&&	(i == mazeSize-1	|| mazeMap[i + 1][j] == (int)Legend.FreeCell)
										&&	(j != 0				&& (mazeMap[i][j - 1] == (int)Legend.Wall || mazeMap[i][j - 1] == (int)Legend.EdgeOfTheMaze))
										&&	(j != mazeSize-1	&& (mazeMap[i][j + 1] == (int)Legend.Wall || mazeMap[i][j + 1] == (int)Legend.EdgeOfTheMaze))
									)
								)
							)
								thinWalls.Add(new Vector2Int(j, i));
						}
				}

				return thinWalls;
			}
		}

		public List<direction> GetWallsInCell(Vector2Int cell)
		{
			List<direction> directions = new List<direction>();
			if (cell.y != 0 && (mazeMap[cell.y - 1][cell.x] == (int)Legend.Wall || mazeMap[cell.y - 1][cell.x] == (int)Legend.EdgeOfTheMaze))
				directions.Add(direction.up);
			if (cell.x != mazeSize-1 && (mazeMap[cell.y][cell.x + 1] == (int)Legend.Wall || mazeMap[cell.y][cell.x + 1] == (int)Legend.EdgeOfTheMaze))
				directions.Add(direction.right);
			if (cell.y != mazeSize-1 && (mazeMap[cell.y + 1][cell.x] == (int)Legend.Wall || mazeMap[cell.y + 1][cell.x] == (int)Legend.EdgeOfTheMaze))
				directions.Add(direction.down);
			if (cell.x != 0 && (mazeMap[cell.y][cell.x - 1] == (int)Legend.Wall || mazeMap[cell.y][cell.x - 1] == (int)Legend.EdgeOfTheMaze))
				directions.Add(direction.left);

			return directions;
		}
		// inside area - 1, borders - 2. Empty cells - 0, though there are no them in the beginning
		public CatacombMazeMap(int mazeSize)
		{
			this.mazeSize = mazeSize;
			for (int i = 0; i < mazeSize; i++)
			{
				mazeMap.Add(new List<int>());
				for (int j = 0; j < mazeSize; j++)
				{
					if (i == 0 || i == mazeSize - 1 || j == 0 || j == mazeSize - 1) // outside border
						mazeMap[i].Add((int)Legend.EdgeOfTheMaze);
					else
						mazeMap[i].Add((int)Legend.Wall);
				}
			}
		}

		public void DigPathAt(Vector2Int cell)
		{
			if (cell.x > mazeSize - 1 || cell.x < 0 ||
				cell.y > mazeSize - 1 || cell.y < 0)
				throw new System.ArgumentOutOfRangeException("cell", cell, "We got out of the boundaries of the dimensions of the maze");

			if (mazeMap[cell.y][cell.x] >= 1)
				mazeMap[cell.y][cell.x] = (int)Legend.FreeCell;
			else
			{
				PrintMazeMap();
				Debug.Log(cell.x + ", " + cell.y + ": " + mazeMap[cell.y][cell.x]);
				throw new System.Exception("It has already been drilled here!");
			}
		}

		public void PlaceExitNear(Vector2Int cell)
		{
			if ((cell.x > 1 && cell.x < mazeSize - 2) &&
				(cell.y > 1 && cell.y < mazeSize - 2))
				throw new System.ArgumentOutOfRangeException("cell", cell , "The cell is in the inside area, must be adjacent to the edge!");

			else if (cell.x == 0 || cell.x == mazeSize - 1 ||
				cell.y == 0 || cell.y == mazeSize - 1)
				mazeMap[cell.y][cell.x] = (int)Legend.ExitHorizontal; // share between two ("поделить по двум")

			else
			{
				if (cell.x == 1)
					mazeMap[cell.y][0] = (int)Legend.ExitVertical;
				else if (cell.x == mazeSize - 2)
					mazeMap[cell.y][mazeSize - 1] = (int)Legend.ExitVertical;
				else if (cell.y == 1)
					mazeMap[0][cell.x] = (int)Legend.ExitHorizontal;
				else if (cell.y == mazeSize - 2)
					mazeMap[mazeSize - 1][cell.x] = (int)Legend.ExitHorizontal;
				else
					throw new System.Exception("Something went completely wrong while placing up the exit.");
			}
		}

		public void PlaceLattice(Vector2Int cell)
		{
			if (!catacombMazeMap.GetThinWalls.Contains(cell))
				throw new System.InvalidOperationException("This wall is not thin!");
			else if (cell.y == 0 || mazeMap[cell.y - 1][cell.x] == (int)CatacombMazeMap.Legend.FreeCell)
				mazeMap[cell.y][cell.x] = (int)CatacombMazeMap.Legend.LatticeHorizontal;
			else
				mazeMap[cell.y][cell.x] = (int)CatacombMazeMap.Legend.LatticeVertical;
		}

		public void WeedOutCellsAvailableToDigFromList(ref List<Vector2Int> cells)
		{
			foreach (Vector2Int c in cells.ToList())
			{
				if (c.x < 1 || c.x > mazeSize - 2 ||
					c.y < 1 || c.y > mazeSize - 2 ||
					mazeMap[c.x][c.y] != 1) // TODO add the check for the "corridority"
					cells.Remove(c);
			}
		}

		public NavigateCell GetNeighbourCell(NavigateCell navCell) // Vector2Int cell, direction direction)
		{
			return new NavigateCell(
				new Vector2Int(
					navCell.cell.x + (	navCell.direction == direction.left ? -1 :
										navCell.direction == direction.right ? 1
																			: 0),
					navCell.cell.y + (	navCell.direction == direction.up ? -1 :
										navCell.direction == direction.down ? 1
																			: 0)
																),
				navCell.direction);
		}

		public bool CheckCellForPath(NavigateCell navCell) //Vector2Int cell, direction direction)
		{
			try
			{
				if (mazeMap[navCell.cell.y][navCell.cell.x] == 2)
				{
					Debug.Log("Detected \"2\": y = " + navCell.cell.y + ", x = " + navCell.cell.x);
					return false;
				}

			}
			catch
			{
				Debug.LogError("Exception with coords: y = " + navCell.cell.y + ", x = " + navCell.cell.x);
				return false;
			}

			Vector2Int cellToCheck = new Vector2Int();
			cellToCheck = GetNeighbourCell(navCell.turnLeft()).cell;
			if (mazeMap[cellToCheck.y][cellToCheck.x] == 0) return false;
			cellToCheck = GetNeighbourCell(navCell).cell;
			if (mazeMap[cellToCheck.y][cellToCheck.x] == 0) return false;
			cellToCheck = GetNeighbourCell(navCell.turnRight()).cell;
			if (mazeMap[cellToCheck.y][cellToCheck.x] == 0) return false;

			return true;
		}

		public void PrintMazeMap()
		{
			string dbgStr = "";
			for (int i = 0; i < mazeSize; i++)
			{
				dbgStr = "";
				for (int j = 0; j < mazeSize; j++)
					dbgStr += mazeMap[i][j] + " ";
				Debug.Log(dbgStr);
			};
		}
	}

	public static CatacombMazeMap catacombMazeMap;

	private List<List<int>> _generateCatacombMaze(int _mazeSize)
	{
		if (PathProcess.allPathProcesses.Count > 0) PathProcess.allPathProcesses.Clear();
		if (PathProcess.activePathProcesses.Count > 0) PathProcess.activePathProcesses.Clear();

		catacombMazeMap = new CatacombMazeMap(_mazeSize);
		List<List<int>> _mazeMapList = catacombMazeMap.MazeMap; // inside area - 1, borders - 2

		// Vector2Int - order x, y
		new PathProcess(new Vector2Int(_mazeSize/2, _mazeSize/2));

		// then we need to go through everyone and make steps while we can
		while (PathProcess.activePathProcesses.Count > 0)
		{
			PathProcess.activePathProcesses[Random.Range(0,PathProcess.activePathProcesses.Count)].MakeStep();
		}

		Debug.Log("Pathprocesses overall count: " + PathProcess.allPathProcesses.Count);

		List<PathProcess> PathsEndingAtTheEdge = (from pr in PathProcess.allPathProcesses
													where pr.IsEndingAtTheEdge
													select pr).ToList();

		foreach (PathProcess p in PathProcess.allPathProcesses)//PathsEndingAtTheEdge)
		{
			Debug.Log(p.PathOrigin + " " + p.PathEnd);
			Color curDbgColor = Color.white;
			int rndCol = Random.Range(0, 9);
			switch (rndCol)
			{
				case 0:	curDbgColor = Color.blue;	break;
				case 1: curDbgColor = Color.white; break;
				case 2: curDbgColor = Color.green; break;
				case 3: curDbgColor = Color.red; break;
				case 4: curDbgColor = Color.green; break;
				case 5: curDbgColor = Color.yellow; break;
				case 6: curDbgColor = Color.cyan; break;
				case 7: curDbgColor = Color.gray; break;
				case 8: curDbgColor = Color.magenta; break;
					//case 9: curDbgColor = Color.;
			}

			for (int i = 0; i < p.PathLength-1; i++)
				Debug.DrawLine(PositionByCellAddress(p.PathSteps(i)), PositionByCellAddress(p.PathSteps(i+1)), curDbgColor, 1000);
		}

		int rndForExit = Random.Range(0, PathsEndingAtTheEdge.Count);
		try
		{
			catacombMazeMap.PlaceExitNear(PathsEndingAtTheEdge[rndForExit].PathEnd);
		}
		catch (System.ArgumentOutOfRangeException e)
		{
			Debug.LogWarning("Failed PlaceExitNear: " + e.ToString() + "; " + e.ActualValue + "; PathsEndingAtTheEdge.Count = " + PathsEndingAtTheEdge.Count + "; rndForExit = " + rndForExit);
			return null; // a hint of fail
		}

		List<Vector2Int> thinWalls = catacombMazeMap.GetThinWalls;
		foreach (Vector2Int t in thinWalls)
		{
			if (Random.Range(0, 10) == 1)
			{
				catacombMazeMap.PlaceLattice(t);
			}
		}

		return _mazeMapList;
	}

	private void OnDrawGizmos()
	{
	
		foreach (PathProcess p in PathProcess.allPathProcesses)
		{
			Gizmos.DrawSphere(PositionByCellAddress(p.PathEnd), 1);
		}

		/*foreach (Vector2Int cell in catacombMazeMap.GetThinWalls)
		{
			Gizmos.DrawCube(PositionByCellAddress(cell)+Vector3.up*3, Vector3.one);
		}*/
	
	}

	private void OnDestroy()
	{
		SavePlaytimeSettings();

		PathProcess.allPathProcesses.Clear();
		PathProcess.activePathProcesses.Clear();
	}

	private class PathProcess
	{
		bool isActive = true;
		Vector2Int pathOrigin;
		Vector2Int pathEnd;
		//Vector2Int pathLength;
		Vector2Int currentCell;
		direction currentDirection = direction.randomOrUnknown;
		bool wannaBranch = false;

		List<Vector2Int> pathSteps = new List<Vector2Int>();

		public static List<PathProcess> activePathProcesses = new List<PathProcess>();
		public static List<PathProcess> allPathProcesses = new List<PathProcess>();

		public bool IsActive { get => isActive; }
		public Vector2Int PathOrigin { get => pathOrigin; }
		public Vector2Int PathEnd { get => pathEnd; }
		public int PathLength { get => pathSteps.Count; }
		public Vector2Int PathSteps(int i) { return pathSteps[i]; }
		public Vector2Int CurrentCell { get => currentCell; }
		public direction CurrentDirection {	get => currentDirection; }

		public bool IsEndingAtTheEdge
		{
			get
			{
				if (pathEnd.x == 1 || pathEnd.x == catacombMazeMap.MazeSize - 2 ||
					pathEnd.y == 1 || pathEnd.y == catacombMazeMap.MazeSize - 2)
					return true;
				else
					return false;
			}
		}

		public void StopPath()
		{
			isActive = false;
			pathEnd = currentCell;
			activePathProcesses.Remove(this);
		}

		public PathProcess(Vector2Int origin)
		{
			activePathProcesses.Add(this);
			allPathProcesses.Add(this);
			pathOrigin = origin;
			pathSteps.Add(pathOrigin);
			currentCell = pathOrigin;
			catacombMazeMap.DigPathAt(origin);
		}
		public PathProcess(Vector2Int origin, direction direction) : this(origin)
		{
			currentDirection = direction;
		}

		public void MakeStep()
		{
			// determine where we can make a step

			List<direction> desiredDirections = new List<direction>();
			if (currentDirection == direction.randomOrUnknown)
			{
				currentDirection = (direction)Random.Range(0, 4);
			}

			desiredDirections.Add(currentDirection);
			desiredDirections.Add(currentDirection.directionToLeft());
			desiredDirections.Add(currentDirection.directionToRight());

			bool directionDetermined = false;
			List<direction> bannedDirections = new List<direction>();

			bool wannaForward = Random.Range(0, 3) >= 1 ? true : false;
			direction resultDirection = direction.randomOrUnknown;

			while (!directionDetermined && desiredDirections.Count > bannedDirections.Count)
			{

				if (wannaForward && !bannedDirections.Contains(desiredDirections[0])) // means "forward"
				{
					if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell,currentDirection))))
					{
						directionDetermined = true;
						resultDirection = desiredDirections[0];
						break;
					}
					else
					{
						bannedDirections.Add(desiredDirections[0]);
					}
				}
				else
				{
					if (!bannedDirections.Contains(desiredDirections[1]) && !bannedDirections.Contains(desiredDirections[2]))
					{
						if (Random.Range(0, 2) == 1)
						{
							if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1]))))
							{
								directionDetermined = true;
								resultDirection = desiredDirections[1];
								break;
							}
							else
							{
								bannedDirections.Add(desiredDirections[1]);
								// switch to right
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[2]))))
								{
									directionDetermined = true;
									resultDirection = desiredDirections[2];
									break;
								}
								else
								{
									bannedDirections.Add(desiredDirections[2]);
								}
							}
						}
						else
						{
							if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[2]))))
							{
								directionDetermined = true;
								resultDirection = desiredDirections[2];
								break;
							}
							else
							{
								bannedDirections.Add(desiredDirections[2]);
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1]))))
								{
									directionDetermined = true;
									resultDirection = desiredDirections[1];
									break;
								}
								else
								{
									bannedDirections.Add(desiredDirections[1]);
								}
							}
						}
					}
					else
					{
						if (!bannedDirections.Contains(desiredDirections[1]))
						{
							directionDetermined = true;
							resultDirection = desiredDirections[1];
						}
						else if (!bannedDirections.Contains(desiredDirections[2]))
						{
							directionDetermined = true;
							resultDirection = desiredDirections[2];
						}
						else
						{
							wannaForward = true;
						}
					}
				}
			}

			if (directionDetermined)
			{
				// before moving on, we need to decide if we want to make a fork here
				if (!wannaBranch && Random.Range(0, 10) == 1)
					wannaBranch = true;

				if (wannaBranch)
				{
					if (desiredDirections.Count - bannedDirections.Count - 1 > 0)
					{
						desiredDirections = desiredDirections.Except(bannedDirections).ToList();
						desiredDirections.Remove(resultDirection);

						if (desiredDirections.Count == 1)
						{
							if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0]))))
							{
								new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0])).cell, desiredDirections[0]);
								wannaBranch = false;
							}
						}
						else
						{
							// sometimes we will make crossways
							if (Random.Range(0, 2) == 0)
							{
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0])).cell, desiredDirections[0]);
									wannaBranch = false;
								}
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1])).cell, desiredDirections[1]);
									wannaBranch = false;
								}
							}

							// otherwise - only one way out of two
							else if (Random.Range(0, 2) == 1)
							{
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0])).cell, desiredDirections[0]);
									wannaBranch = false;
								}
								else if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1])).cell, desiredDirections[1]);
									wannaBranch = false;
								}
							}
							else
							{
								if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[1])).cell, desiredDirections[1]);
									wannaBranch = false;
								}
								else if (catacombMazeMap.CheckCellForPath(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0]))))
								{
									new PathProcess(catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, desiredDirections[0])).cell, desiredDirections[0]);
									wannaBranch = false;
								}
							}
						}

					}

				}
				//

				currentDirection = resultDirection;
				Vector2Int nextCell = catacombMazeMap.GetNeighbourCell(new NavigateCell(currentCell, resultDirection)).cell;
				try
				{
					catacombMazeMap.DigPathAt(nextCell);
					currentCell = nextCell;
					pathSteps.Add(currentCell);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Broadcasting from catch: " + e.ToString());
					Debug.LogWarning("currentCell: " + currentCell + ", resultDirection: " + resultDirection + ", nextCell: " + nextCell);
				}
			}
			else
				this.StopPath();

			Debug.Log("Active pathprocesses count: " + activePathProcesses.Count);
		}
	}

}

