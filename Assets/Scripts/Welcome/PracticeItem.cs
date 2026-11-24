using UnityEngine;

/// <summary>
/// #175: marks a package on the WelcomeScene practice desk (carton, pouch, loose rings). It keeps its PackageData,
/// so PackageDropper lets it fall when it is let go like every package of the course, but PackageGrabLogger writes
/// no grab_start / grab_end for it: the warm-up is logged only by welcome_practice_done.
/// </summary>
[DisallowMultipleComponent]
public class PracticeItem : MonoBehaviour
{
}
