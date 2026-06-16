using UnityEngine;

public class Belt : MonoBehaviour
{
    [SerializeField] private TileData tileData;
    bool OnConveyor = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Something is on the belt");
        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)transform.up * tileData.walkSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(OnConveyor && other == this)
        {
            OnConveyor = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!OnConveyor && other == this)
        {
            OnConveyor = true;
        }

        Debug.Log("Something is on the belt");
        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            rb.MovePosition(rb.position + (Vector2)transform.up * tileData.walkSpeed * Time.deltaTime);
        }
    }
}
