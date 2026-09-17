using util.gui.misc;

namespace view.sett
{
    public interface SETT_HOVERABLE
    {
        void Hover(GBox text);
        bool CanBeClicked();
        void Click();
    }
}