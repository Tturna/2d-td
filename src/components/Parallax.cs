using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public class Parallax : DrawableGameComponent
{
    Game1 game;
    // layer 0 - farthest from the screen
    // layer 1 - closest to the screen
    private List<ParallaxObject> _objects = new List<ParallaxObject>();

    public Parallax(Game game) : base(game)
    {
        this.game = (Game1)game;
    }

    public override void Initialize()
    {
        var minX = -1000;
        var maxX = 1000;
        var minY = -20;
        var maxY = 20;
        var rnd = new Random();
        var bigObjects = new List<string> {"skyscraper_ruins_1", "skyscraper_ruins_2"};
        for (var i = 1; i < 30; i++)
        {
            int index = rnd.Next(bigObjects.Count);
            var obj1 = new ParallaxObject(
                new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY)),
                0.1f,
                bigObjects[index], Vector2.Zero);
            _objects.Add(obj1);
        }
        var bigSkyObjects = new List<string> {"cloud_z1_1", "cloud_z1_2", "roboship"};
        for (var i = 1; i < 10; i++)
        {
            int index = rnd.Next(bigSkyObjects.Count);
            var obj1 = new ParallaxObject(
                new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY) * 2 - 100),
                0.1f,
                bigSkyObjects[index], Vector2.UnitX);
            _objects.Add(obj1);
        }
        var midObjects = new List<string> {"deadtree_1", "deadtree_2", "ruins_1"};
        for (var i = 1; i < 30; i++)
        {
            int index = rnd.Next(midObjects.Count);
            var obj1 = new ParallaxObject(
                new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY)+40),
                0.7f,
                midObjects[index], Vector2.Zero);
            _objects.Add(obj1);
        }
        var foreground = new List<string> {"smog_1", "smog_2"};
        for (var i = 1; i < 60; i++)
        {
            int index = rnd.Next(foreground.Count);
            var obj1 = new ParallaxObject(
                new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY) + 40),
                0.8f,
                foreground[index], Vector2.UnitX);
            _objects.Add(obj1);
        }

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        foreach (var obj in _objects)
        {
            if (obj.Movement != Vector2.Zero)
            {
                var speed = 10;
                var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                obj.Position = new Vector2(
                    obj.Position.X + obj.Movement.X * dt * speed * obj.ParallaxLayer,
                    obj.Position.Y + obj.Movement.Y * dt * speed * obj.ParallaxLayer);
            }
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        foreach (var obj in _objects) {
            var cx = obj.Position.X - Camera.Position.X;
            var cy = (obj.Position.Y+400) - Camera.Position.Y;
            var position = new Vector2(
                obj.Position.X + cx * obj.ParallaxLayer + Camera.Position.X,
                obj.Position.Y + cy * obj.ParallaxLayer + Camera.Position.Y);
            game.SpriteBatch.Draw(obj.Sprite,
                position,
                sourceRectangle: null,
                Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0.99f-obj.ParallaxLayer/100f);
        }

        base.Draw(gameTime);
    }
}
