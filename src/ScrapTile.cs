using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class ScrapTile : Entity
{
    public static int MaxScrapLevel { get; private set; } = 2;
    public static int MaxFreefloatLevel { get; private set; } = 1;
    private Random random = new();

    public int ScrapLevel { get; private set; } = 1;

    public ScrapTile(Game1 game, Vector2 worldGridPosition) :
        base(game, Grid.SnapPositionToGrid(worldGridPosition), GetBaseScrapTileSprite(game.SpriteBatch))
    {
        var yScale = (float)ScrapLevel / MaxScrapLevel;
        Scale = new Vector2(1f, yScale);
        UpdatePosition(Vector2.UnitY * (Grid.TileLength * (1f - yScale)));
    }

    public void AddToPile(int amount = 1)
    {
        if (ScrapLevel >= MaxScrapLevel)
        {
            return;
        }

        if (ScrapLevel + amount > MaxFreefloatLevel && random.Next(0, 100) > 20)
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
                if (random.Next(0, 2) == 0)
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
        UpdatePosition(-Vector2.UnitY * (Grid.TileLength * (1f / MaxScrapLevel)));
    }

    private static Texture2D GetBaseScrapTileSprite(SpriteBatch spriteBatch)
    {
        var texture = TextureUtility.GetBlankTexture(spriteBatch, Grid.TileLength, Grid.TileLength, Color.White);
        return texture;
    }
}
