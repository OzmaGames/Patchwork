using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HighlightCursor : MonoBehaviour
{
	static Mesh GeneratedMesh;
	static Mesh GeneratedMesh2;

	public static Mesh GenerateSegment(float s, float segmentSize, float requiredGranularity /* = 40.0f */)
	{
		float innerRadius = s * segmentSize;
		float outerRadius = (s + 1) * segmentSize;
		return GenerateDisc(innerRadius, outerRadius, 0.0001f * s, requiredGranularity * (s + 1));
	}

	public static Mesh GenerateDisc(float innerRadius, float outerRadius, float z, float requiredGranularity /* = 40.0f */)
	{
		List<Vector3> meshVertices = new List<Vector3>();
		List<Vector2> meshUVs = new List<Vector2>();
		List<Vector2> meshUVs2 = new List<Vector2>();
		List<Vector4> meshExtras = new List<Vector4>();
		List<int> meshIndices = new List<int>();
		int meshIndicesStart = 0;

		// Generate points for edges.
		List<Vector2> innerPoints = new List<Vector2>();
		List<Vector2> outerPoints = new List<Vector2>();
		List<Vector2> uvrO = new List<Vector2>();
		List<Vector2> uvrI = new List<Vector2>();
		float granularity = (2.0f * Mathf.PI) / requiredGranularity;
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
		for(int i = 0; i < outerPoints.Count; ++i)
		{
			Vector2 innerPointA = innerPoints[i];
			Vector2 innerPointB = innerPoints[(i + 1) % innerPoints.Count];
			Vector2 outerPointA = outerPoints[i];
			Vector2 outerPointB = outerPoints[(i + 1) % outerPoints.Count];				
			Vector3 a = new Vector3(outerPointA.x, outerPointA.y, z);
			Vector3 b = new Vector3(innerPointA.x, innerPointA.y, z);
			Vector3 c = new Vector3(outerPointB.x, outerPointB.y, z);
			Vector3 d = new Vector3(innerPointB.x, innerPointB.y, z);
			meshVertices.Add(a);
			meshVertices.Add(b);
			meshVertices.Add(c);
			meshVertices.Add(d);

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

			float uvScale = outerRadius;
			Vector4 extraA = new Vector4(uvA.x * uvScale, uvA.y * uvScale, 0.0f, 0.0f);
			Vector4 extraB = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			Vector4 extraC = new Vector4(uvC.x * uvScale, uvC.y * uvScale, 0.0f, 0.0f);
			Vector4 extraD = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			meshExtras.Add(extraA);
			meshExtras.Add(extraB);
			meshExtras.Add(extraC);
			meshExtras.Add(extraD);

			meshUVs2.Add(new Vector2(0.0f, 0.0f));
			meshUVs2.Add(new Vector2(0.0f, 1.0f));
			meshUVs2.Add(new Vector2(1.0f, 0.0f));
			meshUVs2.Add(new Vector2(1.0f, 1.0f));

			meshIndices.Add((i * 4) + 0);
			meshIndices.Add((i * 4) + 1);
			meshIndices.Add((i * 4) + 2);
			meshIndices.Add((i * 4) + 2);
			meshIndices.Add((i * 4) + 1);
			meshIndices.Add((i * 4) + 3);
		}
		
		Mesh mesh = new Mesh();
		mesh.vertices = meshVertices.ToArray();
		mesh.uv = meshUVs.ToArray();
		mesh.uv2 = meshUVs2.ToArray();
		mesh.tangents = meshExtras.ToArray();
		mesh.triangles = meshIndices.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		return mesh;
	}

	public static void GenerateSegments(int numSegments, float segmentSize)
	{
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
			
			GeneratedMesh = new Mesh();
			GeneratedMesh.vertices = meshVertices.ToArray();
			GeneratedMesh.uv = meshUVs.ToArray();
			GeneratedMesh.uv2 = meshUVs2.ToArray();
			GeneratedMesh.tangents = meshExtras.ToArray();
			GeneratedMesh.triangles = meshIndices.ToArray();
			GeneratedMesh.RecalculateNormals();
			GeneratedMesh.RecalculateBounds();
			
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
	}

	void Start()
	{
	}
	
	void Update()
	{
	}
}
