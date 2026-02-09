using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

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
    int width = 100;
    int height = 50;

    List<List <int>> noiseGrid = new List<List<int>>();
    List<List <GameObject>> tileGrid = new List<List<GameObject>>();

    float magnification = 10.0f; //Zoom
    int xOffset = 0; //Reduce = move terrain left / Increase = move terrain right
    int yOffset = 0; //Reduce = move terrain down / Increase = move terrain up

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xOffset = Random.Range(-1000, 1000);
        yOffset = Random.Range(-1000, 1000);

        CreateTileSet();
        CreateGroups();
        CreateMap();
    }


    void CreateTileSet()
    {
        //Add tiles to the map dictionary - all available tiles should be added here
        tileSet = new Dictionary <int, GameObject>();
        tileSet.Add(0, grass);
        tileSet.Add(1, water);
        tileSet.Add(2, resource);
    }

    void CreateGroups()
    {
        //Create a new group
        tileGroups = new Dictionary<int, GameObject>();

        foreach (KeyValuePair<int, GameObject> pair in tileSet)
        {
            //Create an empty parent object for this tile type
            GameObject tileGroup = new GameObject(pair.Value.name);

            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector3(0, 0, 0);

            //Store it by tile ID
            tileGroups.Add(pair.Key, tileGroup);
        }
    }



    void CreateMap()
    {
        for (int x = 0; x < width; x++)
        {
            noiseGrid.Add(new List<int> {});
            tileGrid.Add(new List<GameObject> {});

            for (int y = 0; y < height; y++)
            {
                int tileID = GetIdUsingPerlin(x, y);
                noiseGrid[x].Add(tileID);
                CreateTile(tileID, x, y);
            }
        }
    }

    int GetIdUsingPerlin( int x, int y)
    {
        float perlinNoise =  Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);
        //float clampPerlin = Mathf.Clamp(perlinNoise, 0.0f, 1.0f);
        //float scalePerlin = clampPerlin * tileSet.Count;

        //if(scalePerlin == 3)
        //{
        //    scalePerlin = 2;
        //}

        if (perlinNoise < 0.4f) return 1; // water
        if (perlinNoise < 0.75f) return 0; // grass
        return 2;


        //return Mathf.FloorToInt(scalePerlin);
    }

    void CreateTile(int tileID, int x, int y)
    {
        GameObject tilePrefab = tileSet[tileID];
        GameObject tileGroup = tileGroups[tileID];

        GameObject tile = Instantiate(tilePrefab, tileGroup.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);

        tileGrid[x].Add(tile);
    }
}
