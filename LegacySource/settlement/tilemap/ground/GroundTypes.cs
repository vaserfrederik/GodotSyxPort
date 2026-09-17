using System;
using System.IO;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Sprite;
using Util.SpriteComposer;
using Util.Text;

namespace Settlement.Tilemap.Ground
{
    public class GroundTypes
    {
        private static readonly string ¤¤nnmae = "Rich";
        private static readonly string ¤¤ndesc = "Richest of soils, very suitable for farming.";
        private static readonly string ¤¤fnmae = "Alluvium";
        private static readonly string ¤¤fdesc = "Trees will grow here, but soil is poor for agriculture. Suitable for woodcutters and orchards.";
        private static readonly string ¤¤pnmae = "Till";
        private static readonly string ¤¤pdesc = "Not the best soil, but all right for agriculture.";
        private static readonly string ¤¤rnmae = "Rock";
        private static readonly string ¤¤rdesc = "Can not be cultivated";
        private static readonly string ¤¤snmae = "Sand";
        private static readonly string ¤¤sdesc = "Sand is devoid of nutrients and can hardly be cultivated.";

        private static readonly string ¤¤poor = "Infertile";
        private static readonly string ¤¤poorD = "Infertile soil is devoid of nutrients and hard to work.";

        static GroundTypes()
        {
            D.ts(typeof(GroundTypes));
        }

        public const int VARS = 16 * 4;
        private readonly TILE_SHEET s_masks;
        private readonly TILE_SHEET c_masks;
        public readonly GroundType NORMAL;
        public readonly GroundType FOREST;
        public readonly GroundType PASTURE;
        public readonly GroundType SAND;
        public readonly GroundType ROCK;
        public readonly GroundType INFERTILE;
        public readonly GroundType[] ALL;

        public GroundTypes() : base()
        {
            s_masks = new ComposerThings.ITileSheet(PATHS.SPRITE_SETTLEMENT_MAP().get("Ground"), 536, 408)
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, 0, 1, 1, 8, 1, d.s16);
                    s.full.setSkip(8, 0);
                    s.full.paste(true);
                    s.full.pasteRotated(1, true);
                    s.full.pasteRotated(2, true);
                    s.full.pasteRotated(3, true);
                    return d.s16.saveGame();
                }
            }.get();

            c_masks = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, 0, 1, 1, 16, 1, d.s16);
                    s.full.setSkip(8, 8);
                    s.full.paste(true);
                    s.full.pasteRotated(1, true);
                    s.full.pasteRotated(2, true);
                    s.full.pasteRotated(3, true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_normal = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_rock = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_sand = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_tree = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_pasture = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            TILE_SHEET s_infertile = new ComposerThings.ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.full.init(0, s.full.body().y2(), 1, 1, 16, 4, d.s16);
                    s.full.paste(true);
                    return d.s16.saveGame();
                }
            }.get();

            Json j = new Json(PATHS.CONFIG().init.gets("SettColors"));

            NORMAL = new GroundType(0, s_normal, ¤¤nnmae, ¤¤ndesc, 1, 1.1);
            FOREST = new GroundType(1, s_tree, ¤¤fnmae, ¤¤fdesc, 1, 0.75);
            PASTURE = new GroundType(2, s_pasture, ¤¤pnmae, ¤¤pdesc, 1, 0.9);
            INFERTILE = new GroundType(3, s_infertile, ¤¤poor, ¤¤poorD, 0.25, 0.5);
            ROCK = new GroundType(4, s_rock, ¤¤rnmae, ¤¤rdesc, 1, 0.4).setColors(j.json("GROUND_ROCK"));
            SAND = new GroundType(5, s_sand, ¤¤snmae, ¤¤sdesc, 0.1, 0.4).setColors(j.json("GROUND_SAND"));

            ROCK.miniC.set(COLOR.WHITE25);
            ALL = new GroundType[] {
                NORMAL,
                FOREST,
                PASTURE,
                INFERTILE,
                ROCK,
                SAND
            };
        }
    }
}