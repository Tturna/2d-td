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
    private static SpriteFont? pixelsixFont;
    private static FpsUtility? fpsUtility;
    private static List<UIEntity>? debugButtons;

    public static void Update(Game1 game, GameTime gameTime)
    {
        if (InputSystem.IsKeyTapped(Keys.F1))
        {
            debugEnabled = !debugEnabled;
            Console.WriteLine($"Debug mode: {debugEnabled}");

            if (debugEnabled)
            {
                ShowDebugButtons(game);
            }
            else
            {
                HideDebugButtons();
            }
        }

        if (!debugEnabled) return;

        if (fpsUtility is null) fpsUtility = new FpsUtility();

        fpsUtility.Update(gameTime);

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

        if (pixelsixFont is null)
        {
            pixelsixFont = AssetManager.GetFont("pixelsix");
        }

        var statusText = "Debug mode";
        var statusTextWidth = pixelsixFont.MeasureString(statusText) * 2;
        var corner = new Vector2(Game1.Instance.NativeScreenWidth, 20);
        var statusPos = corner - Vector2.UnitX * (statusTextWidth.X + 10);
        spriteBatch.DrawString(pixelsixFont,
            statusText,
            statusPos,
            Color.White,
            rotation: 0,
            origin: default,
            scale: Vector2.One * 2,
            effects: SpriteEffects.None,
            layerDepth: default);

        if (fpsUtility is not null)
        {
            fpsUtility.DrawFps(spriteBatch, statusPos + new Vector2(-32, 20), Color.White);
        }
    }

    public static void ResetLines()
    {
        LineSet.Clear();
    }

    private static (Vector2, Vector2) NormalizeLine(Vector2 a, Vector2 b)
    {
        return a.X < b.X || (a.X == b.X && a.Y < b.Y) ? (a, b) : (b, a);
    }

    private static void ShowDebugButtons(Game1 game)
    {
        if (pixelsixFont is null)
        {
            pixelsixFont = AssetManager.GetFont("pixelsix");
        }

        if (debugButtons is null)
        {
            debugButtons = new();
        }

        var buttonSprite = AssetManager.GetTexture("btn_square_empty");
        var vsyncButtonPos = new Vector2(game.NativeScreenWidth - buttonSprite.Width - 8,
            100);
        var vsyncButton = new UIEntity(game, vsyncButtonPos, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, buttonSprite);

        var vsyncText = "Toggle V-Sync";
        var vsyncButtonText = new UIEntity(game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, pixelsixFont, vsyncText);
        var vsyncButtonTextWidth = pixelsixFont.MeasureString(vsyncText).X;
        vsyncButtonText.Position = vsyncButtonPos + new Vector2(-vsyncButtonTextWidth - 10, 4);

        var vsyncStatusText = new UIEntity(game, UIComponent.Instance.AddUIEntity,
            UIComponent.Instance.RemoveUIEntity, pixelsixFont,
            game.Graphics.SynchronizeWithVerticalRetrace ? "On" : "Off");
        vsyncStatusText.Position = vsyncButtonPos + new Vector2(buttonSprite.Width, buttonSprite.Height) / 2
            - vsyncStatusText.Size / 2;

        vsyncButton.ButtonPressed += () =>
        {
            game.Graphics.SynchronizeWithVerticalRetrace = !game.Graphics.SynchronizeWithVerticalRetrace;
            game.Graphics.ApplyChanges();
            vsyncStatusText.Text = game.Graphics.SynchronizeWithVerticalRetrace ? "On" : "Off";
        };

        debugButtons.Add(vsyncButton);
        debugButtons.Add(vsyncButtonText);
    }

    private static void HideDebugButtons()
    {
        if (debugButtons is null) return;

        foreach (var button in debugButtons)
        {
            button.Destroy();
        }

        debugButtons.Clear();
    }
}
