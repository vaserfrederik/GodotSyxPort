using System;
using System.Collections.Generic;
using System.Linq;
using Snake2D;
using Util.Colors;
using Util.Gui.Misc;
using Util.Gui.Slider;
using Util.Info;
using Util.Text;
using View.Sett.Ui.Room.Priority.Filter;

namespace View.Sett.Ui.Room.Priority
{
    class Row : GuiSection
    {
        private static string ¤¤left = "increase priority of all filtered work-groups in the row";
        private static string ¤¤right = "decrease priority of all filtered work-groups in the row";
        private static string ¤¤MasterPrio = "¤Master Priority";
        private static string ¤¤priority = "Priority:";
        private static string ¤¤highest = "highest";
        private static string ¤¤lowest = "lowest";
        private static string ¤¤ban = "ban";
        private static string ¤¤banD = "This work group is banned.";
        private static string ¤¤employedTarget = "employed target";
        private static string ¤¤workForce = "work force";
        private static string ¤¤employed = "employed";
        private static string ¤¤Total = "Total";

        private readonly RoomEmployment _emp;
        private readonly Filter.Filter _filter;
        private readonly List<Card> _cards = new List<Card>();
        private Card _dragging;
        private Chunk _draggingChunk;

        private const int EW = 100;
        private const int HH = 120;
        private const int MM = 10;

        static Row()
        {
            TextDomain.Add("view/sett/ui/room/priority/row");
        }

        public Row(RoomEmployment emp, Filter.Filter filter)
        {
            _emp = emp;
            _filter = filter;

            var leftButton = new GButt.ButtPanel(UI.IconSmall("left"))
            {
                ToolTip = T(¤¤left)
            };
            leftButton.OnClick += () =>
            {
                foreach (var card in _cards)
                {
                    if (_filter.Active(card.G))
                    {
                        _emp.Priorities.Add(card.E, Math.Max(0, _emp.Priorities[card.E] - 1));
                    }
                }
            };
            Add(leftButton, 0, 0);

            var rightButton = new GButt.ButtPanel(UI.IconSmall("right"))
            {
                ToolTip = T(¤¤right)
            };
            rightButton.OnClick += () =>
            {
                foreach (var card in _cards)
                {
                    if (_filter.Active(card.G))
                    {
                        _emp.Priorities.Add(card.E, Math.Min(_emp.MaxPriority, _emp.Priorities[card.E] + 1));
                    }
                }
            };
            Add(rightButton, leftButton.Body.X2, 0);

            var header = new Header(_emp);
            Add(header, rightButton.Body.X2, 0);

            for (int i = 0; i <= _emp.MaxPriority; i++)
            {
                var chunk = new Chunk(i);
                Add(chunk, rightButton.Body.X2 + header.Body.Width + i * EW, 0);
                _draggingChunk = chunk;
            }

            foreach (var group in _filter.All())
            {
                var card = new Card((WGroup)group.Object, group.Sprite);
                _cards.Add(card);
                Add(card, 0, 0);
            }

            UpdateChunks();
        }

        private void UpdateChunks()
        {
            foreach (var chunk in All<Chunk>())
            {
                chunk.Clear();
                foreach (var card in _cards)
                {
                    if (_filter.Active(card.G) && _emp.Priorities[card.E] == chunk.Priority)
                    {
                        chunk.Add(card, 0, 0);
                    }
                }
            }
        }

        private class Header : GuiSection
        {
            public Header(RoomEmployment emp)
            {
                Add(emp.Blueprint().IconBig(), 0, 0);

                AddDownC(0, new GTarget(20, false, true, emp.Priority));

                var hh = new Sprite(60, 14)
                {
                    Render = (r, x1, x2, y1, y2) =>
                    {
                        double dd = 1;
                        double n = emp.NeededWorkers();
                        GMeter.GMeterCol cc = GMeter.C.Green;
                        if (n > 0)
                        {
                            dd = emp.Target.Get() / n;
                            if (dd < 1)
                            {
                                cc = GMeter.C.Red;
                            }
                        }
                        GMeter.Render(r, cc, dd, x1, x2, y1, y2);
                    }
                };

                AddRelBody(-4, Dir.N, hh);
                AddOnTopC(new GStat()
                {
                    Update = text =>
                    {
                        GFormat.I(text, emp.Target.Get());
                    }
                }.Bg().R(Dir.N));
            }
        }

