using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TDgame;

public static class AssetManager
{
    private static ContentManager _content;
    private static Dictionary<string, Texture2D> _textures;
    private static Dictionary<string, SpriteFont> _fonts;

    // CALL FIRST!
    public static void Initialize(ContentManager contentManager)
    {
        _content = contentManager;
        _textures = new Dictionary<string, Texture2D>();
        _fonts = new Dictionary<string, SpriteFont>();
    }

    // call initialize before doing these!
    public static void LoadTexture(string name)
    {
        _textures.Add(name, _content.Load<Texture2D>(name));
    }

    public static void LoadAllAssets()
    {
        LoadTexture("empty_box");
    }

    public static Texture2D GetTexture(string name)
    {
        if (_textures[name] == null)
        {
            throw new KeyNotFoundException($"texture '{name}' not found");
        }
        return _textures[name];
    }
}