using view.interrupter;
using view.menu;
using view.ui.message;

namespace view.main
{
    public class Interrupters
    {
        public readonly InterManager manager = new InterManager();
        public readonly IMenu menu = new IMenu(manager);
        public readonly ITextInput input = new ITextInput(manager);
        public readonly IPromtScreen fullScreen = new IPromtScreen(manager);
        public readonly IPromtYesNO yesNo = new IPromtYesNO(manager);
        public readonly IDebugPanel debugpanel;
        // public readonly ITmpPanel panelTmp = new ITmpPanel(manager);
        public readonly IMouseMessage mouseMessage = new IMouseMessage();
        public readonly InterGuisection section = new InterGuisection(manager);
        public readonly Messages messages;
        public readonly IPopup popup = new IPopup(manager);
        public readonly IPopup popup2 = new IPopup(manager);
        public readonly ILoadScreen load = new ILoadScreen(manager);

        public Interrupters()
        {
            messages = new Messages(manager);
            debugpanel = new IDebugPanel(manager);
        }

        // public InterManager getManager() {
        //     return manager;
        // }
    }
}