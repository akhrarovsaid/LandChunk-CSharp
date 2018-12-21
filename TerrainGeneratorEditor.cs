using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LandChunk))]
public class TerrainGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		

		LandChunk myScript = (LandChunk)target;
		if (GUILayout.Button("Generate Terrain"))
		{
			myScript.GenerateTerrain();
		}

		if (GUILayout.Button("Simulate Water"))
		{
			int size = myScript.GetChunkSize();
			float seaLevel = myScript.GetComponent<Renderer>().sharedMaterial.GetFloat("Vector1_3DF9EEE7");
			myScript.GetComponentInChildren<NoiseWater>().Initialize(seaLevel, size);
		}
	}
}
