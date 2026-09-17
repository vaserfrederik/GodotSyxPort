using System;
using System.IO;
using System.Text;
using game.boosting;
using game.boosting;
using game.boosting;
using game.boosting;
using game.boosting;
using init.paths;
using init.sprite;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.file;
using util.info;
using util.keymap;

namespace init.religion
{
    public sealed class Religion : MAPPED
    {
        private readonly int index;
        public readonly string key;
        public readonly COLOR color;
        public readonly INFO info;
        public readonly string diety;
        private double[] liking;
        public readonly Icon icon;
        public readonly double inclination;
        public readonly Boostable conversionCity;
        public readonly BoostSpecs boosts;

        public Religion(string key, int index) : base()
        {
            this.key = key;
            this.index = index;
            Json d = Json();
            Json t = new Json(PATHS.TEXT().getFolder("religion").gets(key));
            info = new INFO(t);

            diety = t.text("DEITY");

            color = new ColorImp(d);
            icon = SPRITES.icons().get(d);
            inclination = d.d("DEFAULT_SPREAD");

            conversionCity = BOOSTING.push(key + "_CITY", 1, info.name, info.desc, icon, BoostableCat.ALL().RELIGION);
            boosts = new BoostSpecs(info.name, icon, false);
            boosts.read(d, BValue.VALUE1);
        }

        private Json Json()
        {
            return new Json(PATHS.INIT().getFolder("religion").gets(key));
        }

        public double opposition(Religion other)
        {
            return liking[other.index()];
        }

        void init()
        {
            liking = new double[RELIGIONS.ALL().size()];
            RELIGIONS.MAP().readFill("OPPOSITION", liking, Json(), 0, 100);
        }

        public override int index()
        {
            return index;
        }

        public override string ToString()
        {
            return "[" + index + "]" + key;
        }

        public override string key()
        {
            return key;
        }
    }
}