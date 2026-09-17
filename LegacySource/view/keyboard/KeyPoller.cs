using System.Collections.Generic;
using snake2d;

namespace view.keyboard
{
    public interface KeyPoller
    {
        void poll(LIST<KeyEvent> keys);
    }
}