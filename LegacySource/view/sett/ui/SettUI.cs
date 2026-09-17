using game;
using init.type;
using view.interrupter;
using view.sett.ui.army;
using view.sett.ui.bottom;
using view.sett.ui.food;
using view.sett.ui.home;
using view.sett.ui.law;
using view.sett.ui.noble;
using view.sett.ui.room;
using view.sett.ui.room.construction;
using view.sett.ui.room.copy;
using view.sett.ui.room.prints;
using view.sett.ui.standing;
using view.sett.ui.subject;

namespace view.sett.ui
{
    public sealed class SettUI
    {
        public readonly UIRooms rooms = new UIRooms();
        public readonly UISubjects subjects = new UISubjects();
        public readonly UIArmy army = new UIArmy(GAME.ARMIES());
        public readonly UICitizens standing = new UICitizens(HCLASSES.CITIZEN());
        public readonly UICitizens slaves = new UICitizens(HCLASSES.SLAVE());
        public readonly UINobles nobles = new UINobles();
        public readonly UILaw law = new UILaw();
        public readonly UIHomes home = new UIHomes();
        public readonly UIFood prod = new UIFood();
        public readonly UIRoomPlacer placer = new UIRoomPlacer();
        public readonly UISavedPrints prints = new UISavedPrints();
        public readonly UICopier copier = new UICopier();
        public readonly UIBuildPanel bottom;

        public SettUI(InterManager m)
        {
            bottom = new UIBuildPanel(placer, m);
        }
    }
}