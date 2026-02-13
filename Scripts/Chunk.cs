using System;
using System.CodeDom.Compiler;


public class Chunk
{
    public const int Size = 16; // Define o tamanho da chunk
    private byte[,,] blocks = new byte[Size, Size, Size];

    public Chunk()
    {
        Generate();
    }

    public void Generate()
    {
        for (int x = 0; x < Size; x++)
        for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            blocks[x, y, z] = 1; // a posição x,y,z vai ser preenchida por um bloco do tipo 1 (ex:terra)
        }
    }

    public byte GetBlock(int x, int y, int z)
    {
        return blocks[x, y, z]; //retorna o conteúdo armazenado
    }
}