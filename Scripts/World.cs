using Godot;
using System;

public partial class World : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//gerando um quadrado de 5x5 chunks
		for (int x = -10; x <= 10; x++)
			for (int z = -10; z <= 10; z++)
			{
				Chunk chunk = new Chunk();
				chunk.GridPosition = new Vector2I(x, z);
				chunk.Position = new Vector3(chunk.GridPosition.X * Chunk.CHUNK_WIDTH, 0, chunk.GridPosition.Y * Chunk.CHUNK_DEPTH);
				chunk.InitializeBlocks();
				chunk.GenerateMesh();
				AddChild(chunk); 
			}

	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
