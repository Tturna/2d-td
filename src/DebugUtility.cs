using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class DebugUtility
{
    public static HashSet<(Vector2, Vector2, Color)> LineSet { get; private set; } = new();

    public static void DrawDebugLine(Vector2 startPoint, Vector2 endPoint, Color color)
    {
        var lineTuple = NormalizeLine(startPoint, endPoint);
        LineSet.Add((lineTuple.Item1, lineTuple.Item2, color));
    }

    public static void ResetLines()
    {
        LineSet.Clear();
    }

    private static (Vector2, Vector2) NormalizeLine(Vector2 a, Vector2 b)
    {
        return a.X < b.X || (a.X == b.X && a.Y < b.Y) ? (a, b) : (b, a);
    }
}
