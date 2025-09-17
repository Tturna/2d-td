using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public static class DebugUtility
{
    public struct Line
    {
        public Vector2 PointA;
        public Vector2 PointB;
    }

    public static Dictionary<Line, Color> lineMap = new();

    public static void DrawDebugLine(Vector2 startPoint, Vector2 endPoint, Color color)
    {
        var line = new Line { PointA = startPoint, PointB = endPoint };
        lineMap.TryAdd(line, color);
    }
}
