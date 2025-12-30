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
    private List<ParallaxObject> _objects = new ();
    public Vector3 Tint = new(1, 1, 1);
    private int currentZone;

    public Parallax(Game game, int currentZone) : base(game)
    {
        this.game = (Game1)game;
        this.currentZone = currentZone;
    }

    public override void Initialize()
    {
        var minX = -1000;
        var maxX = 1000;
        var minY = -20;
        var maxY = 20;
        var rnd = new Random();

        string[] bigSkyObjects = currentZone switch
        {
            1 => [ ],
            2 => [ ],
            3 => [ ],
            4 => [
                "cloud_z1_1",
                "cloud_z1_2",
                "roboship"
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(currentZone), $"Zone {currentZone} is out of range (1 to 4)")
        };

        if (bigSkyObjects.Length > 0)
        {
            for (var i = 1; i < 20; i++)
            {
                int index = rnd.Next(bigSkyObjects.Length);

                var obj1 = new ParallaxObject(
                    pos: new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY) * 2 - 100),
                    layer: rnd.Next(7,18) / 100f,
                    sprite: bigSkyObjects[index],
                    movement: Vector2.UnitX);

                _objects.Add(obj1);
            }
        }

        string[] midObjects = currentZone switch
        {
            1 => [
                "satellite_building",
                "turret_building"
            ],
            2 => [
                "tree1_z2",
                "tree2_z2"
            ],
            3 => [
                "deadtree_1",
                "deadtree_2",
                "ruins_1"
            ],
            4 => [
                "deadtree_1",
                "deadtree_2",
                "ruins_1"
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(currentZone), $"Zone {currentZone} is out of range (1 to 4)")
        };

        for (var i = 1; i < 30; i++)
        {
            int index = rnd.Next(midObjects.Length);

            var obj1 = new ParallaxObject(
                pos: new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY) + 40),
                layer: rnd.Next(65,75) / 100f,
                sprite: midObjects[index],
                movement: Vector2.Zero);

            _objects.Add(obj1);
        }

        string[] midMovingObjects = currentZone switch
        {
            1 => [
                "fighterjet"
            ],
            2 => [
            ],
            3 => [
            ],
            4 => [
                "hovercraft"
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(currentZone), $"Zone {currentZone} is out of range (1 to 4)")
        };

        var movementSpeed = currentZone switch
        {
            1 => 33,
            _ => 1
        };

        var count = currentZone switch
        {
            1 => 3,
            _ => 20
        };

        if (midMovingObjects.Length > 0)
        {
            for (var i = 1; i < count; i++)
            {
                var height = currentZone switch
                {
                    1 => rnd.Next(minY * 3, maxY) + 10,
                    _ => rnd.Next(minY, maxY) + 40
                };

                int index = rnd.Next(midMovingObjects.Length);

                var obj1 = new ParallaxObject(
                    pos: new Vector2(rnd.Next(minX, maxX), height),
                    layer: rnd.Next(75, 85) / 100f,
                    sprite: midMovingObjects[index],
                    movement: Vector2.UnitX * movementSpeed);

                _objects.Add(obj1);
            }
        }

        string[] foreground = currentZone switch
        {
            1 => [
                "smog_1",
                "smog_2"
            ],
            2 => [
                "smog_1",
                "smog_2"
            ],
            3 => [
                "smog_1",
                "smog_2"
            ],
            4 => [
                "smog_1",
                "smog_2"
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(currentZone), $"Zone {currentZone} is out of range (1 to 4)")
        };

        for (var i = 1; i < 60; i++)
        {
            int index = rnd.Next(foreground.Length);

            var obj1 = new ParallaxObject(
                pos: new Vector2(rnd.Next(minX, maxX), rnd.Next(minY, maxY) + 40),
                layer: rnd.Next(85,95)/100f,
                sprite: foreground[index],
                movement: Vector2.UnitX);

            _objects.Add(obj1);
        }

        var bgSpriteName = $"zone{currentZone}_backdrop";
        var bigGradient = new ParallaxObject(
            pos: new Vector2(-game.NativeScreenWidth / 2, -game.NativeScreenHeight / 2),
            layer: 0f,
            sprite: bgSpriteName,
            movement: Vector2.Zero);

        _objects.Add(bigGradient);

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        var speed = 10;
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var obj in _objects)
        {
            if (obj.Movement == Vector2.Zero) continue;

            obj.Position = new Vector2(
                obj.Position.X + obj.Movement.X * dt * speed * obj.ParallaxLayer,
                obj.Position.Y + obj.Movement.Y * dt * speed * obj.ParallaxLayer);

            var fixedPos = obj.Position;

            if (obj.Position.X > 1000)
            {
                fixedPos.X = -1000;
            }
            else if (obj.Position.X < -1000)
            {
                fixedPos.X = 1000;
            }
            if (obj.Position.Y > 1000)
            {
                fixedPos.Y = -1000;
            }
            else if (obj.Position.Y < -1000)
            {
                fixedPos.Y = 1000;
            }

            obj.Position = fixedPos;
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        foreach (var obj in _objects) {
            var color = new Color(new Color(Tint), (int)((obj.ParallaxLayer / 2 + 0.5f) * 255));

            // this is very hacky/hardcoded, just the original spawn location of the camera
            var offset = 400;
            var cx = (obj.Position.X + offset) - Camera.Position.X;
            var cy = (obj.Position.Y + offset) - Camera.Position.Y;

            var position = new Vector2(
                obj.Position.X + cx * obj.ParallaxLayer + Camera.Position.X,
                obj.Position.Y + cy * obj.ParallaxLayer + Camera.Position.Y);

            game.SpriteBatch.Draw(obj.Sprite,
                position,
                sourceRectangle: null,
                color,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: Vector2.One,
                effects: SpriteEffects.None,
                layerDepth: 0.99f - obj.ParallaxLayer / 100f);
        }

        base.Draw(gameTime);
    }
}
