using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class BinGrid<T> where T : Entity
{
    public enum LineDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private int cellSideLength;
    private Dictionary<Point, List<T>> bins = new();

    public int TotalValueCount { get; private set; }

    public BinGrid(int cellSideLength)
    {
        this.cellSideLength = cellSideLength;
    }

    /// <summary>
    /// Add value to the bin grid
    /// </summary>
    public void Add(T value)
    {
        var gridPosition = WorldToGridPosition(value.Position);

        if (bins.TryGetValue(gridPosition, out var bin))
        {
            if (bin is null) bin = new();
            bin.Add(value);
        }
        else
        {
            bins.Add(gridPosition, new());
            bins[gridPosition].Add(value);
        }

        TotalValueCount++;
        return;
    }

    /// <summary>
    /// Attempt to remove a value from the bin grid. Returns true when value was removed and
    /// false if removal failed.
    /// </summary>
    public bool Remove(T value)
    {
        var gridPosition = WorldToGridPosition(value.Position);

        if (!bins.ContainsKey(gridPosition)) return false;

        var bin = bins[gridPosition];

        if (bin is null) return false;

        var canRemove = bin.Remove(value);

        if (canRemove)
        {
            TotalValueCount--;
        }

        return canRemove;
    }

    public List<T> GetBinAndNeighborValues(Vector2 worldPosition)
    {
        var gridPosition = WorldToGridPosition(worldPosition);

        var total = new List<T>();

        for (var i = -1; i <= 1; i++)
        {
            for (var j = -1; j <= 1; j++)
            {
                var cellPosition = gridPosition + new Point(i, j);

                if (!bins.ContainsKey(cellPosition)) continue;
                if (bins[cellPosition] is null) continue;

                total.AddRange(bins[cellPosition]);
            }
        }

        return total;
    }

    public List<T> GetValuesFromBinsInRange(Vector2 worldPosition, float range)
    {
        var gridPosition = WorldToGridPosition(worldPosition);
        var cellRadius = (int)MathF.Ceiling(range / cellSideLength);
        var total = new List<T>();

        for (var i = -cellRadius; i <= cellRadius; i++)
        {
            for (var j = -cellRadius; j <= cellRadius; j++)
            {
                var cellPosition = gridPosition + new Point(i, j);

                if (!bins.ContainsKey(cellPosition)) continue;
                if (bins[cellPosition] is null) continue;

                var cellWorldPosition = GridToWorldPosition(cellPosition);
                if (!Collision.AreRectAndCircleColliding(cellWorldPosition, Vector2.One * cellSideLength,
                    worldPosition, range)) continue;
                        
                total.AddRange(bins[cellPosition]);
            }
        }

        return total;
    }

    public List<T> GetValuesInBinLine(Vector2 worldPosition, LineDirection direction, int lineWidthAdditionInCells = 1, int maxCheckedBinDistance = 10)
    {
        var gridPosition = WorldToGridPosition(worldPosition);
        var lineDirection = direction switch
        {
            LineDirection.Up => new Point(0, -1),
            LineDirection.Down => new Point(0, 1),
            LineDirection.Left => new Point(-1, 0),
            LineDirection.Right => new Point(1, 0),
            _ => Point.Zero
        };

        var perpendicularDirection = new Point(-lineDirection.Y, lineDirection.X);
        var total = new List<T>();
        
        for (int i = 0; i <= maxCheckedBinDistance; i++)
        {
            for (int j = -lineWidthAdditionInCells - 1; j <= lineWidthAdditionInCells; j++)
            {
                var lengthWiseAddition = new Point(lineDirection.X * i, lineDirection.Y * i);
                var widthWiseAddition = new Point(perpendicularDirection.X * j, perpendicularDirection.Y * j);
                var cellPosition = gridPosition + lengthWiseAddition + widthWiseAddition;

                if (!bins.ContainsKey(cellPosition)) continue;
                if (bins[cellPosition] is null) continue;

                total.AddRange(bins[cellPosition]);
            }
        }

        return total;
    }

    // ChatGPT vibe coded
    public T? GetClosestValue(Vector2 worldPosition, int maxSearchRadiusInCells = 10)
    {
        if (TotalValueCount == 0)
            return null;

        var gridPosition = WorldToGridPosition(worldPosition);
        T? closest = null;
        float closestDistSq = float.MaxValue;

        // Search outward in "rings" of bins
        for (int radius = 0; radius <= maxSearchRadiusInCells; radius++)
        {
            bool foundAnyInThisRing = false;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    // Skip inner rings (only edges of current radius)
                    if (Math.Abs(i) != radius && Math.Abs(j) != radius)
                        continue;

                    var cellPosition = gridPosition + new Point(i, j);

                    if (!bins.TryGetValue(cellPosition, out var bin) || bin == null)
                        continue;

                    foundAnyInThisRing = true;

                    foreach (var value in bin)
                    {
                        float distSq = Vector2.DistanceSquared(worldPosition, value.Position);

                        if (distSq < closestDistSq)
                        {
                            closestDistSq = distSq;
                            closest = value;
                        }
                    }
                }
            }

            if (foundAnyInThisRing && closest != null)
                break;
        }

        return closest;
    }

    public Point WorldToGridPosition(Vector2 worldPosition)
    {
        return new Point((int)MathF.Floor(worldPosition.X / cellSideLength),
                (int)MathF.Floor(worldPosition.Y / cellSideLength));
    }

    public Vector2 GridToWorldPosition(Point gridPosition)
    {
        return new Vector2(gridPosition.X, gridPosition.Y) * cellSideLength;
    }

    public void Destroy()
    {
        foreach (var item in bins)
        {
            var values = item.Value.ToArray();
            foreach (var value in values)
            {
                value.Destroy();
            }

            item.Value.Clear();
        }

        bins = new();
    }
}
