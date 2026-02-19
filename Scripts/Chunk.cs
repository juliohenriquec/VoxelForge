using System.CodeDom.Compiler;
using Godot;
using System;

public partial class Chunk : Node3D
{
    private FastNoiseLite _noise; //Ruido do terreno
    
    //Dimensões da Chunk
    public const int CHUNK_WIDTH = 16;
    public const int CHUNK_HEIGHT = 384;
    public const int CHUNK_DEPTH = 16;
    
    public Vector2I GridPosition { get; set; } // define a posição da chunk, em um vetor2I (x,z)
    private int [,] _heightMap; // [x, z]
    
    private BlockType[,,] _blocks; //Array 3D de Blocos
    private static StandardMaterial3D _standardMaterial; //Material 3D

    
    private MeshInstance3D _meshInstance;
    
    public World WorldRef { get; set; }
    
    //Criando tipos de blocos e salvando em um enum
    public enum BlockType : byte
    {
        Air = 0,
        Grass = 1,
        Dirt = 2,
        Stone = 3,
    }
    
    public BlockType GetBlock(int x, int y, int z)
    {
        return _blocks[x, y, z];
    }
    
    
    public void GenerateMesh()
    {
        if (_meshInstance != null)
        {
            _meshInstance.QueueFree();
            _meshInstance = null;
        }
        
        //obter vizinhos(pode ser null)
        Chunk northChunk = WorldRef?.GetChunk(new Vector2I(GridPosition.X, GridPosition.Y - 1));
        Chunk southChunk = WorldRef?.GetChunk(new Vector2I(GridPosition.X, GridPosition.Y + 1));
        Chunk westChunk = WorldRef?.GetChunk(new Vector2I(GridPosition.X-1, GridPosition.Y));
        Chunk eastChunk = WorldRef?.GetChunk(new Vector2I(GridPosition.X+1, GridPosition.Y));
        
        //GD.Print($"Gerando chunk {GridPosition}. Vizinhos: N:{northChunk?.GridPosition} S:{southChunk?.GridPosition} L:{eastChunk?.GridPosition} O:{westChunk?.GridPosition}");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        int vertexCount = 0;
        
        for(int x = 0; x< CHUNK_WIDTH; x++ )
        for (int z = 0; z < CHUNK_DEPTH; z++)
        {
            int maxY = _heightMap[x, z];
            for (int y = 0; y <= maxY; y++)
            {
                if (_blocks[x, y, z] == BlockType.Air) continue;
                
                Color blockColor = GetColor(_blocks[x, y, z]);
                
                //Face superior
                if (y + 1 >= CHUNK_HEIGHT || _blocks[x, y + 1, z] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x, y + 1, z),
                        new Vector3(x + 1, y + 1, z),
                        new Vector3(x + 1, y + 1, z + 1),
                        new Vector3(x, y + 1, z + 1),
                        Vector3.Up,
                        blockColor);
                }

                //Face inferior
                if (y == 0 || _blocks[x, y - 1, z] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x, y, z + 1),
                        new Vector3(x + 1, y, z + 1),
                        new Vector3(x + 1, y, z),
                        new Vector3(x, y, z),
                        Vector3.Down,
                        blockColor);
                }


                //Face norte z = z
                if (z == 0)
                {
                    // Verifica se há chunk norte e se o bloco correspondente é sólido
                    bool hidden = (northChunk != null && northChunk.GetBlock(x,y, CHUNK_DEPTH - 1)  != BlockType.Air);

                    if (!hidden)
                    {
                        GD.Print($"Face norte em ({x},{y},{z}) não oculta, hidden={hidden}, northChunk existe? {northChunk != null}");
                        AddFace(st,
                            ref vertexCount,
                            new Vector3(x, y, z),
                            new Vector3(x + 1, y, z),
                            new Vector3(x + 1, y + 1, z),
                            new Vector3(x, y + 1, z),
                            Vector3.Forward,
                            blockColor);
                    }
                }
                else if (_blocks[x, y, z - 1] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x, y, z),
                        new Vector3(x + 1, y, z),
                        new Vector3(x + 1, y + 1, z),
                        new Vector3(x, y + 1, z),
                        Vector3.Forward,
                        blockColor);
                }


                // Face sul (z = z+1)
                if (z == CHUNK_DEPTH -1 )
                {
                    // Verifica se há chunk sul e se o bloco correspondente é sólido
                    bool hidden = (southChunk != null && southChunk.GetBlock(x,y,0)  != BlockType.Air);

                    if (!hidden)
                    {
                        GD.Print($"Face sul em ({x},{y},{z}) não oculta, hidden={hidden}, SouthChunk existe? {southChunk != null}");
                        AddFace(st,
                            ref vertexCount,
                            new Vector3(x, y, z + 1),
                            new Vector3(x, y + 1, z + 1),
                            new Vector3(x + 1, y + 1, z + 1),
                            new Vector3(x + 1, y, z + 1),
                            Vector3.Back,
                            blockColor);
                    }

                }
                else if (_blocks[x, y, z + 1] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x, y, z + 1),
                        new Vector3(x, y + 1, z + 1),
                        new Vector3(x + 1, y + 1, z + 1),
                        new Vector3(x + 1, y, z + 1),
                        Vector3.Back,
                        blockColor);
                }


                // Face leste (x = x+1)
                if (x == CHUNK_WIDTH - 1)
                {
                    bool hidden = (eastChunk != null && eastChunk.GetBlock(0, y, z) != BlockType.Air);
                    if (!hidden)
                    {
                        GD.Print($"Face leste em ({x},{y},{z}) não oculta, hidden={hidden}, eastChunk existe? {eastChunk != null}");
                        AddFace(st, ref vertexCount,
                            new Vector3(x + 1, y, z),
                            new Vector3(x + 1, y, z + 1),
                            new Vector3(x + 1, y + 1, z + 1),
                            new Vector3(x + 1, y + 1, z),
                            Vector3.Right,
                            blockColor);
                    }
                }
                else if (_blocks[x + 1, y, z] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x + 1, y, z),
                        new Vector3(x + 1, y, z + 1),
                        new Vector3(x + 1, y + 1, z + 1),
                        new Vector3(x + 1, y + 1, z),
                        Vector3.Right,
                        blockColor);
                }


                // Face oeste (x = x)
                if (x == 0 )
                {
                    bool hidden = (westChunk != null && westChunk.GetBlock(CHUNK_WIDTH - 1, y, z) != BlockType.Air);
                    if (!hidden)
                    {
                        GD.Print($"Face oeste em ({x},{y},{z}) não oculta, hidden={hidden}, westChunk existe? {westChunk != null}");
                        AddFace(st, ref vertexCount,
                            new Vector3(x, y + 1, z),
                            new Vector3(x, y + 1, z + 1),
                            new Vector3(x, y, z + 1),
                            new Vector3(x, y, z),
                            Vector3.Left,
                            blockColor);
                    }
                }
                else if (_blocks[x - 1, y, z] == BlockType.Air)
                {
                    AddFace(st,
                        ref vertexCount,
                        new Vector3(x, y + 1, z),
                        new Vector3(x, y + 1, z + 1),
                        new Vector3(x, y, z + 1),
                        new Vector3(x, y, z),
                        Vector3.Left,
                        blockColor);
                }
            }
        }

        Mesh mesh = st.Commit();
        MeshInstance3D meshblock= new MeshInstance3D();
        meshblock.Mesh = mesh;

        if (_standardMaterial == null)
        {
            _standardMaterial = new StandardMaterial3D();
            _standardMaterial.VertexColorUseAsAlbedo = true;
        }

        meshblock.MaterialOverride = _standardMaterial;
        
        _meshInstance = meshblock;
        AddChild(_meshInstance);
        sw.Stop();
        //GD.Print($"Chunk {GridPosition} gerado em {sw.ElapsedMilliseconds} ms");
        
    }
    
    public void InitializeBlocks()// define o tipo de bloco a ser gerado
    {
        _noise = new FastNoiseLite(); // instancia o ruido
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin; //Define o tipo de ruido
        _noise.Seed = 12345; //seed
        _noise.Frequency = 0.001f; //quanto maior a frequencia, mas aleatorio e caótico fica
        _blocks =  new BlockType[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_DEPTH];
        _heightMap = new int[CHUNK_WIDTH, CHUNK_DEPTH];    
        
        for( int x = 0; x< CHUNK_WIDTH; x++ )
            for(int y = 0; y< CHUNK_HEIGHT; y++)
                for (int z = 0; z < CHUNK_DEPTH; z++) 
                    _blocks[x,y,z] = BlockType.Air;

        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_DEPTH; z++)
            {
                
                // Coordenada global X Z: desloca o chunk para sua posição no mundo e soma a posição local do bloco.
                // Isso garante que os blocos fiquem contínuos entre chunks, sem sobreposição ou buracos.
                float globalX = (GridPosition.X * CHUNK_WIDTH) + x; 
                float globalZ = (GridPosition.Y * CHUNK_DEPTH) + z; 
                
                //Aplicando a matematica do ruido
                float noiseValue = _noise.GetNoise2D(globalX, globalZ); 
                float mappedHeight = (noiseValue + 1f) / 2f * (CHUNK_HEIGHT-1);
                int groundHeight = (int)Mathf.Clamp(mappedHeight, 0, CHUNK_HEIGHT-1);
                _heightMap[x,z] = groundHeight;
                
                for(int y = 0; y<= groundHeight; y++)
                {
                    if (y == groundHeight)
                        _blocks[x,y,z] = BlockType.Grass;
                    else if (y > groundHeight - 4)
                        _blocks[x,y,z] = BlockType.Dirt;
                    else
                        _blocks[x,y,z] = BlockType.Stone;
                }
            }

        }
    }

    public void AddFace(SurfaceTool st, ref int vertexCount,Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal, Color color)
    {
        //Adiciona os 4 vértices
        st.SetNormal(normal);
        st.SetColor(color);
        st.SetUV(new Vector2(0,0));
        st.AddVertex(v0);
        
        st.SetNormal(normal);
        st.SetColor(color);
        st.SetUV(new Vector2(1,0));
        st.AddVertex(v1);
        
        st.SetNormal(normal);
        st.SetColor(color);
        st.SetUV(new Vector2(1,1));
        st.AddVertex(v2);
        
        st.SetNormal(normal);
        st.SetColor(color);
        st.SetUV(new Vector2(0,1));
        st.AddVertex(v3);
        
        // Adiciona os índices dos dois triângulos
        //usar indices evita que a o v0 v2 sejam desenhados duas vezes no mesmo quadrado, reduzindo de 6 para 4 vertices
        //triangulo 1
        st.AddIndex(vertexCount);
        st.AddIndex(vertexCount + 1);
        st.AddIndex(vertexCount + 2);
        //triangulo 2
        st.AddIndex(vertexCount);
        st.AddIndex(vertexCount + 2);
        st.AddIndex(vertexCount + 3);
        
        vertexCount += 4;
    }

    private Color GetColor(BlockType type)
    {
        switch (type)
        {
            
            case BlockType.Grass: return Colors.GreenYellow;
            case BlockType.Dirt: return Colors.SaddleBrown;
            case BlockType.Stone: return Colors.Gray;
            default: return Colors.White;

        }

    }
    
    public void RegenerateMesh()
    {
        // Remove a malha antiga (se houver)
        foreach (Node child in GetChildren())
        {
            if (child is MeshInstance3D)
            {
                child.QueueFree();
            }
        }
        GenerateMesh();
    }
}