using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenuButton : MonoBehaviour
{
    [SerializeField] BuildingData building;
    private Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = GetComponent<Button>();
        
        gameObject.SetActive(building.unlocked);

        if (!building.unlocked) return;

        GetComponent<Image>().sprite = building.tileData.image;

        button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        BuildingPlacementManager.Instance.PlaceBuilding(building.tileData);
    }
}
