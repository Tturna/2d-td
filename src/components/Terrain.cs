using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace _2d_td;

#nullable enable
public class Terrain : DrawableGameComponent
{
    private struct HeavyTile
    {
        public int TileId;
        public HealthSystem? Health;
        public float DamageDelayTimer;

        public HeavyTile(int tileId)
        {
            TileId = tileId;
        }
    }

    private Game1 game;
    private readonly Tileset TerrainTileset;
    private readonly Tileset BackgroundTileset;
    private readonly Tileset PlayerLightTileset;
    private readonly Tileset PlayerHeavyTileset;
    private readonly string LevelPath;
    private readonly string BgLevelPath;

    private Dictionary<Vector2, int> tiles = new();
    private Dictionary<Vector2, int> backgroundTiles = new();
    private Dictionary<Vector2, int> lightTiles = new();
    private Dictionary<Vector2, HeavyTile> heavyTiles = new();
    private Vector2 levelOffset = new Vector2(0, 64 * Grid.TileLength);
    private float tileDamageDelay = 0.5f;

    private Vector2 topRightMostTile;

    public Terrain(Game game, int zone, int level, string tilesetName = "groundtiles_zone1",
        string bgTilesetName = "blacktiles", int tilesetWidth = 12, int tilesetHeight = 4,
        int bgTilesetWidth = 4, int bgTilesetHeight = 4) : base(game)
    {
        this.game = (Game1)game;

        if (zone == 0)
        {
            throw new ArgumentException("Zone can't be less than 1", nameof(zone));
        }
        else if (level == 0)
        {
            throw new ArgumentException("Level can't be less than 1", nameof(level));
        }

        var levelName = $"zone{zone}level{level}";
        LevelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
            "data", "levels", levelName, $"{levelName}.csv");
        BgLevelPath = Path.Combine(AppContext.BaseDirectory, game.Content.RootDirectory,
            "data", "levels", levelName, $"{levelName}_bg.csv");

