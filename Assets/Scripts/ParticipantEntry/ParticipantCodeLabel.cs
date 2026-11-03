using TMPro;
using UnityEngine;

// Shows the participant code on the end-of-course panel (#27).
public class ParticipantCodeLabel : MonoBehaviour
{
    // Loc table UI (#54).
    private const string CodeKey = "complex.end.code";

    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        // Defaults to the TMP on the same GameObject when nothing is assigned.
        if (label == null)
            label = GetComponent<TMP_Text>();

        if (RequiredReference.IsMissing(label, nameof(label), this))
            return;

        string id = ResearchLogger.ParticipantId;
        if (id == null)
        {
            gameObject.SetActive(false);
            return;
        }

        label.text = Loc.Get(CodeKey, ("participant_id", id));
    }
}
