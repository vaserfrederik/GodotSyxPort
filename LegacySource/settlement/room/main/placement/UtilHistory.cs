using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Placement
{
    class UtilHistory
    {
        private const int historyAmount = 1024;
        private HistoryI[] histories = new HistoryI[historyAmount];
        private Stack<int> hStack = new Stack<int>(historyAmount);
        private short historyFirst = -1;
        private readonly RoomPlacer p;

        private short tick;
        private int currentTick;

        public UtilHistory(RoomPlacer p)
        {
            for (int i = 0; i < historyAmount; i++)
            {
                histories[i] = new HistoryI(i);
                hStack.Push(i);
            }
            this.p = p;
        }

        public void Clear()
        {
            while (historyFirst != -1)
            {
                hStack.Push(historyFirst);
                historyFirst = histories[historyFirst].next;
            }

            historyFirst = -1;
        }

        private void Init()
        {
            if (currentTick != GAME.updateI())
            {
                tick++;
                currentTick = GAME.updateI();
            }
        }

        public void PlaceDoor(int x1, int y1, int delta)
        {
            Init();
            if (hStack.Count == 0)
            {
                ClearHistory(-1, historyFirst, 10);
            }
            int hi = hStack.Pop();
            HistoryI h = histories[hi];
            h.x = (short)x1;
            h.y = (short)y1;
            h.action = (short)(delta == 1 ? HistoryI.actionDoor : HistoryI.actionDoorRemove);
            h.next = historyFirst;
            h.tick = tick;
            historyFirst = (short)hi;
        }

        public void PlaceItem(FurnisherItem it, int x1, int y1, int delta)
        {
            Init();
            if (hStack.Count == 0)
            {
                ClearHistory(-1, historyFirst, 10);
            }
            int hi = hStack.Pop();
            HistoryI h = histories[hi];

            if (delta < 0)
            {
                h.action = (short)(it.Index() * delta);
            }
            else
            {
                h.action = (short)(1 + it.Index());
                x1 += it.FirstX();
                y1 += it.FirstY();
            }
            h.x = (short)x1;
            h.y = (short)y1;
            h.next = historyFirst;
            h.tick = tick;
            historyFirst = (short)hi;
        }

        public void PlaceEmbryo(int x1, int y1, int delta)
        {
            Init();
            if (hStack.Count == 0)
            {
                ClearHistory(-1, historyFirst, 64);
            }
            int hi = hStack.Pop();
            HistoryI h = histories[hi];
            h.x = (short)x1;
            h.y = (short)y1;
            h.action = delta == -1 ? HistoryI.actionShrink : HistoryI.actionExpand;
            h.next = historyFirst;
            h.tick = tick;
            historyFirst = (short)hi;
        }

        private int ClearHistory(int previous, int current, int amount)
        {
            if (current == -1)
                return amount;
            if (ClearHistory(current, histories[current].next, amount) > 0)
            {
                hStack.Push(current);
                histories[current].next = -1;
                if (previous == -1)
                {
                    historyFirst = -1;
                }
                else
                {
                    histories[previous].next = -1;
                }

                return amount - 1;
            }
            return 0;
        }

        public bool HasHistory()
        {
            return historyFirst != -1;
        }

        public void PopHistory()
        {
            short hi = historyFirst;
            int t = histories[hi].tick;
            while (hi != -1 && histories[hi].tick == t)
            {
                HistoryI h = histories[hi];
                hStack.Push(hi);
                hi = h.next;
                historyFirst = hi;
                if (h.action < 256)
                {
                    if (h.action <= 0)
                    {
                        FurnisherItem it = p.Blueprint().Constructor().Item(-h.action);
                        if (p.Placability.ItemProblem(h.x, h.y, it.Group, it, p.Instance) != null)
                        {
                            Clear();
                            return;
                        }

                        for (int ry = 0; ry < it.Height(); ry++)
                        {
                            for (int rx = 0; rx < it.Width(); rx++)
                            {
                                if (p.Placability.ItemPlacable(h.x + rx, h.y + ry, rx, ry, it, p.Instance) != null || !p.Instance.Is(h.x + rx, h.y + ry))
                                {
                                    Clear();
                                    return;
                                }
                            }
                        }
                        SETT.ROOMS().FData.ItemSet(h.x, h.y, it, p.Instance);
                    }
                    else
                    {
                        SETT.ROOMS().FData.ItemClear(h.x, h.y, p.Instance);
                    }
                }
                else if (h.action == HistoryI.actionExpand)
                {
                    p.Instance.Clear(h.x, h.y);
                }
                else if (h.action == HistoryI.actionShrink)
                {
                    p.Instance.Set(h.x, h.y);
                }
                else if (h.action == HistoryI.actionDoor)
                {
                    p.Door.RemoveWithoutHistory(h.x, h.y);
                }
                else if (h.action == HistoryI.actionDoorRemove)
                {
                    p.Door.PlaceWithoutHistory(h.x, h.y);

                }
            }
        }

        private class HistoryI
        {
            private static short actionExpand = (short)(256 + 1);
            private static short actionShrink = (short)(256 + 2);
            private static short actionDoor = (short)(actionShrink + 1);
            private static short actionDoorRemove = (short)(actionDoor + 16 * 2);

            public short next;
            public short x, y;
            public short action;
            public short tick;

            public HistoryI(int index)
            {
            }
        }
    }
}