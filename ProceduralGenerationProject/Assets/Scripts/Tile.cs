using UnityEngine;

[CreateAssetMenu(fileName = "Tile", menuName = "ScriptableObjects/Tile", order = 1)]
public class Tile : ScriptableObject
{
    [SerializeField] public int ID;
    [SerializeField] public string name;
    [SerializeField] public bool walkable;
    [SerializeField] public Color colour;
}
