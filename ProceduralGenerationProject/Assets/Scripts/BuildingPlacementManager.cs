using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance;

    [SerializeField] Transform previewImagesGroup;
    [SerializeField] Transform buildingsGroup;
    [SerializeField] Sprite targetImage;
    [SerializeField] GameObject mapGenerator;
    GridGenerator grid;

    private TileData selectedBuilding;
    private GameObject previewImage;
    private GameObject target;
    SpriteRenderer targetRenderer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        grid = mapGenerator.GetComponent<GridGenerator>();

    }

    public void CreatePreviewImageOfTile(TileData tile)
    {
        previewImage = new GameObject("previewImage");
        previewImage.transform.SetParent(previewImagesGroup);
        SpriteRenderer spriteRenderer = previewImage.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = tile.image;
        spriteRenderer.sortingOrder = 1;
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);
    }
    public void PlaceBuilding(TileData building)
    {
        selectedBuilding = building;
        Debug.Log("Placing: " + building.name);

        target = new GameObject("targetImage");
        target.transform.SetParent(previewImagesGroup);
        targetRenderer = target.AddComponent<SpriteRenderer>();
        targetRenderer.sprite = targetImage;
        targetRenderer.sortingOrder = 2;

        CreatePreviewImageOfTile(building);

    }

    void Update()
    {
        if (selectedBuilding == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        previewImage.transform.position = mousePos;
        target.transform.position = mousePos;


        if(grid.tileGrid[(int)mousePos.x, (int)mousePos.y].tileData.buildOnTop.Length > 0)
        {
            //can build here
            target.GetComponent<SpriteRenderer>().color = Color.green;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                InstantiateBuilding(mousePos, previewImage.transform);
            }
        }
        else
        {
            targetRenderer.color = Color.red;
        }


        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            previewImage.transform.Rotate(0, 0, -90);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            selectedBuilding = null;
            Destroy(previewImage);
            Destroy(target);
        }

    }

    void InstantiateBuilding(Vector2 worldPos, Transform previewImageTransform)
    {
        worldPos.x = Mathf.Round(worldPos.x);
        worldPos.y = Mathf.Round(worldPos.y);


        GameObject building = Instantiate(selectedBuilding.prefab, worldPos, Quaternion.identity);
        building.transform.rotation = previewImageTransform.rotation;
        building.transform.SetParent(buildingsGroup);
    }
}
