using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "UnitStatData", menuName = "Unit/UnitStatData")]
public class UnitStatData : ScriptableObject
{
    public int uid;
    public int maxHp;
    public int attack;
}