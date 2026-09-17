using init.settings;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.misc;
using util.data.DOUBLE_O;
using view.main;

namespace view.sett.ui.subject
{
    internal class DebugInput
    {
        private static DOUBLE_OE<Induvidual> data;
        private static string name = "Set value";
        private static Humanoid h;
        private static readonly STRING_RECIEVER rec = new STRING_RECIEVER
        {
            public void acceptString(CharSequence string)
            {
                try
                {
                    double v = double.Parse("" + string);
                    data.setD(h.indu(), v);
                }
                catch (Exception e)
                {
                }
            }
        };

        internal static void activate(DOUBLE_OE<Induvidual> data, Humanoid h)
        {
            if (!S.get().developer)
                return;
            DebugInput.data = data;
            DebugInput.h = h;
            VIEW.inters().input.requestInput(rec, name);
        }
    }
}