using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class MainMenuUIComponent : DrawableGameComponent
{
    private Game1 game;
    private List<UIEntity> uiEntities = new();
    private SpriteFont pixelsixFont = AssetManager.GetFont("pixelsix");
    private Texture2D buttonSprite = AssetManager.GetTexture("btn_square");
    private Texture2D playButtonSprite = AssetManager.GetTexture("main_play_button");
    private Texture2D quitButtonSprite = AssetManager.GetTexture("main_quit_button");
    private Texture2D settingsButtonSprite = AssetManager.GetTexture("main_settings_button");
    private Stack<Action> menuScreenStack = new();

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
        if (InputSystem.IsKeyTapped(Keys.Escape))
        {
            if (menuScreenStack.Count == 0)
            {
                LoadMainMenu();
            }
            else
            {
                var loadPreviousScreen = menuScreenStack.Pop();
                loadPreviousScreen();
            }
        }

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
        var playBtnFrameSize = new Vector2(playButtonSprite.Width, playButtonSprite.Height);
        var quitBtnFrameSize = new Vector2(quitButtonSprite.Width, quitButtonSprite.Height);
        var settingsBtnFrameSize = new Vector2(settingsButtonSprite.Width, settingsButtonSprite.Height);

        var playBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: playButtonSprite,
            frameCount: 1,
            frameSize: playBtnFrameSize,
            delaySeconds: 0
        );

        var quitBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: quitButtonSprite,
            frameCount: 1,
            frameSize: quitBtnFrameSize,
            delaySeconds: 0
        );

        var settingsBtnAnimationData = new AnimationSystem.AnimationData
        (
            texture: settingsButtonSprite,
            frameCount: 1,
            frameSize: settingsBtnFrameSize,
            delaySeconds: 0
        );

        const int Margin = 16;
        const int Gap = 4;
        var playButtonPos = new Vector2(Margin, halfScreenHeight);
        var playButton = new UIEntity(game, uiEntities, playButtonPos, playBtnAnimationData);

        var settingsButtonPos = new Vector2(Margin, halfScreenHeight + playButtonSprite.Height + Gap);
        var settingsButton = new UIEntity(game, uiEntities, settingsButtonPos, settingsBtnAnimationData);

        var exitButtonPos = new Vector2(Margin, halfScreenHeight + playButtonSprite.Height + settingsButtonSprite.Height + Gap * 2);
        var exitButton = new UIEntity(game, uiEntities, exitButtonPos, quitBtnAnimationData);

        playButton.ButtonPressed += () => LoadZoneSelector();
        exitButton.ButtonPressed += () => game.Exit();
    }

    private void LoadZoneSelector()
    {
        ClearUI();

        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var btnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var titleText = "Select Zone";
        var title = new UIEntity(game, uiEntities, pixelsixFont, titleText);
        title.Scale = Vector2.One * 2;
        var titleOffset = -pixelsixFont.MeasureString(titleText).X * title.Scale.X / 2;
        title.SetPosition(new Vector2(halfScreenWidth + titleOffset, halfScreenHeight - 40));
            
        var btnAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: btnFrameSize,
            delaySeconds: 0.5f
        );

        var zones = 3;
        var btnMargin = 20;
        var selectorWidth = btnFrameSize.X * zones + btnMargin * (zones - 1);

        for (int i = 0; i < zones; i++)
        {
            var offset = btnFrameSize.X * i + btnMargin * i;
            var pos = new Vector2(halfScreenWidth - selectorWidth / 2 + offset, halfScreenHeight);
            var btn = new UIEntity(game, uiEntities, pos, btnAnimationData);
            var text = $"{i + 1}";

            if (ProgressionManager.IsZoneUnlocked(i + 1))
            {
                // Create new variable so that the lambda function doesn't create a closure that
                // captures the "i" iteration variable.
                var zoneNumber = i + 1;
                btn.ButtonPressed += () =>
                {
                    menuScreenStack.Push(LoadZoneSelector);
                    LoadLevelSelector(zoneNumber);
                };
            }
            else
            {
                text += " (x)";
            }

            var zoneText = new UIEntity(game, uiEntities, pixelsixFont, text);
            zoneText.Scale = Vector2.One * 2;
            var textXOffset = -pixelsixFont.MeasureString(text).X * zoneText.Scale.X / 2;
            zoneText.SetPosition(btn.Position + new Vector2(btnFrameSize.X / 2 + textXOffset, btnFrameSize.Y + 4));
        }
    }

    private void LoadLevelSelector(int selectedZone)
    {
        ClearUI();

        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var btnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var titleText = "Select Level";
        var title = new UIEntity(game, uiEntities, pixelsixFont, titleText);
        title.Scale = Vector2.One * 2;
        var titleOffset = -pixelsixFont.MeasureString(titleText).X * title.Scale.X / 2;
        title.SetPosition(new Vector2(halfScreenWidth + titleOffset, halfScreenHeight - 40));

        var btnAnimationData = new AnimationSystem.AnimationData
            (
             texture: buttonSprite,
             frameCount: 2,
             frameSize: btnFrameSize,
             delaySeconds: 0.5f
            );

        var levels = 5;
        var btnMargin = 20;
        var selectorWidth = btnFrameSize.X * levels + btnMargin * (levels - 1);

        for (int i = 0; i < levels; i++)
        {
            var offset = btnFrameSize.X * i + btnMargin * i;
            var pos = new Vector2(halfScreenWidth - selectorWidth / 2 + offset, halfScreenHeight);
            var btn = new UIEntity(game, uiEntities, pos, btnAnimationData);
            var text = $"{i + 1}";

            if (ProgressionManager.IsLevelUnlocked(selectedZone, i + 1))
            {
                var levelNumber = i + 1;
                btn.ButtonPressed += () => LoadLevel(selectedZone, levelNumber);
            }
            else
            {
                text += " (x)";
            }

            var zoneText = new UIEntity(game, uiEntities, pixelsixFont, text);
            zoneText.Scale = Vector2.One * 2;
            var textXOffset = -pixelsixFont.MeasureString(text).X * zoneText.Scale.X / 2;
            zoneText.SetPosition(btn.Position + new Vector2(btnFrameSize.X / 2 + textXOffset, btnFrameSize.Y + 4));
        }
    }

    private void LoadLevel(int zone, int level)
    {
        game.SetCurrentZoneAndLevel(zone, level);
        SceneManager.LoadGame();
    }
}
