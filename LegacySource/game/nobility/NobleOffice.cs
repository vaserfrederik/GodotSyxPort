using System;
using System.Collections.Generic;
using game.boosting;
using game.nobility;
using init.sprite.UI;
using init.type;
using settlement.room.main;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.info;

namespace game.nobility
{
    public abstract class NobleOffice
    {
        public readonly string name;
        public readonly string desc;
        public readonly BoostSpecs boosts;
        public readonly double add;
        public readonly Boostable target;
        public readonly Icon icon;
        public readonly int index;
        public bool special;

        protected NobleOffice(ArrayListGrower<NobleOffice> all, double add, Boostable target, string name, string desc, Icon icon)
        {
            this.add = add;
            this.target = target;
            this.name = name;
            this.desc = desc;
            boosts = new BoostSpecs(HCLASSES.NOBLE().name, UI.icons().s.noble, false);
            boosts.push(target, add, false);
            this.icon = icon;
            this.index = all.add(this);
        }

        public abstract double value(int slots);

        public abstract int popBoosted(int slots);

        public abstract void hoverValue(GBox b, int slots);

        public int allocated(Noble n)
        {
            if (n.office() == this)
                return 1 + 4 * n.rank();
            return 0;
        }

        public RoomBlueprintIns<?> room()
        {
            return null;
        }

        public bool leavesMap()
        {
            return false;
        }

        public void hover(GUI_BOX box)
        {
            GBox b = (GBox)box;
            b.title(name);
            b.text(desc);
            b.NL(4);
            b.add(target.icon);
            b.textL(target.name);
            b.tab(6);
            double d = value(1);
            d *= add;
            b.add(GFORMAT.f0(b.text(), d, 4));
        }
    }
}