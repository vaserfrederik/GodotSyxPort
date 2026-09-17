using System;
using System.Collections.Generic;
using util.keymap;
using util.text;
using snake2d.util.sets;
using init.sprite.UI;

namespace init.type
{
    public class HCLASSES
    {
        static HCLASSES()
        {
            D.gInit(new HCLASSES());
        }

        private readonly ArrayListGrower<HCLASS> all = new ArrayListGrower<HCLASS>();
        private readonly ArrayListGrower<HCLASS> allP = new ArrayListGrower<HCLASS>();

        private readonly KeyMap<HCLASS> map = new KeyMap<HCLASS>();
        private readonly HCLASS NOBLE = new HCLASS(all, allP,
                "NOBLE",
                D.g("Noble"), D.g("Nobilities"),
                D.g("NobilityD", "The Nobility are the top social layer of your kingdom. They do not work traditionally and demand a salary amongst high tier services. The rewards for having nobles around can be great however."),
                true, new ColorImp(3, 1, 19))
        {
            public override Icon icon()
            {
                return SPRITES.icons().m.noble;
            }

            public override Icon iconSmall()
            {
                return SPRITES.icons().s.noble;
            }
        };

        private readonly HCLASS CITIZEN = new HCLASS(all, allP,
                "CITIZEN",
                D.g("Plebeian"), D.g("Plebeians"),
                D.g("PlebeianD", "Plebeians are the bulk of your population and will carry out your wishes."),
                true, new ColorImp(3, 1, 19))
        {
            public override Icon icon()
            {
                return SPRITES.icons().m.citizen;
            }

            public override Icon iconSmall()
            {
                return SPRITES.icons().s.citizen;
            }
        };

        private readonly HCLASS SLAVE = new HCLASS(all, allP,
                "SLAVE",
                D.g("Slave"), D.g("Slaves"),
                D.g("SlaveD", "Slaves do mundane and hard work, but need little in return. They can not be trained into soldiers, or be educated, or replicated, but are gained through processing your captives. If you mistreat slaves they may revolt."),
                true, new ColorImp(20, 20, 8))
        {
            public override Icon icon()
            {
                return SPRITES.icons().m.slave;
            }

            public override Icon iconSmall()
            {
                return SPRITES.icons().s.slave;
            }
        };

        private readonly HCLASS OTHER = new HCLASS(all, allP,
                "OTHER",
                "Other", "Others",
                "",
                false, new ColorImp(20, 20, 8))
        {
            public override Icon icon()
            {
                return SPRITES.icons().m.citizen;
            }

            public override Icon iconSmall()
            {
                return SPRITES.icons().s.human;
            }
        };

        private readonly RMAPS<HCLASS> MAP = new RMAPS<HCLASS>("CLASS", all);

        private HCLASSES()
        {
            self = this;
        }

        private static HCLASSES self;

        public static HCLASS NOBLE()
        {
            return self.NOBLE;
        }

        public static HCLASS CITIZEN()
        {
            return self.CITIZEN;
        }

        public static HCLASS SLAVE()
        {
            return self.SLAVE;
        }

        public static HCLASS OTHER()
        {
            return self.OTHER;
        }

        public static RMAPS<HCLASS> MAP()
        {
            return self.MAP;
        }

        public static LIST<HCLASS> ALL()
        {
            return self.all;
        }

        public static LIST<HCLASS> ALLP()
        {
            return self.allP;
        }
    }
}