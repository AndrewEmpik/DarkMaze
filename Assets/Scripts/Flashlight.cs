using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
	[SerializeField] MazeGenerator _mazeGenerator;

	public void Start()
	{
		Vector2Int flashlightCellAddress = new Vector2Int();
		if (_mazeGenerator.LevelType == LevelType.ClassicMaze)
			flashlightCellAddress = new Vector2Int(Random.Range(0, _mazeGenerator.MazeSize), Random.Range(0, _mazeGenerator.MazeSize));
		else if (_mazeGenerator.LevelType == LevelType.CatacombMaze)
		{
			List<Vector2Int> cellsForFlashlight = MazeGenerator.catacombMazeMap.GetFreeCells;
			flashlightCellAddress = cellsForFlashlight[Random.Range(0, cellsForFlashlight.Count)];
		}
		//Debug.Log(flashlightCellAddress);
		Vector3 flashlightCoords = _mazeGenerator.PositionByCellAddress(flashlightCellAddress.x, flashlightCellAddress.y);
		//Debug.Log(flashlightCoords);
		flashlightCoords += (Vector3.right * Random.Range(-1f, 1f) +
									Vector3.forward * Random.Range(-1f, 1f)) * _mazeGenerator.CellSize / 2 * 0.75f;
		transform.position = flashlightCoords;
		transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
	}
}
