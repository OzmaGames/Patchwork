using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : MonoBehaviour {

	public int Segments = 1;
	private int CurrentSegment = 0;
	private float SegmentScale = 1.0f;
	private float InnerRadius = 1.0f;
	private float OuterRadius = 1.0f;

	public Texture2D[] PatternTextures;

	bool placed;
	float size = 0.0f;
	float maxSize = 0.0f;

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
		List<Vector4> extras = new List<Vector4>();
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
			Vector2 uvA = new Vector3(outerUVPointA.x, outerUVPointA.y);
			Vector2 uvB = new Vector3(0.5f, 0.5f);
			Vector2 uvC = new Vector3(outerUVPointB.x, outerUVPointB.y);
			Vector2 uvD = new Vector3(0.5f, 0.5f);

			uvs.Add(uvA);
			uvs.Add(uvB);
			uvs.Add(uvC);
			uvs.Add(uvD);

			Vector4 extraA = new Vector4(uvA.x * outerRadius, uvA.y * outerRadius, 0.0f, 0.0f);
			Vector4 extraB = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			Vector4 extraC = new Vector4(uvC.x * outerRadius, uvC.y * outerRadius, 0.0f, 0.0f);
			Vector4 extraD = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			extras.Add(extraA);
			extras.Add(extraB);
			extras.Add(extraC);
			extras.Add(extraD);

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
		mesh.tangents = extras.ToArray();
		mesh.triangles = indices.ToArray();
//		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		return mesh;
	}

	static float Saturate(float x)
	{
		return Mathf.Clamp01(x);
		//return Mathf.Min(0.0f, Mathf.Max(x, 1.0f));
	}
	static float SmuttStep(float x, float y, float z)
	{
		return Saturate((z - x) / (y - x));
	}
	static float Mix(float x, float y, float a)
	{
		return (x * (1.0f - a)) + (y * a);
	}

	public static Texture2D CreateGradientTexture(Color[] colors)
	{
		const int size = 256;
		float stepSize = 1.0f / (colors.Length - 0);

		Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
		for(int x = 0; x < size; ++x)
		{
			float index = x / 255.0f;

			// Mix color.
			float r = colors[0].r;
			float g = colors[0].g;
			float b = colors[0].b;
			float step = stepSize;
			for(int i = 1; i < colors.Length; ++i)
			{
				r = Mix(r, colors[i].r, SmuttStep(step, step + stepSize, index));
				g = Mix(g, colors[i].g, SmuttStep(step, step + stepSize, index));
				b = Mix(b, colors[i].b, SmuttStep(step, step + stepSize, index));
				step += stepSize;
			}

			// Output a row of color to texture.
			for(int y = 0; y < size; ++y)
			{
				tex.SetPixel(x, y, new Color(r, g, b, 1.0f));
			}
		}
		tex.Apply();
		
		return tex;
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
	void Start ()
	{
	}

	public void Generate(int segments, Texture2D[] patternTextures, Color[] colors, Texture2D gradientTexture)
	{
		// Setup initial values.
		Segments = segments;
		CurrentSegment = 1;

		InnerRadius = 0.0f;
		OuterRadius = Segments * SegmentScale;

		PatternTextures = patternTextures;

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = CreateCircle(InnerRadius, OuterRadius);
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.material.shader = Shader.Find("Custom/CirclePatch");
		renderer.material.mainTexture = CreatePatternTexture((int)(Random.value * 255.0f));
		renderer.material.SetTexture("_PatternTexture1", PatternTextures[0]);
		renderer.material.SetTexture("_PatternTexture2", PatternTextures[1]);
		size = CurrentSegment * SegmentScale;
		maxSize = size;
		renderer.material.SetFloat("_CirclePatchSize", size);
		renderer.material.SetFloat("_CirclePatchMaxSize", size);
		renderer.material.SetFloat("_CirclePatchRadius", OuterRadius);
		renderer.material.SetFloat("_CirclePatchLayer", CurrentSegment);
		renderer.material.SetColor("_Color0", colors[0]);
		renderer.material.SetColor("_Color1", colors[1]);
		renderer.material.SetColor("_Color2", colors[2]);
		renderer.material.SetColor("_Color3", colors[3]);
		renderer.material.SetTexture("_GradientTexture", gradientTexture);
	}

	public void Place()
	{
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;
		renderer.material.SetFloat("_CirclePatchSize", size);
		renderer.material.SetFloat("_CirclePatchMaxSize", maxSize); 
		renderer.material.SetFloat("_CirclePatchRadius", OuterRadius);
		renderer.material.SetFloat("_CirclePatchLayer", CurrentSegment);
		placed = true;
	}

	public void NextSegment()
	{
		if(CurrentSegment < Segments)
		{
			++CurrentSegment;
			maxSize = CurrentSegment * SegmentScale;;
		}
	}
	
	void Update ()
	{
		if(placed)
		{
			if(size < maxSize)
			{
				MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
				renderer.material.SetFloat("_CirclePatchSize", size);
				renderer.material.SetFloat("_CirclePatchMaxSize", maxSize); 
				renderer.material.SetFloat("_CirclePatchRadius", OuterRadius);
				renderer.material.SetFloat("_CirclePatchLayer", CurrentSegment);
				size += 0.01f;
			}
		}
	}
}
