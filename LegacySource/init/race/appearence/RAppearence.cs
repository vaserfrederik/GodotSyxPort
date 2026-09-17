using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Snake2D;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using Util.SpriteComposer;
using World.Map.Regions.Centre;

public sealed class RAppearence
{
    public readonly RColors colors;
    public readonly Icon icon;
    public readonly Icon iconBig;
    public readonly TILE_SHEET sleep;
    public readonly RExtras extra;
    public readonly int off;
    public readonly LIST<string> lastNamesNoble;
    public readonly RFloors floors;

    public readonly RCrown crown;
    public readonly RType child;
    public readonly TILE_SHEET infant;
    public readonly LIST<RType> types;
    public double tMax;
    public readonly WorldRaceSheet world;

    public RAppearence(Race race, Json data, ExpandInit init, int hitboxSize) throws IOException
    {
        icon = SPRITES.icons().get(data, "ICON_SMALL");
        iconBig = SPRITES.icons().get(data, "ICON_BIG");
        floors = new RFloors(data);
        data = new Json(PATHS.RACE().init.getFolder("sprite").gets(data.value("SPRITE_FILE")));
        colors = new RColors(data);

        lastNamesNoble = RNames.names("NAMESET_FILE_NOBLE", data, init.names);

        world = new WorldRaceSheet(data.json("WORLD"));

        string s = data.value("SPRITE_EXTRA_FILE");
        if (init.extras.ContainsKey(s))
        {
            this.extra = init.extras[s];
        }
        else
        {
            this.extra = new RExtras(init.sg.getFolder("extra").get(s));
            init.extras[s] = this.extra;
        }

        string ssleep = data.value("SLEEP_FILE");
        if (init.sleep.ContainsKey(ssleep))
        {
            this.sleep = init.sleep[ssleep];
        }
        else
        {
            sleep = new ITileSheet(init.sg.getFolder("sleep").get(ssleep), 164, 44)
            {
                protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                {
                    int am = c.getSource().height / 38;
                    s.singles.init(0, 0, 1, 1, 2, am, d.s32);
                    for (int i = 0; i < am; i++)
                    {
                        s.singles.setSkip(i * 2, 2).paste(3, true);
                    }
                    return d.s32.saveGame();
                }
            }.get();

            init.sleep[ssleep] = sleep;
        }

        crown = new RCrown(init, data);

        {
            Json jchild = data.json("CHILD");

            this.child = new RType(colors, jchild, extra, init);

            if (jchild.has("INFANT_FILE"))
            {
                string key = jchild.value("INFANT_FILE");
                if (init.infants.ContainsKey(key))
                {
                    this.infant = init.infants[key];
                }
                else
                {
                    this.infant = new ITileSheet(init.sg.getFolder("infant").get(key), 352, 22)
                    {
                        protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            s.full2.init(0, 0, 8, 1, 1, 1, d.s16);

                            for (int i = 0; i < 4; i++)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    s.full2.setVar(4 + k).pasteRotated(i, true);
                                }
                                for (int k = 0; k < 4; k++)
                                {
                                    s.full2.setVar(k).pasteRotated((i + 1) % 4, true);
                                }
                            }

                            return d.s16.saveGame();
                        }
                    }.get();

                    init.infants[key] = infant;
                }
            }
            else
            {
                this.infant = TILE_SHEET.DUMMY;
            }
        }

        {
            Json[] jjs = data.jsons("TYPES", 1, 4);

            ArrayList<RType> types = new ArrayList<RType>(jjs.Length);

            foreach (Json j in jjs)
            {
                types.add(new RType(colors, j, extra, init));
            }

            this.types = types;
        }

        double bb = 0;
        foreach (RType t in types)
        {
            bb += t.spec.occurrence;
        }
        tMax = bb;

        off = (types.get(0).sheet.sheet.size() - hitboxSize) / 2;
    }

    public RType adult()
    {
        return types.get(0);
    }

    public RType child()
    {
        return child;
    }

    public RType sheet(Induvidual indu)
    {
        return indu.hType().parent() != indu.hType() ? child : types.getC(STATS.APPEARANCE().gender.get(indu));
    }

    public RType sheet(int gender)
    {
        return types.getC(gender);
    }

    public TILE_SHEET skelleton(Induvidual indu)
    {
        return sheet(indu).sheet_skelleton;
    }

    public TILE_SHEET skelleton(bool adult)
    {
        return !adult ? types.get(0).sheet_skelleton : child.sheet_skelleton;
    }

    public void renderBaby(SPRITE_RENDERER r, int cx, int cy, int rot, int ran)
    {
        int frame = (int)(ran + TIME.currentSecond() * 4.0);
        if (((frame >> 6) & 0b11) != 0)
        {
            frame = ran;
        }

        frame = frame & 0b011;
        rot *= 4;
        int x = cx - infant.size() / 2;
        int y = cy - infant.size() / 2;
        infant.render(r, frame + rot, x, y);
    }
}