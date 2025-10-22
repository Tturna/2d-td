using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

#nullable enable
public static class DebugUtility
{
    public static HashSet<(Vector2, Vector2, Color)> LineSet { get; private set; } = new();

    private static bool debugEnabled;
    private static SpriteFont? defaultFont;

    public static void Update(Game1 game)
    {
        if (InputSystem.IsKeyTapped(Keys.F1))
        {
            debugEnabled = !debugEnabled;
            Console.WriteLine($"Debug mode: {debugEnabled}");
        }

        if (!debugEnabled) return;

        if (InputSystem.IsKeyTapped(Keys.E))
        {
            EnemySystem.SpawnWalkerEnemy(game, InputSystem.GetMouseWorldPosition());
        }

        if (InputSystem.IsKeyTapped(Keys.R))
        {
            ScrapSystem.AddScrap(game, InputSystem.GetMouseWorldPosition());
        }

        if (InputSystem.IsKeyTapped(Keys.Q))
        {
            var enemies = EnemySystem.Enemies.ToArray();

            foreach (var enemy in enemies)
            {
                enemy.Destroy();
            }

            EnemySystem.Enemies.Clear();
        }

        if (InputSystem.IsKeyTapped(Keys.T))
        {
            CurrencyManager.AddBalance(10);
        }
    }

    public static void DrawDebugLine(Vector2 startPoint, Vector2 endPoint, Color color)
    {
        var lineTuple = NormalizeLine(startPoint, endPoint);
        LineSet.Add((lineTuple.Item1, lineTuple.Item2, color));
    }

    public static void DrawDebugScreen(SpriteBatch spriteBatch)
    {
        if (!debugEnabled) return;

        if (defaultFont is null)
        {
            defaultFont = AssetManager.GetFont("default");
        }

        var statusText = "Debug mode";
        var statusTextWidth = defaultFont.MeasureString(statusText);
        var corner = new Vector2(Game1.Instance.NativeScreenWidth, 0);
        var statusPos = corner - Vector2.UnitX * (statusTextWidth.X + 10);
        spriteBatch.DrawString(defaultFont, statusText, statusPos, Color.White);
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
