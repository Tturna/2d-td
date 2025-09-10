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
        var buttonSprite = AssetManager.GetTexture("btn_square");
        var halfScreenWidth = game.NativeScreenWidth / 2;
        var halfScreenHeight = game.NativeScreenHeight / 2;

        var halfButtonWidth = buttonSprite.Bounds.Width / 2;
        var halfButtonHeight = buttonSprite.Bounds.Height / 2;

        var playButton = new UIEntity(game, buttonSprite);
        playButton.Position = new Vector2(halfScreenWidth - halfButtonWidth, halfScreenHeight - halfButtonHeight);

        var exitButton = new UIEntity(game, buttonSprite);
        exitButton.Position = new Vector2(halfScreenWidth - halfButtonWidth, halfScreenHeight + halfButtonHeight + 10);

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
            uiEntity.DrawCustom();
        }

        base.Draw(gameTime);
    }
}
