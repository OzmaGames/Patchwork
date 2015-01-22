using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : GamePieceBase {

	const int MAX_PATTERNS = 4;
	const float FLASH_TIME = 1.0f;
	const float FLASH_SPEED = 4.0f;
	const float GROWTH_SPEED = 0.5f;

	public GameObject CirclePatchSizePrefab;
	GameObject circlePatchSize;
	GameObject circlePatchSymbol;

	static Mesh GeneratedMesh;

	public int Segments = 1;
	private int CurrentSegment = 0;
	private float SegmentScale = 1.0f;
	private float InnerRadius = 1.0f;
	private float OuterRadius = 1.0f;
	private float CurrentSegmentArcSize = 0.0f;

	private DecorationCircleStopper Decoration;

	public enum Symbols
	{
		Square,
		Triangle,
		Circle
	}
	private Symbols Symbol = Symbols.Square;

	public Texture2D[] PatternTextures;

	struct PatchSegment
	{
		public int colorIndex;
		public int patternIndex;
	}
	PatchSegment[] patchSegments;

	Player Owner;

	bool doneGrowing = false;
	bool collided = false;
	bool placed = false;
	float size = 0.0f;
	float maxSize = 0.0f;

	bool isFlashing = false;
	float flashValue = 0.0f;
	float flashTimer = 0.0f;

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

	public Symbols GetSymbol()
	{
		return Symbol;
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

	static int idas = 0;
	public void Generate(int segments, Texture2D[] patternTextures, Gradient[] colors, Gradient complementColor)
	{
		transform.position = new Vector3(0.0f, 0.0f, ZPos);
		ZPos += ZPosAdd;

		// Setup initial values.
		Segments = segments;
		CurrentSegment = 1;

		InnerRadius = 0.0f;
		OuterRadius = Segments * SegmentScale;

		PatternTextures = patternTextures;

		patchSegments = new PatchSegment[Segments]; 

		// Setup mesh.
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
		renderer.materials = new Material[GeneratedMesh.subMeshCount];
		Material material;
		List<string> shaderKeywords;
		int prevPatternIndex = -1;
		for(int s = 0; s < GeneratedMesh.subMeshCount; ++s)
		{
			material = renderer.materials[s];
			material.shader = Shader.Find("Custom/CirclePatch");
			material.mainTexture = CreatePatternTexture((int)(Random.value * 255.0f));
			material.SetTexture("_FabricTexture", PatternTextures[0]);
			material.SetTexture("_PatternTexture2", PatternTextures[1]);
			material.SetVector("_CirclePatchSize", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			material.SetFloat("_CirclePatchRadius", OuterRadius);
			material.SetFloat("_CirclePatchLayer", CurrentSegment + s);
			material.SetFloat("_CurrentSegmentArcSize", 0.0f);
			int paletteIndex = Random.Range(0, colors.Length);
			material.SetColor("_BaseColor1", colors[paletteIndex].colorKeys[0].color);
			material.SetColor("_BaseColor2", colors[paletteIndex].colorKeys[1].color);
			material.SetColor("_ComplementColor1", complementColor.colorKeys[0].color);
			material.SetColor("_ComplementColor2", complementColor.colorKeys[1].color);
			int patternIndex = Random.Range(0, MAX_PATTERNS);
			if(patternIndex == prevPatternIndex)
			{
				patternIndex = (patternIndex + 1) % MAX_PATTERNS;
			}
			shaderKeywords = new List<string> { "DO_SEGMENT_" + patternIndex };
			material.shaderKeywords = shaderKeywords.ToArray();
			prevPatternIndex = patternIndex;

			// Setup patch segment.
			if(s < Segments)
			{
				patchSegments[s].colorIndex = paletteIndex;
				patchSegments[s].patternIndex = patternIndex;
			}
		}
		material = renderer.materials[0];
		material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, 0.0f));
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;

		CirclePatchSizePrefab = Resources.Load<GameObject>("Prefab/TextPrefab");
		circlePatchSize = (GameObject)Instantiate(CirclePatchSizePrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z + (ZPosAdd * 0.5f)), Quaternion.identity);


		//circlePatchSize = new GameObject(gameObject.name + "_Size");
		circlePatchSize.transform.parent = gameObject.transform;
		circlePatchSize.transform.localPosition = new Vector3(0.0f, 0.0f, ZPosAdd * 0.5f);
		//TextMesh circlePatchSizeText = circlePatchSize.AddComponent<TextMesh>();
		TextMesh circlePatchSizeText = circlePatchSize.GetComponent<TextMesh>();

		// Fix aspect ratio of the text.
		float pixelRatio = (Camera.main.orthographicSize * 2.0f) / Camera.main.pixelHeight;
		circlePatchSize.transform.localScale = new Vector3(pixelRatio * 10.0f, pixelRatio * 10.0f, pixelRatio * 0.1f);
		circlePatchSizeText.fontSize = 30;
		circlePatchSizeText.text = segments.ToString();

		// Creat symbol quad.
		circlePatchSymbol = GameObject.CreatePrimitive(PrimitiveType.Quad);
		if(idas == 0)
		{
			Symbol = Symbols.Square;
			circlePatchSymbol.renderer.material.mainTexture = Resources.Load<Texture2D>("Textures/SymbolSquare");
			idas = 1;
		}
		else if(idas == 1)
		{
			Symbol = Symbols.Triangle;
			circlePatchSymbol.renderer.material.mainTexture = Resources.Load<Texture2D>("Textures/SymbolTriangle");
			idas = 2;
		}
		else
		{
			Symbol = Symbols.Circle;
			circlePatchSymbol.renderer.material.mainTexture = Resources.Load<Texture2D>("Textures/SymbolCircle");
			idas = 0;
		}
		circlePatchSymbol.renderer.material.shader = Shader.Find("Unlit/Transparent");
		circlePatchSymbol.transform.parent = gameObject.transform;
		circlePatchSymbol.transform.localPosition = new Vector3(0.0f, 0.0f, ZPosAdd * 0.25f);
		circlePatchSymbol.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
	}

	public void SetOwner(Player player)
	{
		Owner = player;
		SwapColors(Owner.Colors, Owner.ComplementColor);
	}

	public void SwapColors(Gradient[] colors, Gradient complementColor)
	{
		Material material;
		for(int s = 0; s < Segments; ++s)
		{
			material = renderer.materials[s];
			int paletteIndex = patchSegments[s].colorIndex;
			material.SetColor("_BaseColor1", colors[paletteIndex].colorKeys[0].color);
			material.SetColor("_BaseColor2", colors[paletteIndex].colorKeys[1].color);
			material.SetColor("_ComplementColor1", complementColor.colorKeys[0].color);
			material.SetColor("_ComplementColor2", complementColor.colorKeys[1].color);
		}
	}
	
	public override void Place()
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
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}
		placed = true;
	}

	public void PlaceDecoration(DecorationCircleStopper decoration)
	{
		Decoration = decoration;
		SetGrowthDone(true);
	}

	public DecorationCircleStopper GetDecoration()
	{
		return Decoration;
	}

	public void NextSegment()
	{
		if(CurrentSegment < Segments)
		{
			++CurrentSegment;
			maxSize = (CurrentSegment * SegmentScale);
			CurrentSegmentArcSize = 0.0f;
		}
	}

	public void SetCollided(bool collide)
	{
		collided = collide;
		SetGrowthDone(collide);
	}

	public bool HasCollided()
	{
		return collided;
	}
	
	public void SetGrowthDone(bool enable)
	{
		doneGrowing = enable;
		circlePatchSize.renderer.enabled = false;
		SetShowSymbol(false);
		Owner.AddScore((int)size);

		// Start flashing to notify that it is done.
		StartFlash();
	}

	void StartFlash()
	{
		isFlashing = true;
		flashValue = 0.0f;
		flashTimer = 0.0f;
	}

	void UpdateFlash()
	{
		if(flashTimer < FLASH_TIME)
		{
			Material material;
			for(int s = 0; s < CurrentSegment; ++s)
			{
				material = renderer.materials[s];
				material.SetColor("_AddColor", new Vector4(flashValue, flashValue, flashValue, 0.0f));
			}
		}
		else
		{
			Material material;
			for(int s = 0; s < CurrentSegment; ++s)
			{
				material = renderer.materials[s];
				material.SetColor("_AddColor", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			}
			isFlashing = false;
		}
		flashValue += FLASH_SPEED * Time.deltaTime;
		if(flashValue > 0.5f)
		{
			flashValue = -0.5f;
		}

		flashTimer += Time.deltaTime;
	}
	
	public void SetShowSymbol(bool show)
	{
		circlePatchSymbol.renderer.enabled = show;
	}

	public bool HasStoppedGrowing()
	{
		return doneGrowing;
	}

	public override void SetPosition(float x, float y)
	{
		transform.position = new Vector3(x, y, transform.position.z);
	}
	
	void Update ()
	{
		if(placed)
		{
			if(!doneGrowing)
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
						material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
					}
					//CurrentSegmentArcSize += 10.0f;
					//if(CurrentSegmentArcSize > 360.0f)
					{
						size += GROWTH_SPEED * Time.deltaTime;
						//CurrentSegmentArcSize = 0.0f;
					}
				}
				else if(CurrentSegment >= Segments)
				{
					SetGrowthDone(true);
				}
			}
			else if(isFlashing)
			{
				UpdateFlash();
			}
		}
	}
}
