using System.Collections.Generic;
using settlement.entity.humanoid;
using settlement.main;
using snake2d;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using view.interrupter;
using view.keyboard;
using view.main;

namespace view.sett.ui.subject
{
    public sealed class UISubject
    {
        private readonly UISubjectType[] all;
        private UISubjectType current;
        private AInfo a = new AInfo();
        private readonly Str title = new Str(24);
        private readonly ISidePanel panel;

        public UISubject()
        {
            all = new UISubjectType[HTYPES.ALL().Size()];
            foreach (HTYPE t in HTYPES.ALL())
            {
                all[t.Index()] = new UISubjectType(a, t);
            }

            GuiSection section = new GuiSection
            {
                Render = (r, ds) =>
                {
                    if (a.a == null || a.a.IsRemoved())
                    {
                        VIEW.s().Panels.Remove(panel);
                        return;
                    }
                    if (a.a.Indu().HType() != current.Type)
                    {
                        Activate(a.a, false);
                        return;
                    }

                    base.Render(r, ds);

                    if (a.a != null)
                    {
                        SETT.OVERLAY().Add(a.a);

                        if (KEYS.MoveDown())
                            a.follow--;
                        if (a.follow > 0)
                            VIEW.s().Window.Centerer.Set(a.a.Body().CX(), a.a.Body().CY());
                    }
                }
            };
            panel = new ISidePanel(section);

            new SPortraitsDebug();
        }

        public void Activate(Humanoid a, bool disturb)
        {
            this.a.a = a;
            current = all[a.Indu().HType().Index()];
            panel.Section().Clear();
            panel.Section().Add(current);
            this.a.follow = 20;

            title.Clear();
            title.Add(a.Race().Info.NamePosessive).Add(' ').Add(a.Indu().HType().Name);
            panel.TitleSet(title);
            VIEW.s().Panels.Add(panel, disturb);
            VIEW.s().Window.Centerer.Set(a.Body().CX(), a.Body().CY());
        }

        public Humanoid Showing()
        {
            return VIEW.s().Panels.Added(panel) ? a.a : null;
        }
    }
}