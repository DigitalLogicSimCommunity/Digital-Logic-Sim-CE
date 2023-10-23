using Core;
using UnityEngine.Serialization;

public enum ChipType
{
    Compatibility,
    Gate,
    Miscellaneous,
    Custom
};
public class SpawnableChip : Chip
{
    public PackageGraphicData PackageGraphicData;
    public ChipType ChipType;

    public virtual void Init()
    {
    }
}