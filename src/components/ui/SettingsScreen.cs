using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class SettingsScreen : UIEntity
{
    private Game1 game;
    private int margin = 10;
    private int textAreaWidth;

    private Slider masterVolumeSlider;
    private Slider sfxVolumeSlider;
    private Slider musicVolumeSlider;
    private UIEntity fullscreenButton;
    private UIEntity backButton;

    public delegate void OnDestroyedHandler();
    public event OnDestroyedHandler OnDestroyed;
    public static event OnDestroyedHandler OnSettingsSaved;

    public SettingsScreen(Game1 game, List<UIEntity> uiEntities) : base(game, uiEntities,
        GetSettingsScreenBackgroundSprite(game))
    {
        this.game = game;
        UpdatePosition(new Vector2(Size.X / 2, 20));
        DrawLayerDepth = 1f;
        textAreaWidth = (int)(Size.X / 3);

        var masterVolumeSliderPosition = Position + new Vector2(textAreaWidth + margin * 2, margin * 3);
        var sfxVolumeSliderPosition = Position + new Vector2(textAreaWidth + margin * 2, margin * 5);
        var musicVolumeSliderPosition = Position + new Vector2(textAreaWidth + margin * 2, margin * 7);
        masterVolumeSlider = new Slider(game, uiEntities, masterVolumeSliderPosition);
        sfxVolumeSlider = new Slider(game, uiEntities, sfxVolumeSliderPosition);
        musicVolumeSlider = new Slider(game, uiEntities, musicVolumeSliderPosition);

        masterVolumeSlider.SetValue(SettingsSystem.MasterVolume);
        sfxVolumeSlider.SetValue(SettingsSystem.RawSoundEffectVolume);
        musicVolumeSlider.SetValue(SettingsSystem.RawMusicVolume);

        masterVolumeSlider.OnValueChanged += (float newValue) => SettingsSystem.MasterVolume = newValue;
        sfxVolumeSlider.OnValueChanged += (float newValue) => SettingsSystem.RawSoundEffectVolume = newValue;
        musicVolumeSlider.OnValueChanged += (float newValue) => SettingsSystem.RawMusicVolume = newValue;

        var fullscreenButtonPosition = Position + new Vector2(textAreaWidth + margin * 2, margin * 9);
        var toggleButtonSprite = AssetManager.GetTexture("settings_toggle_button");
        fullscreenButton = new UIEntity(game, uiEntities, fullscreenButtonPosition, toggleButtonSprite);
        fullscreenButton.ButtonPressed += () =>
        {
            game.Graphics.IsFullScreen = !game.Graphics.IsFullScreen;
            game.Graphics.ApplyChanges();
        };

        var saveButtonSprite = AssetManager.GetTexture("settings_save_button");
        var backButtonPosition = Position + new Vector2(margin, Size.Y - saveButtonSprite.Height - margin);
        backButton = new UIEntity(game, uiEntities, backButtonPosition, saveButtonSprite);
        backButton.ButtonPressed += () =>
        {
            Destroy();
            SoundSystem.PlaySound("menuClick");
        };

        SoundSystem.PlaySound("menuClick");
    }

    public override void Destroy()
    {
        SavingSystem.SaveGame();
        masterVolumeSlider.Destroy();
        sfxVolumeSlider.Destroy();
        musicVolumeSlider.Destroy();
        fullscreenButton.Destroy();
        backButton.Destroy();
        OnDestroyed?.Invoke();
        OnSettingsSaved?.Invoke();

        base.Destroy();
    }

    public override void DrawCustom(GameTime gameTime)
    {
        var settingsTextPos = Position + new Vector2(margin);
        var masterVolumeTextPos = Position + new Vector2(margin, margin * 3);
        var sfxVolumeTextPos = Position + new Vector2(margin, margin * 5);
        var musicVolumeTextPos = Position + new Vector2(margin, margin * 7);
        var fullscreenTogglePos = Position + new Vector2(margin, margin * 9);

        game.SpriteBatch.DrawString(game.DefaultFont, "Settings", settingsTextPos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, "Master volume", masterVolumeTextPos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, "Effects volume", sfxVolumeTextPos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, "Music volume", musicVolumeTextPos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, "Fullscreen", fullscreenTogglePos, Color.White);

        var valueTextOrigin = masterVolumeTextPos + Vector2.UnitX * (textAreaWidth + 80 + margin * 3);
        var masterVolumeValuePos = valueTextOrigin;
        var sfxVolumeValuePos = valueTextOrigin + new Vector2(0, margin * 2);
        var musicVolumeValuePos = valueTextOrigin + new Vector2(0, margin * 4);
        var fullscreenTextPos = valueTextOrigin + new Vector2(0, margin * 6);
        var fullscreenText = game.Graphics.IsFullScreen ? "Enabled" : "Disabled";

        game.SpriteBatch.DrawString(game.DefaultFont, ((int)(masterVolumeSlider.Value * 100)).ToString(),
            masterVolumeValuePos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, ((int)(sfxVolumeSlider.Value * 100)).ToString(),
            sfxVolumeValuePos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, ((int)(musicVolumeSlider.Value * 100)).ToString(),
            musicVolumeValuePos, Color.White);
        game.SpriteBatch.DrawString(game.DefaultFont, fullscreenText,
            fullscreenTextPos, Color.White);

        base.DrawCustom(gameTime);
    }

    private static Texture2D GetSettingsScreenBackgroundSprite(Game1 game)
    {
        var width = game.NativeScreenWidth / 2;
        var height = game.NativeScreenHeight / 5 * 3;
        return TextureUtility.GetBlankTexture(game.SpriteBatch, width, height, Color.Gray);
    }
}
