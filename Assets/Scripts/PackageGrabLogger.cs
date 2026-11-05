using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// #29, decision 021: logs grab_start / grab_end for every Grabbable with a PackageData. Created at runtime
/// once per loaded scene (no component in any scene or prefab); subscribes like InstructionStepHighlighter,
/// inactive packages included. A grab starts with the first selecting pointer and ends when the last one
/// releases (Unselect or Cancel), so two hands on one package give one grab. Events of a SnapInteractor
/// (package held by a snap place) are ignored: before that, every snap logged a grab that never ended (pilot P802).
/// </summary>
public class PackageGrabLogger : MonoBehaviour
{
    private readonly List<(Grabbable grabbable, Action<PointerEvent> handler)> packageGrabs = new();
    private readonly Dictionary<Grabbable, HashSet<int>> selecting = new();

    private static PackageGrabLogger current;

    // The first scene raises no sceneLoaded the logger can hear, so it is covered here; later loads by the event.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        CreateFor(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            CreateFor(scene);
    }

    private static void CreateFor(Scene scene)
    {
        if (current != null && current.gameObject.scene == scene)
            return;

        ResearchObjectId.Clear();
        var host = new GameObject(nameof(PackageGrabLogger));
        SceneManager.MoveGameObjectToScene(host, scene);
        current = host.AddComponent<PackageGrabLogger>();
    }

    private void Awake()
    {
        foreach (Grabbable grabbable in FindObjectsByType<Grabbable>(FindObjectsInactive.Include))
        {
            PackageData package = grabbable.GetComponentInParent<PackageData>(true);

            if (package == null)
                continue;

            // Taken now, before any ride can reparent the package.
            ResearchObjectId.Of(package);
            Grabbable target = grabbable;
            Action<PointerEvent> handler = evt => HandlePointerEvent(evt, target, package);
            packageGrabs.Add((grabbable, handler));
            grabbable.WhenPointerEventRaised += handler;
        }
    }

    private void OnDestroy()
    {
        foreach ((Grabbable grabbable, Action<PointerEvent> handler) in packageGrabs)
        {
            if (grabbable != null)
            {
                grabbable.WhenPointerEventRaised -= handler;
            }
        }
    }

    private void HandlePointerEvent(PointerEvent evt, Grabbable grabbable, PackageData package)
    {
        // A SnapInteractor on the package selects its Grabbable while it sits in a snap place (Basic, Mixed EKZ 003);
        // that is no hand grab. Its event Data is the interactor itself (its _data field is empty in every scene).
        if (evt.Data is SnapInteractor)
        {
            return;
        }

        if (!selecting.TryGetValue(grabbable, out HashSet<int> pointers))
        {
            pointers = new HashSet<int>();
            selecting[grabbable] = pointers;
        }

        switch (evt.Type)
        {
            case PointerEventType.Select:
                if (pointers.Add(evt.Identifier) && pointers.Count == 1)
                {
                    ResearchLogger.LogGrabStart(ResearchObjectId.Of(package), package.MaterialKey);
                }
                break;

            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                if (pointers.Remove(evt.Identifier) && pointers.Count == 0)
                {
                    ResearchLogger.LogGrabEnd(ResearchObjectId.Of(package), package.MaterialKey);
                }
                break;
        }
    }
}
