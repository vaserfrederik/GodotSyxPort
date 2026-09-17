using System.Collections.Generic;
using init.sprite.UI;
using settlement.room.main;
using snake2d.util.color;
using snake2d.util.sets;

namespace settlement.room.main.category
{
    public sealed class RoomCategorySub
    {
        private readonly ArrayListGrower<RoomBlueprintImp> all = new ArrayListGrower<RoomBlueprintImp>();
        public readonly COLOR color;
        private readonly string name;
        private readonly Icon icon;
        public RoomCategoryMain main;

        public RoomCategorySub(ArrayListGrower<RoomCategorySub> all, string name, Icon icon, COLOR color)
        {
            this.name = name;
            this.icon = icon;
            this.color = color;
            all.add(this);
        }

        public int Add(RoomBlueprintImp imp)
        {
            return all.add(imp);
        }

        public string Name()
        {
            return name;
        }

        public Icon Icon()
        {
            return icon;
        }

        public LIST<RoomBlueprintImp> Rooms()
        {
            return all;
        }

        public RoomCategoryMain Main()
        {
            return main;
        }
    }
}