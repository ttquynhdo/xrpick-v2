using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// #35, docs/decisions/011: marks the lesson of the open scene in the Menu with bold text and a "» " mark,
/// so it is not shown by colour only. #54: also writes the menu title and every lesson name in the current
/// language. Each item is found by the SceneMenuController method its toggle calls, not by its text.
/// #42: in a research session only the next lesson (build order) can be opened, and only after the current
/// lesson is done (ILessonCompletion or IOrderCompletion found in the scene); locked items are dimmed and
/// not interactable (MenuLockRule).
/// </summary>
public class MenuCurrentLesson : MonoBehaviour
{
    private const string Mark = "» ";
    private const string TitleKey = "menu.title";
    private const string ItemLabelName = "Title";
    private const float LockedAlpha = 0.35f;

    private static readonly Dictionary<string, string> SceneByMethod = new()
    {
        { nameof(SceneMenuController.OpenWelcome), "WelcomeScene" },
        { nameof(SceneMenuController.OpenConcept), "ConceptScene" },
        { nameof(SceneMenuController.OpenBasicPicking), "BasicPickingScene" },
        { nameof(SceneMenuController.OpenMixedPicking), "MixedPickingScene" },
        { nameof(SceneMenuController.OpenComplex), "ComplexScene" },
    };

    [Tooltip("The heading above the lesson items.")]
    [SerializeField] private TMP_Text menuTitle;

    private ILessonCompletion lesson;
    private IOrderCompletion order;
    private bool sourceSearched;

    // OnEnable, not Start: in WelcomeScene the Menu is shown again only after the session has started.
    private void OnEnable()
    {
        FindCompletionSource();
        if (lesson != null)
            lesson.LessonCompleted += ApplyLock;
        if (order != null)
            order.OrderCompleted += ApplyLock;

        ApplyLock();
    }

    private void OnDisable()
    {
        if (lesson != null)
            lesson.LessonCompleted -= ApplyLock;
        if (order != null)
            order.OrderCompleted -= ApplyLock;
    }

    private void ApplyLock()
    {
        string participantId = ResearchLogger.ParticipantId;
        int current = SceneManager.GetActiveScene().buildIndex;
        bool lessonDone = (lesson != null && lesson.IsLessonCompleted) || (order != null && order.IsCompleted);

        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true))
        {
            if (!TryGetScene(toggle, out string scene))
                continue;

            SetOpen(toggle, MenuLockRule.IsOpen(participantId, current, BuildIndexOf(scene), lessonDone));
            EnsureLockedTouchRelay(toggle, scene);
        }
    }

    // #29: a touch on a locked item is logged as menu_select with blocked = true.
    private static void EnsureLockedTouchRelay(Toggle toggle, string scene)
    {
        if (!toggle.TryGetComponent(out LockedMenuItemRelay relay))
            relay = toggle.gameObject.AddComponent<LockedMenuItemRelay>();

        relay.Init(scene);
    }

    // One per scene: WelcomePracticeFeedback, PackageQuizController or a validator. Found, not wired, so the
    // five Menu copies need no Inspector change.
    private void FindCompletionSource()
    {
        if (sourceSearched)
            return;

        sourceSearched = true;
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (lesson == null && behaviour is ILessonCompletion lessonSource)
                lesson = lessonSource;
            if (order == null && behaviour is IOrderCompletion orderSource)
                order = orderSource;
        }

        if (lesson == null && order == null)
            Debug.LogWarning($"MenuCurrentLesson: no lesson or order completion in {SceneManager.GetActiveScene().name}; in a research session the next lesson stays locked.", this);
    }

    private void Start()
    {
        if (!RequiredReference.IsMissing(menuTitle, nameof(menuTitle), this))
            menuTitle.text = Loc.Get(TitleKey);

        string activeScene = SceneManager.GetActiveScene().name;
        bool marked = false;

        foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true))
        {
            if (!TryGetScene(toggle, out string scene) || !LessonNames.TryGetDisplayName(scene, out string lesson))
                continue;

            TMP_Text label = LabelOf(toggle);
            if (label == null)
            {
                Debug.LogWarning($"MenuCurrentLesson: menu item '{toggle.name}' has no TMP named {ItemLabelName}.", this);
                continue;
            }

            bool current = scene == activeScene;
            label.text = current ? Mark + lesson : lesson;
            if (current)
            {
                label.fontStyle |= FontStyles.Bold;
                marked = true;
            }
        }

        if (!marked)
            Debug.LogWarning($"MenuCurrentLesson: no menu item opens {activeScene}.", this);
    }

    private static bool TryGetScene(Toggle toggle, out string scene)
    {
        for (int i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++)
        {
            if (SceneByMethod.TryGetValue(toggle.onValueChanged.GetPersistentMethodName(i), out scene))
                return true;
        }

        scene = null;
        return false;
    }

    // A CanvasGroup with interactable off also turns the toggle off; raycasts stay blocked, so a touch on a
    // locked item does nothing but the LockedMenuItemRelay log line.
    private static void SetOpen(Toggle toggle, bool open)
    {
        CanvasGroup group = toggle.GetComponent<CanvasGroup>();
        if (group == null)
        {
            if (open)
                return;
            group = toggle.gameObject.AddComponent<CanvasGroup>();
        }

        group.interactable = open;
        group.alpha = open ? 1f : LockedAlpha;
    }

    private static int BuildIndexOf(string scene)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == scene)
                return i;
        }

        return -1;
    }

    private static TMP_Text LabelOf(Toggle toggle)
    {
        foreach (TMP_Text text in toggle.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == ItemLabelName)
                return text;
        }

        return null;
    }
}
