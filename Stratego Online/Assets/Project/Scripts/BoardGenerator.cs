using System.Linq;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    private ConfigManager config;
    private GameObject[,] tiles;

    public void Initialize(ConfigManager config)
    {
        this.config = config;
    }

    public GameObject[,] GenerateBoard()
    {
        int tileCountX = config.BoardRows;
        int tileCountY = config.BoardColumns;
        tiles = new GameObject[tileCountX, tileCountY];

        for (int y = 0; y < tileCountY; y++)
        {
            for (int x = 0; x < tileCountX; x++)
            {
                if (IsLakeTile(x, y))
                {
                    tiles[x, y] = GenerateSingleTile(config.TileSize, x, y, LayerMask.NameToLayer("Lake"), config.TileMaterialLake);
                    tiles[x, y].GetComponent<Tile>().IsLake = true;
                }
                else
                {
                    tiles[x, y] = GenerateSingleTile(config.TileSize, x, y, LayerMask.NameToLayer("Tile"), (x + y) % 2 == 0 ? config.TileMaterialWhite : config.TileMaterialBlack);
                }
            }
        }
        return tiles;
    }

    private readonly Vector2Int[] lakeTiles = new Vector2Int[]
    {
        new Vector2Int(2, 4), new Vector2Int(2, 5),
        new Vector2Int(3, 4), new Vector2Int(3, 5),
        new Vector2Int(6, 4), new Vector2Int(6, 5),
        new Vector2Int(7, 4), new Vector2Int(7, 5)
    };

    private bool IsLakeTile(int x, int y)
    {
        return lakeTiles.Contains(new Vector2Int(x, y));
    }


    private GameObject GenerateSingleTile(float tileSize, int x, int y, LayerMask layer, Material material)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;
        tileObject.transform.localPosition = new Vector3(x * tileSize, 0f, y * tileSize);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = material;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, tileSize);
        vertices[2] = new Vector3(tileSize, 0, 0);
        vertices[3] = new Vector3(tileSize, 0, tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = layer;
        tileObject.AddComponent<BoxCollider>();
        tileObject.AddComponent<Tile>();
        tileObject.GetComponent<Tile>().IndexInMatrix = new Vector2Int(x, y);

        return tileObject;
    }
}
