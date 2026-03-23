using Microsoft.Unity.VisualStudio.Editor;
using System.Collections.Generic;
using System.Resources;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    private MapConfig currentMap;
    private TerrainPerlinGen perlinGen;

    [SerializeField] RawImage miniMap;
    [SerializeField] int seed;
    [SerializeField] int magnification;

    int xOffset = 0; //Reduce = move terrain left / Increase = move terrain right
    int yOffset = 0; //Reduce = move terrain down / Increase = move terrain up

    public Tile[,] noiseGrid { get; private set; }

    private System.Random random = new System.Random();

    private void Awake()
    {
        currentMap = GetComponent<MapConfig>();
        perlinGen = GetComponent<TerrainPerlinGen>();
    }

    public bool CheckIfInBoundaries(int x, int y)
    {
        if (x < 0 || y < 0 || x >= currentMap.mapData.width || y >= currentMap.mapData.height) return false;

        return true;
    }

    public void CreateNoiseGrid()
    {
        if (seed != 0)
        {
            random = new System.Random(seed);
        }
        else
        {
            random = new System.Random();
        }

        noiseGrid = new Tile[currentMap.mapData.width, currentMap.mapData.height];
        xOffset = random.Next(-10000, 10000);
        yOffset = random.Next(-10000, 10000);
        Debug.Log("xOffset: " + xOffset + " , " + "yOffset " + yOffset);

        //Create a 2D grid using perlin noise functon and storing it as IDs and gameobjects
        for (int x = 0; x < currentMap.mapData.width; x++)
        {
            for (int y = 0; y < currentMap.mapData.height; y++)
            {
                noiseGrid[x, y] = perlinGen.CreateMainTerrain(x, y, xOffset, yOffset, magnification);
            }
        }

        miniMap.texture = Map.CreateTexture(noiseGrid);
    }
}
