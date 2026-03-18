using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.EventSystems.EventTrigger;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] RawImage miniMap;

    //Dictonary for groups of tiles
    //Dictionary <int, GameObject> tileGroups;

    [SerializeField] Tile[] tiles;

    Texture2D texture;

    //Tiles
    [SerializeField] GameObject defaultTile;
    [SerializeField] Transform player;


    //Adjustables in the inspector
    [SerializeField] int seed; 
    [SerializeField] private MapData mapData;
    [SerializeField] private int candidatesToCheckForResourcePlacement;
    [SerializeField] float magnification;

    //Adjustable Cluster Sizes

    [SerializeField] int minClusterRadiusIronium, maxClusterRadiusIronium;
    [SerializeField] int minClusterRadiusSiberiums, maxClusterRadiusSiberiums;

    //Tile Names
    enum TileType
    {
        Grass,
        DeepWater,
        Water,
        Shroud,
        Siberiums,
        Ironium
    }

    public Tile[,] noiseGrid { get; private set; }

    int xOffset = 0; //Reduce = move terrain left / Increase = move terrain right
    int yOffset = 0; //Reduce = move terrain down / Increase = move terrain up

    //Helper directions array
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    private System.Random random = new System.Random();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (seed != 0)
        {
            random = new System.Random(seed);
        }
        else
        {
            random = new System.Random();
        }

        noiseGrid = new Tile[mapData.width, mapData.height];
        xOffset = random.Next(-10000, 10000);
        yOffset = random.Next(-10000, 10000);
        Debug.Log("xOffset: " + xOffset + " , " + "yOffset " + yOffset);

        CreateNoiseGrid();
        ApplyResources();

        ConfigureClusters(tiles[(int)TileType.Siberiums], 15, 300, tiles[(int)TileType.Grass]);
        ConfigureClusters(tiles[(int)TileType.Ironium], 15, 300, tiles[(int)TileType.Grass]);
        ConfigureClusters(tiles[(int)TileType.Water], 10, 5000, tiles[(int)TileType.Grass]);


        //BackUP cluster creation for tiles if none available on the map
        //***
        CreateClusters(tiles[(int)TileType.Water], tiles[(int)TileType.Grass]);
        CreateClusters(tiles[(int)TileType.Siberiums], tiles[(int)TileType.Grass]);
        CreateClusters(tiles[(int)TileType.Ironium], tiles[(int)TileType.Grass]);

        //***

        FindAndUpdateEdges();

        BuildMapFromGrid();

        miniMap.texture = Map.CreateTexture(noiseGrid);

        SpawnPlayer();
    }

    public MapData GetMapData()
    {
        return mapData;
    }

    public bool CheckIfInBoundaries(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapData.width || y >= mapData.height) return false;

        return true;
    }

    void CreateTile(Tile tile, int x, int y)
    {
        GameObject tileObj = Instantiate(defaultTile);
        tileObj.GetComponent<SpriteRenderer>().color = tile.colour;
        //tileObj.AddComponent<TileController>().tile = tile;

        //tileObj.GetComponent<TileController>().UpdateColour(tile.colour);


        //Inspector naming format for ease of access to the tile - coordinates mentioned in the name
        tileObj.name = string.Format("tile_x{0}_y{1}", x, y);

        //Gives its position
        tileObj.transform.localPosition = new Vector3(x, y, 0);
    }

    Tile CreateMainTerrain(int x, int y)
    {
        //Generate perlin noise value from coordinates input
        float terrainNoise = Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);

        //Thresholds for tile spawns
        if (terrainNoise < 0.10f) return tiles[(int)TileType.DeepWater]; //deepwater
        else if (terrainNoise < 0.20f) return tiles[(int)TileType.Water]; //water
        return tiles[(int)TileType.Grass]; //land
    }

    void CreateNoiseGrid()
    {
        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                noiseGrid[x, y] = CreateMainTerrain(x, y);
            }
        }
    }

    /* void ApplyResources()
     {
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 Tile baseTile = noiseGrid[x, y];

                 if (baseTile.ID == tiles[(int)TileType.DeepWater].ID || baseTile.ID == tiles[(int)TileType.Water].ID) continue;

                 float resourceNoise = Mathf.PerlinNoise((x + xOffset) / magnification,(y + yOffset) / magnification);

                 if (baseTile.ID == tiles[(int)TileType.Grass].ID)
                 {
                     if (resourceNoise < 0.05f) noiseGrid[x, y] = tiles[(int)TileType.Siberiums];
                     else if (resourceNoise < 0.15f) noiseGrid[x, y] = tiles[(int)TileType.Ironium];
                 }
             }
         }
     }*/


    void ApplyResources()
    {
        int resAmount = random.Next(mapData.minResAmount, mapData.maxResAmount);

        //Siberium deposits
        List<Vector2Int> siberiumNodes = GenerateBestCandidatePoints(resAmount, 50, tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in siberiumNodes)
        {
            int radius = random.Next(minClusterRadiusSiberiums, maxClusterRadiusSiberiums);
            GrowResourceCluster(pos, tiles[(int)TileType.Siberiums], tiles[(int)TileType.Grass], radius);
        }

        //Ironium deposits
        List<Vector2Int> ironiumNodes = GenerateBestCandidatePoints(resAmount, 50, tiles[(int)TileType.Grass]);

        foreach (Vector2Int pos in ironiumNodes)
        {
            int radius = random.Next(minClusterRadiusIronium, maxClusterRadiusIronium);
            GrowResourceCluster(pos, tiles[(int)TileType.Ironium], tiles[(int)TileType.Grass], radius);
        }
    }

    void GrowResourceCluster(Vector2Int center, Tile resource, Tile biomeToCheck, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int nx = center.x + x;
                int ny = center.y + y;

                if (!CheckIfInBoundaries(nx, ny)) continue;

                if (noiseGrid[nx, ny].ID == biomeToCheck.ID)
                {
                    //Calculate the diagonal distance
                    float distance = Mathf.Sqrt(x * x + y * y);

                    //Only populate the space if the value is within the circle 
                    if (distance <= radius && Random.value > 0.3f) //added a chance to not spawn tiles so they are distributed unevenly
                    {
                        noiseGrid[nx, ny] = resource;
                    }
                }
            }
        }
    }

    //Mitchells best candidate algorithm
    List<Vector2Int> GenerateBestCandidatePoints(int count, int candidates, Tile biomeToCheck)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        //Starting point
        points.Add(new Vector2Int(Random.Range(0, mapData.width), Random.Range(0, mapData.height)));

        for (int i = 1; i < count; i++)
        {
            Vector2Int bestCandidate = Vector2Int.zero;
            //-1f so the first candidate is always valid
            float bestDistance = -1f;

            for (int c = 0; c < candidates; c++)
            {
                int x = Random.Range(0, mapData.width);
                int y = Random.Range(0, mapData.height);

                if (noiseGrid[x, y].ID != biomeToCheck.ID) continue;

                //maxValue so the first candidate is always valid
                float minDist = float.MaxValue;

                foreach (Vector2Int p in points)
                {
                    float distance = Vector2Int.Distance(new Vector2Int(x, y), p);
                    if (distance < minDist) minDist = distance; //Now next check is gonna be distance < previous distance
                }


                if (minDist > bestDistance)
                {
                    bestDistance = minDist;
                    bestCandidate = new Vector2Int(x, y);
                }
            }

            points.Add(bestCandidate);
        }

        return points;
    }

    Vector2Int FindPlayerSpawnArea()
    {
        for (int i = 0; i < 100; i++)
        {
            //Random Position
            int x = Random.Range(0, mapData.width);
            int y = Random.Range(0, mapData.height);

            //Check if the tile is land
            if (noiseGrid[x, y].ID == tiles[(int)TileType.Grass].ID) return new Vector2Int(x, y);
        }

        //Center of map - backup positioning
        return new Vector2Int(mapData.width /2, mapData.height /2);
    }

    void BuildMapFromGrid()
    {
        for(int x = 0; x < mapData.width; x++)
        {
            for(int y = 0; y < mapData.height; y++)
            {
                CreateTile(noiseGrid[x, y], x, y);
            }
        }
    }

    //FloodFill algorithm to check for all the connected tiles of the same type in a cluster
    List<Vector2Int> FloodFill(int x, int y, int tileID, bool[,] visited)
    {
        //List of tiles in a cluster
        List<Vector2Int> cluster = new List<Vector2Int>();

        //Waiting list for checking tiles
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        //Start at the first tile and mark it as visited to avoid infinite looping
        queue.Enqueue(new Vector2Int(x, y));
        visited[x, y] = true;

        //If there are tiles unchecked in the queue keep looping
        while (queue.Count > 0)
        {
            //Take a tile out of the queue and add it to the current cluster
            Vector2Int currentTile = queue.Dequeue();
            cluster.Add(currentTile);

            foreach(Vector2Int direction in directions)
            {
                int newX = currentTile.x + direction.x;
                int newY = currentTile.y + direction.y;

                if (!CheckIfInBoundaries(newX, newY)) continue;

                //If this tile has not been checked and is of the same type
                if(!visited[newX, newY] && noiseGrid[newX, newY].ID == tileID)
                {
                    //Mark it as visited and add it to the queue to look at its neighbours later
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }

        return cluster;
    }

    void ConfigureClusters(Tile clusterTile, int minSize, int maxSize, Tile replacementTile)
    {
        bool[,] visited = new bool[mapData.width, mapData.height];

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                //Check to see if a tile has not been visited and is of the required type
                if (!visited[x,y] && noiseGrid[x, y].ID == clusterTile.ID)
                {
                    //Use floodfill algorithm to get the full cluster
                    List<Vector2Int> cluster = FloodFill(x, y, clusterTile.ID, visited);

                    if (cluster.Count < minSize || cluster.Count > maxSize)
                    {
                        //If its not, replace the cluster with another specified type of tiles
                        foreach(Vector2Int tile in cluster)
                        {
                            noiseGrid[tile.x, tile.y] = replacementTile;
                        }
                    }
                }
            }

        }
    }

    //Function to find if a certain tile exists anywhere on the map
    bool FindTile(Tile tile)
    {
        bool doesExist = false;

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                if (noiseGrid[x, y].ID == tile.ID)
                {
                    doesExist = true;
                }
            }
        }

        return doesExist;
    }

    void CreateClusters(Tile replacementTile, Tile tileToReplace)
    {
        if (!FindTile(replacementTile))
        {
            bool hasCreated = false;

            while (!hasCreated)
            { 
                int x = Random.Range(0, mapData.width);
                int y = Random.Range(0, mapData.height);

                if (!hasCreated && noiseGrid[x, y].ID == tileToReplace.ID)
                {
                    noiseGrid[x, y] = replacementTile;

                    foreach (Vector2Int direction in directions)
                    {
                        int newX = x + direction.x;
                        int newY = y + direction.y;

                        if (newX < 0 || newY < 0 || newX >= mapData.width || newY >= mapData.height) continue;

                        if (noiseGrid[newX, newY].ID == tileToReplace.ID)
                        {
                            noiseGrid[newX, newY] = replacementTile;
                        }

                        foreach (Vector2Int secondDirection in directions)
                        {
                            int newX2 = newX + secondDirection.x;
                            int newY2 = newY + secondDirection.y;

                            if (newX2 < 0 || newY2 < 0 || newX2 >= mapData.width || newY2 >= mapData.height) continue;

                            if (noiseGrid[newX2, newY2].ID == tileToReplace.ID)
                            {
                                noiseGrid[newX2, newY2] = replacementTile;
                            }
                        }
                    }
                  
                    hasCreated = true;
                    
                }
            }
        }
    }

    void FindAndUpdateEdges()
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                if(x < 1 || y < 1 || x >= mapData.width - 1 || y >= mapData.height - 1)
                {
                    noiseGrid[x, y] = tiles[(int)TileType.Shroud];
                }
            }
        }
    }

    void SpawnPlayer()
    {
        Vector2Int spawn = FindPlayerSpawnArea();

        //Moves the player
        player.position = new Vector3(spawn.x, spawn.y, 0);
    }
}
