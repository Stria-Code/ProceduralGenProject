using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class DestroyBuildingButton : MonoBehaviour
{
    [SerializeField] Transform previewImagesGroup;
    [SerializeField] Sprite image;
    private GameObject previewImage;
    private Button button;
    bool isButtonClicked;


    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    void Update()
    {
        if (!isButtonClicked) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;
        previewImage.transform.position = mousePos;

        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null && hit.CompareTag("Building"))
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Destroy(hit.gameObject);
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isButtonClicked = false;
            Destroy(previewImage);
        }
    }

    void OnButtonClicked()
    {
        isButtonClicked = true;
        previewImage = new GameObject("previewImage");
        previewImage.transform.SetParent(previewImagesGroup);
        SpriteRenderer spriteRenderer = previewImage.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 2;
        spriteRenderer.sprite = image;
    }
}
