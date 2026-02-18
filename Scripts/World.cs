using Godot;
using System;
using System.Collections.Generic;
using Godot.NativeInterop;

public partial class World : Node3D
{
	[Export] public CharacterBody3D PlayerReference { get; set; } 
	private Godot.Collections.Dictionary<Vector2I, Chunk> _chunks = new Godot.Collections.Dictionary<Vector2I, Chunk>();
	private int loadDistance = 5;
	private CharacterBody3D _player;
	private Timer _updateTimer;
	
	private Queue<Vector2I> _chunksToGenerate = new Queue<Vector2I>();
	private HashSet<Vector2I> _pendingChunks = new HashSet<Vector2I>();
	private int _maxGenerationsPerFrame = 2;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//_player = GetNode<CharacterBody3D>(".../Player");
	
		
		_updateTimer = new Timer();
		_updateTimer.WaitTime = 0.5;
		_updateTimer.Timeout += UpdateChunk;
		_updateTimer.Autostart = true;
		AddChild(_updateTimer);
		//gerando um quadrado de 5x5 chunks
		// for (int x = -2; x <= 2; x++)
		// 	for (int z = -2; z <= 2; z++)
		// 	{
		// 		Chunk chunk = new Chunk();
		// 		chunk.GridPosition = new Vector2I(x, z);
		// 		chunk.Position = new Vector3(chunk.GridPosition.X * Chunk.CHUNK_WIDTH, 0, chunk.GridPosition.Y * Chunk.CHUNK_DEPTH);
		// 		chunk.InitializeBlocks();
		// 		chunk.GenerateMesh();
		// 		AddChild(chunk); 
		// 	}

	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		int count = 0;
		while (_chunksToGenerate.Count > 0 && count < _maxGenerationsPerFrame)
		{
			Vector2I pos = _chunksToGenerate.Dequeue();
			_pendingChunks.Remove(pos);

			if (!_chunks.ContainsKey(pos))
			{
				Chunk chunk = new Chunk();
				chunk.GridPosition = pos;
				chunk.Position = new Vector3(pos.X * Chunk.CHUNK_WIDTH, 0, pos.Y * Chunk.CHUNK_DEPTH);
				
				chunk.InitializeBlocks();
				chunk.GenerateMesh();
				AddChild(chunk); 
				_chunks[pos] = chunk;
				GD.Print($"Carregando chunk {pos}");	
			}
			count++;
		}

	}
	public void UpdateChunk()
	{

		if (PlayerReference == null) return;
		
		int playerChunkX = Mathf.FloorToInt(PlayerReference.GlobalPosition.X / Chunk.CHUNK_WIDTH);
		int playerChunkZ = Mathf.FloorToInt(PlayerReference.GlobalPosition.Z / Chunk.CHUNK_DEPTH);

		
		// Conjunto de chunks que devem estar carregados
		HashSet<Vector2I> needed = new HashSet<Vector2I>();
		for(int x = playerChunkX - loadDistance; x <= playerChunkX + loadDistance; x++)
			for(int z = playerChunkZ - loadDistance; z <= playerChunkZ + loadDistance; z++)
				needed.Add(new Vector2I(x, z));

		//carregar chunks que faltam
		foreach (Vector2I pos in needed )
		{
			if (!_chunks.ContainsKey(pos) && !_pendingChunks.Contains(pos))
			{
				_pendingChunks.Add(pos);
				_chunksToGenerate.Enqueue(pos);
			}
		}
		
		// Remover chunks desnecessários
		List<Vector2I> toRemove = new List<Vector2I>();
		foreach (var kvp in _chunks)
		{
			if (!needed.Contains(kvp.Key))
			{
				kvp.Value.QueueFree();
				toRemove.Add(kvp.Key);
			}
		}

		foreach (var key in toRemove)
		{
			_chunks.Remove(key);
		}

	}
}
