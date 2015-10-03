using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CirclePatch : GamePieceBase {

	public const int MAX_PATTERNS = 4;
	public const float FLASH_TIME = 1.0f;
	public const float SegmentScale = 1.0f;
	const float GROWTH_SPEED = 0.5f;

	public GameObject ScoreMessagePrefab;
	GameObject ScoreMessage;	

	public GameObject patchObject;
	GameObject circlePatchSize;
	Symbol patchSymbol;

	Texture2D[] circlePatchSizes;

	public Color AddColor = Color.black;
	bool doColorOverlay = false;

	static Mesh GeneratedMesh;

	public int Segments = 1;
	public int CurrentSegment = 0;
	float InnerRadius = 1.0f;
	float OuterRadius = 1.0f;
	float CurrentSegmentArcSize = 0.0f;

	DecorationCircleStopper Decoration;

	static GameObject[] PatchSizeNumberPrefabs;
	static Texture2D[] RandomPatternTextures;

	public Game.PlayerPalette Palette;
	public Texture2D[] PatternTextures;
	Texture2D StitchTexture;

	class PatchSegment
	{
		public int MainTexture;
		public int _AtlasTex;
		public int ColorIndex;
		public int PatternIndex;
	}
	PatchSegment[] patchSegments;
	GameObject Seam;
	
	public Player Owner;
	public Transform PatchTransform;

	bool segmentDoneGrowing = false;
	bool doneGrowing = false;
	bool collided = false;
	bool isPlaced = false;
	public float size = 0.0f;
	public float maxSize = 0.0f;

	Color flashColorStart = Color.black;
	Color flashColorEnd = Color.white;
	bool isFlashing = false;
	float flashValue = 0.0f;
	float flashTimer = 0.0f;

	Color highlightColor = Color.black;
	bool doHighlight = false;

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

	public Rect[] AtlasRect;
	public Texture2D AtlasTexture;
	static Rect[] s_AtlasRect;
	static Texture2D s_AtlasTexture;
	static Mesh[] s_Segments;
	static Mesh[] s_SegmentStitches;
	public static void GenerateSegments(int numSegments, float segmentSize, GameObject[] patchSizeNumberPrefabs, Camera patchRendererCamera, Texture2D[] patternTextures)
	{
		PatchRendererCamera = patchRendererCamera;

		PatchSizeNumberPrefabs = patchSizeNumberPrefabs;
		RandomPatternTextures = new Texture2D[6 + patternTextures.Length];
		RandomPatternTextures[0] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[1] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[2] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[3] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[4] = CreatePatternTexture((int)(Random.value * 255.0f));
		RandomPatternTextures[5] = CreatePatternTexture((int)(Random.value * 255.0f));
		for(int i = 0; i < patternTextures.Length; ++i)
		{
			RandomPatternTextures[i + 6] = patternTextures[i];
		}

		s_AtlasTexture = new Texture2D(2048, 2048, UnityEngine.TextureFormat.ARGB32, true);
		s_AtlasRect = s_AtlasTexture.PackTextures(RandomPatternTextures, 0);

		s_Segments = new Mesh[numSegments];

		GeneratedMesh = new Mesh();
		GeneratedMesh.subMeshCount = numSegments;

		float requiredGranularity = 40.0f;
		float granularity = (2.0f * Mathf.PI) / requiredGranularity;

		List<Vector3> meshVertices = new List<Vector3>();
		List<Vector2> meshUVs = new List<Vector2>();
		List<Vector2> meshUVs2 = new List<Vector2>();
		List<Vector4> meshExtras = new List<Vector4>();
		List<int> meshIndices = new List<int>();
		int meshIndicesStart = 0;

		List<Vector3> subMeshVertices = new List<Vector3>();
		List<Vector2> subMeshUVs = new List<Vector2>();
		List<Vector2> subMeshUVs2 = new List<Vector2>();
		List<Vector4> subMeshExtras = new List<Vector4>();
		List<int[]> subMeshIndices = new List<int[]>();
		int subMeshIndicesStart = 0;
		for(int s = 0; s < numSegments; ++s)
		{
			meshVertices.Clear();
			meshUVs.Clear();
			meshUVs2.Clear();
			meshExtras.Clear();
			meshIndices.Clear();
			meshIndicesStart = 0;

			float innerRadius = s * segmentSize;
			float outerRadius = (s + 1) * segmentSize;

			// Generate points for edges.
			List<Vector2> innerPoints = new List<Vector2>();
			List<Vector2> outerPoints = new List<Vector2>();
			List<Vector2> uvrO = new List<Vector2>();
			List<Vector2> uvrI = new List<Vector2>();
			granularity = (2.0f * Mathf.PI) / (requiredGranularity * (s + 1));
			for(float phi = 0.0f; phi < (Mathf.PI * 2.0f); phi += granularity)
			{
				float ca = Mathf.Cos(phi);
				float sa = Mathf.Sin(phi);
				innerPoints.Add(new Vector2(innerRadius * ca, innerRadius * sa));
				outerPoints.Add(new Vector2(outerRadius * ca, outerRadius * sa));
				uvrO.Add(new Vector2((ca * 0.5f) + 0.5f, (sa * 0.5f) + 0.5f));
				float pp = innerRadius / outerRadius;
				uvrI.Add(new Vector2(((ca * pp) * 0.5f) + 0.5f, ((sa * pp) * 0.5f) + 0.5f));
			}

			float uvWrapScale = 10.0f;
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
				meshVertices.Add(a);
				meshVertices.Add(b);
				meshVertices.Add(c);
				meshVertices.Add(d);
				subMeshVertices.Add(a);
				subMeshVertices.Add(b);
				subMeshVertices.Add(c);
				subMeshVertices.Add(d);

				Vector2 outerUVPointA = uvrO[i];
				Vector2 outerUVPointB = uvrO[(i + 1) % uvrO.Count];
				Vector2 innerUVPointA = uvrI[i];
				Vector2 innerUVPointB = uvrI[(i + 1) % uvrI.Count];
				Vector2 uvA = new Vector3(outerUVPointA.x, outerUVPointA.y);
				Vector2 uvB = new Vector3(innerUVPointA.x, innerUVPointA.y);
				Vector2 uvC = new Vector3(outerUVPointB.x, outerUVPointB.y);
				Vector2 uvD = new Vector3(innerUVPointB.x, innerUVPointB.y);
				meshUVs.Add(uvA);
				meshUVs.Add(uvB);
				meshUVs.Add(uvC);
				meshUVs.Add(uvD);
				subMeshUVs.Add(uvA);
				subMeshUVs.Add(uvB);
				subMeshUVs.Add(uvC);
				subMeshUVs.Add(uvD);

				float uvScale = outerRadius;
				Vector4 extraA = new Vector4(uvA.x * uvScale, uvA.y * uvScale, 0.0f, 0.0f);
				Vector4 extraB = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				Vector4 extraC = new Vector4(uvC.x * uvScale, uvC.y * uvScale, 0.0f, 0.0f);
				Vector4 extraD = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				meshExtras.Add(extraA);
				meshExtras.Add(extraB);
				meshExtras.Add(extraC);
				meshExtras.Add(extraD);
				subMeshExtras.Add(extraA);
				subMeshExtras.Add(extraB);
				subMeshExtras.Add(extraC);
				subMeshExtras.Add(extraD);
				
				meshUVs2.Add(new Vector2(0.0f, 0.0f));
				meshUVs2.Add(new Vector2(0.0f, 1.0f));
				meshUVs2.Add(new Vector2(1.0f, 0.0f));
				meshUVs2.Add(new Vector2(1.0f, 1.0f));
				subMeshUVs2.Add(new Vector2(0.0f, 0.0f));
				subMeshUVs2.Add(new Vector2(0.0f, 1.0f));
				subMeshUVs2.Add(new Vector2(1.0f, 0.0f));
				subMeshUVs2.Add(new Vector2(1.0f, 1.0f));

				meshIndices.Add((i * 4) + 0);
				meshIndices.Add((i * 4) + 1);
				meshIndices.Add((i * 4) + 2);
				meshIndices.Add((i * 4) + 2);
				meshIndices.Add((i * 4) + 1);
				meshIndices.Add((i * 4) + 3);
				indices.Add((i * 4) + 0 + subMeshIndicesStart);
				indices.Add((i * 4) + 1 + subMeshIndicesStart);
				indices.Add((i * 4) + 2 + subMeshIndicesStart);
				indices.Add((i * 4) + 2 + subMeshIndicesStart);
				indices.Add((i * 4) + 1 + subMeshIndicesStart);
				indices.Add((i * 4) + 3 + subMeshIndicesStart);
			}

			s_Segments[s] = new Mesh();
			s_Segments[s].vertices = meshVertices.ToArray();
			s_Segments[s].uv = meshUVs.ToArray();
			s_Segments[s].uv2 = meshUVs2.ToArray();
			s_Segments[s].tangents = meshExtras.ToArray();
			s_Segments[s].triangles = meshIndices.ToArray();
			s_Segments[s].RecalculateNormals();
			s_Segments[s].RecalculateBounds();

			subMeshIndices.Add(indices.ToArray());
			subMeshIndicesStart = subMeshVertices.Count;
		}

		GeneratedMesh.vertices = subMeshVertices.ToArray();
		GeneratedMesh.uv = subMeshUVs.ToArray();
		GeneratedMesh.uv2 = subMeshUVs2.ToArray();
		GeneratedMesh.tangents = subMeshExtras.ToArray();
		// Add triangles to submesh.
		for(int s = 0; s < numSegments; ++s)
		{
			GeneratedMesh.SetTriangles(subMeshIndices[s], s);
		}
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();

	
		// Generate stitches.
		s_SegmentStitches = new Mesh[numSegments];
		subMeshIndicesStart = 0;
		requiredGranularity = 40.0f;
		for(int s = 0; s < numSegments; ++s)
		{
			meshVertices.Clear();
			meshUVs.Clear();
			meshUVs2.Clear();
			meshExtras.Clear();
			meshIndices.Clear();
			meshIndicesStart = 0;
			
			float innerRadius = (s * segmentSize) + (segmentSize - 0.07f);//(s * segmentSize) + (segmentSize * 0.95f);
			float outerRadius = (s * segmentSize) + (segmentSize + 0.07f);
			
			// Generate points for edges.
			List<Vector2> innerPoints = new List<Vector2>();
			List<Vector2> outerPoints = new List<Vector2>();
			List<Vector2> uvrO = new List<Vector2>();
			List<Vector2> uvrI = new List<Vector2>();
			granularity = (2.0f * Mathf.PI) / (requiredGranularity * (s + 1));
			for(float phi = 0.0f; phi < (Mathf.PI * 2.0f); phi += granularity)
			{
				float ca = Mathf.Cos(phi);
				float sa = Mathf.Sin(phi);
				innerPoints.Add(new Vector2(innerRadius * ca, innerRadius * sa));
				outerPoints.Add(new Vector2(outerRadius * ca, outerRadius * sa));
				uvrO.Add(new Vector2((ca * 0.5f) + 0.5f, (sa * 0.5f) + 0.5f));
				float pp = innerRadius / outerRadius;
				uvrI.Add(new Vector2(((ca * pp) * 0.5f) + 0.5f, ((sa * pp) * 0.5f) + 0.5f));
			}
			
			float uvWrapScale = 10.0f;
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
				meshVertices.Add(a);
				meshVertices.Add(b);
				meshVertices.Add(c);
				meshVertices.Add(d);
				subMeshVertices.Add(a);
				subMeshVertices.Add(b);
				subMeshVertices.Add(c);
				subMeshVertices.Add(d);
				
				Vector2 outerUVPointA = uvrO[i];
				Vector2 outerUVPointB = uvrO[(i + 1) % uvrO.Count];
				Vector2 innerUVPointA = uvrI[i];
				Vector2 innerUVPointB = uvrI[(i + 1) % uvrI.Count];
				Vector2 uvA = new Vector3(outerUVPointA.x, outerUVPointA.y);
				Vector2 uvB = new Vector3(innerUVPointA.x, innerUVPointA.y);
				Vector2 uvC = new Vector3(outerUVPointB.x, outerUVPointB.y);
				Vector2 uvD = new Vector3(innerUVPointB.x, innerUVPointB.y);
				meshUVs.Add(uvA);
				meshUVs.Add(uvB);
				meshUVs.Add(uvC);
				meshUVs.Add(uvD);
				subMeshUVs.Add(uvA);
				subMeshUVs.Add(uvB);
				subMeshUVs.Add(uvC);
				subMeshUVs.Add(uvD);
				
				float uvScale = outerRadius;
				Vector4 extraA = new Vector4(uvA.x * uvScale, uvA.y * uvScale, 0.0f, 0.0f);
				Vector4 extraB = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				Vector4 extraC = new Vector4(uvC.x * uvScale, uvC.y * uvScale, 0.0f, 0.0f);
				Vector4 extraD = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
				meshExtras.Add(extraA);
				meshExtras.Add(extraB);
				meshExtras.Add(extraC);
				meshExtras.Add(extraD);
				subMeshExtras.Add(extraA);
				subMeshExtras.Add(extraB);
				subMeshExtras.Add(extraC);
				subMeshExtras.Add(extraD);
				
				meshUVs2.Add(new Vector2(0.0f, 0.0f));
				meshUVs2.Add(new Vector2(0.0f, 1.0f));
				meshUVs2.Add(new Vector2(1.0f, 0.0f));
				meshUVs2.Add(new Vector2(1.0f, 1.0f));
				subMeshUVs2.Add(new Vector2(0.0f, 0.0f));
				subMeshUVs2.Add(new Vector2(0.0f, 1.0f));
				subMeshUVs2.Add(new Vector2(1.0f, 0.0f));
				subMeshUVs2.Add(new Vector2(1.0f, 1.0f));
				
				meshIndices.Add((i * 4) + 0);
				meshIndices.Add((i * 4) + 1);
				meshIndices.Add((i * 4) + 2);
				meshIndices.Add((i * 4) + 2);
				meshIndices.Add((i * 4) + 1);
				meshIndices.Add((i * 4) + 3);
				indices.Add((i * 4) + 0 + subMeshIndicesStart);
				indices.Add((i * 4) + 1 + subMeshIndicesStart);
				indices.Add((i * 4) + 2 + subMeshIndicesStart);
				indices.Add((i * 4) + 2 + subMeshIndicesStart);
				indices.Add((i * 4) + 1 + subMeshIndicesStart);
				indices.Add((i * 4) + 3 + subMeshIndicesStart);
			}
			
			s_SegmentStitches[s] = new Mesh();
			s_SegmentStitches[s].vertices = meshVertices.ToArray();
			s_SegmentStitches[s].uv = meshUVs2.ToArray();
			s_SegmentStitches[s].uv2 = meshUVs2.ToArray();
			s_SegmentStitches[s].tangents = meshExtras.ToArray();
			s_SegmentStitches[s].triangles = meshIndices.ToArray();
			s_SegmentStitches[s].RecalculateNormals();
			s_SegmentStitches[s].RecalculateBounds();
			
			subMeshIndices.Add(indices.ToArray());
			subMeshIndicesStart = subMeshVertices.Count;
		}
		
		/*GeneratedMesh.vertices = subMeshVertices.ToArray();
		GeneratedMesh.uv = subMeshUVs.ToArray();
		GeneratedMesh.uv2 = subMeshUVs2.ToArray();
		GeneratedMesh.tangents = subMeshExtras.ToArray();
		// Add triangles to submesh.
		for(int s = 0; s < numSegments; ++s)
		{
			GeneratedMesh.SetTriangles(subMeshIndices[s], s);
		}
		GeneratedMesh.RecalculateNormals();
		GeneratedMesh.RecalculateBounds();*/
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

	public Material MakeMaterial(int segment, Shader shader)
	{
		PatchSegment patchSegment = patchSegments[segment];
		
		Material material = new Material(shader);
		material.mainTexture = RandomPatternTextures[patchSegment.MainTexture];
		material.SetTexture("_FabricTexture", PatternTextures[0]);
		material.SetTexture("_PatternTexture2", PatternTextures[1]);
		material.SetVector("_CirclePatchSize", new Vector4(segment * SegmentScale, size, (segment * SegmentScale) + SegmentScale, 0.0f));
		material.SetFloat("_Border", -0.1f);
		material.SetFloat("_CirclePatchRadius", OuterRadius);
		material.SetFloat("_CirclePatchLayer", segment);
		material.SetFloat("_CurrentSegmentArcSize", 0.0f);
		material.SetTexture("_AtlasTex", AtlasTexture);
		material.SetTextureOffset("_AtlasTex", AtlasRect[patchSegment._AtlasTex].position);
		material.SetTextureScale("_AtlasTex", AtlasRect[patchSegment._AtlasTex].size);
		material.SetColor("_BaseColor1", Palette.Colors[patchSegment.ColorIndex].colorKeys[0].color);
		material.SetColor("_BaseColor2", Palette.Colors[patchSegment.ColorIndex].colorKeys[1].color);
		material.SetColor("_ComplementColor1", Palette.ComplementColor.colorKeys[0].color);
		material.SetColor("_ComplementColor2", Palette.ComplementColor.colorKeys[1].color);
		material.shaderKeywords = new string[] { "DO_SEGMENT_" + patchSegment.PatternIndex };
		
		return material;
	}
	
	public GameObject[] PatchSegments;
	public void Generate(PatchConfig config, Texture2D[] patternTextures, Game.PlayerPalette palette, Texture2D stitchTexture)
	{
		AtlasRect = s_AtlasRect;
		AtlasTexture = s_AtlasTexture;

		transform.position = new Vector3(0.0f, 0.0f, 0.0f);

		// Setup initial values.
		Segments = config.NumSegments;
		CurrentSegment = 1;
		size = CurrentSegment * SegmentScale;
		maxSize = size;

		InnerRadius = 0.0f;
		OuterRadius = Segments * SegmentScale;

		Palette = palette;
		PatternTextures = patternTextures;
		StitchTexture = stitchTexture;

		patchSegments = new PatchSegment[Segments];

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		Material material;
		List<string> shaderKeywords;

		Seam = new GameObject("Seam");
		Seam.SetActive(false);
		meshFilter = Seam.AddComponent<MeshFilter>();
		meshFilter.mesh = s_SegmentStitches[0];
		meshRenderer = Seam.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
		meshRenderer.material.mainTexture = StitchTexture;
		Seam.transform.SetParent(PatchTransform, false);
		Seam.transform.localPosition = new Vector3(0.0f, 0.0f, Game.ZPosAdd * 0.25f);

		// Setup patch segments.
		PatchSegments = new GameObject[Segments];
		for(int s = 0; s < Segments; ++s)
		{
			// Setup segment.
			PatchSegment patchSegment = new PatchSegment();
			patchSegment.MainTexture = Random.Range(0,6);
			patchSegment._AtlasTex = 6;
			patchSegment.ColorIndex = config.PaletteIndices[s];
			patchSegment.PatternIndex = config.PatternIndices[s];
			patchSegments[s] = patchSegment;

			PatchSegments[s] = new GameObject("PatchSegment_" + s);
			PatchSegments[s].SetActive((s < CurrentSegment) ? true : false);
			meshFilter = PatchSegments[s].AddComponent<MeshFilter>();
			meshFilter.mesh = s_Segments[s];
			meshRenderer = PatchSegments[s].AddComponent<MeshRenderer>();
			meshRenderer.material = MakeMaterial(s, Shader.Find((s < CurrentSegment) ? "Custom/CirclePatch" : "Custom/CirclePatchTrans"));
			PatchSegments[s].transform.SetParent(PatchTransform, false);
			PatchSegments[s].transform.localPosition = Vector3.zero;
		}
		CurrentSegment = 0;
		PatchSegments[0].GetComponent<MeshRenderer>().material.SetFloat("_Border", 0.5f);

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
		patchSymbol = Symbol.Instantiate(Symbol.GetRandomSymbolType(), Symbol.SymbolColor.Black).GetComponent<Symbol>();
		patchSymbol.transform.SetParent(PatchTransform, false);
		patchSymbol.transform.localPosition = new Vector3(0.0f, 0.0f, Game.ZPosAdd * 0.25f);

		// Create size quad.
		circlePatchSize = Instantiate(PatchSizeNumberPrefabs[Segments - 1]);
		circlePatchSize.transform.SetParent(PatchTransform, false);
		circlePatchSize.transform.localPosition = new Vector3(0.5f, 0.5f, Game.ZPosAdd * 0.35f);
	}
	
	public override Player GetOwner()
	{
		return Owner;
	}

	public override void SetOwner(Player player)
	{
		Owner = player;
		SwapColors(Owner.Palette);
	}

	public void SwapColors(Game.PlayerPalette palette)
	{
		Palette = palette;

		Material material;
		for(int s = 0; s < Segments; ++s)
		{
			material = PatchSegments[s].GetComponent<MeshRenderer>().material;
			int paletteIndex = patchSegments[s].ColorIndex;
			material.SetColor("_BaseColor1", Palette.Colors[paletteIndex].colorKeys[0].color);
			material.SetColor("_BaseColor2", Palette.Colors[paletteIndex].colorKeys[1].color);
			material.SetColor("_ComplementColor1", Palette.ComplementColor.colorKeys[0].color);
			material.SetColor("_ComplementColor2", Palette.ComplementColor.colorKeys[1].color);
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
		transform.position = new Vector3(transform.position.x, transform.position.y, Game.BGZPos);
		Game.BGZPos += Game.ZPosAdd;
		PlaceChilds();
		isPlaced = true;
		PatchSegments[0].GetComponent<MeshRenderer>().material.SetFloat("_Border", -0.1f);
	}

	public void PlaceDecoration(DecorationCircleStopper decoration)
	{
		Decoration = decoration;
		//if(!HasStoppedGrowing())
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
			if(CurrentSegment < Segments)
			{
				maxSize = (CurrentSegment * SegmentScale) + SegmentScale;
				CurrentSegmentArcSize = 0.0f;
				SetSegmentGrowthDone(false);
				PatchSegments[CurrentSegment].SetActive(true);

				/*Material material = PatchSegments[CurrentSegment].GetComponent<MeshRenderer>().material;
				material.shader = Shader.Find("Custom/CirclePatchTrans");
				material.mainTexture = RandomPatternTextures[Random.Range(0,6)];
				material.SetTexture("_FabricTexture", PatternTextures[0]);
				material.SetTexture("_PatternTexture2", PatternTextures[1]);
				material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, size, (CurrentSegment * SegmentScale) + SegmentScale, 0.0f));
				material.SetFloat("_CirclePatchRadius", OuterRadius);
				material.SetFloat("_CirclePatchLayer", CurrentSegment);
				material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
				material.SetTexture("_AtlasTex", AtlasTexture);
				material.SetTextureOffset("_AtlasTex", AtlasRect[6].position);
				material.SetTextureScale("_AtlasTex", AtlasRect[6].size);

				// Setup patch segment.
				int paletteIndex = patchSegments[CurrentSegment].colorIndex;
				material.SetColor("_BaseColor1", Palette.Colors[paletteIndex].colorKeys[0].color);
				material.SetColor("_BaseColor2", Palette.Colors[paletteIndex].colorKeys[1].color);
				material.SetColor("_ComplementColor1", Palette.ComplementColor.colorKeys[0].color);
				material.SetColor("_ComplementColor2", Palette.ComplementColor.colorKeys[1].color);
				
				int patternIndex = patchSegments[CurrentSegment].patternIndex;
				List<string> shaderKeywords = new List<string> { "DO_SEGMENT_" + patternIndex };
				material.shaderKeywords = shaderKeywords.ToArray();*/
			}
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

		Material material;
		/*int s = 0;
		for(; s < CurrentSegment; ++s)
		{
			material = PatchSegments[s].GetComponent<MeshRenderer>().material;
			material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchLayer", CurrentSegment);
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}*/
		if(CurrentSegment < Segments)
		{
			material = PatchSegments[CurrentSegment].GetComponent<MeshRenderer>().material;
			material.SetVector("_CirclePatchSize", new Vector4(CurrentSegment * SegmentScale, size, (CurrentSegment * SegmentScale) + SegmentScale, 0.0f));
			material.SetFloat("_CirclePatchLayer", CurrentSegment);
			material.SetFloat("_CurrentSegmentArcSize", CurrentSegmentArcSize);
		}
//		material = gameObject.GetComponent<MeshRenderer>().material;
//		material.SetVector("_CirclePatchSize", new Vector4(s * SegmentScale, size, (s * SegmentScale) + SegmentScale, 0.0f));
	}
	
	public void SetSegmentGrowthDone(bool enable)
	{
		segmentDoneGrowing = enable;
		if(enable && (CurrentSegment < Segments))
		{
			PatchSegments[CurrentSegment].GetComponent<MeshRenderer>().material = MakeMaterial(CurrentSegment, Shader.Find("Custom/CirclePatch"));
		}
		if((segmentDoneGrowing) && ((CurrentSegment + 1) >= Segments))
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
		ActivePlayfield.PieceDone(this);
		doneGrowing = enable;
		SetShowSymbol(false);

		// Show seam.
		int segment = CurrentSegment;
		if(HasCollided() && (GetSize() < GetMaxSize()))
		{
			segment = Mathf.Max(CurrentSegment - 1, 0);
		}
		Seam.GetComponent<MeshFilter>().mesh = s_SegmentStitches[segment];
		Seam.SetActive(true);

		
		// Start flashing to notify that it is done.
//		StartFlash(new Color(-0.5f, -0.5f, -0.5f), new Color(0.5f, 0.5f, 0.5f), FLASH_TIME);
		//transform.parent.
		GetComponent<GamePiece>().StartEffect("Done");
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
		Material material;
		if(!doHighlight)
		{
			for(int s = 0; s <= CurrentSegment; ++s)
			{
				if(s >= Segments)
				{
					break;
				}
				material = PatchSegments[s].GetComponent<MeshRenderer>().material;
				material.SetColor("_AddColor", Color.black);
			}
			return;
		}
		for(int s = 0; s <= CurrentSegment; ++s)
		{
			if(s >= Segments)
			{
				break;
			}
			material = PatchSegments[s].GetComponent<MeshRenderer>().material;
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
			for(int s = 0; s <= CurrentSegment; ++s)
			{
				if(s >= Segments)
				{
					break;
				}
				material = PatchSegments[s].GetComponent<MeshRenderer>().material;
				material.SetColor("_AddColor", Color.Lerp(flashColorStart, flashColorEnd, flashValue));
			}

			flashTimer -= Time.deltaTime;
		}
		else
		{
			Material material;
			for(int s = 0; s <= CurrentSegment; ++s)
			{
				if(s >= Segments)
				{
					break;
				}
				material = PatchSegments[s].GetComponent<MeshRenderer>().material;
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
		Material material;
		for(int s = 0; s <= CurrentSegment; ++s)
		{
			if(s >= Segments)
			{
				break;
			}
			material = PatchSegments[s].GetComponent<MeshRenderer>().material;
			material.SetColor("_AddColor", AddColor);
		}
	}
	
	public override void UpdateEffect(Color addColor)
	{
		Debug.Log("GOOOOOGD");
		Material material;
		for(int s = 0; s <= CurrentSegment; ++s)
		{
			if(s >= Segments)
			{
				break;
			}
			material = PatchSegments[s].GetComponent<MeshRenderer>().material;
			material.SetColor("_AddColor", addColor);
		}
		UpdateChildsEffect(addColor);
	}
	
	public void SetShowSymbol(bool show)
	{
		patchSymbol.GetComponent<Renderer>().enabled = show;
		circlePatchSize.GetComponent<Renderer>().enabled = show;
	}

	public void ShowScoreMessage(int score, Color color)
	{
		ScoreMessage = Instantiate(ScoreMessagePrefab);
		ScoreMessage.transform.SetParent(GameObject.Find("Canvas").transform, false);
		//ScoreMessage.GetComponent<UnityEngine.RectTransform>();

		Vector3 patchPos = transform.position;
		Vector3 messagePos = new Vector3(patchPos.x, patchPos.y, ScoreMessage.transform.position.z);
		Vector3 vp = Camera.main.WorldToViewportPoint(messagePos);
		ScoreMessage.transform.position = messagePos;


		UnityEngine.UI.Text msgText = ScoreMessage.GetComponentInChildren<UnityEngine.UI.Text>();
		msgText.text = score.ToString();
		msgText.color = color;
		ScoreMessage.GetComponent<Animator>().Play("Show");
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
		Destroy(Seam);
		Seam = null;
		Destroy(circlePatchSize);
		circlePatchSize = null;
		Destroy(patchSymbol);
		patchSymbol = null;
		circlePatchSizes = null;
		if(Decoration != null)
		{
			Destroy(Decoration.gameObject);
			Decoration = null;
		}
		PatternTextures = null;
		patchSegments = null;
		Owner = null;
		Destroy(ScoreMessage);
		ScoreMessage = null;
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
			/*if(doHighlight)
			{
				UpdateHighlight();
			}
			if(isFlashing)
			{
				UpdateFlash();
			}*/
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

		/*if(doColorOverlay)
		{
			UpdateColorOverlay();
		}*/
	}
}
