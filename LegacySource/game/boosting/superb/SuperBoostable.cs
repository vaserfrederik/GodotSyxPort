using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.save;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.text;

namespace game.boosting.superb
{
    public class SuperBoostable<T> where T : SuperBoostableObj
    {
        private int[] saveOrder;

        private KeyMap<SuperSpecImp<T>> map = new KeyMap<SuperSpecImp<T>>();
        private ArrayListGrower<SuperSpecImp<T>> ups = new ArrayListGrower<SuperSpecImp<T>>();
        private ArrayListGrower<SuperSpec<T>> all = new ArrayListGrower<SuperSpec<T>>();
        public readonly Boostable bo;

        public SuperBoostable(Boostable bo)
        {
            this.bo = bo;

            GAME.saver().addSpecialSaver(new Savable("BOOST_" + bo.key)
            {
                protected override void save(FilePutter file)
                {
                    file.i(ups.size());
                    foreach (SuperSpecImp<T> s in ups)
                        file.chars(s.key);
                }

                protected override void load(FileGetter file)
                {
                    int am = file.i();
                    saveOrder = Alloc.ii(am);
                    for (int i = 0; i < saveOrder.Length; i++)
                        saveOrder[i] = -1;
                    for (int i = 0; i < am; i++)
                    {
                        string k = file.chars();
                        if (map.containsKey(k))
                        {
                            saveOrder[i] = map.get(k).index;
                        }
                    }
                }
            });

            GAME.addOnViewInit(new ACTION
            {
                SuperBoostable<T> self = this;
                public void exe()
                {
                    foreach (Booster b in bo.all())
                    {
                        new SuperSpec.Wrap<T>(b, self, "", b.info, null);
                    }
                }
            });
        }

        public void clear()
        {

        }

        public SuperData makeData()
        {
            return new SuperData(this);
        }

        private int[] saveOrder()
        {
            if (saveOrder == null)
            {
                saveOrder = Alloc.ii(ups.size());
                for (int i = 0; i < saveOrder.Length; i++)
                    saveOrder[i] = i;
            }
            return saveOrder;
        }

        public void update(T t, double ds)
        {
            for (int i = 0; i < ups.size(); i++)
            {
                SuperSpecImp<T> spec = ups.get(i);
                spec.update(t, ds);
            }
        }

        public LIST<SuperSpec<T>> all()
        {
            return all;
        }

        public LIST<SuperSpecImp<T>> imps()
        {
            return ups;
        }

        public void hover(GUI_BOX box, T roy)
        {
            BHoverer.hover(box, all, roy, Dic.Boosts, bo.baseValue, false);
        }

        public void hoverDetailed(GUI_BOX box, T roy)
        {
            BHoverer.hoverDetailed(box, all, roy, Dic.Boosts, bo.baseValue, false);
        }

        public double get(T bo)
        {
            return BUtil.value(all, bo, this.bo.baseValue, 1, -100);
        }
    }
}