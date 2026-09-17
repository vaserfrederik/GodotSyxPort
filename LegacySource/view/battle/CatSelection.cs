using System.Collections.Generic;
using settlement.main;
using settlement.room.military.artillery;
using snake2d.util.sets;

namespace view.battle
{
    public sealed class CatSelection
    {
        private readonly ArrayListResize<ArtilleryInstance> all = new ArrayListResize<ArtilleryInstance>(128);
        private readonly ArrayListResize<ArtilleryInstance> selection = new ArrayListResize<ArtilleryInstance>(128);
        private int upI = -1;

        public LIST<ArtilleryInstance> All()
        {
            if (upI != GAME.updateI())
            {
                all.clearSoft();
                selection.clearSoft();
                upI = GAME.updateI();
                foreach (ROOM_ARTILLERY cat in SETT.ROOMS().ARTILLERY)
                {
                    for (int i = 0; i < cat.instancesSize(); i++)
                    {
                        ArtilleryInstance ins = cat.getInstance(i);
                        all.add(ins);
                        if (ins.selected)
                            selection.add(ins);
                    }
                }
            }
            return all;
        }

        public void Select(ArtilleryInstance s)
        {
            if (!s.selected)
            {
                s.selected = true;
                selection.add(s);
            }
        }

        public void DeSelect(ArtilleryInstance s)
        {
            if (s.selected)
            {
                s.selected = false;
                selection.remove(s);
            }
        }

        public void Clear()
        {
            foreach (ArtilleryInstance ins in All())
            {
                ins.selected = false;
                ins.hovered = false;
            }
            selection.clearSoft();
        }

        public LIST<ArtilleryInstance> Selection()
        {
            All();
            return selection;
        }

        public bool IsClear()
        {
            return selection.size() == 0;
        }

        public void Toggle(ArtilleryInstance f)
        {
            if (f.selected)
            {
                DeSelect(f);
            }
            else
                Select(f);
        }

        public void ClearHover()
        {
            foreach (ArtilleryInstance ins in All())
            {
                ins.hovered = false;
            }
        }
    }
}