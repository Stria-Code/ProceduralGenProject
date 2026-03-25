using UnityEngine;
using UnityEngine.InputSystem;
using TileTypes;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] GameObject buildMenu;
    [SerializeField] GameObject mapMenu;
    [SerializeField] GameObject map;
    GridGenerator gridGenerator;
    MapConfig currentMap;

    private void Awake()
    {
        currentMap = map.GetComponent<MapConfig>();
        gridGenerator = map.GetComponent<GridGenerator>();  
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if ((Keyboard.current.bKey.wasPressedThisFrame))
        {
            ToggleBuildMenu();
        }

        if ((Keyboard.current.mKey.wasPressedThisFrame))
        {
            ToggleMap();
        }

        if (Mouse.current.leftButton.wasPressedThisFrame )//&& gridGenerator.noiseGrid[(int)mousePos.x, (int)mousePos.y].buildOnTop.Length > 0)
        {
            ChangeTileWithClick(mousePos);
        }
    }


    void ToggleBuildMenu()
    {
        buildMenu.SetActive(!buildMenu.activeSelf);
    }

    void ToggleMap()
    {
        mapMenu.SetActive(!mapMenu.activeSelf);
    }


    void ChangeTileWithClick(Vector2 mousePos)
    {
         gridGenerator.SetTileAtPos((int)mousePos.x, (int)mousePos.y, currentMap.mapData.tiles[(int)TileType.DeepWater]);

    }
}
