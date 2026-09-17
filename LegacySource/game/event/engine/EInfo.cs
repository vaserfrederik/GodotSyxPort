using System;
using System.Collections.Generic;
using init.sprite;
using init.sprite.UI;
using snake2d.util.file;
using snake2d.util.sprite;

namespace game.event.engine
{
    public sealed class EInfo
    {
        public string name = "";
        public string[] messages = new string[0];
        public string desc = "";
        public string subject = "";
        public SPRITE icon = UI.icons().l.event;
        public readonly bool showRemaining;

        EInfo()
        {
            showRemaining = true;
        }

        public EInfo(Json data, Json text)
        {
            if (text != null)
            {
                name = text.text("NAME", "");
                desc = text.text("DESC", "");
                messages = text.textsTry("MESSAGE");
                subject = text.text("SUBJECT", "");
                text.has("CHOICES");
                text.checkUnused();
            }

            if (data.has("ICON"))
                icon = SPRITES.icons().get(data);
            showRemaining = data.bool("SHOW_TIME", true);

            EContext.insert.check(desc);
            EContext.insert.check(messages);
        }
    }
}