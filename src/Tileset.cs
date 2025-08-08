using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Tileset
{
    private Texture2D texture;
    private int tilesetWidth;
    private int tilesetHeight;

    public Tileset(Texture2D texture, int tilesetWidth, int tilesetHeight)
    {
        this.texture = texture;
        this.tilesetWidth = tilesetWidth;
        this.tilesetHeight = tilesetHeight;
    }

    public void DrawTile(SpriteBatch spriteBatch, int tileId, Vector2 position)
    {
        (int yPos, int xPos) = int.DivRem(tileId, tilesetWidth);

        const int tileLength = Grid.TileLength;
        var sourceRect = new Rectangle(xPos * tileLength, yPos * tileLength, tileLength, tileLength);

        spriteBatch.Draw(texture,
                position,
                sourceRectangle: sourceRect,
                Color.White);
    }
}
