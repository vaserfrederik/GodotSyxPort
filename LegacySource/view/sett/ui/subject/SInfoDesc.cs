using System;
using System.Collections.Generic;
using init.race.bio;
using init.sprite.UI;
using init.type;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.table;
using util.gui.table.GTableBuilder;

namespace view.sett.ui.subject
{
    final class SInfoDesc : GuiSection
    {
        private readonly AInfo ss;
        private Str str = new Str(1024);
        private int lines = 0;
        private readonly int width = UISubjectInfo.width - 48;

        private int[] starts = Alloc.ii(128);
        private int[] ends = Alloc.ii(128);
        private COLOR[] cols = new COLOR[128];

        private readonly Font font = UI.FONT().M;

        private readonly LIST<Str> impr = new ArrayList<Str>
        {
            new Str(64),
            new Str(64),
            new Str(64),
            new Str(64)
        };

        public SInfoDesc(AInfo ss, int height)
        {
            this.ss = ss;

            for (int i = 0; i < cols.Length; i++)
                cols[i] = COLOR.WHITE100;
            GTableBuilder b = new GTableBuilder
            {
                NrOFEntries = () => lines
            };

            int li = (height - 10) / font.Height();
            b.Column(null, width, new GRowBuilder
            {
                Build = ier => new RENDEROBJ.RenderImp(width, font.Height)
                {
                    Render = (r, ds) =>
                    {
                        int s = starts[ier.Get()];
                        int e = ends[ier.Get()];
                        cols[ier.Get()].Bind();
                        font.Render(r, str, Body().X1(), Body().Y1(), s, e, 1.0);
                        COLOR.Unbind();
                    }
                }
            });

            Body().SetDim(16, 1);

            Add(b.Create(li, false), 16, 0);
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            if (ss.a == null)
                return;

            str.Clear();

            int end = 0;
            lines = 0;

            if (ss.a.Indu().Clas() == HCLASSES.CITIZEN())
            {
                str.Add(ss.a.Race().Bio().OpinionTitle(ss.a));
                end = SetColor(end, COLOR.WHITE100);
                str.NL();

                ss.a.Race().Bio().Opinions(impr, ss.a);
                foreach (CharSequence s in impr)
                {
                    if (s.Length > 0)
                    {
                        str.S(4).Add('-').S();
                        str.Add(s);
                        str.NL();
                    }
                }
                end = SetColor(end, GCOLOR.T().WARNING);
                str.NL();
            }

            bool nl = false;
            foreach (BioLine d in ss.a.Race().Bio().Lines())
            {
                CharSequence s = d.Get(ss.a);
                if (s != null)
                {
                    if (nl)
                        for (int i = 0; i < 4; i++)
                            str.S();

                    str.Add(s);
                    nl = d.Nl();
                    if (nl)
                        str.NL();
                    else
                        str.S();
                }
            }
            end = SetColor(end, COLOR.WHITE100);
            str.NL();

            base.Render(r, ds);
        }

        private int SetColor(int end, COLOR color)
        {
            while (end < str.Length() && lines < starts.Length)
            {
                int start = font.GetStartIndex(str, end);
                end = font.GetEndIndex(str, start, width);
                starts[lines] = start;
                ends[lines] = end;
                lines++;
                cols[lines] = color;
            }
            return end;
        }
    }
}