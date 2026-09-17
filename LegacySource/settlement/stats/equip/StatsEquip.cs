using System;
using System.Collections.Generic;
using System.IO;
using settlement.stats.equip;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using settlement.stats.stat;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.keymap;
using util.rendering;
using util.text;

namespace settlement.stats.equip
{
    public sealed class StatsEquip : StatCollection
    {
        private readonly ArrayList<Equip> all;
        private readonly ArrayList<EquipBattle> military;
        private readonly LIST<EquipBattle> military_all;
        private readonly ArrayList<EquipRange> ammo;
        private readonly ArrayList<EquipCivic> civic;
        private readonly ArrayListGrower<EquipBattle.HumanSprite> sprites = new ArrayListGrower<EquipBattle.HumanSprite>();
        public readonly ArrayListGrower<EquipBattle> mounts = new ArrayListGrower<EquipBattle>();

        private static readonly CharSequence ¤¤name = "Equipment";
        private static readonly CharSequence ¤¤desc = "Having your subjects equip certain items can boost them in different ways. It can also improve happiness amongst them.";
        static readonly CharSequence ¤¤Level = "¤{0} Level";
        static readonly CharSequence ¤¤Target = "¤{0} Target";
        static readonly CharSequence ¤¤Level_desc = "¤The target number of items each individual should equip.";
        public static readonly CharSequence ¤¤Wear = "¤Wear-out rate per item and year:";

        public readonly Equip CLOTHES;
        public readonly RMAP<EquipBattle> militaryColl;
        public readonly RMAP<Equip> collAll;

        static StatsEquip()
        {
            D.ts(typeof(StatsEquip));
        }

        public StatsEquip(StatsInit init) : base(init, "EQUIP", ¤¤name, ¤¤desc)
        {
            LinkedList<Equip> all = new LinkedList<Equip>();

            PATH data = init.pd.GetFolder("equip");

            {
                LinkedList<EquipCivic> tmp = new LinkedList<EquipCivic>();
                PATH d = data.GetFolder("civic");
                this.CLOTHES = new EquipCivic("_CLOTHES", d, all, tmp, init);
                foreach (string k in d.GetFiles())
                {
                    new EquipCivic(k, d, all, tmp, init);
                }
                this.civic = new ArrayList<EquipCivic>(tmp);
            }

            LinkedList<EquipBattle> mil = new LinkedList<EquipBattle>();
            KeyMap<TILE_SHEET> sprite = new KeyMap<TILE_SHEET>();
            {
                LinkedList<EquipBattle> tmp = new LinkedList<EquipBattle>();
                PATH d = data.GetFolder("battle");
                foreach (string k in d.GetFiles())
                {
                    EquipBattle e = new EquipBattle("BATTLE", k, d, all, mil, init, sprite);
                    tmp.Add(e);
                }
                this.military = new ArrayList<EquipBattle>(tmp);
            }

            {
                LinkedList<EquipRange> tmp = new LinkedList<EquipRange>();
                PATH d = data.GetFolder("ranged");
                foreach (string k in d.GetFiles())
                {
                    new EquipRange(k, d, all, tmp, mil, init, sprite);
                }
                this.ammo = new ArrayList<EquipRange>(tmp);
            }

            this.military_all = new ArrayList<EquipBattle>(mil);
            KeyMap<EquipBattle> map = new KeyMap<EquipBattle>();
            foreach (EquipBattle mm in military_all)
                map.Put(mm.eKey(), mm);

            militaryColl = new RMAP<EquipBattle>("EQUIPMENT", military_all);

            this.all = new ArrayList<Equip>(all);

            collAll = new RMAP<Equip>("EQUIPMENT", this.all);

            D.t(this);

            init.updatable.Add(new StatUpdatableI()
            {
                public void Update16(Humanoid h, int updateR, bool day, int updateI)
                {
                    foreach (Equip t in all)
                    {
                        t.Update16(h, updateI, updateI, day);
                    }
                }
            });

            init.onArrival.Add(new StatInitable()
            {
                public void Init(Induvidual h)
                {
                    foreach (Equip t in all)
                    {
                        t.Set(h, t.arrivalAmount);
                    }
                }
            });

            foreach (EquipBattle e in military_all)
            {
                if (e.sprite != null)
                    sprites.Add(e.sprite);
            }

            foreach (EquipBattle e in military_all)
            {
                if (e.mount != null)
                    mounts.Add(e);
            }
        }

        public void Drop(Humanoid h)
        {
            foreach (Equip e in all)
            {
                int a = (int)Math.Round(e.stat().indu().Get(h.indu()) * RND.rFloat());
                if (a > 0)
                {
                    SETT.THINGS().resources.Create(h.physics.tileC(), e.resource(), a);
                }
            }
        }

        public LIST<Equip> AllE()
        {
            return all;
        }

        public LIST<EquipCivic> Civics()
        {
            return civic;
        }

        public LIST<EquipBattle> BattleMelee()
        {
            return military;
        }

        public LIST<EquipRange> Ranged()
        {
            return ammo;
        }

        public LIST<EquipBattle> BattleAll()
        {
            return military_all;
        }

        public void RenderExtra(Induvidual a, DIR dir, Renderer r, ShadowBatch shadow, double forward, int x, int y)
        {
            foreach (EquipBattle.HumanSprite s in sprites)
            {
                s.Render(a, r, dir, forward, x, y, shadow);
            }
        }
    }
}