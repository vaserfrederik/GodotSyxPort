using System.Collections.Generic;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace view.sett.ui.room.prints
{
    class Cat
    {
        public bool expanded = false;
        public int entries;
        public SPRITE icon;
        readonly System.Type classs;
        public readonly ArrayListGrower<RoomBlueprintImp> prints = new ArrayListGrower<RoomBlueprintImp>();

        public Cat(RoomBlueprintImp blue)
        {
            this.icon = blue.iconBig();
            this.classs = blue.GetType();
            this.prints.add(blue);
        }
    }
}