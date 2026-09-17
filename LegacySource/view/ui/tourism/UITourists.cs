using init.sprite.UI;
using util.text;
using view.ui.manage;

namespace view.ui.tourism
{
    public sealed class UITourists : IFullView
    {
        public UITourists() : base(Dic.¤¤Tourists, UI.icons().l.tourist)
        {
            section.body().setWidth(WIDTH).setHeight(1);

            section.addDownC(0, new Tourism(HEIGHT));
        }
    }
}