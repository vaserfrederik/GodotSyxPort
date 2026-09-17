using game.boosting;
using init.sprite.UI;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.text;

namespace game.boosting
{
    public class BoostableCat
    {
        public static class All
        {
            public readonly BoostableCat WORLD_CIVICS = new BoostableCat("WORLD_", Dic.¤¤World + ": " + Dic.¤¤Civics, "", TYPE_WORLD, UI.icons().s.world);
            public readonly BoostableCat WORLD_PRODUCTION = new BoostableCat("WORLD_", Dic.¤¤World + ": " + Dic.¤¤Production, "", TYPE_WORLD, UI.icons().s.world);
            public readonly BoostableCat WORLD = new BoostableCat("WORLD_", Dic.¤¤World, "", TYPE_WORLD, UI.icons().s.world);
            public readonly BoostableCat RELIGION = new BoostableCat("RELIGION_", ¤¤conversion, "", TYPE_WORLD | TYPE_SETT, UI.icons().s.shrine);
            public readonly BoostableCat WORLD_DUMP = new BoostableCat("WORLD_", Dic.¤¤World + ": " + Dic.¤¤Misc, "", TYPE_CRAP, UI.icons().s.world);
        }

        public const int TYPE_CRAP = 0b0001;
        public const int TYPE_WORLD = 0b0010;
        public const int TYPE_SETT = 0b0100;

        public readonly string prefix;
        public readonly ICharSequence name;
        public readonly ICharSequence desc;
        public readonly SPRITE icon;
        public readonly int typeMask;
        private readonly ArrayListGrower<Boostable> all = new ArrayListGrower<Boostable>();
        private static ICharSequence ¤¤conversion = "¤Conversion";
        static
        {
            D.ts(typeof(BoostableCat));
        }

        public BoostableCat(string prefix, ICharSequence name, ICharSequence desc, int typeMask, SPRITE icon)
        {
            this.prefix = prefix;
            this.name = name;
            this.desc = desc;
            this.typeMask = typeMask;
            this.icon = icon;
        }

        public LIST<Boostable> AllBoostables()
        {
            return all;
        }

        private static All al;

        public static void Init()
        {
            al = new All();
        }

        public static All ALL()
        {
            return al;
        }

        public readonly BOOLEANO<BoostSpec> filter = new BOOLEANO<BoostSpec>(t => t.boostable.cat == this);
    }
}