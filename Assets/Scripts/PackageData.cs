using UnityEngine;

public class PackageData : MonoBehaviour
{
    public PackageType packageType;

    // #17, docs/decisions/007: one material has the same key, Mat.-Nr and VPE in every scene.
    [Header("Material")]
    [Tooltip("Stable, language-independent key (snake_case), e.g. ring_d. Never shown to trainees.")]
    [SerializeField] private string materialKey;

    [Tooltip("Fake material number printed on the label (Mat.-Nr).")]
    [SerializeField] private string materialNumber;

    [Tooltip("Pieces per carton or bag (VPE). Loose pieces carry the VPE of their carton.")]
    [SerializeField] private int vpe;

    public string MaterialKey => materialKey;
    public string MaterialNumber => materialNumber;
    public int Vpe => vpe;

    private void Awake()
    {
        if (string.IsNullOrEmpty(materialKey))
        {
            Debug.LogError($"PackageData: materialKey is not assigned on {name}.", this);
        }
    }
}
