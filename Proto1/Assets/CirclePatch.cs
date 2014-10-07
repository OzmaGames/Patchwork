using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : MonoBehaviour {

	public float InnerRadius = 0.2f;
	public float OuterRadius = 1.0f;

	bool placed;
	float size = 0.0f;

	class PatchEdge
	{
		Vector2 PointA;
		Vector2 PointB;
		Vector2 Origin;
	};
	
	List<PatchEdge> innerEdges = new List<PatchEdge>();
	List<PatchEdge> outerEdges = new List<PatchEdge>();

	static Mesh CreateCircle(float innerRadius, float outerRadius)
	{
		const float requiredGranularity = 40.0f;
		const float granularity = (2.0f * Mathf.PI) / requiredGranularity;

		List<Vector2> innerPoints = new List<Vector2>();
		List<Vector2> outerPoints = new List<Vector2>();
		List<Vector2> uvr = new List<Vector2>();
		for(float angle = 0.0f; angle < (Mathf.PI * 2.0f); angle += granularity)
		{
			innerPoints.Add(new Vector2(innerRadius * Mathf.Cos(angle), innerRadius * Mathf.Sin(angle)));
			outerPoints.Add(new Vector2(outerRadius * Mathf.Cos(angle), outerRadius * Mathf.Sin(angle)));
			uvr.Add(new Vector2((Mathf.Cos(angle) * 0.5f) + 0.5f, (Mathf.Sin(angle) * 0.5f) + 0.5f));
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector2> uvs2 = new List<Vector2>();
		List<int> indices = new List<int>();
		for(int i = 0; i < outerPoints.Count; ++i)
		{
			Vector2 innerPointA = innerPoints[i];
			Vector2 innerPointB = innerPoints[(i + 1) % innerPoints.Count];
			Vector2 outerPointA = outerPoints[i];
			Vector2 outerPointB = outerPoints[(i + 1) % outerPoints.Count];
			
			Vector3 a = new Vector3(outerPointA.x, outerPointA.y);
			Vector3 b = new Vector3(innerPointA.x, innerPointA.y);
			Vector3 c = new Vector3(outerPointB.x, outerPointB.y);
			Vector3 d = new Vector3(innerPointB.x, innerPointB.y);
			vertices.Add(a);
			vertices.Add(b);
			vertices.Add(c);
			vertices.Add(d);


			Vector2 outerUVPointA = uvr[i];
			Vector2 outerUVPointB = uvr[(i + 1) % uvr.Count];
			Vector3 uvA = new Vector3(outerUVPointA.x, outerUVPointA.y);
			Vector3 uvB = new Vector3(0.5f, 0.5f);
			Vector3 uvC = new Vector3(outerUVPointB.x, outerUVPointB.y);
			Vector3 uvD = new Vector3(0.5f, 0.5f);

			uvs.Add(uvA);
			uvs.Add(uvB);
			uvs.Add(uvC);
			uvs.Add(uvD);

			uvs2.Add(new Vector2(0.0f, 0.0f));
			uvs2.Add(new Vector2(0.0f, 1.0f));
			uvs2.Add(new Vector2(1.0f, 0.0f));
			uvs2.Add(new Vector2(1.0f, 1.0f));

			indices.Add((i * 4) + 0);
			indices.Add((i * 4) + 1);
			indices.Add((i * 4) + 2);
			indices.Add((i * 4) + 2);
			indices.Add((i * 4) + 1);
			indices.Add((i * 4) + 3);
		}

		/*vertices.Add(new Vector3(100.0f, 100.0f, 0.0f));
		vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
		vertices.Add(new Vector3(0.0f, 100.0f, 0.0f));
		uvs.Add(new Vector2(0.0f, 0.0f));
		uvs.Add(new Vector2(0.0f, 0.0f));
		uvs.Add(new Vector2(0.0f, 0.0f));
		indices.Add(0);
		indices.Add(1);
		indices.Add(2);*/

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.uv2 = uvs2.ToArray();
		mesh.triangles = indices.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		return mesh;
	}

	static Texture2D CreatePatternTexture(int uniquePatternSeed)
	{
		const float granularity = 128.0f;

		Texture2D tex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
		for(int y = 0; y < 256; ++y)
		{
			for(int x = 0; x < 256; ++x)
			{
				tex.SetPixel(
					x ^ uniquePatternSeed,
					y ^ uniquePatternSeed,
					new Color(x / granularity, y / granularity, ((x ^ y) ^ uniquePatternSeed) / granularity, 1.0f));
			}
		}
		tex.Apply();

		return tex;
	}

	// Use this for initialization
	void Start () {
	}

	public void Generate(float innerRadius, float outerRadius)
	{
		InnerRadius = innerRadius;
		OuterRadius = outerRadius;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = CreateCircle(InnerRadius, OuterRadius);
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		//renderer.material.shader = Shader.Find("Unlit/Texture");
		renderer.material.shader = Shader.Find("Custom/CirclePatch");
		renderer.material.mainTexture = CreatePatternTexture((int)(Random.value * 255.0f));
		size = 0.1f;
		renderer.material.SetFloat("_CirclePatchSize", size);
	}

	public void Place()
	{
		placed = true;
	}
	
	// Update is called once per frame
	void Update () {
		if(placed)
		{
			if(size < 1.0f)
			{
				MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
				renderer.material.SetFloat("_CirclePatchSize", size);
				size += 0.01f;
			}
		}
	}
}
