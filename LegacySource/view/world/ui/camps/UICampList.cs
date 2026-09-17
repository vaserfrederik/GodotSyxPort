using System;
using System.Collections.Generic;
using System.Linq;

public class UICampList : ISidePanel
{
    private readonly Rec hBody = new Rec(C.TILE_SIZE * 2);
    private readonly ArrayListResize<WHaven> al = new ArrayListResize<WHaven>(256, 1024 * 2);
    private readonly IComparer<WHaven> sort = new CampComparer();

    private readonly GTableBuilder builder;
    private int upI = -1;

    public UICampList()
    {
        new RD.RDOwnerChanger()
        {
            Change = (reg, oldOwner, newOwner) =>
            {
                upI = -1;
            }
        };

        GuiSection s = new GuiSection();

        foreach (WHavenType t in WORLD.camps().types)
        {
            s.AddDownC(4, new CampInfo(t));
        }

        section.Add(s);

        builder = new GTableBuilder()
        {
            NrOFEntries = () => All().Size,
            Hover = (index) =>
            {
                // Region r = sorter.get(index);
                // if (r != null)
                // {
                //     WorldHoverer.hover(VIEW.hoverBox(), r);
                // }
            }
        };

        builder.Column(null, 290, new GRowBuilder()
        {
            Build = (ier) =>
            {
                return new Button(ier);
            }
        });

        s = builder.CreateHeight(HEIGHT - section.Body().Height() - C.SG * 4, true);
        section.AddDown(2, s);

        TitleSet(WORLD.camps().info.name);
    }

    public IList<WHaven> All()
    {
        if (upI == -1 || Math.Abs(upI - GAME.updateI()) > 200)
        {
            upI = GAME.updateI();
            al.ClearSoft();

            foreach (WEntity e in WORLD.ENTITIES().AllSlow())
            {
                if (al.HasRoom() && e is WHaven)
                {
                    al.Add((WHaven)e);
                }
            }
            al.Sort(sort);
        }
        return al;
    }

    protected override void Update(float ds)
    {
    }

    private class Button : GuiSection
    {
        private readonly IGetter<int> ier;

        public Button(IGetter<int> ier)
        {
            this.ier = ier;

            Add(new SPRITE.Imp(Icon.M)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    WHaven f = All().Get(ier.Get());
                    f.type().race.appearance().icon.Render(r, X1, X2, Y1, Y2);
                }
            }, 0, 0);

            AddRightC(4, new GStat()
            {
                Update = (text) =>
                {
                    WHaven f = All().Get(ier.Get());
                    text.SetMaxWidth(232);
                    text.SetMultipleLines(false);
                    text.Lablify().Add(f.name);
                }
            });

            body().SetWidth(276);

            Pad(4, 2);
        }

        protected override void ClickA()
        {
            WHaven f = All().Get(ier.Get());
            VIEW.world().window.centererTile.Set(f.ctx(), f.cty());

            // Region f = sorter.get(ier);
            // VIEW.world().UI.region.openFromList(f, VIEW.world().panels);
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            GButt.ButtPanel.RenderBG(r, true, false, hoveredIs(), body());
            base.Render(r, ds);
            WHaven f = All().Get(ier.Get());
            if (f.faction() != FACTIONS.player())
            {
                OPACITY.O66.Bind();
                COLOR.BLACK.Render(r, body(), -2);
                OPACITY.Unbind();
            }
        }

        public override bool Hover(COORDINATE mCoo)
        {
            if (base.Hover(mCoo))
            {
                WHaven f = All().Get(ier.Get());
                hBody.MoveC(f.ctx() * C.TILE_SIZE + C.TILE_SIZEH, f.cty() * C.TILE_SIZE + C.TILE_SIZEH);
                WORLD.OVERLAY().things.hover(hBody, GCOLOR.MAP().Get(f.faction()), false, 0);
                return true;
            }

            return false;
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            WHaven f = All().Get(ier.Get());
            CampInfo.hover(text, f);
        }
    }

    public ISidePanel Get(WHaven f)
    {
        upI = -1;
        return this;
    }

    public void Hover(GUI_BOX box, WHaven w)
    {
        CampInfo.hover(box, w);
    }
}

class CampComparer : IComparer<WHaven>
{
    public int Compare(WHaven o1, WHaven o2)
    {
        return Get(o1) - Get(o2);
    }

    private int Get(WHaven current)
    {
        int res = current.Index();
        Faction f = current.Faction();
        if (f == FACTIONS.Player())
        {
            // No change
        }
        else if (f == null && WORLD.REGIONS().map.Get(current.ctx(), current.cty()) != null && WORLD.REGIONS().map.Get(current.ctx(), current.cty()).Faction() == FACTIONS.Player())
        {
            res += 10000;
        }
        else if (f == null)
        {
            res += 10000 * 2;
        }
        else
        {
            res += 10000 * 3;
        }

        return res;
    }
}