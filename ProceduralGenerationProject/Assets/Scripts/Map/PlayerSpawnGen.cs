using TileTypes;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class PlayerSpawnGen : MonoBehaviour
{
    private MapConfig currentMap;
    private GridGenerator gridGenerator;

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        gridGenerator = GetComponent<GridGenerator>();
    }

    public Vector2Int FindPlayerSpawnArea()
    {
        for (int i = 0; i < 100; i++)
        {
            //Random Position
            int x = Random.Range(0, currentMap.mapData.width);
            int y = Random.Range(0, currentMap.mapData.height);

            //Check if the tile is land
            if (gridGenerator.tileGrid[x, y].tileData.ID == currentMap.mapData.tiles[(int)TileType.Grass].ID) return new Vector2Int(x, y);
        }

        //Center of map - backup positioning
        return new Vector2Int(currentMap.mapData.width / 2, currentMap.mapData.height / 2);
    }
}
