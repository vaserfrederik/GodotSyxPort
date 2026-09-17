using game.event.actions;
using game.time;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.misc;
using util.text;
using view.main;

namespace game.event.engine
{
    internal sealed class Butt : CLICKABLE.ClickableAbs
    {
        private readonly EVENT_HANDLER e;

        private static readonly CharSequence ¤¤timeRemaining = "Time Remaining";

        static Butt()
        {
            D.ts(typeof(Butt));
        }

        public Butt(EVENT_HANDLER e) : base(64, 48)
        {
            this.e = e;
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            if (e.Current != null)
            {
                GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
                e.Current.Info.Icon.RenderC(r, body.cX(), body.cY());
                GButt.ButtPanel.renderFrame(r, body);
            }
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            if (e.Current == null)
                return;
            GBox b = (GBox)text;
            b.Title(e.Current.Info.Name);
            b.Text(e.Current.Info.Desc);
            b.NL();
            if (e.Current.Info.ShowRemaining && e.Current.Duration.Seconds > 0)
            {
                b.Text(¤¤timeRemaining);
                double t = e.Current.Duration.Seconds - e.TimeElapsed();
                int days = (int)(t / TIME.secondsPerDay());
                GText te = b.Text();
                if (days > 0)
                {
                    DicTime.SetDays(te, days);
                    b.Add(te);
                    te = b.Text();
                }
                t -= days * TIME.secondsPerDay();
                DicTime.SetHours(te, t / TIME.secondsPerHour());
                b.Add(te);
                te = b.Text();
                b.NL();
            }
            b.NL(8);
            foreach (EventAction a in e.Current.On_spawn)
                if (!a.HideUI)
                    a.Hover(b, e.Current, e.Context());

            b.NL();
            foreach (EventAction a in e.Current.On_spawn)
            {
                if (a.HideUI)
                    continue;
                CharSequence s = a.Problem(e.Current, e.Context());
                if (s != null)
                {
                    b.Error(s);
                    b.NL();
                }
            }
        }

        protected override void clickA()
        {
            if (e.Context == null || e.Mess == null)
                return;
            VIEW.Messages().Reopen(e.Mess());
            base.clickA();
        }
    }
}