        TerrainTileset = new Tileset(AssetManager.GetTexture(tilesetName), tilesetWidth, tilesetHeight);
        BackgroundTileset = new Tileset(AssetManager.GetTexture(bgTilesetName), bgTilesetWidth, bgTilesetHeight);
        PlayerHeavyTileset = new Tileset(AssetManager.GetTexture("heavytiles"), 12, 4);
        PlayerLightTileset = new Tileset(AssetManager.GetTexture("lighttiles"), 12, 4);
    }

    public override void Initialize()
    {
        // "using" makes it so Dispose() is automatically called on StreamReader when
        // it's not needed anymore.
        using (var levelReader = new StreamReader(LevelPath))
        {
            string? line = levelReader.ReadLine();
            var row = 0;

            while (line is not null)
            {
                String[] ids = line.Split(",");

                for (int col = 0; col < ids.Length; col++)
                {
                    if (!int.TryParse(ids[col], out int tileId))
                    {
                        throw new InvalidDataException(
                            $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {LevelPath}. Expected an integer."
                        );
                    }

                    if (tileId == -1) continue; // The Tiled editor sets air to -1

                    var tilePosition = new Vector2(col, row);
                    tiles[tilePosition] = tileId;

                    if (col > topRightMostTile.X)
                    {
                        topRightMostTile = tilePosition;
                    }
                }

                line = levelReader.ReadLine();
                row++;
            }
        }

        if (File.Exists(BgLevelPath))
        {
            using (var bgLevelReader = new StreamReader(BgLevelPath))
            {
                string? line = bgLevelReader.ReadLine();
                var row = 0;

                while (line is not null)
                {
                    string[] ids = line.Split(",");

                    for (int col = 0; col < ids.Length; col++)
                    {
                        if (!int.TryParse(ids[col], out int tileId))
                        {
                            throw new InvalidDataException(
                                $"Invalid value '{ids[col]}' at row {row}, column {col + 1} in {BgLevelPath}. Expected an integer."
                            );
                        }

                        if (tileId == -1) continue; // The Tiled editor sets air to -1

                        var tilePosition = new Vector2(col, row);
                        backgroundTiles[tilePosition] = tileId;
                    }

                    line = bgLevelReader.ReadLine();
                    row++;
                }
            }
        }
        else
        {
            Console.WriteLine($"Background tile file ({BgLevelPath}) not found.");
        }

        WaveSystem.WaveEnded += () =>
        {
            foreach (var (pos, tile) in heavyTiles)
            {
                tile.Health?.ResetHealth();
            }
        };
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var (pos, tile) in heavyTiles)
        {
            if (tile.Health is null) continue;

            if (tile.DamageDelayTimer > 0)
            {
                var copy = tile;
                copy.DamageDelayTimer -= deltaTime;
                heavyTiles[pos] = copy;
            }

            tile.Health.UpdateHealthBarGraphics(deltaTime);
        }

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        foreach ((Vector2 tilePosition, int tileId) in tiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            TerrainTileset.DrawTile(game.SpriteBatch, tileId, worldPosition, depth: 0.98f);
        }

        foreach ((Vector2 tilePosition, int tileId) in backgroundTiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            BackgroundTileset.DrawTile(game.SpriteBatch, tileId, worldPosition, depth: 1f);
        }

        foreach ((Vector2 tilePosition, int tileId) in lightTiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            PlayerLightTileset.DrawTile(game.SpriteBatch, tileId, worldPosition, depth: 0.97f);
        }

        foreach ((Vector2 tilePosition, HeavyTile tile) in heavyTiles)
        {
            var worldPosition = tilePosition * Grid.TileLength + levelOffset;
            PlayerHeavyTileset.DrawTile(game.SpriteBatch, tile.TileId, worldPosition, depth: 0.96f);
        }

        foreach (var (pos, tile) in heavyTiles)
        {
            if (tile.Health is null) continue;

            tile.Health.DrawHealthBar(Grid.TileToWorldPosition(pos) + levelOffset + new Vector2(Grid.TileLength / 2, -2));
        }
    }

    public bool PlaceLightTileAt(Vector2 worldPosition)
    {
        if (CanPlaceLightTile(worldPosition) == false)
        {
            return false;
        }

        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);
        lightTiles[tilePosition] = 1;

        return true;
    }

    public bool PlaceTileAt(Vector2 worldPosition,Tileset tileset)
    {
        if (tileset == PlayerLightTileset)
        {
            return PlaceLightTileAt(worldPosition);
        }
        else if (tileset == PlayerHeavyTileset)
        {
            return PlaceHeavyTileAt(worldPosition);
        }

        return false;
    }

    public bool PlaceHeavyTileAt(Vector2 worldPosition)
    {
        if (CanPlaceHeavyTile(worldPosition) == false)
        {
            return false;
        }

        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);
        heavyTiles[tilePosition] = new HeavyTile(36);

        return true;
    }

    public bool CanPlaceLightTile(Vector2 worldPosition)
    {
        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);

        return (lightTiles.ContainsKey(tilePosition + new Vector2(0, 1)) ||
                lightTiles.ContainsKey(tilePosition + new Vector2(0, -1)) ||
                tiles.ContainsKey(tilePosition + new Vector2(0, 1)) ||
                tiles.ContainsKey(tilePosition + new Vector2(0, -1))) &&
                !AnyTileExistsAtTilePosition(tilePosition);
    }

    public Tileset GetPlayerHeavyTileset()
    {
        return PlayerHeavyTileset;
    }

    public Tileset GetPlayerLightTileset()
    {
        return PlayerLightTileset;
    }

    public bool CanPlaceHeavyTile(Vector2 worldPosition)
    {
        bool stable = true;
        int maxUnstableTiles = 4;
        var tilePosition = Grid.WorldToTilePosition(worldPosition - levelOffset);

        if (AnyTileExistsAtTilePosition(tilePosition))
        {
            return false;
        }

        int unstableTiles = CountUnstableHeavyTiles(tilePosition, new List<Vector2>());
        stable = (unstableTiles > maxUnstableTiles) ? false : true;

        if (!stable)
        {
            return false;
        }

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Math.Abs(i + j) != 1)
                {
                    continue;
                }

                if (tiles.ContainsKey(tilePosition + new Vector2(i, j)) ||
                    lightTiles.ContainsKey(tilePosition + new Vector2(0, 1)) ||
                    heavyTiles.ContainsKey(tilePosition + new Vector2(i, j)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int CountUnstableHeavyTiles(Vector2 tilePosition, List<Vector2> checkedTiles)
    {
        checkedTiles.Add(tilePosition);

        if (!AnyTileExistsAtTilePosition(tilePosition + new Vector2(0, 1)) && !AnyTileExistsAtTilePosition(tilePosition + new Vector2(0, -1)))
        {
            int stableLeft = 0;
            int stableRight = 0;

            if (heavyTiles.ContainsKey(tilePosition + new Vector2(-1, 0)) && !checkedTiles.Contains(tilePosition + new Vector2(-1, 0)))
            {
                stableLeft = CountUnstableHeavyTiles(tilePosition + new Vector2(-1, 0), checkedTiles);
            }

            if (heavyTiles.ContainsKey(tilePosition + new Vector2(1, 0)) && !checkedTiles.Contains(tilePosition + new Vector2(1, 0)))
            {
                stableRight = CountUnstableHeavyTiles(tilePosition + new Vector2(1, 0), checkedTiles);
            }

            return stableLeft + stableRight + 1;
        }

        int stableAbove = 0;
        int stableBelow = 0;

        if (heavyTiles.ContainsKey(tilePosition + new Vector2(0, 1)) &&
            !lightTiles.ContainsKey(tilePosition + new Vector2(0, 1)) &&
            !tiles.ContainsKey(tilePosition + new Vector2(0, 1)) &&
            !checkedTiles.Contains(tilePosition + new Vector2(0, 1)))
        {
            stableBelow = CountUnstableHeavyTiles(tilePosition + new Vector2(0, 1),checkedTiles) + 1;
        }

        if (heavyTiles.ContainsKey(tilePosition + new Vector2(0, -1)) &&
            !lightTiles.ContainsKey(tilePosition + new Vector2(0, -1)) &&
            !tiles.ContainsKey(tilePosition + new Vector2(0, -1)) &&
            !checkedTiles.Contains(tilePosition + new Vector2(0, -1)))
        {
            stableAbove = CountUnstableHeavyTiles(tilePosition + new Vector2(0, -1),checkedTiles) + 1;
        }

        return stableAbove + stableBelow;
    }

    public Vector2 GetRightMostTopTileWorldPosition()
    {
        return topRightMostTile * Grid.TileLength + levelOffset;
    }

    public Vector2 GetFirstTilePosition()
    {
        return tiles.Keys.First() * Grid.TileLength + levelOffset;
    }

    public bool SolidTileExistsAtPosition(Vector2 tilePosWithLevelOffset)
    {
        var tilePosition = tilePosWithLevelOffset - levelOffset / Grid.TileLength;

        return tiles.ContainsKey(tilePosition) || heavyTiles.ContainsKey(tilePosition);
    }

    public bool SolidTileExistsAtTilePosition(Vector2 tilePosition)
    {
        return tiles.ContainsKey(tilePosition) || heavyTiles.ContainsKey(tilePosition);
    }

    public bool AnyTileExistsAtTilePosition(Vector2 tilePosition)
    {
        return tiles.ContainsKey(tilePosition) || heavyTiles.ContainsKey(tilePosition) || lightTiles.ContainsKey(tilePosition);
    }

    public bool HeavyTileExistsAtPosition(Vector2 tilePosWithLevelOffset)
    {
        var tilePosition = tilePosWithLevelOffset - levelOffset / Grid.TileLength;

        return heavyTiles.ContainsKey(tilePosition);
    }

    public bool HeavyTileIsIntactAtPosition(Vector2 tilePosWithLevelOffset)
    {
        var tilePosition = tilePosWithLevelOffset - levelOffset / Grid.TileLength;

        return heavyTiles.ContainsKey(tilePosition) &&
            (heavyTiles[tilePosition].Health is null ||
             heavyTiles[tilePosition].Health!.CurrentHealth > 0);
    }

    public bool CollisionTileIsActiveAtPosition(Vector2 tilePosWithLevelOffset)
    {
        if (SolidTileExistsAtPosition(tilePosWithLevelOffset))
        {
            if (HeavyTileExistsAtPosition(tilePosWithLevelOffset))
            {
                return HeavyTileIsIntactAtPosition(tilePosWithLevelOffset);
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    public void DamageHeavyTile(Vector2 tilePosWithLevelOffset, Entity source, int damage)
    {
        var tilePosition = tilePosWithLevelOffset - levelOffset / Grid.TileLength;
        var tile = heavyTiles[tilePosition];

        if (tile.DamageDelayTimer > 0) return;

        if (tile.Health is null)
        {
            tile.Health = new(owner: null, initialHealth: 50);
        }

        tile.Health.TakeDamage(source, damage);
        tile.DamageDelayTimer = tileDamageDelay;
        heavyTiles[tilePosition] = tile;

        var tileWorldPosition = Grid.TileToWorldPosition(tilePosition) + levelOffset;

        UIComponent.SpawnFlyoutText(damage.ToString(), tileWorldPosition, -Vector2.UnitY * 25f,
            1f, Color.White);
    }
}
