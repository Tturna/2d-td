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

    public static void LoadFont(string name, string contentPath = null)
    {
        var path = contentPath is not null ? contentPath : name;
        _fonts.Add(name, _content.Load<SpriteFont>(path));
    }

    // TODO: Split asset loading once there are different levels and zones.
    // No point in loading zone 1 assets when zone 2 levels are loaded.
    public static void LoadAllAssets()
    {
        // towers
        LoadTexture("gunTurretBase", "sprites/Towers/Turret/turret_base/turret_body_base");
        LoadTexture("gunTurretHead", "sprites/Towers/Turret/turret_base/turret_gun_base");
        LoadTexture("gunTurret_botshot_body", "sprites/Towers/Turret/turret_botshot/turret_botshot_body");
        LoadTexture("gunTurret_botshot_gun", "sprites/Towers/Turret/turret_botshot/turret_botshot_gun");
        LoadTexture("gunTurret_doublegun_gun", "sprites/Towers/Turret/turret_doublegun/turret_gun_10");
        LoadTexture("gunTurret_improvedbarrel_gun", "sprites/Towers/Turret/turret_improvedbarrel/turret_improvedbarrel_gun");
        LoadTexture("gunTurret_photoncannon_body", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_body");
        LoadTexture("gunTurret_photoncannon_gun", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_gun");
        LoadTexture("gunTurret_rocketshots_body", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_body");
        LoadTexture("gunTurret_rocketshots_gun", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_gun");

        LoadTexture("gunTurret_botshot_icon", "sprites/Towers/Turret/turret_botshot/turret_botshot_icon");
        LoadTexture("gunTurret_doublegun_icon", "sprites/Towers/Turret/turret_doublegun/turret_doublegun_icon");
        LoadTexture("gunTurret_improvedbarrel_icon", "sprites/Towers/Turret/turret_improvedbarrel/turret_improvedbarrel_icon");
        LoadTexture("gunTurret_photoncannon_icon", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_icon");
        LoadTexture("gunTurret_rocketshots_icon", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_icon");

        LoadTexture("railgun_base_idle", "sprites/Towers/Railgun/railgun_base/railgun_base_idle");
        LoadTexture("railgun_base_fire", "sprites/Towers/Railgun/railgun_base/railgun_base_fire");
        LoadTexture("railgun_antimatterlaser_idle", "sprites/Towers/Railgun/railgun_antimatterlaser/railgun_antiomatterlaser_idle");
        LoadTexture("railgun_antimatterlaser_fire", "sprites/Towers/Railgun/railgun_antimatterlaser/railgun_antiomatterlaser_fire");
        LoadTexture("railgun_cannonball_idle", "sprites/Towers/Railgun/railgun_cannonball/railgun_cannonball_idle");
        LoadTexture("railgun_cannonball_fire", "sprites/Towers/Railgun/railgun_cannonball/railgun_cannonball_fire");
        LoadTexture("railgun_goldengatling_idle", "sprites/Towers/Railgun/railgun_goldengatling/railgun_goldengatling_idle");
        LoadTexture("railgun_goldengatling_fire", "sprites/Towers/Railgun/railgun_goldengatling/railgun_goldengatling_firing");
        LoadTexture("railgun_polishedrounds_idle", "sprites/Towers/Railgun/railgun_polishedrounds/railgun_polishedrounds_idle");
        LoadTexture("railgun_polishedrounds_fire", "sprites/Towers/Railgun/railgun_polishedrounds/railgun_polishedrounds_fire");
        LoadTexture("railgun_tungstenshells_idle", "sprites/Towers/Railgun/railgun_tungstenshells/railgun_tungstenshells_idle");
        LoadTexture("railgun_tungstenshells_fire", "sprites/Towers/Railgun/railgun_tungstenshells/railgun_tungstenshells_fire");

        LoadTexture("railgun_antimatterlaser_icon", "sprites/Towers/Railgun/railgun_antimatterlaser/antimatterlaser_icon");
        LoadTexture("railgun_cannonball_icon", "sprites/Towers/Railgun/railgun_cannonball/railgun_cannonball_icon");
        LoadTexture("railgun_goldengatling_icon", "sprites/Towers/Railgun/railgun_goldengatling/railgun_goldengatling_icon");
        LoadTexture("railgun_polishedrounds_icon", "sprites/Towers/Railgun/railgun_polishedrounds/polishedrounds_icon");
        LoadTexture("railgun_tungstenshells_icon", "sprites/Towers/Railgun/railgun_tungstenshells/tungstenshells_icon");

        LoadTexture("drone_base_idle", "sprites/Towers/Drone/drone_base/drone_base_idle");

        LoadTexture("crane_base_idle", "sprites/Towers/Crane/crane_base/crane_base_idle");
        LoadTexture("crane_base_attack", "sprites/Towers/Crane/crane_base/crane_base_attack");

        LoadTexture("mortar_base_idle", "sprites/Towers/Mortar/mortar_base/mortar_base_idle");
        LoadTexture("mortar_base_fire", "sprites/Towers/Mortar/mortar_base/mortar_base_fire");
        LoadTexture("mortar_bouncingbomb_idle", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncing_bomb_idle");
        LoadTexture("mortar_bouncingbomb_fire", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncing_bomb_fire");
        LoadTexture("mortar_efficientreload_idle", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_idle");
        LoadTexture("mortar_efficientreload_fire", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_fire");
        LoadTexture("mortar_heavyshells_idle", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_idle");
        LoadTexture("mortar_heavyshells_fire", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_fire");
        LoadTexture("mortar_hellrain_idle", "sprites/Towers/Mortar/mortar_hellrain/mortar_hellrain_idle");
        LoadTexture("mortar_hellrain_fire", "sprites/Towers/Mortar/mortar_hellrain/mortar_hellrain_fire");
        LoadTexture("mortar_missilesilo_idle", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_idle");
        LoadTexture("mortar_missilesilo_fire", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_fire");
        LoadTexture("mortar_nuke_idle", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_idle");
        LoadTexture("mortar_nuke_fire", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_fire");

        LoadTexture("mortar_bouncingbomb_icon", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncingbomb_icon");
        LoadTexture("mortar_efficientreload_icon", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_icon");
        LoadTexture("mortar_heavyshells_icon", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_icon");
        // hellrain icon missing?
        LoadTexture("mortar_missilesilo_icon", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_icon");
        LoadTexture("mortar_nuke_icon", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_icon");

        LoadTexture("punchtrap_base", "sprites/Towers/Punch Trap/punchtrap_base/punchtrap_base_idle");

        LoadTexture("turretTwo", "sprites/turretTwo"); // temp

        // projectiles
        LoadTexture("tempprojectile", "sprites/projectiles/railguntemp"); // temp

        // tiles
        LoadTexture("purptiles", "sprites/tiles/purptiles");

        // enemies
        LoadTexture("goon", "sprites/enemies/goon/goon_walk");
        LoadTexture("goon_hit", "sprites/enemies/goon/goon_hurtframe");
        LoadTexture("fridge", "sprites/enemies/fridge/fridge_walk");
        LoadTexture("fridge_hit", "sprites/enemies/fridge/fridge_hurtframe");

        // environment
        LoadTexture("tree", "sprites/tree"); // temp
        LoadTexture("mountain", "sprites/mountain"); // temp
        LoadTexture("big_gradient", "sprites/big_gradient"); // temp

        // - zone1 bg moving objects
        LoadTexture("cloud_z1_1", "sprites/Background Objects/Zone 1/Moving Objects/cloud_z1_1");
        LoadTexture("cloud_z1_2", "sprites/Background Objects/Zone 1/Moving Objects/cloud_z1_2");
        LoadTexture("roboship", "sprites/Background Objects/Zone 1/Moving Objects/roboship_1");
        // - zone1 bg
        LoadTexture("skyscraper_ruins_1", "sprites/Background Objects/Zone 1/skyscraper_ruins_1");
        LoadTexture("skyscraper_ruins_2", "sprites/Background Objects/Zone 1/skyscraper_ruins_2");

        // - zone1 midground moving objects
        LoadTexture("hovercraft", "sprites/Midground Objects/Zone 1/Moving Objects/hovercraft_1");
        // - zone1 mg
        LoadTexture("deadtree_1", "sprites/Midground Objects/Zone 1/deadtree_1");
        LoadTexture("deadtree_2", "sprites/Midground Objects/Zone 1/deadtree_2");
        LoadTexture("ruins_1", "sprites/Midground Objects/Zone 1/ruins_1");

        // - zone1 fg
        LoadTexture("smog_1", "sprites/Foreground Objects/Generic/smog_1");
        LoadTexture("smog_2", "sprites/Foreground Objects/Generic/smog_2");

        // ui
        LoadTexture("btn_square", "sprites/UI/button_square");
        LoadTexture("btn_square_small", "sprites/UI/button_smallsquare");
        LoadTexture("btn_square_empty", "sprites/UI/button_square_empty");
        LoadTexture("btn_info", "sprites/UI/infobutton");
        LoadTexture("btn_pause", "sprites/UI/pausebutton");
        LoadTexture("icon_scrap", "sprites/UI/scrapicon");
        LoadTexture("icon_scrap_small", "sprites/UI/scrapicon_small");
        LoadTexture("upgradebg", "sprites/UI/upgradebg");
        LoadTexture("upgrade_indicator", "sprites/UI/upgradeindicator");
        LoadTexture("main_play_button", "sprites/UI/mainmenu_playbutton");
        LoadTexture("main_quit_button", "sprites/UI/mainmenu_quitbutton");
        LoadTexture("main_settings_button", "sprites/UI/mainmenu_settingsbutton");

        // fonts
        LoadFont("default", "fonts/default_font");
        LoadFont("pixelsix", "fonts/pixelsix/pixelsix_bitmap_test");
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

    public static SpriteFont GetFont(string name)
    {
        if (_fonts.TryGetValue(name, out var font))
        {
            return font;
        }
        else
        {
            throw new KeyNotFoundException($"Font '{name}' not found");
        }
    }
}
