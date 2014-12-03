using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : MonoBehaviour {

	const int MAX_PATTERNS = 4;
	const float ZPosAdd = -0.001f;

	static Mesh GeneratedMesh;
	static float ZPos = ZPosAdd;

	public int Segments = 1;
	private int CurrentSegment = 0;
	private float SegmentScale = 1.0f;
	private float InnerRadius = 1.0f;
	private float OuterRadius = 1.0f;

	public Texture2D[] PatternTextures;

	bool collided = false;
	bool placed = false;
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

	public static void GenerateSegments(int numSegments, float segmentSize)
	{
		GeneratedMesh = new Mesh();
		GeneratedMesh.subMeshCount = numSegments;

		const float requiredGranularity = 40.0f;
		const float granularity = (2.0f * Mathf.PI) / requiredGranularity;

		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Vector2> uvs2 = new List<Vector2>();
		List<Vector4> extras = new List<Vector4>();
		List<int[]> submeshIndices = new List<int[]>();
		int submeshIndicesStart = 0;
		for(int s = 0; s < numSegments; ++s)
		{
			float innerRadius = 0.0f;//s * segmentSize;
			float outerRadius = (s + 1) * segmentSize;

			// Generate points for edges.
			List<Vector2> innerPoints = new List<Vector2>();
			List<Vector2> outerPoints = new List<Vector2>();
			List<Vector2> uvr = new List<Vector2>();
			for(float phi = 0.0f; phi < (Mathf.PI * 2.0f); phi += granularity)
			{
				float ca = Mathf.Cos(phi);
				float sa = Mathf.Sin(phi);
				innerPoints.Add(new Vector2(innerRadius * ca, innerRadius * sa));
				outerPoints.Add(new Vector2(outerRadius * ca, outerRadius * sa));
				uvr.Add(new Vector2((ca * 0.5f) + 0.5f, (sa * 0.5f) + 0.5f));
			}

			// Generate triangles.
			List<int> indices = new List<int>();
			for(int i = 0; i < outerPoints.Count; ++i)
			{
				Vector2 innerPointA = innerPoints[i];
				Vector2 innerPointB = innerPoints[(i + 1) % innerPoints.Count];
				Vector2 outerPointA = outerPoints[i];
				Vector2 outerPointB = outerPoints[(i + 1) % outerPoints.Count];
				
				Vector3 a = new Vector3(outerPointA.x, outerPointA.y, 0.0001f * s);
				Vector3 b = new Vector3(innerPointA.x, innerPointA.y, 0.0001f * s);
				Vector3 c = new Vector3(outerPointB.x, outerPointB.y, 0.0001f * s);
				Vector3 d = new Vector3(innerPointB.x, innerPointB.y, 0.0001f * s);
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

				float uvScale = outerRadius;
				Vector4 extraA = new Vector4(uvA.x * uvScale, uvA.y * uvScale, 0.0f, 0.0f);
				Vector4 extraB = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				Vector4 extraC = new Vector4(uvC.x * uvScale, uvC.y * uvScale, 0.0f, 0.0f);
				Vector4 extraD = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				extras.Add(extraA);
				extras.Add(extraB);
				extras.Add(extraC);
				extras.Add(extraD);
				
				uvs2.Add(new Vector2(0.0f, 0.0f));
				uvs2.Add(new Vector2(0.0f, 1.0f));
				uvs2.Add(new Vector2(1.0f, 0.0f));
				uvs2.Add(new Vector2(1.0f, 1.0f));
				
				indices.Add((i * 4) + 0 + submeshIndicesStart);
				indices.Add((i * 4) + 1 + submeshIndicesStart);
				indices.Add((i * 4) + 2 + submeshIndicesStart);
				indices.Add((i * 4) + 2 + submeshIndicesStart);
				indices.Add((i * 4) + 1 + submeshIndicesStart);
				indices.Add((i * 4) + 3 + submeshIndicesStart);
			}

			submeshIndices.Add(indices.ToArray());
			submeshIndicesStart = vertices.Count;
		}

		GeneratedMesh.vertices = vertices.ToArray();
		GeneratedMesh.uv = uvs.ToArray();
		GeneratedMesh.uv2 = uvs2.ToArray();
		GeneratedMesh.tangents = extras.ToArray();
		// Add triangles to submesh.
		for(int s = 0; s < numSegments; ++s)
		{
			GeneratedMesh.SetTriangles(submeshIndices[s], s);
		}
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();
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
		//float stepSize = 1.0f / (colors.Length - 0);
		int segmentSize = size / (colors.Length / 2);

		Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
		/*for(int x = 0; x < size; ++x)
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

			// Output a column of color to texture.
			for(int y = 0; y < segmentSize; ++y)
			{
				tex.SetPixel(x, y, new Color(r, g, b, 1.0f));
			}
		}*/
		/*for(int s = 0; s < colors.Length; ++s)
		{
			for(int x = 0; x < size; ++x)
			{
				float index = x / 255.0f;
			
				// Mix color.
				float r = Mix(0.0f, colors[s].r, SmuttStep(0.0f, 0.5f, index));
				float g = Mix(0.0f, colors[s].g, SmuttStep(0.0f, 0.5f, index));
				float b = Mix(0.0f, colors[s].b, SmuttStep(0.0f, 0.5f, index));
				r = Mix(r, 1.0f, SmuttStep(0.5f, 1.0f, index));
				g = Mix(g, 1.0f, SmuttStep(0.5f, 1.0f, index));
				b = Mix(b, 1.0f, SmuttStep(0.5f, 1.0f, index));

				// Output a column of color to texture.
				for(int y = (s * segmentSize); y < ((s + 1) * segmentSize); ++y)
				{
					tex.SetPixel(x, y, new Color(r, g, b, 1.0f));
				}
			}
		}*/
		for(int s = 0; s < colors.Length; s += 2)
		{
			int ymin = ((s / 2) * segmentSize);
			int ymax = (((s / 2) + 1) * segmentSize);

			for(int x = 0; x < size; ++x)
			{
				float index = x / 255.0f;
				
				// Mix color.
				float r = Mix(colors[s].r, colors[s + 1].r, SmuttStep(0.0f, 1.0f, index));
				float g = Mix(colors[s].g, colors[s + 1].g, SmuttStep(0.0f, 1.0f, index));
				float b = Mix(colors[s].b, colors[s + 1].b, SmuttStep(0.0f, 1.0f, index));

				// Output a column of color to texture.
				for(int y = ymin; y < ymax; ++y)
				{
					tex.SetPixel(x, y, new Color(r, g, b, 1.0f));
				}
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

	public float GetSize()
	{
		return size;
	}

	public float GetMaxSize()
	{
		return maxSize;
	}

	public bool CollidesAgainst(CirclePatch patch)
	{
		float x1 = transform.position.x;
		float y1 = transform.position.y;
		float x2 = patch.transform.position.x;
		float y2 = patch.transform.position.y;
		float r1 = size * SegmentScale;
		float r2 = patch.GetSize() * SegmentScale;

		float distance = ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
		float sumRadius = ((r1 + r2) * (r1 + r2));
		if(distance <= sumRadius)
		{
			return true;
		}

		return false;
	}
	
	public void Generate(int segments, Texture2D[] patternTextures, Color[] colors, Texture2D gradientTexture)
	{
		transform.position = new Vector3(0.0f, 0.0f, ZPos);
		ZPos += ZPosAdd;

		// Setup initial values.
		Segments = segments;
		CurrentSegment = 1;

		InnerRadius = 0.0f;
		OuterRadius = Segments * SegmentScale;

		PatternTextures = patternTextures;

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.materials = new Material[GeneratedMesh.subMeshCount];
		Material material;
		List<string> shaderKeywords;
		float numColors = (colors.Length) / 2.0f;
		float colorOffset = 0.05f;//(1.0f / numColors) / 2.0f;
		int prevPattern = -1;
		for(int s = 0; s < GeneratedMesh.subMeshCount; ++s)
		{
			material = renderer.materials[s];
			material.shader = Shader.Find("Custom/CirclePatch");
			material.mainTexture = CreatePatternTexture((int)(Random.value * 255.0f));
			material.SetTexture("_FabricTexture", PatternTextures[0]);
			material.SetTexture("_PatternTexture2", PatternTextures[1]);
			size = s * SegmentScale;
			maxSize = size + SegmentScale;
			material.SetVector("_CirclePatchSize", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			material.SetFloat("_CirclePatchRadius", OuterRadius);
			material.SetFloat("_CirclePatchLayer", CurrentSegment + s);
			int paletteIndex = Random.Range(1, ((int)numColors));
			material.SetFloat("_CirclePalette", (((float)paletteIndex) / numColors) + colorOffset);
			material.SetTexture("_GradientTexture", gradientTexture);
			material.SetColor("_BaseColor1", colors[paletteIndex * 2]);
			material.SetColor("_BaseColor2", colors[(paletteIndex * 2) + 1]);
			material.SetColor("_ComplementColor1", colors[0]);
			material.SetColor("_ComplementColor2", colors[1]);
			int pattern = Random.Range(0, MAX_PATTERNS);
			if(pattern == prevPattern)
			{
				pattern = (pattern + 1) % MAX_PATTERNS;
			}
			shaderKeywords = new List<string> { "DO_SEGMENT_" + pattern };
			material.shaderKeywords = shaderKeywords.ToArray();
			prevPattern = pattern;
		}
		material = renderer.materials[0];
		material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, 0.0f));
	}

	public void Place()
	{
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;
		Material material;
		for(int s = 0; s < CurrentSegment; ++s)
		{
			material = renderer.materials[s];
			material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchRadius", OuterRadius);
			material.SetFloat("_CirclePatchLayer", CurrentSegment);
		}
		placed = true;
	}

	public void NextSegment()
	{
		if(CurrentSegment < Segments)
		{
			++CurrentSegment;
			maxSize = (CurrentSegment * SegmentScale);
		}
	}

	public void SetCollided(bool collide)
	{
		collided = collide;
	}

	public bool HasCollided()
	{
		return collided;
	}
	
	void Update ()
	{
		if((placed) && (!collided))
		{
			if(size < maxSize)
			{
				MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
				Material material;
				for(int s = 0; s < CurrentSegment; ++s)
				{
					material = renderer.materials[s];
					material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
					material.SetFloat("_CirclePatchRadius", OuterRadius);
					material.SetFloat("_CirclePatchLayer", CurrentSegment);
				}
				size += 0.01f;
			}
		}
	}
}
