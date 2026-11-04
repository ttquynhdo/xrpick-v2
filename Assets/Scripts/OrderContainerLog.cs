using UnityEngine;

/// <summary>
/// #29: container_enter / container_exit of the order container, called by the three counters only when their
/// package set actually changed. Nothing is logged once the order container is shipped (OrderBoxConveyor),
/// so the ride produces no false exit.
/// </summary>
public static class OrderContainerLog
{
    public static void Changed(MonoBehaviour counter, bool entered, PackageData package, int countAfter)
    {
        OrderBoxConveyor conveyor = counter.GetComponentInParent<OrderBoxConveyor>();

        if (conveyor == null)
        {
            conveyor = Object.FindAnyObjectByType<OrderBoxConveyor>();
        }

        if (conveyor != null && conveyor.IsShipped)
            return;

        ResearchLogger.LogContainerChange(entered, ResearchObjectId.Of(package),
            package != null ? package.MaterialKey : null, countAfter);
    }
}
