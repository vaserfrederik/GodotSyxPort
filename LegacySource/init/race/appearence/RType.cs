using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Init.Race.Appearance
{
    public sealed class RType
    {
        public readonly RTypeSpec Spec;
        public readonly RPortrait Portrait;
        public readonly RNames Names;
        public readonly RaceSheet Sheet;
        public readonly ITileSheet SheetSkelleton;

        public readonly IList<RAddon> AddonsBelow;
        public readonly IList<RAddon> AddonsAbove;

        public RType(RColors colors, Json json, RExtras extra, ExpandInit init) 
        {
            string ssprite = json.Value("SPRITE_FILE");
            {
                if (init.Map.ContainsKey(ssprite))
                {
                    Sheet = init.Map[ssprite];
                }
                else
                {
                    Sheet = new RaceSheet(init.Sg.Get(ssprite));
                    init.Map.Add(ssprite, Sheet);
                }
            }

            {
                string sprite = json.Value("SPRITE_SKELLETON_FILE");
                if (!init.Skelletons.ContainsKey(sprite))
                {
                    SheetSkelleton = new ITileSheet(init.Sg.GetFolder("skelleton").Get(sprite), 316, 120)
                    {
                        protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                        {
                            s.Singles.Init(0, 0, 1, 1, 4, 3, d.S32);
                            int a = 6;
                            for (int i = 0; i < a; i++)
                            {
                                s.Singles.SetSkip(i * 2, 2).Paste(3, true);
                            }
                            return d.S32.SaveGame();
                        }
                    }.Get();
                    init.Skelletons.Add(sprite, SheetSkelleton);
                }
                else
                    SheetSkelleton = init.Skelletons[sprite];
            }

            Spec = new RTypeSpec(colors, json);
            Portrait = new RPortrait(init, colors, json);
            Names = new RNames(json, init.Names);

            LinkedList<RAddon> below = new LinkedList<RAddon>();
            LinkedList<RAddon> above = new LinkedList<RAddon>();

            if (json.Has("ADDONS"))
            {
                new ComposerThings.IInit(init.Sg.Get(ssprite), 448, 546);
                RAddon[] done = new RAddon[8];

                foreach (Json j in json.Jsons("ADDONS"))
                {
                    if (j.Bool("BELOW_HEAD"))
                        below.Add(new RAddon(j, colors, done));
                    else
                        above.Add(new RAddon(j, colors, done));
                }
            }

            this.AddonsAbove = new List<RAddon>(above);
            this.AddonsBelow = new List<RAddon>(below);
        }

        public class RTypeSpec
        {
            public readonly double Occurrence;
            public readonly ColorCollection Skin;
            public readonly ColorCollection Leg;

            public RTypeSpec(RColors colors, Json json)
            {
                Occurrence = json.Has("OCCURRENCE") ? json.D("OCCURRENCE") : 0.5;
                Skin = colors.Collection.Read("COLOR_SKIN", json);
                Leg = colors.Collection.Read("COLOR_LEG", json);
            }
        }
    }
}