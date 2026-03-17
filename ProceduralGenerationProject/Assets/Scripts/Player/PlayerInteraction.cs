using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] GameObject buildMenu;
    [SerializeField] GameObject mapMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((Keyboard.current.bKey.wasPressedThisFrame))
        {
            ToggleBuildMenu();
        }

        if ((Keyboard.current.mKey.wasPressedThisFrame))
        {
            ToggleMap();
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
}
