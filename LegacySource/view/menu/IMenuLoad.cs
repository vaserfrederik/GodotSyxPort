using System;
using game.save;
using init.paths;
using init.sprite;
using snake2d.util.gui;
using util.colors;

namespace view.menu
{
    class IMenuLoad : GuiSection
    {
        private readonly MenuScreenLoad m;

        public IMenuLoad(IMenu m)
        {
            this.m = new MenuScreenLoad(MenuScreenLoad.¤¤name, GCOLOR.T().H1, true, PATHS.local().save())
            {
                protected override void load(SaveFile f)
                {
                    SPRITES.loader().printempty();
                    new GameLoader(f.path).set();
                }

                protected override void back()
                {
                    m.setMain();
                }
            };
            add(this.m);
        }

        public void init()
        {
            m.populateSaves();
        }

        public void setOther()
        {
        }
    }
}