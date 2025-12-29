using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

#nullable enable
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
    private SettingsScreen? settingsScreen;

    private Texture2D[] levelSelectorBackgrounds =
    {
        AssetManager.GetTexture("zone1levelselector_bg"),
        AssetManager.GetTexture("zone2levelselector_bg"),
        AssetManager.GetTexture("zone3levelselector_bg"),
        AssetManager.GetTexture("zone4levelselector_bg")
    };

    private Vector2[][] levelSelectionButtonPositions =
    {
        // Zone 1
        new Vector2[]
        {
            new Vector2(164, 115),
            new Vector2(228, 126),
            new Vector2(332, 90),
            new Vector2(270, 210),
            new Vector2(410, 200)
        },

        // Zone 2
        new Vector2[]
        {
            new Vector2(300, 75),
            new Vector2(230, 115),
            new Vector2(290, 165),
            new Vector2(210, 230),
            new Vector2(390, 180)
        },
        // Zone 3
        new Vector2[]
        {
            new Vector2(164, 100),
            new Vector2(260, 115),
            new Vector2(360, 120),
            new Vector2(250, 205),
            new Vector2(370, 210)
        },
        // Zone 4
        new Vector2[]
        {
            new Vector2(320, 75),
            new Vector2(400, 145),
            new Vector2(305, 200),
            new Vector2(220, 250),
            new Vector2(365, 280)
        }
    };

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
            SoundSystem.PlaySound("menuClick");

            if (menuScreenStack.Count == 0)
            {
                if (settingsScreen is not null)
                {
                    settingsScreen.Destroy();
                }

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
        settingsButton.ButtonPressed += () => settingsScreen = new SettingsScreen(game, uiEntities);
    }

    private void LoadZoneSelector()
    {
        SoundSystem.PlaySound("menuClick");
        ClearUI();

        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var btnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var bg = new UIEntity(game, uiEntities, AssetManager.GetTexture("zoneselector_bg"));

        var titleText = "Select Zone";
        var title = new UIEntity(game, uiEntities, pixelsixFont, titleText);
        title.Scale = Vector2.One * 2;
        var titleOffset = -pixelsixFont.MeasureString(titleText).X * title.Scale.X / 2;
        title.SetPosition(new Vector2(halfScreenWidth + titleOffset, 20));
            
        var btnAnimationData = new AnimationSystem.AnimationData
        (
            texture: buttonSprite,
            frameCount: 2,
            frameSize: btnFrameSize,
            delaySeconds: 0.5f
        );

        var zones = 4;
        var btnMargin = 20;
        var selectorWidth = btnFrameSize.X * zones + btnMargin * (zones - 1);

        var zoneButtonPositions = new Vector2[]
        {
            new Vector2(280, 80),
            new Vector2(150, 100),
            new Vector2(270, 200),
            new Vector2(390, 250)
        };

        var zoneButtonTextures = new Texture2D[]
        {
            AssetManager.GetTexture("zonebutton1"),
            AssetManager.GetTexture("zonebutton2"),
            AssetManager.GetTexture("zonebutton3"),
            AssetManager.GetTexture("zonebutton4")
        };

        for (int i = 0; i < zones; i++)
        {
            var iconAnimationData = new AnimationSystem.AnimationData (
                texture: zoneButtonTextures[i],
                frameCount: 2,
                frameSize: new Vector2(zoneButtonTextures[i].Width / 2, zoneButtonTextures[i].Height),
                delaySeconds: float.PositiveInfinity);

            var pos = zoneButtonPositions[i];
            var btn = new UIEntity(game, uiEntities, pos, iconAnimationData);
            var text = $"{i + 1}";

            if (ProgressionManager.IsZoneUnlocked(i + 1))
            {
                // Create new variable so that the lambda function doesn't create a closure that
                // captures the "i" iteration variable.
                var zoneNumber = i + 1;
                btn.ButtonPressed += () =>
                {
                    if (menuScreenStack.Count == 0 || menuScreenStack.Peek() != LoadZoneSelector)
                    {
                        menuScreenStack.Push(LoadZoneSelector);
                    }

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

        var backButtonPos = new Vector2(0, game.NativeScreenHeight - btnFrameSize.Y);
        var backButton = new UIEntity(game, uiEntities, AssetManager.GetTexture("btn_back"));
        backButton.SetPosition(backButtonPos);
        backButton.ButtonPressed += () =>
        {
            LoadMainMenu();
            SoundSystem.PlaySound("menuClick");
        };
    }

    private void LoadLevelSelector(int selectedZone)
    {
        SoundSystem.PlaySound("menuClick");
        ClearUI();

        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;
        var btnFrameSize = new Vector2(buttonSprite.Bounds.Width / 2, buttonSprite.Bounds.Height);

        var bg = new UIEntity(game, uiEntities, levelSelectorBackgrounds[selectedZone - 1]);

        var titleText = "Select Level";
        var title = new UIEntity(game, uiEntities, pixelsixFont, titleText);
        title.Scale = Vector2.One * 2;
        var titleOffset = -pixelsixFont.MeasureString(titleText).X * title.Scale.X / 2;
        title.SetPosition(new Vector2(halfScreenWidth + titleOffset, 20));

        var btnAnimationData = new AnimationSystem.AnimationData
        (
         texture: buttonSprite,
         frameCount: 2,
         frameSize: btnFrameSize,
         delaySeconds: 0.5f
        );

        var levels = levelSelectionButtonPositions[selectedZone - 1].Length;
        var btnMargin = 20;
        var selectorWidth = btnFrameSize.X * levels + btnMargin * (levels - 1);

        for (int i = 0; i < levels; i++)
        {
            var isLevelUnlocked = ProgressionManager.IsLevelUnlocked(selectedZone, i + 1);
            string textureName;

            if (!isLevelUnlocked)
            {
                textureName = "levelbutton_inactive";
            }
            else
            {
                if (i + 1 < 5)
                {
                    if (ProgressionManager.IsLevelUnlocked(selectedZone, i + 2))
                    {
                        textureName = "levelbutton_cleared";
                    }
                    else
                    {
                        textureName = "levelbutton_uncleared";
                    }
                }
                else
                {
                    if (selectedZone + 1 >= 4)
                    {
                        textureName = "levelbutton_cleared";
                    }
                    else if (ProgressionManager.IsLevelUnlocked(selectedZone + 1, i + 2))
                    {
                        textureName = "levelbutton_cleared";
                    }
                    else
                    {
                        textureName = "levelbutton_uncleared";
                    }
                }
            }

            var iconTexture = AssetManager.GetTexture(textureName);
            var frameCount = textureName == "levelbutton_inactive" ? 1 : 2;
            var iconAnimationData = new AnimationSystem.AnimationData(
                texture: iconTexture,
                frameCount: frameCount,
                frameSize: new Vector2(iconTexture.Width / frameCount, iconTexture.Height),
                delaySeconds: float.PositiveInfinity);

            var pos = levelSelectionButtonPositions[selectedZone - 1][i];
            var btn = new UIEntity(game, uiEntities, pos, iconAnimationData);
            var text = $"{i + 1}";

            if (isLevelUnlocked)
            {
                var levelNumber = i + 1;
                btn.ButtonPressed += () => LoadLevel(selectedZone, levelNumber);
            }
            // else
            // {
            //     text += " (x)";
            // }

            var zoneText = new UIEntity(game, uiEntities, pixelsixFont, text);
            zoneText.Scale = Vector2.One * 2;
            var textXOffset = -pixelsixFont.MeasureString(text).X * zoneText.Scale.X / 2;
            zoneText.SetPosition(btn.Position + new Vector2(btnFrameSize.X / 2 + textXOffset, btnFrameSize.Y + 4));
        }

        var backButtonPos = new Vector2(0, game.NativeScreenHeight - btnFrameSize.Y);
        var backButton = new UIEntity(game, uiEntities, AssetManager.GetTexture("btn_back"));
        backButton.SetPosition(backButtonPos);
        backButton.ButtonPressed += () =>
        {
            LoadZoneSelector();
            InputSystem.ForceClickableInterrupt();
        };
    }

    private void LoadLevel(int zone, int level)
    {
        SoundSystem.PlaySound("menuClick");
        game.SetCurrentZoneAndLevel(zone, level);
        SceneManager.LoadGame();
    }
}
