using System;
using System.Collections.Generic;
using System.Text;

namespace Snake2D.Util.Gui
{
    public interface GUI_BOX
    {
        GUI_BOX Title(string title);

        GUI_BOX NL();

        GUI_BOX NL(int m);

        GUI_BOX Space();

        Text Text();

        default GUI_BOX Text(string text)
        {
            return Add(Text().Set(text));
        }

        default GUI_BOX Text(string text, int maxChar)
        {
            Text t = Text();
            t.Set(text);
            t.SetMaxChars(maxChar);
            return Add(t);
        }

        GUI_BOX Add(SPRITE s);

        GUI_BOX Add(SPRITE s, int width);

        GUI_BOX Add(RENDEROBJ obj);

        bool EmptyIs();
    }
}