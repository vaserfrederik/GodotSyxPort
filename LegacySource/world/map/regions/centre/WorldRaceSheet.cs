using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using util.rendering;
using util.spritecomposer;
using world;

namespace world.map.regions.centre
{
    public class WorldRaceSheet
    {
        private static KeyMap<TILE_SHEET> map = new KeyMap<TILE_SHEET>();
        private static KeyMap<WallSteriods> mapW = new KeyMap<WallSteriods>();

        static WorldRaceSheet()
        {
            new GameDisposable
            {
                protected override void dispose()
                {
                    map.Clear();
                    mapW.Clear();
                }
            };
        }

        public readonly Town town;
        public readonly Village village;
        public readonly WallSteriods walls;
        public readonly WallSteriods walls_village;
        public readonly Overlay overlay;
        public readonly Terrain terrain;
        public readonly Farm farm;

        public WorldRaceSheet(Json json) throws IOException
        {
            PATH getter = PATHS.WORLD().sprite.getFolder("centre");

            town = new Town(getter, json);
            village = new Village(getter, json);
            walls = new WallSteriods(6, "WALL", getter, json);
            walls_village = new WallSteriods(4, "WALL_VILLAGE", getter, json);
            overlay = new Overlay(getter, json);
            terrain = new Terrain(getter, json);
            farm = new Farm(getter, json);
        }

        public class Town
        {
            public readonly TILE_SHEET sheet;
            public readonly COLOR color;
            public const int maxSize = 3;

            public Town(PATH getter, Json json) throws IOException
            {
                string t = "TOWN";
                string f = json.value(t);
                string k = t + "_" + f;
                if (map.ContainsKey(k))
                    sheet = map.Get(k);
                else
                {
                    sheet = new ITileSheet(getter.getFolder("town").get(f), 460, 62)
                    {
                        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            s.singles.init(0, 0, 1, 1, 16, 4, d.s8);
                            s.singles.paste(true);
                            return d.s8.saveGame();
                        }
                    }.get();
                    map.Put(k, sheet);
                }
                color = new ColorImp(json, t + "_COLOR");
            }

            public void render(SPRITE_RENDERER r, int ran, int x, int y)
            {
                color.bind();
                int tile = ran & 0x0F;
                sheet.render(r, tile, x, y);
                COLOR.unbind();
            }
        }

        public class Village
        {
            public readonly TILE_SHEET sheet;
            public readonly COLOR color;
            public const int maxSize = 3;

            public Village(PATH getter, Json json) throws IOException
            {
                string t = "VILLAGE";
                string f = json.value(t);
                string k = t + "_" + f;
                if (map.ContainsKey(k))
                    sheet = map.Get(k);
                else
                {
                    sheet = new ITileSheet(getter.getFolder("village").get(f), 460, 62)
                    {
                        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            s.singles.init(0, 0, 1, 1, 16, 4, d.s8);
                            s.singles.paste(true);
                            return d.s8.saveGame();
                        }
                    }.get();
                    map.Put(k, sheet);
                }
                color = new ColorImp(json, t + "_COLOR");
            }

            public void render(SPRITE_RENDERER r, int ran, int x, int y)
            {
                color.bind();
                int tile = ran & 0x0F;
                sheet.render(r, tile, x, y);
                COLOR.unbind();
            }
        }

        public class WallSteriods
        {
            public readonly TILE_SHEET corners;
            public readonly TILE_SHEET gate;
            public readonly TILE_SHEET walls;
            public const int maxSize = 8;

            public WallSteriods(int var, string name, PATH getter, Json json) throws IOException
            {
                corners = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        final ComposerSources.Full f = s.full;
                        for (int i = 0; i <= 1; i++)
                        {
                            f.setVar(var + VARS * i);
                            f.setSkip(1, 0).paste(true);
                            f.setSkip(1, 5).paste(true);
                            f.setSkip(1, 30).paste(true);
                            f.setSkip(1, 35).paste(true);
                        }

                        return d.s8.saveGame();
                    }
                }.get();

                gate = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        final ComposerSources.Full f = s.full;
                        for (int i = 0; i <= 1; i++)
                        {
                            f.setVar(var + VARS * i);
                            f.setSkip(1, 2).paste(true);
                            f.setSkip(1, 3).paste(true);
                            f.setSkip(1, 12).paste(true);
                            f.setSkip(1, 18).paste(true);
                            f.setSkip(1, 17).paste(true);
                            f.setSkip(1, 23).paste(true);
                            f.setSkip(1, 32).paste(true);
                            f.setSkip(1, 33).paste(true);
                        }

                        return d.s8.saveGame();
                    }
                }.get();

                walls = new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        final ComposerSources.Full f = s.full;
                        for (int i = 0; i <= 1; i++)
                        {
                            f.setVar(var + VARS * i);
                            f.setSkip(1, 1).paste(true);
                            f.setSkip(1, 4).paste(true);
                            f.setSkip(1, 6).paste(true);
                            f.setSkip(1, 11).paste(true);
                            f.setSkip(1, 24).paste(true);
                            f.setSkip(1, 29).paste(true);
                            f.setSkip(1, 31).paste(true);
                            f.setSkip(1, 34).paste(true);
                        }

                        return d.s8.saveGame();
                    }
                }.get();
            }

            public void render(SPRITE_RENDERER r, int mask, int ran, int x, int y)
            {
                color.bind();
                int tile = ran & 0x0F;
                WORLD.BUILDINGS().sprites.terrainStencil.renderTextured(sheet.getTexture(tile), mask, x, y);
                COLOR.unbind();
            }
        }

        public class Terrain
        {
            public readonly TILE_SHEET sheet;
            public readonly COLOR color;

            public Terrain(PATH getter, Json json) throws IOException
            {
                string t = "TERRAIN";
                string f = json.value(t);
                string k = t + "_" + f;
                if (map.ContainsKey(k))
                    sheet = map.Get(k);
                else
                {
                    sheet = new ITileSheet(getter.getFolder("terrain").get(f), 152, 76)
                    {
                        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            ComposerDests.Tile t = d.s16;
                            final ComposerSources.Full f = s.full;
                            f.init(0, 0, 1, 1, 4, 4, t);
                            f.paste(true);
                            return t.saveGame();
                        }
                    }.get();
                    map.Put(k, sheet);
                }
                color = new ColorImp(json, t + "_COLOR");
            }

            public void render(WRenContext con, int mask, int ran, int x, int y)
            {
                color.bind();
                int tile = ran & 0x0F;
                WORLD.BUILDINGS().sprites.terrainStencil.renderTextured(sheet.getTexture(tile), mask, x, y);
                COLOR.unbind();
            }
        }

        public class Farm
        {
            public readonly TILE_SHEET sheet;
            public readonly LIST<ColorImp> color;

            public Farm(PATH getter, Json json) throws IOException
            {
                string t = "FARM";
                string f = json.value(t);
                string k = t + "_" + f;
                if (map.ContainsKey(k))
                    sheet = map.Get(k);
                else
                {
                    sheet = new ITileSheet(getter.getFolder("farm").get(f), 364, 50)
                    {
                        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            s.singles.init(0, 0, 1, 1, 8, 2, d.s16);
                            s.singles.paste(true);
                            return d.s16.saveGame();
                        }
                    }.get();
                    map.Put(k, sheet);
                }
                color = ColorImp.cols(json, t + "_COLOR");
            }

            public void render(WRenContext con, int ran, int x, int y)
            {
                color.getC(ran).bind();
                int tile = (ran >> 8) & 0x0F;
                sheet.render(con.r, tile, x, y);
                COLOR.unbind();
            }
        }
    }
}