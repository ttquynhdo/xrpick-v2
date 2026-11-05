using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// #41, docs/decisions/003: EKZ 3 in BasicPickingScene (#94, decision 028: also Mixed position 1). Each carton in the order container must sit in its
/// own snap place (one by one, aligned). Tracks which cartons are snapped from the SnapInteractable events;
/// the validator asks only when the trainee confirms. Quantity counting never reads this.
/// </summary>
public class EkzPlacementCheck : MonoBehaviour
{
    [Tooltip("The snap places inside the order container, one per carton of the order.")]
    [SerializeField] private SnapInteractable[] places;

    [Tooltip("Optional: played when a carton snaps into a place (the signal must not be colour only).")]
    [SerializeField] private AudioSource snapSound;

    // #94, decision 028: Empf Kz 003 (EKZ 3) is the only class rated by placement.
    private const string OrderedEmpfKz = "003";

    private readonly HashSet<PackageData> snapped = new();

    /// <summary>True when the order line asks for every carton in its own place (Empf Kz 003).</summary>
    public static bool NeedsPlaces(OrderLine line) => line != null && line.empfKz == OrderedEmpfKz;

    // Raised when a carton snaps into or leaves a place, so an old result can be cleared (#4).
    public event Action PlacementChanged;

    private void OnEnable()
    {
        if (places == null || places.Length == 0)
        {
            Debug.LogError("EkzPlacementCheck: places is not assigned.", this);
            return;
        }

        foreach (SnapInteractable place in places)
        {
            if (place == null)
                continue;

            place.WhenSelectingInteractorViewAdded += HandleSnapped;
            place.WhenSelectingInteractorViewRemoved += HandleUnsnapped;

            // Cartons already in a place when this component is enabled (no event for them).
            foreach (IInteractorView interactor in place.SelectingInteractorViews)
            {
                PackageData package = PackageOf(interactor);

                if (package != null)
                {
                    snapped.Add(package);
                }
            }
        }
    }

    private void OnDisable()
    {
        if (places != null)
        {
            foreach (SnapInteractable place in places)
            {
                if (place == null)
                    continue;

                place.WhenSelectingInteractorViewAdded -= HandleSnapped;
                place.WhenSelectingInteractorViewRemoved -= HandleUnsnapped;
            }
        }

        snapped.Clear();
    }

    // True when every active package sits in a snap place. Destroyed or disabled cartons are not in the
    // container any more and are skipped (CalcForWMS gets no OnTriggerExit for them).
    public bool AllPlaced(IEnumerable<PackageData> packages)
    {
        foreach (PackageData package in packages)
        {
            if (package == null || !package.isActiveAndEnabled)
                continue;

            if (!snapped.Contains(package))
                return false;
        }

        return true;
    }

    private void HandleSnapped(IInteractorView interactor)
    {
        PackageData package = PackageOf(interactor);

        if (package == null || !snapped.Add(package))
            return;

        if (snapSound != null)
        {
            snapSound.Play();
        }

        PlacementChanged?.Invoke();
    }

    private void HandleUnsnapped(IInteractorView interactor)
    {
        PackageData package = PackageOf(interactor);

        if (package != null && snapped.Remove(package))
        {
            PlacementChanged?.Invoke();
        }
    }

    // The SnapInteractor sits on a child of the carton that holds PackageData.
    private static PackageData PackageOf(IInteractorView interactor)
    {
        return interactor is Component component ? component.GetComponentInParent<PackageData>() : null;
    }
}
