using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TDgame;

// this is a class that contains the components for Entitys
public class Entity : DrawableGameComponent
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Texture2D Texture { get; set; }

    IHealthManager healthManager;

    public static event EventHandler Damaged;

    public Entity(Game game, Vector2 position, int width, int height, IHealthManager healthManager) : base(game)
    {
        Position = position;
        Width = width;
        Height = height;

        this.healthManager = healthManager;
    }



    public void takeDamage(int amount)
    {
        healthManager.ChangeHealth(-amount);
        OnDamaged(EventArgs.Empty);
    }

    public void OnDamaged(EventArgs e)
    {
        Console.Write("Damaged!");
    }

    // monogame methods
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {

        base.Draw(gameTime);
    }
}