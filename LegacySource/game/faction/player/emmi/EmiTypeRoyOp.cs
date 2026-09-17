using game.faction;
using game.faction.royalty;
using game.faction.royalty.opinion;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.gui.misc;
using util.info;
using util.text;

namespace game.faction.player.emmi
{
    public abstract class EmiTypeRoyOp : EmiTypeRoy
    {
        private static readonly string ¤¤target = "(target)";
        private static readonly string ¤¤effective = "(effective)";

        static EmiTypeRoyOp()
        {
            D.ts(typeof(EmiTypeRoyOp));
        }

        protected EmiTypeRoyOp(SPRITE icon, string name, string desc) : base(icon, name, desc)
        {
        }

        public override void hover(Royalty t, GUI_BOX text)
        {
            base.hover(t, text);
            GBox b = (GBox)text;
            b.NL(4);
            b.textLL(b.text().add(ROPINION.¤¤name).s().add(¤¤target));
            b.tab(6);
            b.add(GFORMAT.f(b.text(), ROPINION.EMMI().opinionTarget(t, 1.0)));
            b.NL();
            b.textLL(b.text().add(ROPINION.¤¤name).s().add(¤¤effective));
            b.tab(6);
            b.add(GFORMAT.f(b.text(), ROPINION.EMMI().opinionTarget(t, FACTIONS.player().emissaries.penaltyMul())));
            b.NL();
            b.textLL(b.text().add(ROPINION.trust().bo.name).s().add(¤¤target));
            b.tab(6);
            b.add(GFORMAT.perc(b.text(), ROPINION.EMMI().trustTarget(t, 1.0)));
            b.NL();
            b.textLL(b.text().add(ROPINION.trust().bo.name).s().add(¤¤effective));
            b.tab(6);
            b.add(GFORMAT.perc(b.text(), ROPINION.EMMI().trustTarget(t, FACTIONS.player().emissaries.penaltyMul())));
            b.NL();
        }
    }
}