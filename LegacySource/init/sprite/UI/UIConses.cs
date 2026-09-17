using System;
using System.Collections.Generic;
using System.IO;
using init.constant;
using init.paths;
using snake2d.util.sets;
using util.spritecomposer;

namespace init.sprite.UI
{
    public class UIConses
    {
        static UIConses()
        {
            new IInit(PATHS.SPRITE_UI().get("Cons"), 1008, 328)
            {
                protected override void init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.init(0, 144, 1, 1, 32, 1, d.s16);
                }
            };
        }

        public readonly Small TINY = new Small();
        public readonly Big BIG = new Big();

        public readonly Icons ICO = new Icons();
        public readonly Rotaters ROT = new Rotaters();
        public readonly TILE_SHEET fullArrows = new ITileSheet()
        {
            protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
            {
                s.singles.init(0, s.singles.body().y2(), 1, 1, 1, 1, d.s16);
                s.singles.paste(3, true);
                s.combo.init(s.singles.body().x2(), s.singles.body().y1(), 1, 1, 2, d.s16);
                s.combo.paste(3, true);
                s.combo.init(s.combo.body().x2(), s.singles.body().y1(), 1, 1, 3, d.s16);
                s.combo.paste(3, true);
                return d.s16.saveGame();
            }
        }.get();

        public UIConses()
        {
        }

        public class Small
        {
            public readonly UICons high = new UICons(new ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.init(0, 0, 7, 1, d.s8);
                    s.house.setVar(0).paste(true);
                    return d.s8.save(C.SCALE * 2);
                }
            }.get());

            public readonly UICons low = getTiny(1);
            public readonly UICons flat = getTiny(2);
            public readonly UICons outline = getTiny(3);
            public readonly UICons dashed = getTiny(4);
            public readonly UICons full = getTiny(5);
            public readonly UICons dots = getTiny(6);

            private Small()
            {
            }

            private UICons getTiny(int nr)
            {
                return new UICons(new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.house.setVar(nr).paste(true);
                        return d.s8.save(C.SCALE * 2);
                    }
                }.get());
            }
        }

        public class Big
        {
            public readonly UICons outline = new UICons(new ITileSheet()
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.house.init(0, s.house.body().y2(), 7, 2, d.s16);
                    s.singles.init(0, s.house.body().y2(), 1, 1, 16, 1, d.s16);
                    s.house.setVar(0).paste(true);
                    s.singles.setSkip(0, 1);
                    s.singles.pasteEdges(true);
                    return d.s16.saveGame();
                }
            }.get(), Small.TINY.outline);

            public readonly UICons dashed = getSmall(1, Small.TINY.dashed);
            public readonly UICons dashedThick = getSmall(2, Small.TINY.dashed);
            public readonly UICons solid = getSmall(3, Small.TINY.full);
            public readonly UICons dots = getSmall(4, Small.TINY.dots);
            public readonly UICons line = getSmall(5, Small.TINY.dashed);
            public readonly UICons dashed_hollow = getSmall(7, Small.TINY.dashed);

            public readonly UICons filled = getSmall(9, Small.TINY.full);
            public readonly UICons filled_striped = getSmall(11, Small.TINY.full);

            private Big()
            {
            }

            private UICons getSmall(int nr, UICons tiny)
            {
                return new UICons(new ITileSheet()
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.house.setVar(nr).paste(true);
                        s.singles.setSkip(nr, 1);
                        s.singles.pasteEdges(true);
                        return d.s16.saveGame();
                    }
                }.get(), tiny);
            }
        }

        public class Icons
        {
            public readonly SPRITE unclear = ISprite.game(new ISpriteData()
            {
                protected override SpriteData init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.init(0, s.singles.body().y2(), 1, 1, 19, 1, d.s16);
                    s.singles.setSkip(0, 1).paste(true);
                    return d.s16.saveSprite();
                }
            }.get());

            public readonly SPRITE clear = getS(1);
            public readonly SPRITE cancel = getS(2);

            public readonly LIST<SPRITE> arrows = ISprite.game(new ISpriteList()
            {
                protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.setSkip(3, 1).pasteRotated(i, true);
                    return d.s16.saveSprite();
                }

                protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    return 4;
                }
            }.get());

            public readonly LIST<SPRITE> arrows2 = ISprite.game(new ISpriteList()
            {
                protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.setSkip(4 + (i & 1), 1).pasteRotated(i / 2, true);
                    return d.s16.saveSprite();
                }

                protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    return 8;
                }
            }.get());

            public readonly SPRITE crosshair = getS(6);
            public readonly SPRITE smallup = getS(7);
            public readonly SPRITE repair = getS(8);
            public readonly SPRITE arrows_inward = getS(9);
            public readonly SPRITE warning = getS(10);
            public readonly SPRITE full = getS(11);

            public readonly LIST<SPRITE> arrows_entity;

            private Icons()
            {
                LIST<SPRITE> li = ISprite.game(new ISpriteList()
                {
                    protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.setSkip(0, 1).pasteRotated(i, true);
                        return d.s16.saveSprite();
                    }

                    protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        return 4;
                    }
                }.get());

                LIST<SPRITE> l2 = ISprite.game(new ISpriteList()
                {
                    protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.setSkip(0, 1).pasteRotated(i, true);
                        return d.s16.saveSprite();
                    }

                    protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        return 4;
                    }
                }.get());

                ArrayList<SPRITE> ea = new ArrayList<SPRITE>(8);
                for (int i = 0; i < 4; i++)
                {
                    ea.add(li.get(i));
                    ea.add(l2.get(i));
                }
                arrows_entity = ea;
            }

            private SPRITE getS(int nr)
            {
                return ISprite.game(new ISpriteData()
                {
                    protected override SpriteData init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.setSkip(nr, 1).paste(true);
                        return d.s16.saveSprite();
                    }
                }.get());
            }
        }

        public class Rotaters
        {
            public readonly LIST<SPRITE> single = ISprite.game(new ISpriteList()
            {
                protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.setSkip(0, 1).pasteRotated(i, true);
                    return d.s16.saveSprite();
                }

                protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    s.singles.init(0, s.singles.body().y2(), 1, 1, 8, 1, d.s16);
                    return 4;
                }
            }.get());

            public readonly LIST<SPRITE> join = getS(1);
            public readonly LIST<SPRITE> join_thin = getS(2);
            public readonly LIST<SPRITE> north_south = getS(3);
            public readonly LIST<SPRITE> full = getS(4);
            public readonly LIST<SPRITE> join_big = getS(5);

            private Rotaters()
            {
            }

            private LIST<SPRITE> getS(int nr)
            {
                return ISprite.game(new ISpriteList()
                {
                    protected override SpriteData next(int i, ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.setSkip(nr, 1).pasteRotated(i, true);
                        return d.s16.saveSprite();
                    }

                    protected override int init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        return 4;
                    }
                }.get());
            }
        }
    }
}