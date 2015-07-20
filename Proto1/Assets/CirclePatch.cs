using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : GamePieceBase {

	public const int MAX_PATTERNS = 4;
	public const float FLASH_TIME = 1.0f;
	public const float SegmentScale = 1.0f;
	const float GROWTH_SPEED = 0.5f;

	public GameObject patchObject;
	GameObject circlePatchSize;
	Symbol patchSymbol;

	Texture2D[] circlePatchSizes;

	public Color AddColor = Color.black;
	bool doColorOverlay = false;

	static Mesh GeneratedMesh;

	public int Segments = 1;
	int CurrentSegment = 0;
	float InnerRadius = 1.0f;
	float OuterRadius = 1.0f;
	float CurrentSegmentArcSize = 0.0f;

	DecorationCircleStopper Decoration;

	static GameObject[] SymbolPrefabs;
	static GameObject[] PatchSizeNumberPrefabs;
	static Texture2D[] RandomPatternTextures;

	public Texture2D[] PatternTextures;

	struct PatchSegment
	{
		public int colorIndex;
		public int patternIndex;
	}
	PatchSegment[] patchSegments;

	Player Owner;

	bool segmentDoneGrowing = false;
	bool doneGrowing = false;
	bool collided = false;
	bool isPlaced = false;
	float size = 0.0f;
	float maxSize = 0.0f;

	Color flashColorStart = Color.black;
	Color flashColorEnd = Color.white;
	bool isFlashing = false;
	float flashValue = 0.0f;
	float flashTimer = 0.0f;

	Color highlightColor = Color.black;
	bool doHighlight = false;

	class PatchEdge
	{
		Vector2 PointA;
		Vector2 PointB;
		Vector2 Origin;
	};
	
	List<PatchEdge> innerEdges = new List<PatchEdge>();
	List<PatchEdge> outerEdges = new List<PatchEdge>();

	[System.Serializable]
	public class PatchConfig
	{
		public int NumSegments = 1;
		public float SegmentSize = 1.0f;
		public int[] PaletteIndices;
		public int[] PatternIndices;

		public PatchConfig(int numSegments, float segmentSize, int numPalettes, int numPatterns)
		{
			NumSegments = numSegments;
			SegmentSize = segmentSize;
			PaletteIndices = new int[numSegments];
			PatternIndices = new int[numSegments];

			int prevPatternIndex = -1;
			for(int s = 0; s < NumSegments; ++s)
			{
				int paletteIndex = Random.Range(0, numPalettes);
				int patternIndex = Random.Range(0, numPatterns);
				if(patternIndex == prevPatternIndex)
				{
					patternIndex = (patternIndex + 1) % numPatterns;
				}
				prevPatternIndex = patternIndex;

				PaletteIndices[s] = paletteIndex;
				PatternIndices[s] = patternIndex;
			}
		}

		public PatchConfig(int numSegments, float segmentSize, int[] paletteIndices, int[] patternIndices)
		{
			if((numSegments != paletteIndices.Length) || (numSegments != patternIndices.Length))
			{
				Debug.LogError("Unexpected length of segments.");
				return;
			}
			NumSegments = numSegments;
			SegmentSize = segmentSize;
			PaletteIndices = paletteIndices;
			PatternIndices = patternIndices;
		}
	}

	public static void GenerateSegments(int numSegments, float segmentSize, GameObject[] symbolPrefabs, GameObject[] patchSizeNumberPrefabs, Camera patchRendererCamera)
	{
		PatchRendererCamera = patchRendererCamera;

		SymbolPrefabs = symbolPrefabs;
		PatchSizeNumberPrefabs = patchSizeNumberPrefabs;
		RandomPatternTextures = new Texture2D[6];
		RandomPatternTextures[0] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[1] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[2] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[3] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[4] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[5] = CreatePatternTexture((int)(Random.value * 255.0f));

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

	public Symbol GetSymbol()
	{
		return patchSymbol;
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

	IEnumerator RenderPatch()
	{
		yield return new WaitForEndOfFrame();
		CachePatch();
	}
	
	static Camera PatchRendererCamera;
	public GameObject RealPatch;
	public Texture2D CachedPatchTexture;
	void CachePatch()
	{
		Material material;
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();

		// Set to patch renderer camera layer and center patch.
		Vector3 oldPos = transform.position;
		patchObject.layer = 11;
		transform.position = Vector3.zero;
		
		// Enable patch.
		patchObject.SetActive(true);
		
		// Create a cached version of the patch.
		float tmpSize = Segments * SegmentScale;
		for(int s = 0; s < Segments; ++s)
		{
			material = meshRenderer.materials[s];
			material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, tmpSize, (s * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchLayer", Segments);
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}

		RenderTexture oldActive = RenderTexture.active;
		RenderTexture camTex = PatchRendererCamera.targetTexture;
		RenderTexture.active = camTex;

		// Render patch.
		PatchRendererCamera.Render();

		// Copy texture.
		Texture2D cachedPatchTexture = new Texture2D(camTex.width, camTex.height, TextureFormat.ARGB32, false);
		cachedPatchTexture.ReadPixels(new Rect(0.0f, 0.0f, camTex.width, camTex.height), 0, 0);
		cachedPatchTexture.Apply();

		RenderTexture.active = oldActive;

		// Reset patch.
		for(int s = 0; s < Segments; ++s)
		{
			material = meshRenderer.materials[s];
			if(s < CurrentSegment)
			{
				material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
				material.SetFloat("_CirclePatchLayer", CurrentSegment);
				material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
			}
			else
			{
				material.SetVector("_CirclePatchSize", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
				material.SetFloat("_CirclePatchLayer", CurrentSegment + s);
				material.SetFloat("_CurrentSegmentArcSize", 0.0f);
			}
		}

		// Hide patch.
		patchObject.SetActive(false);
		
		// Restore layer and position.
		patchObject.layer = 0;
		transform.position = oldPos;
		
		// Update cache.
		CachedPatchTexture = cachedPatchTexture;
	}

	public void Generate(PatchConfig config, Texture2D[] patternTextures, Gradient[] colors, Gradient complementColor)
	{
		transform.position = new Vector3(0.0f, 0.0f, 0.0f);//Game.BGZPos);
		//Game.BGZPos += Game.ZPosAdd;

		// Setup initial values.
		Segments = config.NumSegments;
		CurrentSegment = 1;

		InnerRadius = 0.0f;
		OuterRadius = Segments * SegmentScale;

		PatternTextures = patternTextures;

		patchSegments = new PatchSegment[Segments]; 

		// Setup mesh.
		MeshFilter meshFilter = patchObject.GetComponent<MeshFilter>();
		meshFilter.mesh = GeneratedMesh;
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();
		meshRenderer.materials = new Material[GeneratedMesh.subMeshCount];
		Material material;
		List<string> shaderKeywords;
		for(int s = 0; s < GeneratedMesh.subMeshCount; ++s)
		{
			material = meshRenderer.materials[s];
			material.shader = Shader.Find("Custom/CirclePatch");
			material.mainTexture = RandomPatternTextures[Random.Range(0,6)];
			material.SetTexture("_FabricTexture", PatternTextures[0]);
			material.SetTexture("_PatternTexture2", PatternTextures[1]);
			material.SetVector("_CirclePatchSize", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			material.SetFloat("_CirclePatchRadius", OuterRadius);
			material.SetFloat("_CirclePatchLayer", CurrentSegment + s);
			material.SetFloat("_CurrentSegmentArcSize", 0.0f);

			// Setup patch segment.
			if(s < Segments)
			{
				int paletteIndex = config.PaletteIndices[s];
				material.SetColor("_BaseColor1", colors[paletteIndex].colorKeys[0].color);
				material.SetColor("_BaseColor2", colors[paletteIndex].colorKeys[1].color);
				material.SetColor("_ComplementColor1", complementColor.colorKeys[0].color);
				material.SetColor("_ComplementColor2", complementColor.colorKeys[1].color);

				int patternIndex = config.PatternIndices[s];
				shaderKeywords = new List<string> { "DO_SEGMENT_" + patternIndex };
				material.shaderKeywords = shaderKeywords.ToArray();

				patchSegments[s].colorIndex = paletteIndex;
				patchSegments[s].patternIndex = patternIndex;
			}
		}
		material = meshRenderer.materials[0];
		material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, 0.0f));
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;

		//StartCoroutine(RenderPatch());
		//CachePatch();

		// Create mesh used for presenting the cached copy of the patch.
		/*meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = Helpers.GenerateQuad(12.0f, 12.0f, 1.0f);
		material = gameObject.AddComponent<MeshRenderer>().material;
		material.mainTexture = CachedPatchTexture;
		material.shader = Shader.Find("Custom/CirclePatchCached");
		material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, CurrentSegment * SegmentScale, 0.0f));
*/
		// Create symbol.
		patchSymbol = Symbol.Instantiate(Symbol.GetRandomSymbolType()).GetComponent<Symbol>();
		patchSymbol.transform.SetParent(gameObject.transform, false);
		patchSymbol.transform.localPosition = new Vector3(0.0f, 0.0f, Game.ZPosAdd * 0.25f );

		// Create size quad.
		circlePatchSize = Instantiate(PatchSizeNumberPrefabs[Segments - 1]);
		circlePatchSize.transform.SetParent(gameObject.transform, false);
		circlePatchSize.transform.localPosition = new Vector3(0.5f, 0.5f, Game.ZPosAdd * 0.35f);
	}
	
	public override Player GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(Player player)
	{
		Owner = player;
		SwapColors(Owner.Colors, Owner.ComplementColor);
	}

	public void SwapColors(Gradient[] colors, Gradient complementColor)
	{
		Material material;
		Renderer renderer = patchObject.GetComponent<Renderer>();
		for(int s = 0; s < Segments; ++s)
		{
			material = renderer.materials[s];
			int paletteIndex = patchSegments[s].colorIndex;
			material.SetColor("_BaseColor1", colors[paletteIndex].colorKeys[0].color);
			material.SetColor("_BaseColor2", colors[paletteIndex].colorKeys[1].color);
			material.SetColor("_ComplementColor1", complementColor.colorKeys[0].color);
			material.SetColor("_ComplementColor2", complementColor.colorKeys[1].color);
		}
		//CachePatch();
		//gameObject.GetComponent<MeshRenderer>().material.mainTexture = CachedPatchTexture;
	}
	
	public override void AddToDeck()
	{
		transform.position = Vector3.zero;
	}

	public override void RemoveFromDeck()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, Game.BGZPos);
	}

	public override void Place()
	{
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;
		Material material;
		Renderer renderer = patchObject.GetComponent<Renderer>();
		int s = 0;
		for(; s < CurrentSegment; ++s)
		{
			material = renderer.materials[s];
			material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchLayer", CurrentSegment);
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}
//		material = gameObject.GetComponent<MeshRenderer>().material;
//		material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
		transform.position = new Vector3(transform.position.x, transform.position.y, Game.BGZPos);
		Game.BGZPos += Game.ZPosAdd;
		PlaceChilds();
		isPlaced = true;
	}

	public void PlaceDecoration(DecorationCircleStopper decoration)
	{
		Decoration = decoration;
		if(!HasStoppedGrowing())
		{
			SetGrowthDone(true);
		}
	}

	public DecorationCircleStopper GetDecoration()
	{
		return Decoration;
	}

	public void NextSegment()
	{
		if(!HasStoppedGrowing())
		{
			++CurrentSegment;
			maxSize = (CurrentSegment * SegmentScale);
			CurrentSegmentArcSize = 0.0f;
			SetSegmentGrowthDone(false);
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

	void Grow()
	{
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();
		Material material;
		int s = 0;
		for(; s < CurrentSegment; ++s)
		{
			material = meshRenderer.materials[s];
			material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchLayer", CurrentSegment);
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}
//		material = gameObject.GetComponent<MeshRenderer>().material;
//		material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
		//CurrentSegmentArcSize += 10.0f;
		//if(CurrentSegmentArcSize > 360.0f)
		{
			size += GROWTH_SPEED * Time.deltaTime;
			if(size > maxSize)
			{
				size = maxSize;
			}
			//CurrentSegmentArcSize = 0.0f;
		}
	}
	
	public void SetSegmentGrowthDone(bool enable)
	{
		segmentDoneGrowing = enable;
		if((segmentDoneGrowing) && (CurrentSegment >= Segments))
		{
			SetGrowthDone(true);
		}
	}
	
	public bool HasSegmentStoppedGrowing()
	{
		return segmentDoneGrowing || doneGrowing;
	}
	
	public void SetGrowthDone(bool enable)
	{
		doneGrowing = enable;
		SetShowSymbol(false);
		Owner.AddScore((int)size);

		// Start flashing to notify that it is done.
		StartFlash(new Color(-0.5f, -0.5f, -0.5f), new Color(0.5f, 0.5f, 0.5f), FLASH_TIME);
//		transform.parent.GetComponent<GamePiece>().StartEffect("Done");
	}
	
	public bool HasStoppedGrowing()
	{
		return doneGrowing;
	}

	public override void SetHighlight(bool enable, Color color)
	{
		doHighlight = enable;
		highlightColor = color;
	}

	void UpdateHighlight()
	{
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();
		Material material;
		if(!doHighlight)
		{
			for(int s = 0; s < CurrentSegment; ++s)
			{
				material = meshRenderer.materials[s];
				material.SetColor("_AddColor", Color.black);
			}
			return;
		}
		for(int s = 0; s < CurrentSegment; ++s)
		{
			material = meshRenderer.materials[s];
			material.SetColor("_AddColor", highlightColor);
		}
	}

	public override void StartFlash(Color startColor, Color endColor, float time)
	{
		flashColorStart = startColor;
		flashColorEnd = endColor;
		isFlashing = true;
		flashValue = 0.0f;
		flashTimer = time;
		if(Decoration)
		{
			Decoration.StartFlash(startColor, endColor, time);
		}
	}

	void UpdateFlash()
	{
		if(!isFlashing)
		{
			return;
		}
		if(flashTimer > 0.0f)
		{
			Material material;
			Renderer renderer = patchObject.GetComponent<Renderer>();
			for(int s = 0; s < CurrentSegment; ++s)
			{
				material = renderer.materials[s];
				material.SetColor("_AddColor", Color.Lerp(flashColorStart, flashColorEnd, flashValue));
			}

			flashTimer -= Time.deltaTime;
		}
		else
		{
			Material material;
			Renderer renderer = patchObject.GetComponent<Renderer>();
			for(int s = 0; s < CurrentSegment; ++s)
			{
				material = renderer.materials[s];
				material.SetColor("_AddColor", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
			}
			isFlashing = false;
		}
		flashValue += FLASH_SPEED * Time.deltaTime;
		if(flashValue > 1.0f)
		{
			flashValue = 0.0f;
		}
	}

	public void StartColorOverlay()
	{
		doColorOverlay = true;
		UpdateColorOverlay();
	}

	public void EndColorOverlay()
	{
		doColorOverlay = false;
		UpdateColorOverlay();
	}
	
	void UpdateColorOverlay()
	{
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();
		Material material;
		for(int s = 0; s < CurrentSegment; ++s)
		{
			material = meshRenderer.materials[s];
			material.SetColor("_AddColor", AddColor);
		}
	}
	
	public override void UpdateEffect(Color addColor)
	{
		MeshRenderer meshRenderer = patchObject.GetComponent<MeshRenderer>();
		Material material;
		for(int s = 0; s < CurrentSegment; ++s)
		{
			material = meshRenderer.materials[s];
			material.SetColor("_AddColor", addColor);
		}
		UpdateChildsEffect(addColor);
	}
	
	public void SetShowSymbol(bool show)
	{
		patchSymbol.GetComponent<Renderer>().enabled = show;
		circlePatchSize.GetComponent<Renderer>().enabled = show;
	}

	public override void SetPosition(float x, float y)
	{
		transform.position = new Vector3(x, y, transform.position.z);
	}

	public override Bounds GetBounds()
	{
		return new Bounds(transform.position, new Vector3(size, size));
	}

	void OnDestroy()
	{
		circlePatchSize = null;
		patchSymbol = null;
		circlePatchSizes = null;
		Decoration = null;
		PatternTextures = null;
		patchSegments = null;
		Owner = null;
		innerEdges = null;
		outerEdges = null;
	}

	void Update ()
	{
		if(isPlaced)
		{
			if(!HasStoppedGrowing())
			{
				if(size < maxSize)
				{
					Grow();
				}
				else
				{
					SetSegmentGrowthDone(true);
				}
			}
			if(doHighlight)
			{
				UpdateHighlight();
			}
			if(isFlashing)
			{
				UpdateFlash();
			}
		}
		else
		{
			// Handle placement of score along the screen bounds.
			Vector3 scoreLocalPos = new Vector3(0.5f, 0.5f, circlePatchSize.transform.localPosition.z);
			circlePatchSize.transform.localPosition = scoreLocalPos;
			Vector3 vp = Camera.main.WorldToViewportPoint(circlePatchSize.transform.position);
			if(vp.x > 0.97f)
			{
				scoreLocalPos.x -= 1.0f;
			}
			if(vp.y > 0.965f)
			{
				scoreLocalPos.y -= 1.0f;
			}
			circlePatchSize.transform.localPosition = scoreLocalPos;
		}

		if(doColorOverlay)
		{
			UpdateColorOverlay();
		}
	}
}
