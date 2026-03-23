using UnityEngine;


[CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile", order = 1)]
public class TileData : ScriptableObject
{
    [SerializeField] public int ID;
    [SerializeField] public bool walkable;
    [SerializeField] public int walkSpeed;
    [SerializeField] public int[] buildOnTop;
    [SerializeField] public int health;
    [SerializeField] public Color colour;
    [SerializeField] public Sprite image;

}
