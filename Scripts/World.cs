using Godot;
using System;
using System.Data;

public partial class World : Node3D
{
	public override void _Ready()
	{
		Chunk chunk = new Chunk();

		BuildChunkMesh(chunk);
	}

	void BuildChunkMesh(Chunk chunk)
	{
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);
		
		for (int x = 0; x < Chunk.Size; x++)
		for( int y = 0; y < Chunk.Size; y++)
		for (int z = 0; z < Chunk.Size; z++)
		{
			if (chunk.GetBlock(x, y, z) != 0)
			{
				AddCube(st, new Vector3(x, y, z));
			}
		}

		var mesh = st.Commit();

		var meshInstance = new MeshInstance3D();
		meshInstance.Mesh = mesh;
		
		AddChild(meshInstance);
		
	}


	void AddCube(SurfaceTool st, Vector3 pos)
	{
		Vector3[] v = new Vector3[]
		{
			pos + new Vector3(0, 0, 0),
			pos + new Vector3(1, 0, 0),
			pos + new Vector3(1, 1, 0),
			pos + new Vector3(0, 1, 0),
			
			pos + new Vector3(0, 0, 1),
			pos + new Vector3(1, 0, 1),
			pos + new Vector3(1, 1, 1),
			pos + new Vector3(0, 1, 1),
		};

		int[][] faces =
		{
			new[] { 0, 1, 2, 3 },
			new[] { 5, 4, 6, 7 },
			new[] { 4, 0, 3, 7 },
			new[] { 1, 5, 6, 2 },
			new[] { 3, 2, 6, 7 },
			new[] { 4, 5, 1, 0 },
		};

		foreach (var f in faces)
		{
			st.AddVertex(v[f[0]]);
			st.AddVertex(v[f[1]]);
			st.AddVertex(v[f[2]]);
			
			st.AddVertex(v[f[0]]);
			st.AddVertex(v[f[2]]);
			st.AddVertex(v[f[3]]);
		}
		
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
