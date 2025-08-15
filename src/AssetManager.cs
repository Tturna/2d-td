using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace _2d_td;

public static class AssetManager
{
    private static ContentManager _content;
    private static Dictionary<string, Texture2D> _textures = new();
    private static Dictionary<string, SpriteFont> _fonts = new();

    public static void Initialize(ContentManager contentManager)
    {
        _content = contentManager;
    }

    public static void LoadTexture(string name, string contentPath = null)
    {
        var path = contentPath is not null ? contentPath : name;
        _textures.Add(name, _content.Load<Texture2D>(path));
    }

    // TODO: Split asset loading once there are different levels and zones.
    // No point in loading zone 1 assets when zone 2 levels are loaded.
    public static void LoadAllAssets()
    {
        // towers
        LoadTexture("gunTurretBase", "sprites/towers/turret_body_00");
        LoadTexture("gunTurretHead", "sprites/towers/turret_gun_00");
        LoadTexture("turretTwo", "sprites/turretTwo"); // temp

        // tiles
        LoadTexture("purptiles", "sprites/tiles/purptiles");

        // enemies
        LoadTexture("goon", "sprites/enemies/goon/goon_walk");

        // environment
        LoadTexture("tree", "sprites/tree"); // temp
        LoadTexture("mountain", "sprites/mountain"); // temp

        // - zone1 bg moving objects
        LoadTexture("cloud_z1_1", "sprites/background-objects/zone1/moving-objects/cloud_z1_1");
        LoadTexture("cloud_z1_2", "sprites/background-objects/zone1/moving-objects/cloud_z1_2");
        LoadTexture("roboship", "sprites/background-objects/zone1/moving-objects/roboship_1");
        // - zone1 bg
        LoadTexture("skyscraper_ruins_1", "sprites/background-objects/zone1/skyscraper_ruins_1");
        LoadTexture("skyscraper_ruins_2", "sprites/background-objects/zone1/skyscraper_ruins_2");

        // - zone1 midground moving objects
        LoadTexture("hovercraft", "sprites/midground-objects/zone1/moving-objects/hovercraft_1");
        // - zone1 mg
        LoadTexture("deadtree_1", "sprites/midground-objects/zone1/deadtree_1");
        LoadTexture("deadtree_2", "sprites/midground-objects/zone1/deadtree_2");
        LoadTexture("ruins_1", "sprites/midground-objects/zone1/ruins_1");

        // - zone1 fg
        LoadTexture("smog_1", "sprites/foreground-objects/smog_1");
        LoadTexture("smog_2", "sprites/foreground-objects/smog_2");

        // ui
        LoadTexture("slot", "sprites/ui/slot"); // temp
    }

    public static Texture2D GetTexture(string name)
    {
        if (_textures.TryGetValue(name, out var texture))
        {
            return texture;
        }
        else
        {
            throw new KeyNotFoundException($"texture '{name}' not found");
        }
    }
}
