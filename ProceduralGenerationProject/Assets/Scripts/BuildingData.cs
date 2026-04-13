using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/Building", order = 1)]
public class BuildingData : ScriptableObject
{
    [SerializeField] public TileData tileData;
    [SerializeField] public bool unlocked;
    [SerializeField] public int cost; //not used in this version
}
