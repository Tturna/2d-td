using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace _2d_td;

public static class AssetManager
{
    private static ContentManager _content;
    private static Dictionary<string, Texture2D> _textures = new();
    private static Dictionary<string, SpriteFont> _fonts = new();
    private static Dictionary<string, SoundEffect> _soundEffects = new();

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

    public static void LoadSound(string name, string contentPath = null)
    {
        var path = contentPath is not null ? contentPath : name;
        _soundEffects.Add(name, _content.Load<SoundEffect>(path));
    }

    // TODO: Split asset loading once there are different levels and zones.
    // No point in loading zone 1 assets when zone 2 levels are loaded.
    public static void LoadAllAssets()
    {
        // towers
        LoadTexture("gunTurretBase", "sprites/Towers/Turret/turret_base/turret_body_base");
        LoadTexture("gunTurretHead", "sprites/Towers/Turret/turret_base/turret_gun_base");
        LoadTexture("gunTurret_base_bullet", "sprites/Towers/Turret/turret_base/turret_bullet_base");
        LoadTexture("gunTurret_botshot_body", "sprites/Towers/Turret/turret_botshot/turret_botshot_body");
        LoadTexture("gunTurret_botshot_gun", "sprites/Towers/Turret/turret_botshot/turret_botshot_gun");
        LoadTexture("gunTurret_botshot_bullet", "sprites/Towers/Turret/turret_botshot/botshot_pellet");
        LoadTexture("gunTurret_doublegun_gun", "sprites/Towers/Turret/turret_doublegun/turret_gun_10");
        LoadTexture("gunTurret_improvedbarrel_gun", "sprites/Towers/Turret/turret_improvedbarrel/turret_improvedbarrel_gun");
        LoadTexture("gunTurret_improvedbarrel_bullet", "sprites/Towers/Turret/turret_improvedbarrel/turret_improvedbarrel_bullet");
        LoadTexture("gunTurret_photoncannon_body", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_body");
        LoadTexture("gunTurret_photoncannon_gun", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_gun");
        LoadTexture("gunTurret_photoncannon_laser", "sprites/Towers/Turret/turret_photoncannon/photonlaser");
        LoadTexture("laser_particle", "sprites/Towers/Turret/turret_photoncannon/laserparticle");
        LoadTexture("gunTurret_rocketshots_body", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_body");
        LoadTexture("gunTurret_rocketshots_gun", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_gun");
        LoadTexture("gunTurret_rocketshots_rocket", "sprites/Towers/Turret/turret_rocketshot/rocket");

        LoadTexture("gunTurret_botshot_icon", "sprites/Towers/Turret/turret_botshot/turret_botshot_icon");
        LoadTexture("gunTurret_doublegun_icon", "sprites/Towers/Turret/turret_doublegun/turret_doublegun_icon");
        LoadTexture("gunTurret_improvedbarrel_icon", "sprites/Towers/Turret/turret_improvedbarrel/turret_improvedbarrel_icon");
        LoadTexture("gunTurret_photoncannon_icon", "sprites/Towers/Turret/turret_photoncannon/turret_photoncannon_icon");
        LoadTexture("gunTurret_rocketshots_icon", "sprites/Towers/Turret/turret_rocketshot/turret_rocketshot_icon");

        LoadTexture("railgun_base_idle", "sprites/Towers/Railgun/railgun_base/railgun_base_idle");
        LoadTexture("railgun_base_fire", "sprites/Towers/Railgun/railgun_base/railgun_base_fire");
        LoadTexture("railgun_base_bullet", "sprites/Towers/Railgun/railgun_base/railgunbullet");
        LoadTexture("railgun_antimatterlaser_idle", "sprites/Towers/Railgun/railgun_antimatterlaser/railgun_antiomatterlaser_idle");
        LoadTexture("railgun_antimatterlaser_fire", "sprites/Towers/Railgun/railgun_antimatterlaser/railgun_antiomatterlaser_fire");
        LoadTexture("railgun_antimatterlaser_bullet", "sprites/Towers/Railgun/railgun_antimatterlaser/antimatterblast");
        LoadTexture("railgun_antimatterlaser_muzzleflash", "sprites/Towers/Railgun/railgun_antimatterlaser/antimatterlaser_muzzleflash");
        LoadTexture("railgun_cannonball_idle", "sprites/Towers/Railgun/railgun_cannonball/railgun_cannonball_idle");
        LoadTexture("railgun_cannonball_fire", "sprites/Towers/Railgun/railgun_cannonball/railgun_cannonball_fire");
        LoadTexture("railgun_cannonball_bullet", "sprites/Towers/Railgun/railgun_cannonball/cannonball_projectile");
        LoadTexture("railgun_goldengatling_idle", "sprites/Towers/Railgun/railgun_goldengatling/railgun_goldengatling_idle");
        LoadTexture("railgun_goldengatling_fire", "sprites/Towers/Railgun/railgun_goldengatling/railgun_goldengatling_firing");
        LoadTexture("railgun_goldengatling_bullet", "sprites/Towers/Railgun/railgun_goldengatling/goldengatling_projectile");
        LoadTexture("railgun_goldengatling_bullet_flaming", "sprites/Towers/Railgun/railgun_goldengatling/goldengatling_projectile_flaming");
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
        LoadTexture("drone_base_gun", "sprites/Towers/Drone/drone_base/drone_base_gun");
        LoadTexture("drone_advancedweaponry_idle", "sprites/Towers/Drone/drone_advancedweaponry/drone_advancedweaponry_idle");
        LoadTexture("drone_advancedweaponry_gun", "sprites/Towers/Drone/drone_advancedweaponry/drone_advancedweaponry_gun");
        LoadTexture("drone_artificer_idle", "sprites/Towers/Drone/drone_artificer/drone_artificer_idle");
        LoadTexture("drone_artificer_attack", "sprites/Towers/Drone/drone_artificer/drone_artificer_attack");
        LoadTexture("drone_artificer_explosion", "sprites/Towers/Drone/drone_artificer/artificer_explosion");
        LoadTexture("drone_assassindrone_idle", "sprites/Towers/Drone/drone_assassindrone/drone_assassindrone_idle");
        LoadTexture("drone_assassindrone_gun", "sprites/Towers/Drone/drone_assassindrone/drone_assassindrone_gun");
        LoadTexture("drone_assassindrone_bullet", "sprites/Towers/Drone/drone_assassindrone/drone_assassindrone_projectile");
        LoadTexture("drone_flyingarsenal_idle", "sprites/Towers/Drone/drone_flyingarsenal/drone_flyingarsenal_idle");
        LoadTexture("drone_flyingarsenal_gun", "sprites/Towers/Drone/drone_flyingarsenal/drone_flyingarsenal_gun");
        LoadTexture("drone_improvedradar_idle", "sprites/Towers/Drone/drone_improvedradar/drone_improvedradar_idle");
        LoadTexture("drone_quadcopter_idle", "sprites/Towers/Drone/drone_quadcopter/drone_quadcopter_idle");
        LoadTexture("drone_quadcopter_gun", "sprites/Towers/Drone/drone_quadcopter/drone_quadcopter_gun");

        LoadTexture("drone_advancedweaponry_icon", "sprites/Towers/Drone/drone_advancedweaponry/advancedweaponry_icon");
        LoadTexture("drone_artificer_icon", "sprites/Towers/Drone/drone_artificer/artificer_icon");
        LoadTexture("drone_assassindrone_icon", "sprites/Towers/Drone/drone_assassindrone/icon_assassindrone");
        LoadTexture("drone_flyingarsenal_icon", "sprites/Towers/Drone/drone_flyingarsenal/flyingarsenal_icon");
        LoadTexture("drone_improvedradar_icon", "sprites/Towers/Drone/drone_improvedradar/improvedradar_icon");
        LoadTexture("drone_quadcopter_icon", "sprites/Towers/Drone/drone_quadcopter/icon_quadcopter");

        LoadTexture("crane_base_idle", "sprites/Towers/Crane/crane_base/crane_base_idle");
        LoadTexture("crane_base_attack", "sprites/Towers/Crane/crane_base/crane_base_attack");
        LoadTexture("crane_biggerbox_idle", "sprites/Towers/Crane/crane_biggerbox/crane_biggerbox_idle");
        LoadTexture("crane_biggerbox_attack", "sprites/Towers/Crane/crane_biggerbox/crane_biggerbox_attack");
        LoadTexture("crane_biggerbox_box", "sprites/Towers/Crane/crane_biggerbox/biggerbox");
        LoadTexture("crane_biggerbox_icon", "sprites/Towers/Crane/crane_biggerbox/biggerbox_icon");
        LoadTexture("crane_chargedlifts_idle", "sprites/Towers/Crane/crane_chargedlifts/crane_chargedlifts_idle");
        LoadTexture("crane_chargedlifts_attack", "sprites/Towers/Crane/crane_chargedlifts/crane_chargedlifts_attack");
        LoadTexture("crane_chargedlifts_icon", "sprites/Towers/Crane/crane_chargedlifts/chargedlifts_icon");
        LoadTexture("crane_crusher_idle", "sprites/Towers/Crane/crane_crusher/crane_crusher_idle");
        LoadTexture("crane_crusher_attack", "sprites/Towers/Crane/crane_crusher/crane_crusher_fire");
        LoadTexture("crane_crusher_ball", "sprites/Towers/Crane/crane_crusher/crusherball");
        LoadTexture("crane_explosivepayload_idle", "sprites/Towers/Crane/crane_explosivepayload/crane_explosivepayload_idle");
        LoadTexture("crane_explosivepayload_attack", "sprites/Towers/Crane/crane_explosivepayload/crane_explosivepayload_attack");
        LoadTexture("crane_explosivepayload_icon", "sprites/Towers/Crane/crane_explosivepayload/explosivepayload_icon");
        LoadTexture("crane_explosivepayload_box", "sprites/Towers/Crane/crane_explosivepayload/explosivebox");
        LoadTexture("crane_lasergate_idle", "sprites/Towers/Crane/crane_lasergate/crane_lasergate_idle");
        LoadTexture("crane_lasergate_attack", "sprites/Towers/Crane/crane_lasergate/crane_lasergate_attacking");
        LoadTexture("crane_lasergate_icon", "sprites/Towers/Crane/crane_lasergate/lasergate_icon");
        LoadTexture("crane_lasergate_laser", "sprites/Towers/Crane/crane_lasergate/gatelaser");
        LoadTexture("crane_sawblade_idle", "sprites/Towers/Crane/crane_sawblade/crane_sawblade_idle");
        LoadTexture("crane_sawblade_attack", "sprites/Towers/Crane/crane_sawblade/crane_sawblade_attack");
        LoadTexture("crane_sawblade_blade", "sprites/Towers/Crane/crane_sawblade/sawblade");

        LoadTexture("mortar_base_idle", "sprites/Towers/Mortar/mortar_base/mortar_base_idle");
        LoadTexture("mortar_base_fire", "sprites/Towers/Mortar/mortar_base/mortar_base_fire");
        LoadTexture("mortar_base_shell", "sprites/Towers/Mortar/mortar_base/mortarshell");
        LoadTexture("mortar_bouncingbomb_idle", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncing_bomb_idle");
        LoadTexture("mortar_bouncingbomb_fire", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncing_bomb_fire");
        LoadTexture("mortar_bouncingbomb_shell", "sprites/Towers/Mortar/mortar_bouncing_bomb/bouncing_bomb");
        LoadTexture("mortar_bouncingbomb_explosion", "sprites/Towers/Mortar/mortar_bouncing_bomb/explosion_bouncingbomb");
        LoadTexture("mortar_efficientreload_idle", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_idle");
        LoadTexture("mortar_efficientreload_fire", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_fire");
        LoadTexture("mortar_heavyshells_idle", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_idle");
        LoadTexture("mortar_heavyshells_fire", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_fire");
        LoadTexture("mortar_heavyshells_shell", "sprites/Towers/Mortar/mortar_heavyshells/heavyshell");
        LoadTexture("mortar_hellrain_idle", "sprites/Towers/Mortar/mortar_hellrain/mortar_hellrain_idle");
        LoadTexture("mortar_hellrain_fire", "sprites/Towers/Mortar/mortar_hellrain/mortar_hellrain_fire");
        LoadTexture("mortar_hellrain_shell", "sprites/Towers/Mortar/mortar_hellrain/rainmissile");
        LoadTexture("mortar_hellrain_explosion", "sprites/Towers/Mortar/mortar_hellrain/rainmissile_explosion");
        LoadTexture("mortar_missilesilo_idle", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_idle");
        LoadTexture("mortar_missilesilo_fire", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_fire");
        LoadTexture("mortar_missilesilo_shell", "sprites/Towers/Mortar/mortar_missilesilo/ballisticmissile");
        LoadTexture("mortar_nuke_idle", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_idle");
        LoadTexture("mortar_nuke_fire", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_fire");
        LoadTexture("mortar_nuke_shell", "sprites/Towers/Mortar/mortar_nuke/nuke");
        LoadTexture("mortar_nuke_explosion", "sprites/Towers/Mortar/mortar_nuke/nuclear_explosion");
        LoadTexture("mortar_reticle", "sprites/Towers/Mortar/mortar_reticle");

        LoadTexture("mortar_bouncingbomb_icon", "sprites/Towers/Mortar/mortar_bouncing_bomb/mortar_bouncingbomb_icon");
        LoadTexture("mortar_efficientreload_icon", "sprites/Towers/Mortar/mortar_efficientreload/mortar_efficientreload_icon");
        LoadTexture("mortar_heavyshells_icon", "sprites/Towers/Mortar/mortar_heavyshells/mortar_heavyshells_icon");
        LoadTexture("mortar_hellrain_icon", "sprites/Towers/Mortar/mortar_hellrain/mortar_hellrain_icon");
        LoadTexture("mortar_missilesilo_icon", "sprites/Towers/Mortar/mortar_missilesilo/mortar_missilesilo_icon");
        LoadTexture("mortar_nuke_icon", "sprites/Towers/Mortar/mortar_nuke/mortar_nuke_icon");

        LoadTexture("hovership_base_idle", "sprites/Towers/Hovership/hovership_base/hovership_base_idle");
        LoadTexture("hovership_base_platform", "sprites/Towers/Hovership/hovership_base/hovership_base_platform");
        LoadTexture("hovership_base_bomb", "sprites/Towers/Hovership/hovership_base/hovership_bomb");
        LoadTexture("hovership_bombierbay_idle", "sprites/Towers/Hovership/hovership_bombierbay/hovership_bombierbay_idle");
        LoadTexture("hovership_bombierbay_platform", "sprites/Towers/Hovership/hovership_bombierbay/hovership_bombierbay_platform");
        LoadTexture("hovership_carpetoffire_idle", "sprites/Towers/Hovership/hovership_carpetoffire/hovership_carpetoffire_idle");
        LoadTexture("hovership_carpetoffire_platform", "sprites/Towers/Hovership/hovership_carpetoffire/hovership_carpetoffire_platform");
        LoadTexture("hovership_carpetoffire_bomb", "sprites/Towers/Hovership/hovership_carpetoffire/hovership_carpetoffire_firebomb");
        LoadTexture("hovership_efficientengines_idle", "sprites/Towers/Hovership/hovership_efficientengines/hovership_efficientengines_idle");
        LoadTexture("hovership_efficientengines_platform", "sprites/Towers/Hovership/hovership_efficientengines/hovership_efficientengines_platform");
        LoadTexture("hovership_emp_idle", "sprites/Towers/Hovership/hovership_EMP/hovership_EMP_idle");
        LoadTexture("hovership_emp_platform", "sprites/Towers/Hovership/hovership_EMP/hovership_EMP_platform");
        LoadTexture("hovership_emp_bomb", "sprites/Towers/Hovership/hovership_EMP/EMPbomb");
        LoadTexture("hovership_orbitallaser_idle", "sprites/Towers/Hovership/hovership_orbitallaser/hovership_orbitallaser_idle");
        LoadTexture("hovership_orbitallaser_attack", "sprites/Towers/Hovership/hovership_orbitallaser/hovership_orbitallaser_attack");
        LoadTexture("hovership_orbitallaser_firing", "sprites/Towers/Hovership/hovership_orbitallaser/hovership_orbitallaser_firing");
        LoadTexture("hovership_orbitallaser_platform", "sprites/Towers/Hovership/hovership_orbitallaser/hovership_orbitallaser_platform");
        LoadTexture("hovership_orbitallaser_beam", "sprites/Towers/Hovership/hovership_orbitallaser/orbitalbeam");
        LoadTexture("hovership_orbitallaser_impact", "sprites/Towers/Hovership/hovership_orbitallaser/orbitalbeam_impact");
        LoadTexture("hovership_ufo_idle", "sprites/Towers/Hovership/hovership_UFO/hovership_UFO_idle");
        LoadTexture("hovership_ufo_platform", "sprites/Towers/Hovership/hovership_UFO/hovership_UFO_platform");
        LoadTexture("hovership_ufo_tractorbeam", "sprites/Towers/Hovership/hovership_UFO/hovership_UFO_tractorbeam");

        LoadTexture("hovership_bombierbay_icon", "sprites/Towers/Hovership/hovership_bombierbay/bombierbay_icon");
        LoadTexture("hovership_carpetoffire_icon", "sprites/Towers/Hovership/hovership_carpetoffire/carpetoffire_icon");
        LoadTexture("hovership_efficientengines_icon", "sprites/Towers/Hovership/hovership_efficientengines/efficientengines_icon");
        LoadTexture("hovership_emp_icon", "sprites/Towers/Hovership/hovership_EMP/EMP_icon");
        LoadTexture("hovership_orbitallaser_icon", "sprites/Towers/Hovership/hovership_orbitallaser/orbitallaser_icon");
        LoadTexture("hovership_ufo_icon", "sprites/Towers/Hovership/hovership_UFO/UFO_icon");

        LoadTexture("punchtrap_base_idle", "sprites/Towers/Punch Trap/punchtrap_base/punchtrap_base_idle");
        LoadTexture("punchtrap_base_fire", "sprites/Towers/Punch Trap/punchtrap_base/punchtrap_base_fire");
        LoadTexture("punchtrap_chainsaw_idle", "sprites/Towers/Punch Trap/punchtrap_chainsaw/punchtrap_chainsaw_idle");
        LoadTexture("punchtrap_chainsaw_fire", "sprites/Towers/Punch Trap/punchtrap_chainsaw/punchtrap_chainsaw_fire");
        LoadTexture("punchtrap_chainsaw_firing", "sprites/Towers/Punch Trap/punchtrap_chainsaw/punchtrap_chainsaw_running");
        LoadTexture("punchtrap_fatfist_idle", "sprites/Towers/Punch Trap/punchtrap_fatfist/punchtrap_fatfist_idle");
        LoadTexture("punchtrap_fatfist_fire", "sprites/Towers/Punch Trap/punchtrap_fatfist/punchtrap_fatfist_fire");
        LoadTexture("punchtrap_flurryofblows_idle", "sprites/Towers/Punch Trap/punchtrap_flurryofblows/punchtrap_flurryofblows_idle");
        LoadTexture("punchtrap_flurryofblows_fire", "sprites/Towers/Punch Trap/punchtrap_flurryofblows/punchtrap_flurryofblows_fire");
        LoadTexture("punchtrap_megapunch_idle", "sprites/Towers/Punch Trap/punchtrap_megapunch/punchtrap_finalstate");
        LoadTexture("punchtrap_megapunch_fire", "sprites/Towers/Punch Trap/punchtrap_megapunch/punchtrap_megapunch_fire_charged");
        LoadTexture("punchtrap_quickjabs_idle", "sprites/Towers/Punch Trap/punchtrap_quickjabs/punchtrap_quickjabs_idle");
        LoadTexture("punchtrap_quickjabs_fire", "sprites/Towers/Punch Trap/punchtrap_quickjabs/punchtrap_quickjabs_fire");
        LoadTexture("punchtrap_rocketglove_idle", "sprites/Towers/Punch Trap/punchtrap_rocketglove/punchtrap_rocketglove_idle");
        LoadTexture("punchtrap_rocketglove_fire", "sprites/Towers/Punch Trap/punchtrap_rocketglove/punchtrap_rocketglove_fire");
        LoadTexture("rocketglove", "sprites/Towers/Punch Trap/punchtrap_rocketglove/rocketglove");

        LoadTexture("punchtrap_chainsaw_icon", "sprites/Towers/Punch Trap/punchtrap_chainsaw/chainsaw_icon");
        LoadTexture("punchtrap_fatfist_icon", "sprites/Towers/Punch Trap/punchtrap_fatfist/fatfist_icon");
        LoadTexture("punchtrap_flurryofblows_icon", "sprites/Towers/Punch Trap/punchtrap_flurryofblows/flurryofblows_icon");
        LoadTexture("punchtrap_megapunch_icon", "sprites/Towers/Punch Trap/punchtrap_megapunch/megapunch_icon");
        LoadTexture("punchtrap_quickjabs_icon", "sprites/Towers/Punch Trap/punchtrap_quickjabs/quickjabs_icon");
        LoadTexture("punchtrap_rocketglove_icon", "sprites/Towers/Punch Trap/punchtrap_rocketglove/rocketglove_icon");

        // effects
        LoadTexture("muzzleflash_small", "sprites/Generic Effects/muzzleflash_small");
        LoadTexture("muzzleflash_medium", "sprites/Generic Effects/muzzleflash_medium");
        LoadTexture("muzzleflash_large", "sprites/Generic Effects/muzzleflash_large");
        LoadTexture("explosion_small", "sprites/Generic Effects/explosion_small");
        LoadTexture("explosion_large", "sprites/Generic Effects/explosion_large");
        LoadTexture("death_explosion_small", "sprites/Enemy Juice Effects/enemy_deathexplosion_small");
        LoadTexture("botchunk1", "sprites/Particles/botchunk1");
        LoadTexture("botchunk2", "sprites/Particles/botchunk2");
        LoadTexture("botchunk3", "sprites/Particles/botchunk3");
        LoadTexture("botchunk4", "sprites/Particles/botchunk4");
        LoadTexture("botchunk5", "sprites/Particles/botchunk5");
        LoadTexture("botchunk6", "sprites/Particles/botchunk6");
        LoadTexture("smoke", "sprites/Particles/smoke");

        // tiles
        LoadTexture("purptiles", "sprites/tiles/purptiles");
        LoadTexture("blacktiles", "sprites/Tiles/blacktiles");
        LoadTexture("heavytiles", "sprites/Tiles/metaltiles");
        LoadTexture("lighttiles", "sprites/Tiles/scaffoldtiles");
        LoadTexture("groundtiles_zone1", "sprites/Tiles/groundtiles_zone1");
        LoadTexture("heavytilesingle", "sprites/Tiles/metaltiles_single");
        LoadTexture("lighttilesingle", "sprites/Tiles/scaffoldtiles_single");

        // enemies
        LoadTexture("node", "sprites/Enemies/Node/node_body");
        LoadTexture("node_corpse", "sprites/Enemies/Node/node_corpse");
        LoadTexture("bouncer", "sprites/Enemies/Bouncer/bouncer_body");
        LoadTexture("meganode", "sprites/Enemies/Meganode/meganode_body");

        // environment
        LoadTexture("hq", "sprites/Misc/HQ");

        // - zone4 bg moving objects
        LoadTexture("cloud_z1_1", "sprites/Environment Objects/Zone 4/background/Moving Objects/cloud_z1_1");
        LoadTexture("cloud_z1_2", "sprites/Environment Objects/Zone 4/background/Moving Objects/cloud_z1_2");
        LoadTexture("roboship", "sprites/Environment Objects/Zone 4/background/Moving Objects/roboship_1");
        // - zone4 bg
        LoadTexture("zone1_backdrop", "sprites/Backdrops/zone1_backdrop");
        LoadTexture("skyscraper_ruins_1", "sprites/Environment Objects/Zone 4/background/skyscraper_ruins_1");
        LoadTexture("skyscraper_ruins_2", "sprites/Environment Objects/Zone 4/background/skyscraper_ruins_2");

        // - zone4 midground moving objects
        LoadTexture("hovercraft", "sprites/Environment Objects/Zone 4/midground/Moving Objects/hovercraft_1");
        // - zone4 mg
        LoadTexture("deadtree_1", "sprites/Environment Objects/Zone 4/midground/deadtree_1");
        LoadTexture("deadtree_2", "sprites/Environment Objects/Zone 4/midground/deadtree_2");
        LoadTexture("ruins_1", "sprites/Environment Objects/Zone 4/midground/ruins_1");

        LoadTexture("smog_1", "sprites/Smogs/Generic/smog_1");
        LoadTexture("smog_2", "sprites/Smogs/Generic/smog_2");

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
        LoadFont("pixelsix", "fonts/pixelsix/pixelsix_bitmap_test");

        // sounds
        LoadSound("explosion", "sound/explosion/explosion"); // temp
        LoadSound("placeDown", "sound/turret/placeDown"); // temp
        LoadSound("shoot", "sound/turret/shoot"); // temp
        LoadSound("upgrade", "sound/turret/upgrade"); // temp
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

    public static SoundEffect GetSound(string name)
    {
        if (_soundEffects.TryGetValue(name, out var sound))
        {
            return sound;
        }
        else
        {
            throw new KeyNotFoundException($"Sound '{name}' not found");
        }
    }
}
