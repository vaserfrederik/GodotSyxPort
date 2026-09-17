using System;
using System.IO;
using game;
using init.paths;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;
using util.colors;
using util.gui.misc;
using util.info;
using util.spritecomposer;
using util.text;
using view.keyboard;
using view.main;

public class UISpecials
{
    private readonly TILE_SHEET clockwork = new ITileSheet(PATHS.SPRITE_UI().get("Specials"), 1320, 208)
    {
        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
        {
            s.full.init(0, 0, 1, 1, 3, 1, d.s32);
            s.full.paste(true);
            return d.s32.saveGui();
        }
    }.get();

    private readonly SPRITE background = new ITileSprite(6 * 32, 2 * 32, 1)
    {
        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
        {
            s.full.init(s.full.body().x2(), 0, 1, 1, 6, 2, d.s32);
            s.full.paste(true);
            return d.s32.saveGui();
        }
    };

    private readonly TILE_SHEET selest = new ITileSheet()
    {
        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
        {
            s.full.init(0, s.full.body().y2(), 1, 1, 16, 1, d.s16);
            s.full.setVar(0).setSkip(2, 0);
            s.full.paste(true);
            return d.s16.saveGui();
        }
    }.get();

    private readonly TILE_SHEET buttons = new ITileSheet()
    {
        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
        {
            s.full.init(0, 0, 1, 1, 1, 1, d.s32);
            s.full.paste(true);
            return d.s32.saveGui();
        }
    }.get();

    private readonly TILE_SHEET seasons = new ITileSheet()
    {
        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
        {
            s.full.init(0, 0, 1, 1, 1, 1, d.s32);
            s.full.paste(true);
            return d.s32.saveGui();
        }
    }.get();

    public UISpecials()
    {
    }

    public SPRITE lower()
    {
        return lower;
    }

    public SPRITE upper()
    {
        return upper;
    }

    public GuiSection buildLower()
    {
        GuiSection lower = new GuiSection();
        lower.add(new GuiSection() {
            @Override
            public void render(SPRITE_RENDERER r, float ds)
            {
                if (!VIEW.s().isActive() && !VIEW.b().isActive())
                {
                    return;
                }
                super.render(r, ds);
            }
        });

        GuiSection temp = new GStat()
        {
            @Override
            public void update(GText text)
            {
                SETT.WEATHER().temp.format(text);
                if (SETT.WEATHER().temp.cold() > 0)
                    GFORMAT.colorInter(text, SETT.WEATHER().temp.cold(), 1);
                else
                    GFORMAT.colorInterInv(text, SETT.WEATHER().temp.heat(), 1);
            }
        }.hv(SETT.WEATHER().temp.info.name);
        lower.addC(temp, -100, 0);

        GuiSection humidity = new GStat()
        {
            @Override
            public void update(GText text)
            {
                GFORMAT.perc(text, SETT.WEATHER().moisture.getD());
            }
        }.hv(SETT.WEATHER().moisture.info.name);
        lower.addC(humidity, 100, 0);

        GuiSection wind = new GStat()
        {
            @Override
            public void update(GText text)
            {
                GFORMAT.perc(text, SETT.WEATHER().wind.getD());
            }
        }.hv(SETT.WEATHER().wind.info.name);
        lower.addC(wind, -100, 50);

        GuiSection growth = new GStat()
        {
            @Override
            public void update(GText text)
            {
                GFORMAT.perc(text, SETT.WEATHER().growth.getD());
            }
        }.hv(SETT.WEATHER().growth.info.name);
        lower.addC(growth, 100, 50);

        return lower;
    }

    public GuiSection buildUpper()
    {
        GuiSection upper = new GuiSection();

        GuiSection clock = new GuiSection()
        {
            @Override
            public void render(SPRITE_RENDERER r, float ds, boolean isHovered)
            {
                int off = (int)(TIME.currentSecond() * 2.0) % clockwork.size();
                int width = clockwork.size() - off;
                int x1 = body().x1();
                if (off != 0)
                {
                    TextureCoords c = clockwork.getTexture(0);
                    text.get(c.x1 + off, c.y1, width, c.y2 - c.y1);
                    CORE.renderer().renderSprite(body().x1(), body().x1() + width, body().y1() - 0, body().y2() - 0, text);
                    x1 += width;
                }
                else
                {
                    clockwork.render(r, 0, body().x1(), body().y1() - 0);
                    x1 += clockwork.size();
                }
                for (int i = 1; i < clockwork.tiles(); i++)
                {
                    clockwork.render(r, i, x1 + (i - 1) * clockwork.size(), body().y1() - 0);
                }
                if (off != 0)
                {
                    TextureCoords c = clockwork.getTexture(0);
                    text.get(c.x1, c.y1, off, c.y2 - c.y1);
                    CORE.renderer().renderSprite(body().x2() - off, body().x2(), body().y1() - 0, body().y2() - 0, text);
                }
            }
        };

        GuiSection seasonsSection = new GuiSection()
        {
            @Override
            public void render(SPRITE_RENDERER r, float ds, boolean isHovered)
            {
                renderSeasons();
            }
        };

        GuiSection selestSection = new GuiSection()
        {
            @Override
            public void render(SPRITE_RENDERER r, float ds, boolean isHovered)
            {
                int sI = TIME.light().nightIs() ? 1 : 0;
                int w = body().width() + selest.size() - 8;
                int x1 = (int)(body().x1() + 4 - selest.size() + TIME.light().partOf() * w);

                if (!render(selest.size(), selest.getTexture(sI), x1, body().y1() + 8))
                    selest.render(r, sI, x1, body().y1() + 8);
            }
        };

        upper.add(clock);
        upper.add(seasonsSection);
        upper.add(selestSection);

        return upper;
    }

    private void renderSeasons()
    {
        int pw = seasons.size();
        int width = seasons.tiles() * pw;

        int x1 = body().x1();
        int x2 = body().x2();

        int start = x1 - (int)(TIME.years().bitPartOf() * width) - pw / 2;

        int t = 2 * 4 - 2;
        while (start < x2)
        {
            TextureCoords coo = seasons.getTexture(t);

            int offX1 = 0;
            int offX2 = 0;

            if (start < x1)
            {
                offX1 = x1 - start;
            }

            if (start + pw > x2)
            {
                offX2 = start + pw - x2;
            }

            if (offX1 < pw && offX2 < pw)
            {
                text.get(coo.x1 + offX1, coo.y1, pw - (offX2 + offX1), coo.y2 - coo.y1);
                CORE.renderer().renderSprite(start + offX1, start + pw - offX2, body().y1(), body().y1() + text.height(), text);
            }

            start += pw;
            t++;
            t %= seasons.tiles();
        }
    }

    private bool render(int size, TextureCoords c, int x1, int y1)
    {
        if (x1 + size <= body().x1())
            return true;
        if (x1 >= body().x2())
            return true;
        if (x1 < body().x1())
        {
            int off = body().x1() - x1;
            text.get(c.x1 + off, c.y1, size - off, c.y2 - c.y1);
            CORE.renderer().renderSprite(body().x1(), body().x1() + (size - off), y1, y1 + size, text);
            return true;
        }
        else if (x1 + size > body().x2())
        {
            int width = body().x2() - x1;
            text.get(c.x1, c.y1, width, c.y2 - c.y1);
            CORE.renderer().renderSprite(x1, x1 + width, y1, y1 + size, text);
            return true;
        }
        return false;
    }
}