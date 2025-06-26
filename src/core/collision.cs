using System;
using System.Collections.Generic;
namespace TDgame;

public static class Collision
{
    public static bool AABBCheck(Entity ent1, Entity ent2)
    {
        var x1 = ent1.Position.X;
        var y1 = ent1.Position.Y;
        var w1 = ent1.Width;
        var h1 = ent1.Height;
        var x2 = ent2.Position.X;
        var y2 = ent2.Position.Y;
        var w2 = ent2.Width;
        var h2 = ent2.Height;
        if (x1 < x2 + w2 &&
        x1 + w1 > x2 &&
        y1 < y2 + h2 &&
        y1 + h1 > y2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
