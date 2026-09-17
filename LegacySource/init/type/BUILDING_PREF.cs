using System;
using System.Collections.Generic;
using Init.Paths;
using Init.Race;
using Util.Info;
using Util.Keymap;
using Util.Sets;
using Util.Sprite;

namespace Init.Type
{
    public abstract class BuildingPref : Mapped
    {
        public string Name { get; private set; }
        private readonly int index;
        public readonly string key;
        public readonly double[] defaultPref;

        protected BuildingPref(string key, Liste<BuildingPref> all)
        {
            Name = new Info(new Json(PATHS.TEXT_SETTLEMENT().GetFolder("structure").Gets(key))).Name;
            index = all.Add(this);
            this.key = key;
            RACES.Map().ReadFill("PREFERENCE", defaultPref, new Json(PATHS.INIT_SETTLEMENT().GetFolder("structure").Gets(key)), 0, 1);
        }

        public override int Index()
        {
            return index;
        }

        public abstract Sprite Icon();

        public override string Key()
        {
            return key;
        }
    }
}