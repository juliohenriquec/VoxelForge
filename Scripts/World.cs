using Godot;
using System;
using System.Collections.Generic;
using Godot.NativeInterop;

public partial class World : Node3D
{
	[Export] public CharacterBody3D PlayerReference { get; set; } 
	private Godot.Collections.Dictionary<Vector2I, Chunk> _chunks = new Godot.Collections.Dictionary<Vector2I, Chunk>();
	private CharacterBody3D _player;
	private Timer _updateTimer;
	
	private Queue<Vector2I> _chunksToGenerate = new Queue<Vector2I>();
	private HashSet<Vector2I> _pendingChunks = new HashSet<Vector2I>();
	private Queue<Vector2I> _chunksToRegenerate = new Queue<Vector2I>();
	private HashSet<Vector2I> _pendingRegeneration = new HashSet<Vector2I>();
	
	private int loadDistance = 7;
	private int unloadDistance = 9;
	private int _maxGenerationsPerFrame = 4;
	private const float TARGET_FRAME_TIME_MS = 16f; // para 60 FPS (1000/60 ≈ 16.66ms)
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_updateTimer = new Timer();
		_updateTimer.WaitTime = 0.2;
		_updateTimer.Timeout += UpdateChunk;
		_updateTimer.Autostart = true;
		AddChild(_updateTimer);
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var stopwatch = System.Diagnostics.Stopwatch.StartNew();
		int count = 0;

		// Processa regeneração (prioridade, pois é mais rápido)
		while (_chunksToRegenerate.Count > 0 && count < _maxGenerationsPerFrame && stopwatch.ElapsedMilliseconds < TARGET_FRAME_TIME_MS)
		{
			Vector2I pos = _chunksToRegenerate.Dequeue();
			_pendingRegeneration.Remove(pos);

			if (_chunks.TryGetValue(pos, out Chunk chunk))
			{
				chunk.RegenerateMesh();
				count++;
			}
		}

		// Processa geração de novos chunks
		while (_chunksToGenerate.Count > 0 && count < _maxGenerationsPerFrame && stopwatch.ElapsedMilliseconds < TARGET_FRAME_TIME_MS)
		{
			Vector2I pos = _chunksToGenerate.Dequeue();
			_pendingChunks.Remove(pos);

			if (!_chunks.ContainsKey(pos))
			{
				Chunk chunk = new Chunk();
				chunk.WorldRef = this;
				chunk.GridPosition = pos;
				chunk.Position = new Vector3(pos.X * Chunk.CHUNK_WIDTH, 0, pos.Y * Chunk.CHUNK_DEPTH);
				chunk.InitializeBlocks();
				chunk.GenerateMesh(); // geração inicial
				AddChild(chunk);
				_chunks[pos] = chunk;

				// Após gerar, agenda regeneração dos vizinhos (se existirem)
				Vector2I[] neighbors = new Vector2I[]
				{
					new Vector2I(pos.X, pos.Y - 1),
					new Vector2I(pos.X, pos.Y + 1),
					new Vector2I(pos.X - 1, pos.Y),
					new Vector2I(pos.X + 1, pos.Y)
				};
				foreach (var n in neighbors)
				{
					if (_chunks.ContainsKey(n) && !_pendingRegeneration.Contains(n))
					{
						_pendingRegeneration.Add(n);
						_chunksToRegenerate.Enqueue(n);
					}
				}

				count++;
			}
		}
		

	}


	public void UpdateChunk()
	{
		if (PlayerReference == null) return;
		//Pega a posição do player
		int playerChunkX = Mathf.FloorToInt(PlayerReference.GlobalPosition.X / Chunk.CHUNK_WIDTH);
		int playerChunkZ = Mathf.FloorToInt(PlayerReference.GlobalPosition.Z / Chunk.CHUNK_DEPTH);


		// Conjunto de chunks que devem estar carregados
		HashSet<Vector2I> needed = new HashSet<Vector2I>();
		for (int x = playerChunkX - loadDistance; x <= playerChunkX + loadDistance; x++)
		for (int z = playerChunkZ - loadDistance; z <= playerChunkZ + loadDistance; z++)
			needed.Add(new Vector2I(x, z));

		//carregar chunks que faltam
		foreach (Vector2I pos in needed)
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
			int distX = Math.Abs(kvp.Key.X - playerChunkX);
			int distZ = Math.Abs(kvp.Key.Y - playerChunkZ);
			int dist = Math.Max(distX, distZ); // distância Chebyshev

			// Se a distância for maior que loadDistance + buffer, remove
			if (dist > unloadDistance)
			{
				kvp.Value.QueueFree();
				toRemove.Add(kvp.Key);

			}

			foreach (var key in toRemove)
			{
				_chunks.Remove(key);
			}

		}
	}


	public Chunk GetChunk(Vector2I pos)
	{
		_chunks.TryGetValue(pos, out Chunk chunk);
		return chunk;
	}
}
