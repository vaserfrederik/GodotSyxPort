using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Sett.UI.Room
{
    using Init.Constant;
    using Init.Sprite;
    using Init.Sprite.UI;
    using Settlement.Main;
    using Settlement.Room.Main;
    using Settlement.Room.Main.Furnisher;
    using Settlement.Room.Military.Artillery;
    using Snake2D;
    using Util;

    public class UIRoom
    {
        private static readonly string DeleteText = "Are you sure you want to delete this room?";
        private static readonly string ActivateDesc = "Activate/Deactivate the room.";
        private static readonly string RefurnishDesc = "Reconstruct the room.";

        private readonly UIRoomTable _table;
        private readonly UIRoomDetail _detail;
        private readonly UIRoomModule[] _modules;

        public UIRoom(UIRoomTable table, UIRoomDetail detail, IEnumerable<UIRoomModule> modules)
        {
            _table = table;
            _detail = detail;
            _modules = modules.ToArray();
        }

        public void Detail(RoomInstance room)
        {
            _detail.SetRoom(room);
            _detail.Show();
        }

        public void HoverInfoGet(RoomInstance room, GBox box)
        {
            _detail.HoverInfoGet(room, box);
        }

        public void Render(RoomInstance room, SpriteRenderer renderer, float ds, bool isHovered)
        {
            _detail.Render(room, renderer, ds, isHovered);
        }

        private class UIRoomTable
        {
            public void Show()
            {
                // Implementation for showing the table
            }

            public void Hide()
            {
                // Implementation for hiding the table
            }
        }

        private class UIRoomDetail
        {
            private RoomInstance _room;
            private readonly UIRoomModule[] _modules;

            public UIRoomDetail(UIRoomModule[] modules)
            {
                _modules = modules;
            }

            public void SetRoom(RoomInstance room)
            {
                _room = room;
            }

            public void Show()
            {
                // Implementation for showing the detail
            }

            public void Hide()
            {
                // Implementation for hiding the detail
            }

            public void HoverInfoGet(RoomInstance room, GBox box)
            {
                // Implementation for getting hover info
            }

            public void Render(RoomInstance room, SpriteRenderer renderer, float ds, bool isHovered)
            {
                // Implementation for rendering the detail
            }
        }

        private abstract class UIRoomModule
        {
            public abstract void AppendPanelIcon(List<IRenderable> icons, Func<RoomInstance> getRoom);
            public abstract void AppendPanel(GuiSection section, Func<RoomInstance> getRoom, int x, int y);
            public abstract void HoverInfoGet(RoomInstance room, GBox box);
        }
    }
}