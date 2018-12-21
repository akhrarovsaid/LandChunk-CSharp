using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class LandChunk : MonoBehaviour {

	public int chunkSize = 10;					// Size of chunk in vertices
	public int octaves = 1;						// Number of layered octaves to apply
	public int heightMultiplier = 10;			// Heightmap samples are multiplied by this to acquire vertex height
	public AnimationCurve heightModifier;		// Approximate distribution of terrain

	public float frequency = 1f;				// Noise sampling frequency
	public Vector2 offset = Vector2.zero;		// Noise starting offset

	public Gradient terrainGradient;			// Sampled from heightmap and evaluated to find height colour

	private float[,] heightMap;					// heightMap containing geometry of mesh
	private Mesh mesh;							// Respective mesh object

	// Use this for initialization
	void Start () {
		GenerateTerrain();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnValidate()
	{
		if (frequency < 0.001f)
		{
			frequency = 0.001f;
		}

		if (chunkSize < 3)
		{
			chunkSize = 3;
		}

		if (octaves < 1)
		{
			octaves = 1;
		}

		if (heightMultiplier < 1)
		{
			heightMultiplier = 1;
		}
		
	}

	public int GetChunkSize() { return chunkSize; }

	/* Generates a flat plane from vertices and triangles and returns the mesh object */
	public Mesh GeneratePlane()
	{
		Mesh mesh = new Mesh();
		mesh.name = "Procedural Plane";

		Vector3[] vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
		int[] triangles = new int[chunkSize * chunkSize * 6];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		Vector2[] uvs = new Vector2[(chunkSize + 1) * (chunkSize + 1)];
		Debug.Log(vertices.Length +"," + chunkSize + 1);

		int ti = 0, vi = 0;
		for (int i = 0, z = 0; z <= chunkSize; z++)
		{
			for (int x = 0; x <= chunkSize; x++, i++)
			{
				vertices[i] = new Vector3(x, 0, z);
				uvs[i] = new Vector2((float)x, (float)z);
				tangents[i] = tangent;

				if (z < chunkSize && x < chunkSize)
				{
					triangles[ti] = vi;
					triangles[ti + 3] = triangles[ti + 2] = vi + 1;
					triangles[ti + 4] = triangles[ti + 1] = vi + chunkSize + 1;
					triangles[ti + 5] = vi + chunkSize + 2;
					ti += 6;
					vi++;
				}
			}
			vi++;
		}

		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.tangents = tangents;
		mesh.RecalculateNormals();

		MeshFilter filter = GetComponent<MeshFilter>();
		filter.mesh = mesh;

		return mesh;
	}

	public float[,] GenerateHeightMap()
	{
		float[,] result = new float[chunkSize + 1, chunkSize + 1];
		float weight = 1f;
		float wavelength = frequency;
		int sampleSize = chunkSize + 1;

		float minHeight = float.MaxValue, maxHeight = float.MinValue;

		for (int i = 0; i < octaves; i++)
		{

			for (int x = 0; x < sampleSize; x++)
			{
				for (int y = 0; y < sampleSize; y++)
				{
					float inputy = ((float)y / sampleSize) * wavelength;
					float inputX = ((float)x / sampleSize) * wavelength;

					float noiseValue = weight * Mathf.PerlinNoise(inputX + offset.x, inputy + offset.y);
					result[x, y] += noiseValue;
				}
			}

			weight = weight / 2f;
			wavelength = frequency * 1.75f;
		}

		return result;
	}

	public void ApplyHeightMap()
	{
		Vector3[] vertices = mesh.vertices;
		heightMap = new float[chunkSize + 1, chunkSize + 1];

		//float minHeight = float.MaxValue, maxHeight = float.MinValue;

		float weight = 1f;
		float amplitude = 1f;
		int layers = 0;

		while (layers < octaves)
		{

			for (int i = 0, z = 0; z <= chunkSize; z++)
			{
				for (int x = 0; x <= chunkSize; x++, i++)
				{
					float inputZ = ((float)z / chunkSize + 1) * frequency + offset.y;
					float inputX = ((float)x / chunkSize + 1) * frequency + offset.x;

					heightMap[x, z] += weight * Mathf.PerlinNoise(inputZ * amplitude, inputX * amplitude);
					//+ weight/2f * Mathf.PerlinNoise(inputZ * 2f, inputX * 2f)
					//+ weight/4f * Mathf.PerlinNoise(inputZ * 4f, inputX * 4f);

					vertices[i].y = heightMultiplier * heightMap[x, z] * heightModifier.Evaluate(heightMap[x, z]);

					//float result = heightMap[x, z];
					//if (result < minHeight) minHeight = result;
					//if (result > maxHeight) maxHeight = result;
				}
			}

			weight /= 2f;
			amplitude += 2f;
			layers++;
		}

		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}

	public float normalizeSample(float min, float max, float sample)
	{
		return (sample - min) / (max - min);
	}

	public void GenerateTerrain()
	{
		mesh = GeneratePlane();
		//heightMap = GenerateHeightMap();
		ApplyHeightMap();
		ApplyColorData();
	}

	public void ApplyColorData()
	{
		Color[] colorData = new Color[mesh.vertices.Length];

		for (int i = 0, z = 0; z <= chunkSize; z++)
		{
			for (int x = 0; x <= chunkSize; x++, i++)
			{
				float height = heightMap[x, z];

				colorData[i] = terrainGradient.Evaluate(height);
				

			}
		}

		mesh.colors = colorData;
		
	}
}
