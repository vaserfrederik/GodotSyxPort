using System;
using System.IO;
using game.faction;
using init.sprite.UI;
using snake2d.util.file;
using snake2d.util.sprite.text;

namespace world.log
{
    public class LogEntry
    {
        short ii = -1;
        short fa = -1;
        short fb = -1;
        short tx;
        short ty;
        int day;
        public Str message = new Str(64);

        public LogEntry()
        {
        }

        public LogEntry(FileGetter file) : this()
        {
            ii = file.s();
            fa = file.s();
            fb = file.s();
            tx = file.s();
            ty = file.s();
            day = file.i();
            message.load(file);
        }

        public FBanner bannerA()
        {
            if (fa >= 0)
                return FACTIONS.getByIndex(fa).banner();
            return null;
        }

        public FBanner bannerB()
        {
            if (fb >= 0)
                return FACTIONS.getByIndex(fb).banner();
            return null;
        }

        public int daySinceStart()
        {
            return day;
        }

        public Icon icon()
        {
            return UI.icons().s.get(ii);
        }

        public int tx()
        {
            return tx;
        }

        public int ty()
        {
            return ty;
        }

        public void save(FilePutter file)
        {
            file.s(ii);
            file.s(fa);
            file.s(fb);
            file.s(tx);
            file.s(ty);
            file.i(day);
            message.save(file);
        }
    }
}