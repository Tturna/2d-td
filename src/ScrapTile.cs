using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class ScrapTile : Entity
{
    public static int MaxScrapLevel { get; private set; } = 8;
    public static int MaxFreefloatLevel { get; private set; } = 3;
    private Random random = new();

    public int ScrapLevel { get; private set; } = 1;

    public ScrapTile(Game1 game, Vector2 worldGridPosition) : base(game, GetBaseScrapTileSprite(game.SpriteBatch))
    {
        game.Components.Add(this);
        Position = Grid.SnapPositionToGrid(worldGridPosition);

        var yScale = (float)ScrapLevel / MaxScrapLevel;
        Scale = new Vector2(1f, yScale);
        Position += Vector2.UnitY * (Grid.TileLength * (1f - yScale));
    }

    public void AddToPile(int amount = 1)
    {
        if (ScrapLevel < MaxScrapLevel)
        {
            if (ScrapLevel + amount > MaxFreefloatLevel)
            {
                var leftTilePosition = Position - Vector2.UnitX * Grid.TileLength;
                var rightTilePosition = Position + Vector2.UnitX * Grid.TileLength;
                var leftTile = ScrapSystem.GetScrapFromPosition(leftTilePosition);
                var rightTile = ScrapSystem.GetScrapFromPosition(rightTilePosition);
                var leftInTerrain = Collision.IsPointInTerrain(leftTilePosition, Game.Terrain);
                var rightInTerrain = Collision.IsPointInTerrain(rightTilePosition, Game.Terrain);
                var leftAvailable = !leftInTerrain && (leftTile is null || leftTile.ScrapLevel < ScrapLevel + amount - 1);
                var rightAvailable = !rightInTerrain && (rightTile is null || rightTile.ScrapLevel < ScrapLevel + amount - 1);

                if (leftAvailable && rightAvailable)
                {
                    if (random.Next(0, 2) > 0.5f)
                    {
                        ScrapSystem.AddScrap(Game, leftTilePosition);
                        return;
                    }
                    else
                    {
                        ScrapSystem.AddScrap(Game, rightTilePosition);
                        return;
                    }
                }
                else if (leftAvailable && !rightAvailable)
                {
                    ScrapSystem.AddScrap(Game, leftTilePosition);
                    return;
                }
                else if (rightAvailable && !leftAvailable)
                {
                    ScrapSystem.AddScrap(Game, rightTilePosition);
                    return;
                }

                // No available neighboring scrap tiles. Fall through and grow pile.
            }

            ScrapLevel = Math.Min(ScrapLevel + amount, MaxScrapLevel);
            var yScale = (float)ScrapLevel / MaxScrapLevel;
            Scale = new Vector2(1f, yScale);
            Position -= Vector2.UnitY * (Grid.TileLength * (1f / MaxScrapLevel));
        }
        else
        {

        }
    }

    private static Texture2D GetBaseScrapTileSprite(SpriteBatch spriteBatch)
    {
        var texture = new Texture2D(spriteBatch.GraphicsDevice, width: Grid.TileLength, height: Grid.TileLength,
        mipmap: false, SurfaceFormat.Color);

        var colorData = new Color[Grid.TileLength * Grid.TileLength];

        for (var i = 0; i < colorData.Length; i++)
        {
            colorData[i] = Color.White;
        }

        texture.SetData(colorData);

        return texture;
    }
}
