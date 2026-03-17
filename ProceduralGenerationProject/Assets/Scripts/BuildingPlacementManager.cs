using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance;

    [SerializeField] Transform previewImagesGroup;
    [SerializeField] Transform buildingsGroup;
    [SerializeField] MapGenerator map;

    private BuildingData selectedBuilding;
    private GameObject previewImage;

    private Color placeableColour;
    private Color notPlacebleColour;

    private List<Color> originalColours;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        originalColours = new List<Color>();

        placeableColour = new Color(144, 238, 144, 0.5f);
        notPlacebleColour = new Color(255, 120, 120, 0.5f);
    }

    public void PlaceBuilding(BuildingData building)
    {
        selectedBuilding = building;
        Debug.Log("Placing: " + building.name);

        previewImage = new GameObject("previewImage");
        previewImage.transform.SetParent(previewImagesGroup);
        SpriteRenderer spriteRenderer = previewImage.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = building.image;
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);
    }

    void Update()
    {
        if (selectedBuilding == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;
        previewImage.transform.position = mousePos;

        for(int x = 0; x < map.width; x++)
        {
            for(int y = 0; y < map.height; y++)
            {
                originalColours.Add(map.noiseGrid[x, y].colour);

                map.noiseGrid[x, y].colour = notPlacebleColour;

                if (selectedBuilding.canBuildOnResources && (map.noiseGrid[x,y].name == "Ironium" || map.noiseGrid[x,y].name == "Siberiums"))
                {
                    map.noiseGrid[x, y].colour = placeableColour;
                }
                
                if(selectedBuilding.canBuildOnLand && map.noiseGrid[x,y].name == "Grass")
                {
                    map.noiseGrid[x, y].colour = placeableColour;
                }

                if (selectedBuilding.canBuildOnWater && map.noiseGrid[x, y].name == "Water")
                {
                    map.noiseGrid[x, y].colour = placeableColour;
                }  
            }
        }


        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            InstantiateBuilding();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            selectedBuilding = null;
            Destroy(previewImage);

            int index = 0;

            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.height; y++)
                {
                    map.noiseGrid[x, y].colour = originalColours[index];
                    index++;
                }
            }
        }
    }

    void InstantiateBuilding()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        worldPos.z = 0;

        float gridSize = 1f;

        worldPos.x = Mathf.Round(worldPos.x / gridSize) * gridSize;
        worldPos.y = Mathf.Round(worldPos.y / gridSize) * gridSize;


        if (map.noiseGrid[(int)worldPos.x, (int)worldPos.y].colour == notPlacebleColour) return;

        GameObject building = Instantiate(selectedBuilding.prefab, worldPos, Quaternion.identity);
        building.transform.SetParent(buildingsGroup);
    }
}
