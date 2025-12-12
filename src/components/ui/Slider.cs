using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Slider : UIEntity
{
    private Game1 game;
    private Vector2 startPoint;
    private Vector2 endPoint;
    private Texture2D pixelSprite;
    private bool isSliding;
    private int width = 80;

    public float Value { get; private set; }

    public delegate void OnValueChangedHandler(float newValue);
    public event OnValueChangedHandler OnValueChanged;

    public Slider(Game1 game, List<UIEntity> uiEntities, Vector2 position) : base(game, uiEntities, GetSliderSprite(game))
    {
        this.game = game;
        SetPosition(position);
        DrawLayerDepth = 0.9f;
        startPoint = position;
        endPoint = startPoint + Vector2.UnitX * width;
        pixelSprite = TextureUtility.GetBlankTexture(game.SpriteBatch, 1, 1, Color.White);
    }

    public override void Update(GameTime gameTime)
    {
        if (InputSystem.IsLeftMouseButtonDown())
        {
            var mouseScreenPos = InputSystem.GetMouseScreenPosition();

            if (isSliding || Collision.IsPointInEntity(mouseScreenPos, this))
            {
                isSliding = true;
                var x = MathHelper.Clamp(mouseScreenPos.X, startPoint.X, endPoint.X);
                SetPosition(new Vector2(x, startPoint.Y));
                Value = (x - startPoint.X) / (endPoint.X - startPoint.X); // inverse lerp
                OnValueChanged?.Invoke(Value);
            }
        }
        else
        {
            isSliding = false;
        }

        base.Update(gameTime);
    }

    public override void DrawCustom(GameTime gameTime)
    {
        game.SpriteBatch.Draw(pixelSprite,
            position: startPoint + new Vector2(0, 3),
            sourceRectangle: null,
            scale: new Vector2(width + 8, 2),
            origin: Vector2.Zero,
            color: Color.Black,
            rotation: 0f,
            layerDepth: 0.95f,
            effects: SpriteEffects.None);

        base.DrawCustom(gameTime);
    }

    private static Texture2D GetSliderSprite(Game1 game)
    {
        return TextureUtility.GetBlankTexture(game.SpriteBatch, 8, 8, Color.White);
    }

    public void SetValue(float value)
    {
        value = MathHelper.Clamp(value, 0f, 1f);
        Value = value;
        SetPosition(Vector2.Lerp(startPoint, endPoint, Value));
        OnValueChanged?.Invoke(Value);
    }
}
