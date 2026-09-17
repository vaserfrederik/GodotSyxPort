using System;
using System.Collections.Generic;
using game;
using game.battle;
using snake2d.util.sets;
using util.text;

namespace view.battle
{
    public sealed class DivSelection
    {
        public static string ¤¤MusterOneProblem = "¤One or more divisions do not have a position. Set a position by clicking the division, then click and drag on the ground where you want them.";
        public static string ¤¤MusterProblem = "¤The division do not have a position. Set a position by clicking the division, then click and drag on the ground where you want them.";

        static DivSelection()
        {
            D.ts(typeof(DivSelection));
        }

        private readonly bool[] selected = new bool[Armies.DIVISIONS];
        private readonly ArrayList<Div> selection = new ArrayList<Div>(Armies.DIVISIONS);
        private readonly bool[] hovered = new bool[Armies.DIVISIONS];
        private readonly DivFormationImp tmp = new DivFormationImp();

        public void Select(Div f)
        {
            if (selected[f.Index()])
                return;
            selection.Add(f);
            selected[f.Index()] = true;
        }

        public void DeSelect(Div f)
        {
            if (!selected[f.Index()])
                return;
            selection.Remove(f);
            selected[f.Index()] = false;
        }

        public bool Selected(Div f)
        {
            return selected[f.Index()];
        }

        public void SToggle(Div f)
        {
            if (!selected[f.Index()])
                Select(f);
            else
                DeSelect(f);
        }

        public void Clear()
        {
            selection.ClearSloppy();
            for (int i = 0; i < selected.Length; i++)
            {
                selected[i] = false;
            }
            artillery.Clear();
        }

        public LIST<Div> Selection()
        {
            return selection;
        }

        public int AllSelected()
        {
            return selection.Size() + artillery.Selection().Size();
        }

        public bool IsClear()
        {
            return selection.Size() == 0 && artillery.IsClear();
        }

        public void Toggle(Div f)
        {
            if (Selected(f))
            {
                DeSelect(f);
            }
            else
                Select(f);
        }

        public bool Hovered(Div d)
        {
            return hovered[d.Index()];
        }

        public void Hover(Div d)
        {
            hovered[d.Index()] = true;
        }

        public void ClearHover()
        {
            for (int i = 0; i < hovered.Length; i++)
            {
                hovered[i] = false;
            }
            artillery.ClearHover();
        }

        public readonly CatSelection artillery = new CatSelection();

        public int Destinations()
        {
            int i = 0;
            foreach (Div d in Selection())
            {
                if (d.MenNrOf() > 0)
                {
                    d.Order().Dest.Get(tmp);
                    if (tmp.Deployed() > 0)
                        i++;
                }
            }
            return i;
        }

        public string MusterProblem()
        {
            int i = Selection().Size() - Destinations();
            if (i > 1)
            {
                return ¤¤MusterProblem;
            }
            else if (i > 0)
            {
                return ¤¤MusterOneProblem;
            }
            return null;
        }

        public bool ShouldMuster()
        {
            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                if (d.MenNrOf() > 0 && !d.Settings().Muster())
                    return true;
            }
            return false;
        }
    }
}