using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

class SpriteSkulls
{
    private TILE_SHEET small;
    private TILE_SHEET medium;
    private TILE_SHEET big;

    public SpriteSkulls(Json json) 
    {
        small = SPRITES.GAME().raw(SheetType.s1x1, "SKULL_SMALL_1X1", json);
        medium = SPRITES.GAME().raw(SheetType.s1x1, "SKULL_MEDIUM_1X1", json);
        big = SPRITES.GAME().raw(SheetType.s1x1, "SKULL_BIG_1X1", json);

        Sheet sheet = new Sheet(9 * 9 * 4, true, false)
        {
            texture = (tile) =>
            {
                // TODO Auto-generated method stub
                return null;
            },
            renderShadow = (da, x, y, it, shadow, tile, random) =>
            {
                shadow.setDistance2Ground(0);
                shadow.setHeight(4);
                this.render(x, y, DIR.C, it, shadow, random);
            },
            render = (da, x, y, it, sr, tile, random, degrade) =>
            {
                this.render(x, y, DIR.C, it, sr, random);
            }
        };
        SPRITES.GAME().add(SheetType.s1x1, new List<Sheet> { sheet }, "SKULL_MOUND_1X1");

        sheet = new Sheet(9 * 9 * 4, true, false)
        {
            texture = (tile) =>
            {
                // TODO Auto-generated method stub
                return null;
            },
            renderShadow = (da, x, y, it, shadow, tile, random) =>
            {
                shadow.setDistance2Ground(0);
                shadow.setHeight(4);
                DIR d = DIR.get(0.5 - SheetType.s2x2.dx(tile), 0.5 - SheetType.s2x2.dy(tile));
                this.render(x, y, d, it, shadow, random);
            },
            render = (da, x, y, it, sr, tile, random, degrade) =>
            {
                DIR d = DIR.get(0.5 - SheetType.s2x2.dx(tile), 0.5 - SheetType.s2x2.dy(tile));
                this.render(x, y, d, it, sr, random);
            }
        };
        SPRITES.GAME().add(SheetType.s2x2, new List<Sheet> { sheet }, "SKULL_MOUND_2X2");

        sheet = new Sheet(9 * 9 * 4, true, false)
        {
            texture = (tile) =>
            {
                // TODO Auto-generated method stub
                return null;
            },
            renderShadow = (da, x, y, it, shadow, tile, random) =>
            {
                shadow.setDistance2Ground(0);
                shadow.setHeight(4);
                DIR d = DIR.get(1 - SheetType.s3x3.dx(tile), 1 - SheetType.s3x3.dy(tile));
                this.render(x, y, d, it, shadow, random);
            },
            render = (da, x, y, it, sr, tile, random, degrade) =>
            {
                DIR d = DIR.get(1 - SheetType.s3x3.dx(tile), 1 - SheetType.s3x3.dy(tile));
                this.render(x, y, d, it, sr, random);
            }
        };
        SPRITES.GAME().add(SheetType.s3x3, new List<Sheet> { sheet }, "SKULL_MOUND_3X3");
    }

    public void render(int x, int y, DIR dir, RenderIterator it, SPRITE_RENDERER sr, int random)
    {
        int count = GAME.count().EXECUTIONS.current() / 16 + (GUTIL.ran2().get(it.tile()) & 15);

        int ran = it.ran();

        count = 28;

        if (count >= 9)
        {
            big.render(sr, ran % (big.tiles()), (int)x, (int)y);

            if (count >= 15)
            {
                medium.render(sr, ran % (big.tiles()), (int)(x + dir.xN() * 8), (int)(y + dir.yN() * 8));
            }

            if (count >= 12)
            {
                small.render(sr, ran % (big.tiles()), (int)(x + dir.xN() * 8), (int)(y + dir.yN() * 8));
            }
        }
        else if (count >= 6)
        {
            medium.render(sr, ran % (medium.tiles()), x, y);
        }
        else
        {
            small.render(sr, ran % (small.tiles()), x, y);
        }
    }
}