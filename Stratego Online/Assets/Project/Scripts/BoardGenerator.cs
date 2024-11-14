using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    private ConfigManager config;
    private GameObject[,] tiles;
    public GameObject TilePrefab;

    public void Initialize(ConfigManager config)
    {
        this.config = config;
    }

    public GameObject[,] GenerateBoard()
    {
        Debug.Log("Start Generating Board");
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
        Debug.Log("Board Generated");
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
        GameObject tileObject = Instantiate(TilePrefab);
        tileObject.name = string.Format("X:{0}, Y:{1}", x, y);

        tileObject.GetComponent<Tile>().SetMaterial(material);

        tileObject.layer = layer;

        tileObject.transform.parent = this.transform;
        tileObject.transform.localPosition = new Vector3(x * tileSize, 0f, y * tileSize);

        return tileObject;
    }

}
