using UnityEngine;

public class TileController : MonoBehaviour
{
    public Tile tile;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = tile.colour;
    }

    public void UpdateColour(Color newColour)
    {
        spriteRenderer.color = newColour;
    }
}
