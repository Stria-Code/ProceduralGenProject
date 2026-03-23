using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData", order = 1)]
public class MapData : ScriptableObject
{
    [SerializeField] public int width, height;
    [SerializeField] public int minResAmount, maxResAmount;
    [SerializeField] public Tile[] tiles;
}
