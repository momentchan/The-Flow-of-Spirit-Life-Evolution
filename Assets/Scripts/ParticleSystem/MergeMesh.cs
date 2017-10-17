using UnityEngine;

public partial class GPUParticleSystem
{
	class MergeMesh
	{
		// Single combined mesh.
		public Mesh mesh;
       	
		public int particleNums;

		public MergeMesh (Mesh[] shapes, int nums)
		{
			particleNums = nums;
			CombineMeshes (shapes);
		}

		public void Rebuild (Mesh[] shapes, int nums)
		{
			Release ();
			particleNums = nums;
			CombineMeshes (shapes);
		}

		public void Release ()
		{
			if (mesh) {
				DestroyImmediate (mesh);
				particleNums = 0;
			}
		}
			
		// Cache structure which stores shape information
		struct ShapeCacheData
		{
			Vector3[] vertices;
			Vector3[] normals;
			Vector4[] tangents;
			Vector2[] uv;
			int[] indices;

			public ShapeCacheData (Mesh mesh)
			{
				vertices = mesh.vertices;
				normals = mesh.normals;
				tangents = mesh.tangents;
				uv = mesh.uv;
				indices = mesh.GetIndices (0);
			}

			public int VertexCount { get { return vertices.Length; } }

			public int IndexCount { get { return indices.Length; } }

			public void CopyVerticesTo (Vector3[] destination, int position)
			{
				System.Array.Copy (vertices, 0, destination, position, vertices.Length);
			}

			public void CopyNormalsTo (Vector3[] destination, int position)
			{
				System.Array.Copy (normals, 0, destination, position, normals.Length);
			}

			public void CopyTangentsTo (Vector4[] destination, int position)
			{
				System.Array.Copy (tangents, 0, destination, position, tangents.Length);
			}

			public void CopyUVTo (Vector2[] destination, int position)
			{
				System.Array.Copy (uv, 0, destination, position, uv.Length);
			}

			public void CopyIndicesTo (int[] destination, int position, int offset)
			{
				for (var i = 0; i < indices.Length; i++)
					destination [position + i] = offset + indices [i];
			}
		}

		void CombineMeshes (Mesh[] shapes)
		{
			// Store the meshes into the shape cache.
			ShapeCacheData[] cache = new ShapeCacheData[shapes.Length];
			for (var i = 0; i < shapes.Length; i++)
				cache [i] = new ShapeCacheData (shapes [i]);
            

			var vc_shapes = 0;
			var ic_shapes = 0;
			foreach (var s in cache) {
				vc_shapes += s.VertexCount;
				ic_shapes += s.IndexCount;
			}
				

			var vc = 0;
			var ic = 0;
			for (int i = 0; i < particleNums; i++) {
				var s = cache [i % cache.Length];
				vc += s.VertexCount;
				ic += s.IndexCount;
			}

			// Create vertex arrays.
			var vertices = new Vector3[vc];
			var normals = new Vector3[vc];
			var tangents = new Vector4[vc];
			var uv = new Vector2[vc];
			var uv2 = new Vector2[vc];
			var indicies = new int[ic];

			for (int v_i = 0, i_i = 0, e_i = 0; v_i < vc; e_i++) {
				var s = cache [e_i % cache.Length];

				s.CopyVerticesTo (vertices, v_i);
				s.CopyNormalsTo (normals, v_i);
				s.CopyTangentsTo (tangents, v_i);
				s.CopyUVTo (uv, v_i);
				s.CopyIndicesTo (indicies, i_i, v_i);

				var coord = new Vector2 ((float)e_i / particleNums, 0);
				for (var i = 0; i < s.VertexCount; i++)
					uv2 [v_i + i] = coord;

				v_i += s.VertexCount;
				i_i += s.IndexCount;
			}

			// Create a mesh object.
			mesh = new Mesh ();

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.uv = uv;
			mesh.uv2 = uv2;

			mesh.SetIndices (indicies, MeshTopology.Triangles, 0);
		}
	}
}

