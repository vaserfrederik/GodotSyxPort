using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.table;

namespace util.gui.table
{
    public abstract class GMatrixDraggable : GuiSection
    {
        private readonly int columns;
        private readonly ArrayListGrower<Wrap> wraps = new ArrayListGrower<Wrap>();
        private Wrap toMove = null;
        private Wrap toMoveTo = null;

        public GMatrixDraggable(int rows, int columns, int entryWidth, int entryHeight)
        {
            if (columns <= 0)
                throw new RuntimeException();

            this.columns = columns;
            GTableBuilder b = new GTableBuilder
            {
                NrOFEntries = () => (int)Math.Ceiling((double)NrOFEntries() / columns)
            };

            b.Column(null, entryWidth * columns, new GRowBuilder
            {
                Build = ier => new Row(ier, entryHeight)
            });

            Add(b.Create(rows, false));
        }

        public abstract RENDEROBJ Get(int i, int columnI);
        public abstract int NrOFEntries();
        public void MultiSelect(int i) { }
        public abstract void Move(int oldI, int newI);

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            if (toMove != null)
            {
                if (!MButt.LEFT.IsDown())
                {
                    if (toMoveTo != null && toMove != toMoveTo)
                        Move(toMove.i, toMoveTo.i);

                    toMove = null;
                    toMoveTo = null;
                }
            }

            base.Render(r, ds);
        }

        private class Row : GuiSection
        {
            private readonly GETTER<int> ier;

            public Row(GETTER<int> ier, int height)
            {
                this.ier = ier;
                Body().SetHeight(height);
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                int ox = Body().X1();
                int oy = Body().Y1();
                Clear();

                int s = ier.Get() * columns;
                int m = NrOFEntries();
                for (int i = 0; i < columns && s < m; i++)
                {
                    while (s >= wraps.Size)
                        wraps.Add(new Wrap());

                    Wrap w = wraps.Get(s);
                    w.Init(i, ier.Get() * columns, s++);
                    AddRight(0, w);
                }

                Body().MoveX1Y1(ox, oy);
                base.Render(r, ds);
            }
        }

        private class Wrap : CLICKABLE.ClickWrap2
        {
            private int i;
            private RENDEROBJ rr;

            public Wrap() { }

            private void Init(int col, int row, int i)
            {
                this.i = i;
                rr = ((GMatrixDraggable)Parent).Get(i, col);
            }

            protected override RENDEROBJ Get() => rr;

            public override bool Click()
            {
                toMove = this;
                return base.Click();
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                bool hov = HoveredIs();

                base.Render(r, ds);

                if (toMove == this)
                {
                    COLOR.WHITE85.Render(r, Body().X1(), Body().X1() + 2, Body().Y1(), Body().Y2());
                }
                else if (toMove != null && hov)
                {
                    GCOLOR.UI().GOOD.Hovered.Render(r, Body().X1(), Body().X1() + 2, Body().Y1(), Body().Y2());
                    toMoveTo = this;
                }
            }
        }
    }
}