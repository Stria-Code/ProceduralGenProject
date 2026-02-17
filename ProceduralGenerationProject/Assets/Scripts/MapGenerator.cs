using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MapGenerator : MonoBehaviour
{
    //Dictionary for map tiles
    Dictionary <int, GameObject> tileSet;

    //Dictonary for groups of tiles
    Dictionary <int, GameObject> tileGroups;

    //Tiles
    [SerializeField] GameObject grass;
    [SerializeField] GameObject water;
    [SerializeField] GameObject resource;

    //Grid Size
    int width = 300;
    int height = 250;

    List<List <int>> noiseGrid = new List<List<int>>();
    List<List <GameObject>> tileGrid = new List<List<GameObject>>();

    float magnification = 25.0f; //Size
    int xOffset = 0; //Reduce = move terrain left / Increase = move terrain right
    int yOffset = 0; //Reduce = move terrain down / Increase = move terrain up

    //Helper directions array
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    [SerializeField] Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xOffset = Random.Range(-10000, 10000);
        yOffset = Random.Range(-10000, 10000);

        CreateTileSet();
        CreateGroups();
        CreateNoiseGrid();

        CreateClusters(2, 20, 200, 0);

        BuildMapFromGrid();

        SpawnPlayer();
    }


    void CreateTileSet()
    {
        //Add tiles to the map dictionary - all available tiles are added here
        tileSet = new Dictionary <int, GameObject>();
        tileSet.Add(0, grass);
        tileSet.Add(1, water);
        tileSet.Add(2, resource);
    }

    void CreateGroups()
    {
        //Create a new group in the inspector for each of the tile types
        tileGroups = new Dictionary<int, GameObject>();

        foreach (KeyValuePair<int, GameObject> key in tileSet)
        {
            //Create an empty parent object for this tile type
            GameObject tileGroup = new GameObject(key.Value.name);

            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector3(0, 0, 0);

            //Store it by tile ID
            tileGroups.Add(key.Key, tileGroup);
        }
    }

    int GetIdUsingPerlin(int x, int y)
    { 
        //Generate perlin noise value from coordinates input
        float perlinNoise = Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);


        //Thresholds for tile spawns
        if (perlinNoise < 0.25f) return 1; //water
        if (perlinNoise < 0.75f) return 0; //land
        return 2;
    }

    void CreateTile(int tileID, int x, int y)
    {
        GameObject tilePrefab = tileSet[tileID];
        GameObject tileGroup = tileGroups[tileID];

        //Creates a tile using the passed in tile ID to make out which type of tile it should create
        //Groups it in the inspector with similar tiles
        GameObject tile = Instantiate(tilePrefab, tileGroup.transform);

        //Inspector naming format for ease of access to the tile - coordinates mentioned in the name
        tile.name = string.Format("tile_x{0}_y{1}", x, y);

        //Gives its position
        tile.transform.localPosition = new Vector3(x, y, 0);

        //Store the gameobject in the list
        tileGrid[x].Add(tile);
    }

    void CreateNoiseGrid()
    {
        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < width; x++)
        {
            noiseGrid.Add(new List<int> {});

            for (int y = 0; y < height; y++)
            {
                int tileID = GetIdUsingPerlin(x, y);
                noiseGrid[x].Add(tileID);
            }
        }
    }

    Vector2Int FindPlayerSpawnArea()
    {
        for (int i = 0; i < 100; i++)
        {
            //Random Position
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            //Check if the tile is land
            if (noiseGrid[x][y] == 0)
            {
                return new Vector2Int(x, y);
            }
        }

        //Center of map - backup positioning
        return new Vector2Int(width/2, height/2);
    }

    void BuildMapFromGrid()
    {
        for(int x = 0; x < width; x++)
        {
            tileGrid.Add(new List<GameObject>{});

            for(int y = 0; y < height; y++)
            {
                CreateTile(noiseGrid[x][y], x, y);
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

            //Check each neighbour
            foreach(Vector2Int direction in directions)
            {
                int newX = currentTile.x + direction.x;
                int newY = currentTile.y + direction.y;

                //If its outside map bounds, skip it
                if (newX < 0 || newY < 0 || newX >= width || newY >= height) continue;

                //If this tile has not been checked and is of the same type
                if(!visited[newX, newY] && noiseGrid[newX][newY] == tileID)
                {
                    //Mark it as visited and add it to the queue to look at its neighbours later
                    visited[newX, newY] = true;
                    queue.Enqueue(new Vector2Int(newX, newY));
                }
            }
        }

        return cluster;
    }

    void CreateClusters(int tileID, int minSize, int maxSize, int replacementID)
    {
        bool[,] visited = new bool[width, height];

        //Check the whole map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Check to see if a tile has not been visited and is of the required type
                if (!visited[x,y] && noiseGrid[x][y] == tileID)
                {
                    //Use floodfill algorithm to get the full cluster
                    List<Vector2Int> cluster = FloodFill(x, y, tileID, visited);

                    //Check if its within the required sizes
                    if (cluster.Count < minSize || cluster.Count > maxSize)
                    {
                        //If its not, replace the cluster with another specified type of tiles
                        foreach(Vector2Int tile in cluster)
                        {
                            noiseGrid[tile.x][tile.y] = replacementID;
                        }
                    }
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
