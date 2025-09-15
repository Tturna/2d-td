using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class ScrapTile : Entity
{
    private const int MaxScrapLevel = 8;
    private int scrapLevel = 1;

    public ScrapTile(Game1 game, Vector2 worldPosition) : base(game, GetBaseScrapTileSprite(game.SpriteBatch))
    {
        game.Components.Add(this);
        Position = worldPosition;

        while (!Collision.IsPointInTerrain(Position, game.Terrain))
        {
            Position += Vector2.UnitY * Grid.TileLength;
        }

        var yScale = (float)scrapLevel / MaxScrapLevel;
        Scale = new Vector2(1f, yScale);
        Position -= Vector2.UnitY * (Grid.TileLength * yScale);
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    /// <summary>
    /// Add given amount (defaults to 1) to the scrap pile. Returns whether or not the pile is full.
    /// </summary>
    public bool AddToPile(int amount = 1)
    {
        if (scrapLevel < MaxScrapLevel)
        {
            scrapLevel++;
            var yScale = (float)scrapLevel / MaxScrapLevel;
            Scale = new Vector2(1f, yScale);
            Position -= Vector2.UnitY * (Grid.TileLength * (1f / MaxScrapLevel));

            return false;
        }
        else
        {
            return true;
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
