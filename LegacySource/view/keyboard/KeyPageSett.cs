using System;
using util.text;

namespace view.keyboard
{
    class KeyPageSett : KeyPage
    {
        KeyPageSett()
            : base("SETTLEMENT")
        {
        }

        public override CharSequence name()
        {
            return Dic.¤¤Settlement;
        }
    }
}