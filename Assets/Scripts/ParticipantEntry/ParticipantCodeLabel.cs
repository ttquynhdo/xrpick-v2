using TMPro;
using UnityEngine;

// Shows the participant code on the end-of-course panel (#27).
public class ParticipantCodeLabel : MonoBehaviour
{
    // Key in docs/i18n/strings-todo.csv until #11/#12 move it to Loc.Get.
    private const string CodeFormat = "Code: {participant_id}"; // complex.end.code

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

        label.text = CodeFormat.Replace("{participant_id}", id);
    }
}
