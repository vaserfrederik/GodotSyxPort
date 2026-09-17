using System;
using System.Collections.Generic;
using snake2d;
using util.data;
using util.gui.table;

namespace util.gui.table
{
    public abstract class GMatrix : GuiSection
    {
        private readonly int columns;
        private readonly List<Wrap> wraps = new List<Wrap>();

        public GMatrix(int rows, int columns, int entryWidth, int entryHeight)
        {
            if (columns <= 0)
                throw new Exception();

            this.columns = columns;
            GTableBuilder b = new GTableBuilder
            {
                NrOfEntries = () => (int)Math.Ceiling((double)NrOfEntries() / columns)
            };

            b.Column(null, entryWidth * columns, new GRowBuilder
            {
                Build = (ier) => new Row(ier, entryHeight)
            });

            Add(b.Create(rows, false));
        }

        public abstract RENDEROBJ Get(int i, int columnI);

        public abstract int NrOfEntries();

        public void MultiSelect(int i)
        {
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            base.Render(r, ds);
        }

        private class Row : GuiSection
        {
            private readonly GETTER<Integer> ier;

            public Row(GETTER<Integer> ier, int height)
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
                int m = ((GMatrix)this).NrOfEntries();
                for (int i = 0; i < columns && s < m; i++)
                {
                    while (s >= ((GMatrix)this).wraps.Count)
                        ((GMatrix)this).wraps.Add(new Wrap());
                    Wrap w = ((GMatrix)this).wraps[s];
                    w.Init(i, ier.Get() * columns, s++);
                    AddRight(0, w);
                }
                Body().MoveX1Y1(ox, oy);
                base.Render(r, ds);
            }
        }

        private class Wrap : CLICKABLE.ClickWrap2
        {
            private RENDEROBJ rr;

            public Wrap()
            {
            }

            private void Init(int col, int row, int i)
            {
                rr = ((GMatrix)this).Get(i, col);
            }

            protected override RENDEROBJ Get()
            {
                return rr;
            }

            public override void Render(SPRITE_RENDERER r, float ds)
            {
                base.Render(r, ds);
            }
        }
    }
}