/// <summary>
/// #19, docs/decisions/016: how a package counts in the order container. An opened carton stays in the
/// storage box as a source of loose pieces and never counts as a full carton.
/// </summary>
public enum PackageForm
{
    FullCarton,
    LoosePiece,
    OpenedCarton
}
