///*
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MazeGenerator : MonoBehaviour
//{
//	public GameObject WallPrefab;
//	public Vector3 MazeCenter = new Vector3(40f,0f,10f);
//	public int MazeSize = 5;
//	public Transform TestPoint;

//	private float _cellSize = 2;
//	private int[,] _mazeMap;
	

//	void Start()
//    {

		

//		TestPoint.position = MazeCenter;

//		_mazeMap = new int[5,5] //[MazeSize, MazeSize]
//			{
//				{ 0,2,2,2,2 },
//				{ 1,2,0,3,0 },
//				{ 1,0,2,3,1 },
//				{ 1,2,0,3,1 },
//				{ 1,2,2,2,3 }
//			};

//		List<List<int>> _mazeMapList;

//		_mazeMapList = _generateMaze(MazeSize);

//		float _mazeZeroPointSingle = (Mathf.Floor(MazeSize/2f)) * _cellSize;
//		Vector3 _mazeZeroPoint = MazeCenter - new Vector3(_mazeZeroPointSingle, 0f, -_mazeZeroPointSingle);

//		for (int i = 0; i < MazeSize; i++)
//			for (int j = 0; j < MazeSize; j++)
//			{
//				if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 1) != 0) 
//					Instantiate(WallPrefab,_mazeZeroPoint + new Vector3(j * _cellSize, 0,-i * _cellSize),Quaternion.Euler(0f,-90f,0f));
//				if (_mazeMapList[i][j] > 0 && (_mazeMapList[i][j] & 2) != 0)
//					Instantiate(WallPrefab, _mazeZeroPoint + new Vector3(j * _cellSize, 0, -i * _cellSize), Quaternion.Euler(0f, 0f, 0f));
//			}

//        int testN = 3;
//        Debug.Log("testN = " + testN);
//        testN.intManageWall(1, false);
//        Debug.Log("testN = " + testN);

//    }

//    private int manageWall(int where, int which, bool toAdd = true) // 1 пр, 2 лев, 3 всё
//    {
//        if (where < 0 || where > 3) where = 0;
//        return (toAdd ? where | which : where & ~which);
//    }

    

//    private List<List<int>> _generateMaze(int _mazeSize)
//	{
//		//int[,] _mazeMapGenerate = new int[MazeSize, MazeSize] { };
//		List<List<int>> _mazeMapList = new List<List<int>>();

//		for (int i = 0; i < _mazeSize; i++)
//		{
//			_mazeMapList.Add(new List<int>());
//			for (int j = 0; j < _mazeSize; j++)
//			{
//				_mazeMapList[i].Add(-1);
//			}
//		}

		
//        /*
//		for (int i = 0; i < _mazeSize; i++)
//		{
//			for (int j = 0; j < _mazeSize; j++)
//			{
//				_mazeMapList[i][j] = (int)Random.Range(0,4);
//			}
//		}*/


//		// границы
//		_mazeMapList[0][0] = 0;
//        for (int i = 1; i < _mazeSize; i++)
//        {
//            _mazeMapList[i][0] = 1;
//            _mazeMapList[i][_mazeSize - 1] = 1;//(int)(Random.Range(1, 3) == 1 ? 1 : 3);
//        }
//		for (int j = 1; j < _mazeSize; j++)
//        {
//            _mazeMapList[0][j] = 2;
//            _mazeMapList[_mazeSize - 1][j] = 2; //(int)Random.Range(2, 4);
//        }
//        _mazeMapList[_mazeSize - 1][_mazeSize - 1] = 3;

//        // непосредственно генерация при помощи алгоритма Эллера
//        List<int> rowSets = new List<int>();
//        int curCnt=0;
//        bool curHasExit=false;
//        for (int i = 0; i < _mazeSize; i++) rowSets.Add(i); // создание множеств
//        for (int i = 1; i < _mazeSize-1; i++) { // создание стенок справа
//            if (Random.Range(0, 2) == 1)
//                _mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 1, true);
//            else
//                rowSets[i + 1] = rowSets[i];
//        };
//        for (int i = 1; i < _mazeSize; i++) { // создание стенок снизу
//            if (rowSets[i] != rowSets[i-1])
//            {
//                if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
//                    continue;

//                curCnt = 0;
//                curHasExit = false;
//            }
//            curCnt++;

//            if (!curHasExit && (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1]))
//                continue;

//            if (Random.Range(0, 2) == 1)
//                _mazeMapList[1][i] = manageWall(_mazeMapList[1][i], 2, true);
//            else
//                curHasExit = true;
//        };

//        // следующие строки
//        for (int r = 2; r < _mazeSize; r++)
//        {


