using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class MainMenuUIComponent : DrawableGameComponent
{
    private Game1 game;
    private List<UIEntity> uiEntities = new();
    private SpriteFont defaultFont = AssetManager.GetFont("default");
    private Texture2D buttonSprite = AssetManager.GetTexture("btn_square");

    public MainMenuUIComponent(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        LoadMainMenu();

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

    private void ClearUI()
    {
        for (int i = uiEntities.Count - 1; i >= 0; i--)
        {
            var uiEntity = uiEntities[i];
            uiEntity.Destroy();
        }

        uiEntities.Clear();
    }

    private void LoadMainMenu()
    {
        ClearUI();

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

        playButton.ButtonPressed += () => LoadLevelSelector();
        exitButton.ButtonPressed += () => game.Exit();

        var playButtonText = new UIEntity(game, uiEntities, defaultFont, "Play");
        var exitButtonText = new UIEntity(game, uiEntities, defaultFont, "Exit");
        playButtonText.Position = playButtonPos + playButton.Size / 2 - playButtonText.Size / 2;
        exitButtonText.Position = exitButtonPos + exitButton.Size / 2 - exitButtonText.Size / 2;
    }

    private void LoadLevelSelector()
    {
        ClearUI();

        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var btnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var titleText = "Select Zone";
        var title = new UIEntity(game, uiEntities, defaultFont, titleText);
        var titleOffset = -defaultFont.MeasureString(titleText).X / 2;
        title.Position = new Vector2(halfScreenWidth + titleOffset, halfScreenHeight - 40);
            
        var btnAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: btnFrameSize,
            delaySeconds: 0.5f
        );

        var zones = 5;
        var btnMargin = 20;
        var selectorWidth = btnFrameSize.X * zones + btnMargin * (zones - 1);

        for (int i = 0; i < zones; i++)
        {
            var offset = btnFrameSize.X * i + btnMargin * i;
            var pos = new Vector2(halfScreenWidth - selectorWidth / 2 + offset, halfScreenHeight);
            var btn = new UIEntity(game, uiEntities, pos, btnAnimationData);
            var text = $"{i + 1}";
            var zoneText = new UIEntity(game, uiEntities, defaultFont, text);
            var textXOffset = -defaultFont.MeasureString(text).X / 2;
            zoneText.Position = btn.Position + new Vector2(btnFrameSize.X / 2 + textXOffset, btnFrameSize.Y + 4);
        }
    }
}
