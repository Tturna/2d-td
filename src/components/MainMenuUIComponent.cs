using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class MainMenuUIComponent : DrawableGameComponent
{
    private Game1 game;
    private List<UIEntity> uiEntities = new();
    private SpriteFont defaultFont;
    private Vector2 playButtonStringSize;

    public MainMenuUIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        defaultFont = AssetManager.GetFont("default");
        playButtonStringSize = defaultFont.MeasureString("Play");

        var buttonSprite = AssetManager.GetTexture("btn_square");
        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;

        var playBtnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var playBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: playBtnFrameSize,
            delaySeconds: 0.5f
        );

        var playButtonPos = new Vector2(halfScreenWidth - playBtnFrameSize.X / 2, halfScreenHeight - playBtnFrameSize.Y / 2);
        var playButton = new UIEntity(game, playButtonPos, playBtnAnimationData);

        var exitButtonPos = new Vector2(halfScreenWidth - playBtnFrameSize.X / 2, halfScreenHeight + playBtnFrameSize.Y / 2 + 10);
        var exitButton = new UIEntity(game, exitButtonPos, playBtnAnimationData);

        playButton.ButtonPressed += () => SceneManager.LoadGame();
        exitButton.ButtonPressed += () => game.Exit();

        uiEntities.Add(playButton);
        uiEntities.Add(exitButton);

        game.Components.Add(playButton);
        game.Components.Add(exitButton);

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    new public void Draw(GameTime gameTime)
    {
        foreach (var uiEntity in uiEntities)
        {
            uiEntity.DrawCustom(gameTime);
        }

        game.SpriteBatch.DrawString(defaultFont, "Play", new Vector2(400, 240) - playButtonStringSize / 2, Color.White);
        game.SpriteBatch.DrawString(defaultFont, "Exit", new Vector2(400, 270) - playButtonStringSize / 2, Color.White);

        base.Draw(gameTime);
    }
}
