using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class QuadTree<T> where T : Enemy
{
    public enum LineDirection
    {
        Top,
        Right,
        Down,
        Left
    }

    public List<T> Values { get; private set; } = new();

    private QuadTree<T>? parent;
    private QuadTree<T>? topLeftQuad;
    private QuadTree<T>? topRightQuad;
    private QuadTree<T>? botLeftQuad;
    private QuadTree<T>? botRightQuad;
    private Rectangle bounds;
    private int maxValues = 8;
    private int maxDepth = 5;
    private int depth;

    public QuadTree(Rectangle bounds, QuadTree<T>? parent = null, int depth = 0)
    {
        this.bounds = bounds;
        this.parent = parent;
        this.depth = depth;
    }

    private void Split()
    {
        var halfSize = new Vector2(bounds.Width, bounds.Height) / 2;
        var halfWidth = (int)halfSize.X;
        var halfHeight = (int)halfSize.Y;

        var topLeftBounds = new Rectangle(bounds.X, bounds.Y,
            halfWidth, halfHeight);

        var topRightBounds = new Rectangle(bounds.X + halfWidth, bounds.Y,
            halfWidth, halfHeight);

        var botLeftBounds = new Rectangle(bounds.X, bounds.Y + halfHeight,
            halfWidth, halfHeight);

        var botRightBounds = new Rectangle(bounds.X + halfWidth, bounds.Y + halfHeight,
            halfWidth, halfHeight);

        topLeftQuad = new QuadTree<T>(topLeftBounds, this, depth + 1);
        topRightQuad = new QuadTree<T>(topRightBounds, this, depth + 1);
        botLeftQuad = new QuadTree<T>(botLeftBounds, this, depth + 1);
        botRightQuad = new QuadTree<T>(botRightBounds, this, depth + 1);

        // Console.WriteLine($"Splitting tree ({bounds}) into:\n- tl ({topLeftBounds})\n- tr ({topRightBounds})\n- bl ({botLeftBounds})\n- br ({botRightBounds})");
        
        // Subtract number of values because Add() will add them back
        // if (parent is not null)
        // {
        //     Console.WriteLine($"Parent count before split: {parent.TotalValueCount}");
        //     parent.UpdateValueCount(-TotalValueCount);
        //     Console.WriteLine($"Parent count after compensation: {parent.TotalValueCount}");
        // }

        // var oldValue = TotalValueCount;
        // TotalValueCount = 0;

        foreach (var val in Values)
        {
            Add(val);
        }

        // Console.WriteLine($"After split: TotalValueCount = {TotalValueCount}, expected = {oldValue}");

        Values.Clear();
    }

    public bool CanMerge()
    {
        if (topLeftQuad is null) return false;

        return (topLeftQuad.topLeftQuad is null && topLeftQuad.Values.Count == 0 &&
                topRightQuad!.topLeftQuad is null && topRightQuad.Values.Count == 0 &&
                botLeftQuad!.topLeftQuad is null && botLeftQuad.Values.Count == 0 &&
                botRightQuad!.topLeftQuad is null && botRightQuad.Values.Count == 0);
    }

    public void Merge()
    {
        if (!CanMerge()) return;

        topLeftQuad = null;
        topRightQuad = null;
        botLeftQuad = null;
        botRightQuad = null;

        if (parent is not null && parent.CanMerge())
        {
            parent.Merge();
        }
    }

    public bool IsWithinBounds(Vector2 position)
    {
        return (position.X >= bounds.X &&
            position.X < bounds.X + bounds.Width &&
            position.Y >= bounds.Y &&
            position.Y < bounds.Y + bounds.Height);
    }

    public bool TryGetSmallestQuad(Vector2 position, out QuadTree<T> smallestQuad)
    {
        smallestQuad = this;

        if (!IsWithinBounds(position))
        {
            return false;
        }

        // this quad is not split
        if (topLeftQuad is null)
        {
            return true;
        }

        var normalizedPosition = new Vector2(position.X - bounds.X, position.Y - bounds.Y);
        var halfSize = new Vector2(bounds.Width, bounds.Height) / 2;
        var halfWidth = (int)halfSize.X;
        var halfHeight = (int)halfSize.Y;

        if (normalizedPosition.X < halfWidth && normalizedPosition.Y < halfHeight)
        {
            return topLeftQuad.TryGetSmallestQuad(position, out smallestQuad);
        }
        else if (normalizedPosition.X >= halfWidth && normalizedPosition.Y < halfHeight)
        {
            return topRightQuad!.TryGetSmallestQuad(position, out smallestQuad);
        }
        else if (normalizedPosition.X < halfWidth && normalizedPosition.Y >= halfHeight)
        {
            return botLeftQuad!.TryGetSmallestQuad(position, out smallestQuad);
        }
        // else if (normalizedPosition.X >= halfWidth && normalizedPosition.Y >= halfHeight)
        else
        {
            return botRightQuad!.TryGetSmallestQuad(position, out smallestQuad);
        }
    }

    public List<T> GetValuesInOverlappingQuads(Vector2 position, int range)
    {
        if (topLeftQuad is null)
        {
            // Unsplit quad. range doesn't matter
            return Values;
        }

        List<T> overlappingQuadValues = new();
        var topLeftPos = new Vector2(topLeftQuad.bounds.X, topLeftQuad.bounds.Y);
        var topRightPos = new Vector2(topRightQuad!.bounds.X, topRightQuad.bounds.Y);
        var botLeftPos = new Vector2(botLeftQuad!.bounds.X, botLeftQuad.bounds.Y);
        var botRightPos = new Vector2(botRightQuad!.bounds.X, botRightQuad.bounds.Y);
        var quadSize = new Vector2(topLeftQuad.bounds.Width, topLeftQuad.bounds.Height);

        if (Collision.AreRectAndCircleColliding(topLeftPos, quadSize, position, range))
        {
            overlappingQuadValues.AddRange(topLeftQuad.GetValuesInOverlappingQuads(position, range));
        }

        if (Collision.AreRectAndCircleColliding(topRightPos, quadSize, position, range))
        {
            overlappingQuadValues.AddRange(topRightQuad!.GetValuesInOverlappingQuads(position, range));
        }

        if (Collision.AreRectAndCircleColliding(botLeftPos, quadSize, position, range))
        {
            overlappingQuadValues.AddRange(botLeftQuad!.GetValuesInOverlappingQuads(position, range));
        }

        if (Collision.AreRectAndCircleColliding(botRightPos, quadSize, position, range))
        {
            overlappingQuadValues.AddRange(botRightQuad!.GetValuesInOverlappingQuads(position, range));
        }

        return overlappingQuadValues;
    }

    // TODO: Make this work with big enemies whose real position might not be within
    // any horizontal quads, but their bodies might
    public List<T> GetValuesInQuadLine(Vector2 position, LineDirection direction)
    {
        switch (direction)
        {
            case LineDirection.Top:
                if (position.X < bounds.X || position.X > bounds.X + bounds.Width ||
                    position.Y < bounds.Y)
                {
                    return [];
                }
                break;
            case LineDirection.Down:
                if (position.X < bounds.X || position.X > bounds.X + bounds.Width ||
                    position.Y > bounds.Y + bounds.Height)
                {
                    return [];
                }
                break;
            case LineDirection.Left:
                if (position.Y < bounds.Y || position.Y > bounds.Y + bounds.Height ||
                    position.X < bounds.X)
                {
                    return [];
                }
                break;
            case LineDirection.Right:
                if (position.Y < bounds.Y || position.Y > bounds.Y + bounds.Height ||
                    position.X > bounds.X + bounds.Width)
                {
                    return [];
                }
                break;
        }

        if (topLeftQuad is null)
        {
            return Values;
        }

        List<T> totalValues = new();
        totalValues.AddRange(topLeftQuad.GetValuesInQuadLine(position, direction));
        totalValues.AddRange(topRightQuad!.GetValuesInQuadLine(position, direction));
        totalValues.AddRange(botLeftQuad!.GetValuesInQuadLine(position, direction));
        totalValues.AddRange(botRightQuad!.GetValuesInQuadLine(position, direction));

        return totalValues;
    }

    public void Add(T value)
    {
        TryGetSmallestQuad(value.Position, out var smallestQuad);
        // Console.WriteLine($"Putting enemy ({value.Position}) into ({smallestQuad.bounds})");

        if (smallestQuad == this)
        {
            Values.Add(value);
            value.SetContainingQuad(this);

            if (Values.Count > maxValues && depth < maxDepth)
            {
                Split();
                // EnemySystem.EnemyTree.PrintTree();
                return;
            }

            // EnemySystem.EnemyTree.PrintTree();
            return;
        }

        smallestQuad.Add(value);
        return;
    }

    public bool Remove(T value)
    {
        TryGetSmallestQuad(value.Position, out var smallestQuad);
        var canRemove = false;

        if (smallestQuad == this)
        {
            canRemove = Values.Remove(value);

            if (canRemove)
            {
                if (parent is not null)
                {
                    if (parent.CanMerge())
                    {
                        parent.Merge();
                    }
                }

                // EnemySystem.EnemyTree.PrintTree();
            }

            return canRemove;
        }

        canRemove = smallestQuad.Remove(value);

        return canRemove;
    }

    public void Destroy()
    {
        if (topLeftQuad is null)
        {
            foreach (var value in Values)
            {
                value.Destroy();
            }

            Values.Clear();
            return;
        }

        topLeftQuad.Destroy();
        topRightQuad!.Destroy();
        botLeftQuad!.Destroy();
        botRightQuad!.Destroy();
        Merge();
    }

    public bool IsEmpty()
    {
        return topLeftQuad is null && Values.Count == 0;
    }

    public int CountValues()
    {
        if (topLeftQuad is null) return Values.Count;

        var count = topLeftQuad.CountValues();
        count += topRightQuad!.CountValues();
        count += botLeftQuad!.CountValues();
        count += botRightQuad!.CountValues();

        return count;
    }

    public void VisualizeTree()
    {
        var startPoint = new Vector2(bounds.X, bounds.Y);
        var endPoint = new Vector2(bounds.X + bounds.Width, bounds.Y);
        DebugUtility.DrawDebugLine(startPoint, endPoint, Color.Green);

        endPoint = new Vector2(bounds.X, bounds.Y + bounds.Height);
        DebugUtility.DrawDebugLine(startPoint, endPoint, Color.Green);

        startPoint = new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height);
        DebugUtility.DrawDebugLine(startPoint, endPoint, Color.Green);

        endPoint = new Vector2(bounds.X + bounds.Width, bounds.Y);
        DebugUtility.DrawDebugLine(startPoint, endPoint, Color.Green);

        if (topLeftQuad is null) return;

        topLeftQuad.VisualizeTree();
        topRightQuad!.VisualizeTree();
        botLeftQuad!.VisualizeTree();
        botRightQuad!.VisualizeTree();
    }

    public void PrintTree()
    {
        var lines = "";
        while (lines.Length < depth) lines += "-";
        Console.WriteLine($"{lines}Tree ({depth})");

        if (topLeftQuad is null)
        {
            foreach (var val in Values)
            {
                Console.WriteLine($"{lines}{val.ToString()}");
            }

            return;
        }

        topLeftQuad.PrintTree();
        topRightQuad!.PrintTree();
        botLeftQuad!.PrintTree();
        botRightQuad!.PrintTree();
    }
}
