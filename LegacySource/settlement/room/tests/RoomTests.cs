using System;
using settlement.room.main;
using settlement.room.military.artillery;
using snake2d.util.misc;
using view.interrupter;
using view.main;
using view.sett;

public sealed class RoomTests
{
    public RoomTests(ROOMS r)
    {
        foreach (ROOM_ARTILLERY a in r.ARTILLERY)
        {
            IDebugPanelSett.Add(new ArtilleryTest(a.eplacer));
        }

        IDebugPanel.Add("production & trade panel", new ACTION(() =>
        {
            VIEW.Inters().Popup.Show(new UITradeDebug(), null);
        }));
    }
}