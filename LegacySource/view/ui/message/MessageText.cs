using System;
using System.Collections.Generic;
using util.gui.misc;
using util.gui.table;
using snake2d.util.file;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace view.ui.message
{
    public class MessageText : Message
    {
        private static readonly long serialVersionUID = 1L;
        private string[] paragraphs = new string[0];

        public MessageText(ICharSequence title) : base(title)
        {
        }

        public MessageText(ICharSequence title, ICharSequence body) : this(title)
        {
            Paragraph(body);
        }

        public MessageText(Json json) : this(json.Text("TITLE"))
        {
            if (json.Has("PARAGRAPHS"))
            {
                foreach (string s in json.Texts("PARAGRAPHS"))
                    Paragraph(s);
            }
            else if (json.Has("MESSAGE"))
            {
                Paragraph(json.Text("MESSAGE"));
            }
        }

        public MessageText Paragraph(ICharSequence text)
        {
            string[] ps = new string[paragraphs.Length + 2];
            for (int i = 0; i < paragraphs.Length; i++)
            {
                ps[i] = paragraphs[i];
            }
            ps[ps.Length - 2] = " ";
            ps[ps.Length - 1] = text.ToString();
            paragraphs = ps;
            return this;
        }

        protected override RENDEROBJ MakeSection()
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
            Font f = UI.FONT().M;

            int mw = 0;

            foreach (string body in paragraphs)
            {
                int ei = 0;
                while (ei < body.Length)
                {
                    int n = f.GetEndIndex(body, ei, WIDTH);
                    GTextR t = new GTextR(f, body.Substring(ei, n - ei));
                    mw = Math.Max(mw, t.Body().Width());
                    rows.Add(t);

                    n = f.GetStartIndex(body, n);
                    ei = f.GetStartIndex(body, n);
                }
            }

            if (rows.Count * f.Height() < HEIGHT)
            {
                GuiSection s = new GuiSection();
                foreach (RENDEROBJ r in rows)
                    s.AddDown(0, r);
                return s;
            }

            rows.Add(new RENDEROBJ.RenderDummy(mw + 16, 1));
            return new GScrollRows(rows, HEIGHT).View();
        }
    }
}