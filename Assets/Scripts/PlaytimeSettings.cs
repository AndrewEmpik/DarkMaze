using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlaytimeSettings : ScriptableObject
{
	public int Classic_MazeSize;
	public int Catacombs_MazeSize;
	public float Classic_WallHeight;
	//public float Catacombs_WallHeight;
	public float Classic_LightIntensity;
	//public float Catacombs_LightIntensity;
	public bool AdditionLightOn;
	public int TypeOfAddLight;
	public int CameraPosition;
	public bool PostEffectsOn;
	public bool ReflectionsOn;
	public bool CrosshairOn;
	public Material Classic_WallMaterial;
}
