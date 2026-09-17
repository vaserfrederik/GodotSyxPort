using System;
using System.Collections.Generic;
using System.Linq;
using game.faction;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.room.service.module;
using settlement.room.water;
using settlement.tilemap.ground;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.main.furnisher
{
    public abstract class FurnisherStat : INDEXED, RoomBoost
    {
        protected readonly int index;
        private readonly string name;
        protected string desc;
        public readonly double min;
        private readonly INFO info;

        private static readonly string ¤¤Services = "Services";
        private static readonly string ¤¤serviceDesc = "Total amount of people that can be served simultaneously. The other (number) is an estimate of how many subjects the room will be able to serve, derived from your subjects' properties.";
        private static readonly string ¤¤productionD = "Estimation of daily output of resources.";
        private static readonly string ¤¤Efficiency = "Efficiency";
        private static readonly string ¤¤EfficiencyD = "Efficiency is increased by certain items and can increase the usefulness of the room.";
        private static readonly string ¤¤employeesD = "The amount of subjects needed to operate this room. The room might require less or more workers depending on circumstances.";
        private static readonly string ¤¤wdesc = "Moisture is essential to all growing things. Most land has some moisture to begin with. This can be improved by fresh water access from irrigation or natural bodies of water at a later stage.";

        static FurnisherStat()
        {
            D.ts(typeof(FurnisherStat));
        }

        public FurnisherStat(Furnisher furnisher, string name, string desc, double min)
        {
            this.index = furnisher.stats.add(this);
            this.name = name;
            this.desc = desc;
            info = new INFO(name, desc);
            this.min = min;
        }

        public FurnisherStat(Furnisher furnisher)
        {
            if (Furnisher.jsonStat.Length == furnisher.stats.size())
            {
                throw new DataError("invalid number of stats have been declared");
            }

            this.name = Furnisher.jsonStat[furnisher.stats.size()].text("NAME");
            this.desc = Furnisher.jsonStat[furnisher.stats.size()].text("DESC");
            this.index = furnisher.stats.add(this);
            info = new INFO(name, desc);
            this.min = 0;
        }

        public FurnisherStat(Furnisher furnisher, double min)
        {
            if (Furnisher.jsonStat.Length == furnisher.stats.size())
            {
                throw new DataError("invalid number of stats have been declared");
            }

            this.name = Furnisher.jsonStat[furnisher.stats.size()].text("NAME");
            this.desc = Furnisher.jsonStat[furnisher.stats.size()].text("DESC");
            this.index = furnisher.stats.add(this);
            info = new INFO(name, desc);
            this.min = min;
        }

        public string name()
        {
            return name;
        }

        public string desc()
        {
            return desc;
        }

        public INFO info()
        {
            return info;
        }

        public abstract GText format(GText t, double value);

        public double get(AREA area, double[] fromItems)
        {
            return get(area, fromItems[index]);
        }

        public abstract double get(AREA area, double acc);

        public double get(RoomInstance i)
        {
            return get(i.area(), i.stats());
        }

        public double get(RoomInstance i, double[] fromItems)
        {
            return get(i.area(), fromItems);
        }

        public int index()
        {
            return index;
        }

        public void appendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
        {
            section.addRelBody(4, DIR.S, new GStat()
            {
                text = t => format(t, get((RoomInstance)get.get())).toString()
            }.hh(name).hoverInfoSet(desc));
        }

        public void appendManageScr(GGrid icons, GGrid text, GuiSection sExtra)
        {
            icons.NL();
            icons.add(new GStat()
            {
                text = t => format(t, getStat(index())).toString()
            }.decrease().hh(info));
        }

        public void hover(GBox box, Room i, int rx, int ry)
        {
            box.NL();
            box.text(name);
            box.add(format(box.text(), get((RoomInstance)i)));
            box.NL();
        }

        public UIRoomModule applier(RoomBlueprintIns<?> blue)
        {
            return new UIRoomModule()
            {
                appendPanel = (section, get, x1, y1) =>
                {
                    section.addRelBody(4, DIR.S, new GStat()
                    {
                        text = t => format(t, get((RoomInstance)get.get())).toString()
                    }.hh(name).hoverInfoSet(desc));
                },
                appendManageScr = (icons, text, sExtra) =>
                {
                    icons.NL();
                    icons.add(new GStat()
                    {
                        text = t => format(t, blue.getStat(index())).toString()
                    }.decrease().hh(info));
                },
                hover = (box, i, rx, ry) =>
                {
                    box.NL();
                    box.text(name);
                    box.add(format(box.text(), get((RoomInstance)i)));
                    box.NL();
                }
            };
        }

        protected abstract double getBase(AREA area, double[] fromItems);

        protected abstract double getBase();

        private static class DataError : Exception
        {
            public DataError(string message) : base(message) { }
        }
    }
}