using System;
using System.Collections.Generic;
using game.battle;
using init.constant;
using init.race;
using init.sprite;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.file;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using snake2d.util.sprite;
using view.tool;
using view.ui.div;

namespace view.sett.ui.army
{
    internal sealed class FormationDebugPlacer : PlacableFixedImp
    {
        private const int SIZES = 5;
        private readonly Army team;
        private readonly int[] widths = Alloc.ii(SIZES);
        private readonly int[] height = Alloc.ii(SIZES);
        private Div div;
        private readonly List<Div> divs = new List<Div>(1);
        private readonly List<CLICKABLE> butts;
        private readonly UIDivEditor editor = new UIDivEditor(STATS.BATTLE().TRAINING_ALL.size(), true, true, false, RACES.all());

        public FormationDebugPlacer(Army team) : base(team == GAME.ARMIES().player() ? "Place Division" : "Place Division Enemy", 1, SIZES)
        {
            this.team = team;

            for (int i = 0; i < SIZES; i++)
            {
                double s = (i + 1) * Config.battle().MEN_PER_DIVISION / SIZES;
                int w = (int)Math.Ceiling(Math.Sqrt(s));
                int h = (int)Math.Ceiling(s / w);
                widths[i] = w;
                height[i] = h;
            }

            butts = new List<CLICKABLE>(editor);
        }

        public override SPRITE getIcon()
        {
            return SPRITES.icons().m.for_loose;
        }

        public override void place(int tx, int ty, int rx, int ry)
        {
            place(tx, ty);
        }

        public override void afterPlaced(int tx1, int ty1)
        {
            int x1 = tx1;
            int y1 = ty1;
            int x2 = x1 + width();
            x1 = x1 * C.TILE_SIZE + C.TILE_SIZEH;
            y1 = y1 * C.TILE_SIZE + C.TILE_SIZEH;
            x2 = x2 * C.TILE_SIZE + C.TILE_SIZEH;
            GAME.ARMIES().placer.deploy(divs, x1, x2, y1, y1);

            GAME.ARMIES().initAndTeleport(divs);
        }

        private void place(int tx, int ty)
        {
            if (div.menNrOf() == 0)
            {
                div.info.copyFrom(editor.div());
                div.settings().musteringSet(true);
                div.info.menSet(width() * height());
                divs.Clear();
                divs.Add(div);
            }

            if (div.menNrOf() < Config.battle().MEN_PER_DIVISION)
            {
                HTYPE t = team != GAME.ARMIES().player() ? HTYPES.ENEMY() : HTYPES.SUBJECT();
                Humanoid h = new Humanoid(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH, editor.div().race(), t, null);
                foreach (StatTraining tr in STATS.BATTLE().TRAINING_ALL)
                {
                    tr.stat.indu().setD(h.indu(), editor.div().training(tr));
                }

                foreach (EquipBattle b in STATS.EQUIP().BATTLE_ALL())
                {
                    b.stat().indu().setD(h.indu(), editor.div().equip(b));
                }

                STATS.BATTLE().COMBAT_EXPERIENCE.indu().setD(h.indu(), editor.div().experience());

                STATS.BATTLE().basicTraining.setD(h.indu(), 1.0);
                div.info.menSet(Config.battle().MEN_PER_DIVISION);
                h.setDivision(div);
            }
        }

        private bool setDiv()
        {
            div = team.getNextEmptyOrdered();
            if (div != null)
            {
                return true;
            }
            return false;
        }

        public override string placableWhole(int tx1, int ty1)
        {
            if (!setDiv())
                return E;
            return null;
        }

        public override string placable(int tx, int ty, int rx, int ry)
        {
            if (!IN_BOUNDS(tx, ty))
                return E;

            if (PATH().solidity.is(tx, ty))
            {
                return E;
            }
            if (ENTITIES().hasAtTile(tx, ty))
                return E;

            return null;
        }

        public override int width()
        {
            return widths[size()];
        }

        public override int height()
        {
            return height[size()];
        }

        public override List<CLICKABLE> getAdditionalButt()
        {
            return butts;
        }
    }
}