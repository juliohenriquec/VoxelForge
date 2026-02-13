using Godot;
using System;
using System.Data;

public partial class World : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Chunk chunk = new Chunk();

		for (int x = 0;x<Chunk.Size;x++)
		for (int y=0;y<Chunk.Size;y++)
		for (int z = 0; z < Chunk.Size; z++)
		{
			if (chunk.GetBlock(x, y, z) != 0)
			{
				var cube = new MeshInstance3D();
				cube.Mesh = new BoxMesh();
				
				cube.Position = new Vector3(x,y,z);
				
				AddChild(cube);
			}
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