        private class Chunk : GuiSection
        {
            public int Priority { get; }

            public Chunk(int priority)
            {
                Priority = priority;
                Body.SetDim(EW, HH);
            }

            public override void Render(SpriteRenderer r, float ds)
            {
                base.Render(r, ds);
                if (Priority == 0)
                {
                    Opacity.O50.Bind();
                    Color.Red100.Render(r, Body, -4);
                    Opacity.Unbind();
                }
                GColor.UI().Border().Render(r, Body.X1(), Body.X1() + 1, Body.Y1() + 6, Body.Y2() - 6);
            }
        }

        private class Card : GButt.ButtPanel
        {
            public readonly FilterEntry<WGroup> G;
            public readonly WGroup E;

            public Card(FilterEntry<WGroup> g, Sprite icon) : base(icon)
            {
                G = g;
                E = (WGroup)g.Object;
            }

            protected override void ClickA()
            {
                _dragging = this;
            }

            protected override void RenAction()
            {
                base.RenAction();
                IsSelected = _dragging == this;
            }

            protected override void Render(SpriteRenderer r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                base.Render(r, ds, isActive, isSelected, isHovered);
                GMeter.Render(r, GMeter.C.Orange, ds, Body);
            }

            public override void HoverInfoGet(GUI.Box text)
            {
                base.HoverInfoGet(text);
                var b = (GBox)text;

                b.Add(_emp.Blueprint().Icon);
                b.TextLL(b.Text().Add(_emp.Blueprint().Info.Names).S().Add('-').Add('>').S().Add(G.Name));
                b.NL();

                b.TextLL(E.Name);
                b.Tab(6);
                b.Add(GFormat.I(b.Text(), _emp.Target.Group(E)));
                b.NL();

                b.TextLL(Standings.Citizen().Fullfillment.Info.Name);
                b.Tab(6);
                b.Add(GFormat.Perc(b.Text(), E.Race.Pref().GetWork(_emp)));
                b.NL();

                if (_emp.Blueprint().Bonus() != null)
                {
                    b.TextLL(T(Dic.¤Skill));
                    b.Tab(6);
                    b.Add(GFormat.Perc(b.Text(), E.Race.BValue(_emp.Blueprint().Bonus())));
                    b.NL();
                }

                b.TextLL(b.Text().Add(_emp.Blueprint().Info.Name).S().Add(T(¤employedTarget)));
                b.Tab(6);
                b.Add(GFormat.I(b.Text(), _emp.Target.Get()));
                b.NL();

                b.Sep();

                b.Add(E.Icon);
                b.TextLL(b.Text().Add(G.Name).S().Add('(').Add(T(¤Total)).Add(')'));
                b.NL();

                b.TextLL(T(¤workForce));
                b.Tab(6);
                b.Add(GFormat.I(b.Text(), Stats.Work().Workforce(E)));
                b.NL();

                b.TextLL(T(¤employed));
                b.Tab(6);
                b.Add(GFormat.I(b.Text(), SETT.Rooms().Employment.TARGET.Get(E)));
                b.NL();

                double f = 0;
                double am = 0;
                foreach (var p in SETT.Rooms().Employment.ALL())
                {
                    f += E.Race.Pref().GetWork(p) * p.Target.Group(E);
                    am += p.Target.Group(E);
                }
                if (am > 0)
                    f /= am;
                b.TextLL(Standings.Citizen().Fullfillment.Info.Name);
                b.Tab(6);
                b.Add(GFormat.Perc(b.Text(), f));
                b.NL();

                f = 0;
                am = 0;
                foreach (var p in SETT.Rooms().Employment.ALL())
                {
                    if (p.Blueprint().Bonus() != null)
                    {
                        f += E.Race.BValue(p.Blueprint().Bonus()) * p.Target.Group(E);
                    }
                    am += p.Target.Group(E);
                }
                if (am > 0)
                    f /= am;
                b.TextLL(T(Dic.¤Skill));
                b.Tab(6);
                b.Add(GFormat.Perc(b.Text(), f));
                b.NL();
            }
        }
    }
}