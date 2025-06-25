using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TDgame;

// this is a class that contains the components for Entitys
public class Entity
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Texture2D Texture { get; set; }


    public Entity(Vector2 position, int width, int height)
    {
        Position = position;
        Width = width;
        Height = height;
    }

    // monogame methods
    public virtual void Initialize()
    {

    }

    protected virtual void LoadContent()
    {
        ;
    }

    public virtual void Update(GameTime gameTime)
    {

    }

    public virtual void Draw(GameTime gameTime)
    {

    }
}