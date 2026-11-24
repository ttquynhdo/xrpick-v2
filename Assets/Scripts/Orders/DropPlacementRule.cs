using System;
using System.Collections.Generic;

/// <summary>
/// One package as the drop rule sees it (#168): an oriented rectangle on the floor plane (world x/z, metres) turned
/// by <see cref="Yaw"/> (radians, Unity's rotation about the vertical axis), plus its height span.
/// </summary>
public readonly struct DropBody
{
    public DropBody(double x, double z, double halfX, double halfZ, double yaw, double bottom, double top)
    {
        X = x;
        Z = z;
        HalfX = halfX;
        HalfZ = halfZ;
        Yaw = yaw;
        Bottom = bottom;
        Top = top;
    }

    public double X { get; }

    public double Z { get; }

    public double HalfX { get; }

    public double HalfZ { get; }

    public double Yaw { get; }

    public double Bottom { get; }

    public double Top { get; }

    public double Height => Top - Bottom;

    /// <summary>Rings and clamps lying flat: whatever is dropped onto them lies on them.</summary>
    public bool IsThin => Height <= DropPlacementRule.ThinHeight;

    public DropBody At(double x, double z, double bottom) => new(x, z, HalfX, HalfZ, Yaw, bottom, bottom + Height);

    // Unity: Quaternion.Euler(0, yaw, 0) * right = (cos, 0, -sin), * forward = (sin, 0, cos).
    internal (double x, double z) AxisU => (Math.Cos(Yaw), -Math.Sin(Yaw));

    internal (double x, double z) AxisV => (Math.Sin(Yaw), Math.Cos(Yaw));

    internal double ExtentX => Math.Abs(Math.Cos(Yaw)) * HalfX + Math.Abs(Math.Sin(Yaw)) * HalfZ;

    internal double ExtentZ => Math.Abs(Math.Sin(Yaw)) * HalfX + Math.Abs(Math.Cos(Yaw)) * HalfZ;
}

/// <summary>The inside of a container: where a package may lie (x/z inside the walls, from the floor up to the rim).</summary>
public readonly struct DropSpace
{
    public DropSpace(double minX, double maxX, double minZ, double maxZ, double floor, double rim)
    {
        MinX = minX;
        MaxX = maxX;
        MinZ = minZ;
        MaxZ = maxZ;
        Floor = floor;
        Rim = rim;
    }

    public double MinX { get; }

    public double MaxX { get; }

    public double MinZ { get; }

    public double MaxZ { get; }

    public double Floor { get; }

    public double Rim { get; }
}

/// <summary>How the dropped package came to rest (for tests and MCP checks).</summary>
public enum DropOutcome
{
    /// <summary>Nothing below it: on the container floor.</summary>
    Floor,
    /// <summary>On the packages below it (a thin one, or they carry at least half of its base).</summary>
    OnTop,
    /// <summary>Pushed aside just far enough to clear a package that carried less than half of it.</summary>
    PushedAside,
    /// <summary>A wall stopped the push: on top, or lower if on top would stick out above the rim.</summary>
    Blocked
}

/// <summary>Where the dropped package lies: centre of its footprint and its bottom height.</summary>
public readonly struct DropPlan
{
    public DropPlan(double x, double z, double bottom, DropOutcome outcome)
    {
        X = x;
        Z = z;
        Bottom = bottom;
        Outcome = outcome;
    }

    public double X { get; }

    public double Z { get; }

    public double Bottom { get; }

    public DropOutcome Outcome { get; }
}

