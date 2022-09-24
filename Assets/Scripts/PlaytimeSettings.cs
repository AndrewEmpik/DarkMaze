using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlaytimeSettings : ScriptableObject
{
	public int MazeSize;
	public float WallHeight;
	public float LightIntensity;
	public bool AdditionLightOn;
	public int TypeOfAddLight;
	public int CameraPosition;
	public bool PostEffectsOn;
	public bool ReflectionsOn;
	public bool CrosshairOn;
	public Material WallMaterial;
}
