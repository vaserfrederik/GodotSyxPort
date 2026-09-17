using System;
using System.Collections.Generic;
using System.IO;
using init.paths;
using init.race.appearence;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace init.race
{
    public sealed class ExpandInit
    {
        public readonly KeyMap<Tuple<Icon, Icon>> icons = new KeyMap<Tuple<Icon, Icon>>();
        public readonly KeyMap<RaceSheet> map = new KeyMap<RaceSheet>();
        public readonly KeyMap<RExtras> extras = new KeyMap<RExtras>();
        public readonly KeyMap<RaceSheet> children = new KeyMap<RaceSheet>();
        public readonly KeyMap<string[]> names = new KeyMap<string[]>();
        public readonly KeyMap<TILE_SHEET> skelletons = new KeyMap<TILE_SHEET>();
        public readonly KeyMap<TILE_SHEET> portraits = new KeyMap<TILE_SHEET>();
        public readonly KeyMap<TILE_SHEET> sleep = new KeyMap<TILE_SHEET>();
        public readonly KeyMap<TILE_SHEET> infants = new KeyMap<TILE_SHEET>();
        public readonly KeyMap<TILE_SHEET> crowns = new KeyMap<TILE_SHEET>();
        public readonly KeyMap<KingMessages> kmessagess = new KeyMap<KingMessages>();

        public readonly PATH p = PATHS.INIT().getFolder("race");
        public readonly PATH pt = PATHS.TEXT().getFolder("race");
        public readonly PATH sg = PATHS.SPRITE().getFolder("race");

        public readonly RaceFrameMaker fm = new RaceFrameMaker();

        public ExpandInit() throws IOException
        {
            // Constructor implementation
        }
    }
}