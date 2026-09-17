using System;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using world;
using world.map.regions;
using world.region;

namespace view.world.ui.region
{
    final class PlayMilitary : GuiSection
    {
        public PlayMilitary(GETTER_IMP<Region> g, int WIDTH)
        {
            body().incrW(64);
            body().incrH(1);

            addRight(0, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.iIncr(text, (int)RD.MILITARY().conscriptTarget.get(g.get()));
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.title(RD.MILITARY().conscriptTarget.name);
                    b.text(RD.MILITARY().conscriptTarget.desc);
                    b.sep();

                    RD.MILITARY().conscriptTarget.hover(b, g.get(), null, true);
                }
            }.hv(UI.icons().m.sword));

            addRightC(64, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, RD.MILITARY().defensePower(g.get()));

                    //GFORMAT.iofkInv(text, RD.MILITARY().garrison.get(g.get()), (int)RD.MILITARY().garrisonTarget(g.get()));
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.title(Dic.¤¤Defences);
                    b.text(Dic.¤¤DefencesD);
                    b.NL();
                    b.textL(Dic.¤¤Besiege);
                    b.tab(6);
                    b.add(GFORMAT.perc(b.text(), RD.MILITARY().besigeMul(g.get())));
                    Str.TMP.clear();
                    Str.TMP.add('(');
                    DicTime.setDays(Str.TMP, WORLD.BATTLES().besigedTime(g.get()) / TIME.secondsPerDay());
                    Str.TMP.add(')');
                    b.text(Str.TMP);

                    b.sep();
                    b.textLL(RD.MILITARY().bgarrison.name);
                    b.add(GFORMAT.iofkInv(b.text(), RD.MILITARY().garrison.get(g.get()), (int)RD.MILITARY().garrisonTarget(g.get())));
                    b.NL();

                    b.textLL(Dic.¤¤Power);
                    b.add(GFORMAT.iIncr(b.text(), (int)RD.MILITARY().power.getD(g.get())));
                    b.NL();

                    b.textLL(RD.MILITARY().bFortification.name);
                    b.add(GFORMAT.fofkInv(b.text(), RD.MILITARY().fort.getD(g.get()), RD.MILITARY().bFortification.get(g.get())));
                    b.NL();
                }
            }.hv(UI.icons().m.shield));

            addRightCAbs(64, MiscMore.garrison(g, WIDTH - body().width() - 64));
        }
    }
}