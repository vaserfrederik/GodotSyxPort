using System;
using snake2d;
using util.text;

namespace view.keyboard
{
    public class KeyPageBattle : KeyPage
    {
        public KeyPageBattle()
            : base("BATTLE")
        {
            D.gInit(this);
        }

        public readonly Key UP = new Key("UP", D.g("Up"), D.g("UpD", "Move selected Divisions."), this, KEYCODES.KEY_UP);
        public readonly Key DOWN = new Key("DOWN", D.g("Down"), UP.desc, this, KEYCODES.KEY_DOWN);
        public readonly Key LEFT = new Key("LEFT", D.g("Left"), UP.desc, this, KEYCODES.KEY_LEFT);
        public readonly Key RIGHT = new Key("RIGHT", D.g("Right"), UP.desc, this, KEYCODES.KEY_RIGHT);
        public readonly Key SELECT_ALL = new Key("SELECT_ALL", D.g("Select"), D.g("SelectD", "Selects all divisions."), this, KEYCODES.KEY_LEFT_CONTROL, KEYCODES.KEY_SPACE);
        public readonly Key SHOW_DIVISIONS = new Key("SHOW_DIVISION", D.g("Show"), D.g("ShowD", "Shows all division positions."), this, KEYCODES.KEY_SPACE);

        public readonly Key FORM_LOOSE = new Key("FORM_LOOSE", D.g("floose", "Loose Formation"), D.g("flooseD", "Loose Formation. Good against projectiles"), this, KEYCODES.KEY_L);
        public readonly Key FORM_TIGHT = new Key("FORM_TIGHT", D.g("ftight", "Tight Formation"), D.g("ftightD", "Tight Formation. Good in melee."), this, KEYCODES.KEY_T);

        public readonly Key GUARD = new Key("GUARD", D.g("Guard"), D.g("guardD", "When in guard, soldiers will maintain formation in battle, and stay on the defensive."), this, KEYCODES.KEY_G);
        //public readonly Key RUN = new Key("RUN", D.g("Run"), D.g("runD", "Soldiers will make haste when moving at the cost of exhaustion."), this, KEYCODES.KEY_LEFT_SHIFT, KEYCODES.KEY_R);
        public readonly Key CHARGE = new Key("CHARGE", D.g("Charge"), D.g("stopD", "Soldiers will start running in their current direction until they reach an enemy or obstacle. Soldiers will not have a lot of defense, but have a lot of extra force when colliding, as well as scaring the enemy force."), this, KEYCODES.KEY_C);

        public override CharSequence name()
        {
            return Dic.¤¤Battle;
        }
    }
}