/// <summary>
/// #168: where a package let go over a container comes to rest, without real physics (packages stay kinematic).
/// 1. A thin package below (ring, clamp): the new one lies on it. 2. Cartons/bags below carry at least half of its base:
/// it lies on them. 3. They carry less than half: it is pushed aside by exactly the overlap (away from the package below)
/// and lies next to it. 4. A wall stops that push: it lies on top, or lower (partly inside the others) if on top would
/// stick out above the rim. 5. It always stays where the hand put it in x/z (except rule 3 and being pushed in from the
/// walls), and its top never rises above the rim, so the counter always sees it: where lying on top would stick out,
/// it is pushed aside like in rule 3 if that takes at most <see cref="MaxRimPushShare"/> of its longest side, and only
/// otherwise lies lower, partly inside the others.
/// </summary>
public static class DropPlacementRule
{
    public const double ThinHeight = 0.02;
    public const double MinSupport = 0.5;
    // Tops within this height of the highest one form the layer the package would lie on.
    public const double LayerTolerance = 0.01;
    // Overlap up to this depth counts as touching, not lying on it (cartons are a little tilted, labels stick out).
    public const double Nestle = 0.005;
    // A push that moves less than this made no progress (a wall is in the way).
    public const double Touch = 0.001;
    // Gap left next to a package it was pushed away from, and below the rim.
    public const double Gap = 0.002;
    public const double RimClearance = 0.002;
    // Lying on top would stick out above the rim: it may be pushed aside instead, at most this share of its longest
    // side in all (so it never wanders far from the hand).
    public const double MaxRimPushShare = 0.6;
    private const int MaxPushes = 4;
    private const int SupportSamples = 8;

    public static DropPlan Plan(DropBody package, DropSpace space, IReadOnlyList<DropBody> others)
    {
        (double startX, double startZ) = ClampInside(package, package.X, package.Z, space);
        double x = startX;
        double z = startZ;
        bool pushed = false;
        bool blocked = false;
        double rimPushLeft = MaxRimPushShare * 2 * Math.Max(package.HalfX, package.HalfZ);

        for (int push = 0; push <= MaxPushes; push++)
        {
            DropBody here = package.At(x, z, space.Floor);
            List<DropBody> below = Overlapping(here, others);
            if (below.Count == 0)
                return Finish(package, x, z, space.Floor, space, pushed ? DropOutcome.PushedAside : DropOutcome.Floor);

            double top = HighestTop(below);
            List<DropBody> layer = below.FindAll(b => b.Top >= top - LayerTolerance);
            bool carried = layer.Exists(b => b.IsThin) || Support(here, layer) >= MinSupport;
            bool fits = top + package.Height <= space.Rim - RimClearance;
            if (carried && fits)
                return Finish(package, x, z, top, space, pushed ? DropOutcome.PushedAside : DropOutcome.OnTop);

            if (push == MaxPushes)
                break;

            DropBody widest = Widest(here, layer);
            (double dx, double dz) = Separation(here, widest);
            if (carried)
            {
                // On top it would stick out above the rim (rule 5): beside it instead, if that is near enough.
                rimPushLeft -= Math.Sqrt(dx * dx + dz * dz);
                if (rimPushLeft < 0)
                    break;
            }
            // As far as the walls allow; what is left of the overlap is looked at again in the next round.
            (double nx, double nz) = ClampInside(package, x + dx, z + dz, space);
            if (Math.Sqrt((nx - x) * (nx - x) + (nz - z) * (nz - z)) < Touch)
            {
                blocked = true;
                break;
            }

            x = nx;
            z = nz;
            pushed = true;
        }

        // No free spot next to it (wall, or more packages around): back where the hand put it, on top.
        double bottom = HighestTop(Overlapping(package.At(startX, startZ, space.Floor), others), space.Floor);
        return Finish(package, startX, startZ, bottom, space, blocked ? DropOutcome.Blocked : DropOutcome.OnTop);
    }

    private static DropPlan Finish(DropBody package, double x, double z, double bottom, DropSpace space, DropOutcome outcome)
    {
        double highest = space.Rim - RimClearance - package.Height;
        if (bottom > highest)
        {
            bottom = Math.Max(space.Floor, highest);
            if (outcome == DropOutcome.OnTop)
                outcome = DropOutcome.Blocked;
        }
        return new DropPlan(x, z, bottom, outcome);
    }

    private static (double x, double z) ClampInside(DropBody package, double x, double z, DropSpace space)
    {
        return (Clamp(x, space.MinX + package.ExtentX, space.MaxX - package.ExtentX),
            Clamp(z, space.MinZ + package.ExtentZ, space.MaxZ - package.ExtentZ));
    }