//            string bstr = "";
//            // foreach (int b in rowSets) bstr += b.ToString() + " ";
//            //  Debug.Log(bstr);

//            for (int i = 1; i < _mazeSize; i++)
//                if (_mazeMapList[r-1][i] == 2 || _mazeMapList[r-1][i] == 3)
//                    rowSets[i] = i + _mazeSize*r; // присваиваем уникальное множество тем ячейкам, у которых была граница снизу

//            //    /*string */ bstr = "";
//            //  foreach (int b in rowSets) bstr += b.ToString() + " ";
//            //    Debug.Log(bstr);

//            for (int i = 1; i < _mazeSize - 1; i++)
//            { // создание стенок справа
//                if (rowSets[i] == rowSets[i + 1] || Random.Range(0, 2) == 1)
//                    _mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 1, true);
//                else
//                    rowSets[i + 1] = rowSets[i];
//            };

//            for (int i = 1; i < _mazeSize; i++)
//            { // создание стенок снизу
//                if (rowSets[i] != rowSets[i - 1])
//                {
//                    if (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1])
//                    {
//                        Debug.Log("Не строим стенку для rowSets[" + i + "], = " + rowSets[i]);
//                        continue;
//                    }

//                    curCnt = 0;
//                    curHasExit = false;
//                }
//                curCnt++;

//                if (!curHasExit && (i == _mazeSize - 1 || rowSets[i] != rowSets[i + 1]))
//                    continue;

//                if (Random.Range(0, 4) > 1)// || curHasExit) 
//                    _mazeMapList[r][i] = manageWall(_mazeMapList[r][i], 2, true);
//                else
//                    curHasExit = true;
//            };
//        };

//        // последняя строка
//        bool allTheSameSet = false;
//        while (!allTheSameSet)
//        {
//            for (int i = 1; i < _mazeSize - 1; i++)
//            {
//                if (rowSets[i + 1] != rowSets[i])
//                {
//                    _mazeMapList[_mazeSize-1][i] = manageWall(_mazeMapList[_mazeSize-1][i], 1, false);
//                    //int s;
//                    int formerSet = rowSets[i + 1];
//                    //while (rowSets[i + 2 + s] == rowSets[i];)
//                    rowSets[i + 1] = rowSets[i];
//                    for (int s = i+1; s < _mazeSize - 1; s++)
//                        if (rowSets[s] == formerSet)
//                            rowSets[s] = rowSets[i];
//                }
//            }

//            allTheSameSet = true; // *пока не доказано обратное
//            for (int i = 1; i < _mazeSize-1; i++)
//            {
//                if (rowSets[i + 1] != rowSets[i])
//                    allTheSameSet = false;
//            }
//        }

//        // выход
//        int exitAt = Random.Range(0, (_mazeSize - 1) * 4);
//        int exitAtOffset = (exitAt % (_mazeSize - 1)) + 1;
//        switch (exitAt / (_mazeSize - 1))
//        {
//            case 0:
//                _mazeMapList[0][exitAtOffset] = 0;
//                break;
//            case 1:
//                _mazeMapList[exitAtOffset][_mazeSize - 1] =
//                    manageWall(_mazeMapList[exitAtOffset][_mazeSize - 1], 1, false);
//                // _mazeMapList[exitAtOffset][_mazeSize - 1].intManageWall(1, false);
//                break;
//            case 2:
//                _mazeMapList[_mazeSize - 1][exitAtOffset] =
//                    manageWall(_mazeMapList[_mazeSize - 1][exitAtOffset], 2, false);
//                // _mazeMapList[_mazeSize - 1][exitAtOffset].intManageWall(2, false);
//                break;
//            case 3:
//                _mazeMapList[exitAtOffset][0] = 0;
//                break;
//        }

//        //////
//        for (int i = 0; i < _mazeSize; i++)
//		{
//			string  bstr = "";
//			foreach (int b in _mazeMapList[i]) bstr += b.ToString() + " ";
//			Debug.Log(bstr);
//		}
		
		

//		return _mazeMapList;
//	}

//}



//public static class intExpression
//{
//    public static void intManageWall(this ref int where, int which, bool toAdd = true)
//    {
//       // Debug.Log("where = " + where);
//        if (where < 0 || where > 3) where = 0;
//        //Debug.Log(where + " " + which + " " + ~which + " " + (where & ~which) + " " + (toAdd ? where | which : where & ~which));
//       // Debug.Log("where = " + where);
//        where = toAdd ? where | which : where & ~which;
//        //Debug.Log("where = " + where);
//    }
//}