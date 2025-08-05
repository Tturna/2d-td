using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2d_td;

public class Game1 : Game
{
    public GraphicsDeviceManager Graphics;
    public SpriteBatch SpriteBatch;

    private Vector2 gridMousePosition;
    private bool canPlaceTurret;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        AssetManager.Initialize(Content);
        // Load here to prevent components from trying to access assets before they're loaded.
        AssetManager.LoadAllAssets();

        var ui = new UIComponent(this);
        Components.Add(ui);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        InputSystem.Update();
        gridMousePosition = Grid.SnapPositionToGrid(InputSystem.GetMousePosition());
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Make a system that doesn't require collision checks against every entity.
        // This could be done by connecting tiles or tile coordinates to entities and checking
        // if the tile under the mouse has a connected entity.
        var isColliding = false;

        foreach (var component in Components)
        {
            if (component is not Entity) continue;

            var entity = (Entity)component;

            if (entity.Position == gridMousePosition)
            {
                isColliding = true;
                break;
            }
        }

        canPlaceTurret = !isColliding;

        if (canPlaceTurret && InputSystem.IsLeftMouseButtonClicked())
        {
            Components.Add(new Entity(this, gridMousePosition, AssetManager.GetTexture("turret")));
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        var turretTexture = AssetManager.GetTexture("turret");
        var turretTwoTexture = AssetManager.GetTexture("turretTwo");
        var texture = canPlaceTurret ? turretTexture : turretTwoTexture;

        // Draw building hologram at a certain depth so stuff like existing buildings
        // can be drawn under it.
        SpriteBatch.Begin(sortMode: SpriteSortMode.BackToFront, depthStencilState: DepthStencilState.Default);
        SpriteBatch.Draw(texture,
                gridMousePosition,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0.1f);
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
