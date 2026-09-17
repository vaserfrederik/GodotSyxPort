using System;

namespace View.Sett.Ui.Room.Construction
{
    public class State
    {
        public int[] item;
        //public int[] upgradeMax;
        public PLACEMENT placement;
        public RoomBlueprintImp b;
        public bool refurnishing;
        public Config config;
        private int itemI;
        public RoomCategorySub collection;

        public FurnisherItemGroup problemGroup;
        public bool problemneedDoor;
        public bool problemneedArea;
        public double problemTimer;

        public State()
        {
            item = Alloc.ii(SETT.ROOMS().AMOUNT_OF_BLUEPRINTS);
            //upgradeMax = Alloc.ii(SETT.ROOMS().AMOUNT_OF_BLUEPRINTS);
            placement = SETT.ROOMS().placement;
            config = new Config(this);
        }

        public void SetItem(int itI)
        {
            itemI = CLAMP.i(itI, 0, b.constructor().pgroups().size());
            item[b.index()] = itemI;
        }

        public int Item()
        {
            return itemI;
        }

        public void Init(RoomBlueprintImp b2, bool refurnishing)
        {
            collection = null;

            config.build = true;
            this.b = b2;
            if (b2.constructor().usesArea())
                SetItem(0);
            else
                SetItem(item[b2.index()]);
            this.refurnishing = refurnishing;
            //SetUpgrade(FACTIONS.player().locks.maxUpgrade(b2), b2);
            problemGroup = null;
            problemneedDoor = false;
            problemneedArea = false;
        }

        //void SetUpgrade(int up, RoomBlueprintImp b2)
        //{
        //    int max = FACTIONS.player().locks.maxUpgrade(b2);
        //    upgradeMax[b2.index()] = max;
        //    upgrade[b2.index()] = CLAMP.i(up, 0, upgradeMax[b2.index()]);
        //}

        public void Init(RoomBlueprintImp b2, RoomCategorySub collection)
        {
            Init(b2, false);
            this.collection = collection;
        }
    }
}