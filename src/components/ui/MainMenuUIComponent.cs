using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2d_td;

public class MainMenuUIComponent : DrawableGameComponent
{
    private Game1 game;
    private List<UIEntity> uiEntities = new();

    public MainMenuUIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        var defaultFont = AssetManager.GetFont("default");
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
        var playButton = new UIEntity(game, uiEntities, playButtonPos, playBtnAnimationData);

        var exitButtonPos = new Vector2(halfScreenWidth - playBtnFrameSize.X / 2, halfScreenHeight + playBtnFrameSize.Y / 2 + 10);
        var exitButton = new UIEntity(game, uiEntities, exitButtonPos, playBtnAnimationData);

        playButton.ButtonPressed += () => SceneManager.LoadGame();
        exitButton.ButtonPressed += () => game.Exit();

        var playButtonText = new UIEntity(game, uiEntities, defaultFont, "Play");
        var exitButtonText = new UIEntity(game, uiEntities, defaultFont, "Exit");
        playButtonText.Position = playButtonPos + playButton.Size / 2 - playButtonText.Size / 2;
        exitButtonText.Position = exitButtonPos + exitButton.Size / 2 - exitButtonText.Size / 2;

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

        base.Draw(gameTime);
    }
}