    private static double Clamp(double value, double min, double max)
    {
        if (min > max)
            return (min + max) / 2;
        return Math.Min(max, Math.Max(min, value));
    }

    private static List<DropBody> Overlapping(DropBody package, IReadOnlyList<DropBody> others)
    {
        var result = new List<DropBody>();
        foreach (DropBody other in others)
        {
            if (Penetration(package, other, out _, out _) > Nestle)
            {
                result.Add(other);
            }
        }
        return result;
    }

    private static double HighestTop(List<DropBody> bodies, double floor = double.NegativeInfinity)
    {
        double top = floor;
        foreach (DropBody body in bodies)
        {
            top = Math.Max(top, body.Top);
        }
        return top;
    }

    /// <summary>Share of the package's base (0..1) that lies over one of the given bodies.</summary>
    public static double Support(DropBody package, IReadOnlyList<DropBody> layer)
    {
        (double ux, double uz) = package.AxisU;
        (double vx, double vz) = package.AxisV;
        int inside = 0;
        for (int i = 0; i < SupportSamples; i++)
        {
            for (int j = 0; j < SupportSamples; j++)
            {
                double a = ((i + 0.5) / SupportSamples * 2 - 1) * package.HalfX;
                double b = ((j + 0.5) / SupportSamples * 2 - 1) * package.HalfZ;
                double px = package.X + a * ux + b * vx;
                double pz = package.Z + a * uz + b * vz;
                foreach (DropBody other in layer)
                {
                    if (Contains(other, px, pz))
                    {
                        inside++;
                        break;
                    }
                }
            }
        }
        return inside / (double)(SupportSamples * SupportSamples);
    }

    private static bool Contains(DropBody body, double px, double pz)
    {
        (double ux, double uz) = body.AxisU;
        (double vx, double vz) = body.AxisV;
        double dx = px - body.X;
        double dz = pz - body.Z;
        return Math.Abs(dx * ux + dz * uz) <= body.HalfX && Math.Abs(dx * vx + dz * vz) <= body.HalfZ;
    }

    private static DropBody Widest(DropBody package, List<DropBody> layer)
    {
        DropBody widest = layer[0];
        double best = -1;
        foreach (DropBody other in layer)
        {
            double share = Support(package, new[] { other });
            if (share > best)
            {
                best = share;
                widest = other;
            }
        }
        return widest;
    }

    // The shortest move (separating axes) that clears the package from the other one, plus Gap.
    private static (double dx, double dz) Separation(DropBody package, DropBody other)
    {
        double depth = Penetration(package, other, out double ax, out double az);
        double sign = (package.X - other.X) * ax + (package.Z - other.Z) * az >= 0 ? 1 : -1;
        double distance = depth + Gap;
        return (sign * ax * distance, sign * az * distance);
    }

    // Smallest overlap of the two rectangles over the four separating axes (<= 0: apart) and that axis.
    private static double Penetration(DropBody a, DropBody b, out double axisX, out double axisZ)
    {
        var axes = new[] { a.AxisU, a.AxisV, b.AxisU, b.AxisV };
        double smallest = double.PositiveInfinity;
        axisX = 1;
        axisZ = 0;
        foreach ((double x, double z) axis in axes)
        {
            double overlap = Radius(a, axis) + Radius(b, axis) - Math.Abs((b.X - a.X) * axis.x + (b.Z - a.Z) * axis.z);
            if (overlap < smallest)
            {
                smallest = overlap;
                axisX = axis.x;
                axisZ = axis.z;
            }
        }
        return smallest;
    }

    private static double Radius(DropBody body, (double x, double z) axis)
    {
        (double ux, double uz) = body.AxisU;
        (double vx, double vz) = body.AxisV;
        return body.HalfX * Math.Abs(ux * axis.x + uz * axis.z) + body.HalfZ * Math.Abs(vx * axis.x + vz * axis.z);
    }
}
