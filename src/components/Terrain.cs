using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Terrain : GameComponent
{
    private Game1 game;

    public Terrain(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        var initialPos = new Vector2(0, 500);

        for (int y = 0; y < 10; y++)
        {
            Texture2D sprite = y == 0 ? null : AssetManager.GetTexture("tileTwo");

            for (int x = 0; x < 80; x++)
            {
                AddTile(initialPos + new Vector2(x, y) * Grid.TileLength, sprite);
            }
        }
    }

    private void AddTile(Vector2 position, Texture2D sprite = null)
    {
        sprite ??= AssetManager.GetTexture("tile");
        game.Components.Add(new Entity(game, position, sprite));
    }
}
