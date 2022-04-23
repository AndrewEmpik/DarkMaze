using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
	public GameObject WallPrefab;
	public GameObject ExitWallPrefab;
	private Material _curMaterial;
	public Vector3 MazeCenter = new Vector3(40f, 0f, 10f);
	public int MazeSize = 5;
	//public Transform TestPoint;

	public Camera MainCamera;
	public Camera PlayerCamera;


	public WinCanvas WinCanvas;

	private float _cellSize = 3;

	public List<GameObject> Walls;
	private GameObject _newWall;

	public Text MazeSizeText;

	private float _curWallHeight = 1f;
	private int _curTorchType = 3;
	private float _curDayTime = 0.2f;

	[SerializeField]
	private GameObject _globalLight;

	[SerializeField]
	private Dropdown _drpCamera;
	[SerializeField]
	private Dropdown _drpLight;

	[SerializeField]
	private Toggle _tglAddLight;

	public List<GameObject> Torches;
	public GameObject TorchPrefab;
	private GameObject _newTorch;

	public int TorchProbability = 50;

	private Vector3 _mazeZeroPoint = Vector3.zero;

	public enum PinnedPosition
	{
		Center,
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
		Exit
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
		foreach (GameObject W in Walls)
			Destroy(W);

		foreach (GameObject T in Torches)
			Destroy(T);

		Walls.Clear();
		Torches.Clear();

		Start();

		SetWallsHeight(_curWallHeight);
		SetTorchType(_curTorchType);

		FindObjectOfType<FliBall>().Respawn();
		FindObjectOfType<PlayerMove>().transform.position = PositionByCellAddress(PinnedPosition.Center);


	}

	public void ToggleTorch(bool val)
	{
		foreach (GameObject T in Torches)
			T.SetActive(val);
	}

	public void NextCameraDropdownValue()
	{
		if (_drpCamera.value >= _drpCamera.options.Count - 1)
			_drpCamera.value = 0;
		else
			_drpCamera.value++;
	}
	public void NextLightDropdownValue()
	{
		if (_drpLight.value >= _drpLight.options.Count - 1)
			_drpLight.value = 0;
		else
			_drpLight.value++;
	}

	public void SetWallsHeight(float val)
	{
		foreach (GameObject W in Walls)
			W.transform.localScale = new Vector3(W.transform.localScale.x, val, W.transform.localScale.z);

		foreach (GameObject T in Torches)
		{
			try
			{
				T.transform.position = new Vector3(T.transform.position.x, Walls[0].transform.GetChild(1).position.y, T.transform.position.z);
			}
			catch
			{
				Debug.LogWarning("��� � �������! ���� ���������!");
			}
		}
		

		_curWallHeight = val;
	}

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
			case 0: // ������� �����
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
				}
				break;
			case 1: // ������� �������
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
				}
				break;
			case 2: // ����� �������
				foreach (GameObject T in Torches)
				{
					for (int c = 1; c <= 4; c++) T.transform.GetChild(0).GetChild(c).gameObject.SetActive(false);
					T.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);
				}
				break;
			case 3: // �����
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

	private float fps = 30f;

	void OnGUI()
	{
		//float newFPS = 1.0f / Time.smoothDeltaTime;
		fps = 1.0f / Time.smoothDeltaTime;  //Mathf.Lerp(fps, newFPS, 0.0005f);
		GUI.Label(new Rect(0, 0, 100, 100), "FPS: " + ((int)fps).ToString());
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
		res = Mathf.Clamp(res, 2, 100);
		MazeSize = res;
		MazeSizeText.text = (res - 1).ToString();
	}


	void Start()
	{
		//MainCamera.enabled = true;
		//PlayerCamera.enabled = false;
		//SetTorchType(3);
		SetDayTime(_curDayTime);

		MazeSizeText.text = (MazeSize - 1).ToString();

		//TestPoint.position = MazeCenter;

		List<List<int>> _mazeMapList;

		_mazeMapList = _generateMaze(MazeSize);

		float _mazeZeroPointSingle = (Mathf.Floor(MazeSize / 2f)) * _cellSize;
		_mazeZeroPoint = MazeCenter - new Vector3(_mazeZeroPointSingle, 0f, -_mazeZeroPointSingle);

		//Vector3 _prevPoint = _mazeZeroPoint-new Vector3(10,0,-10);

		// j - �����������, i - ���������
		for (int i = 0; i < MazeSize; i++)
			for (int j = 0; j < MazeSize; j++)
			{
				//Debug.DrawLine( _prevPoint,
				//				_mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), 
				//				Color.cyan,1000);
				//_prevPoint = _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize);

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


				// ������ "�����"
				if (i == 0 && j >= 1 && _mazeMapList[i][j] == 0)
				{
					_newWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
					Walls.Add(_newWall);
				}
				if (i == MazeSize-1 && j >= 1 && _mazeMapList[i][j] <= 1) // 0 ��� 1
				{
					_newWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
					Walls.Add(_newWall);
				}
				if (j == 0 && i >= 1 && _mazeMapList[i][j] == 0)
				{
					_newWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, -90f, 0f));
					Walls.Add(_newWall);
				}
				if (j == MazeSize-1 && i >= 1 && (_mazeMapList[i][j] == 0 || _mazeMapList[i][j] == 2)) // 0 ��� 2, ���� ������ �������� "���", ��� � ��� ���� ��� ��������
				{
					_newWall = Instantiate(ExitWallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, -90f, 0f));
					Walls.Add(_newWall);
				}
			}

		if (_curMaterial != null)
			SetMaterial(_curMaterial);

	}

	private int manageWall(int where, int which, bool toAdd = true) // 1 ��, 2 ���, 3 ��
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

		// �������
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

		// ��������������� ��������� ��� ������ ��������� ������
		List<int> rowSets = new List<int>();
		int curCnt = 0;
		bool curHasExit = false;
		for (int i = 0; i < _mazeSize; i++) rowSets.Add(i); // �������� ��������
		for (int i = 1; i < _mazeSize - 1; i++)
		{ // �������� ������ ������
			if (Random.Range(0, 2) == 1)
				_mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 1, true);
			else
				rowSets[i + 1] = rowSets[i];
		};
		for (int i = 1; i < _mazeSize; i++)
		{ // �������� ������ �����
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

		// ��������� ������
		for (int r = 2; r < _mazeSize; r++)
		{

			for (int i = 1; i < _mazeSize; i++)
				if (_mazeMapList[r - 1][i] == 2 || _mazeMapList[r - 1][i] == 3)
					rowSets[i] = i + _mazeSize * r; // ����������� ���������� ��������� ��� �������, � ������� ���� ������� �����

			for (int i = 1; i < _mazeSize - 1; i++)
			{ // �������� ������ ������
				if (rowSets[i] == rowSets[i + 1] || Random.Range(0, 2) == 1)
					_mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 1, true);
				else
					rowSets[i + 1] = rowSets[i];
			};

			for (int i = 1; i < _mazeSize; i++)
			{ // �������� ������ �����
				if (rowSets[i] != rowSets[i - 1])
				{
					if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
					{
						//Debug.Log("�� ������ ������ ��� rowSets[" + i + "], = " + rowSets[i]);
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

		// ��������� ������
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

			allTheSameSet = true; // *���� �� �������� ��������
			for (int i = 1; i < _mazeSize - 1; i++)
			{
				if (rowSets[i + 1] != rowSets[i])
					allTheSameSet = false;
			}
		}

		// �����
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

	public void QuitApplication()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false; // �������� ������ � ���������
		#else
			Application.Quit(); // �������� ������ � �����
		#endif
	}

}
