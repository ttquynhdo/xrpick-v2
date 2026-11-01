// Serialized as ints on PackageData: never remove or reorder values (#17, docs/decisions/007).
// GG/VPE are not package types any more (A2); the material is identified by PackageData.MaterialKey.
public enum PackageType
{
    GG,
    VPEFull,
    VPEOpenedDisplay,
    GGA,
    GGB,
    LooseItemDisplay
}
