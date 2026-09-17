using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using game;
using game.save;
using game.time;
using init.constant;
using init.paths;
using init.sprite;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.panel;
using util.gui.table;
using util.text;
using view.interrupter;
using view.main;

public sealed class Messages
{
    private readonly ArrayList<Message> all = new ArrayList<Message>(256);
    private int unread = 0;
    private readonly List list;
    private readonly IMessage imess;
    private readonly InterManager manager;
    private readonly KeyMap<string> hideMap = new KeyMap<string>();
    private readonly Queue<Message> queued = new Queue<Message>();

    private const string BlockedFile = "blocked.json";

    public Messages(InterManager manager)
    {
        this.manager = manager;
        list = new List(this);
        imess = new IMessage(this);
        LoadBlockedMessages();
    }

    private void LoadBlockedMessages()
    {
        if (File.Exists(BlockedFile))
        {
            var json = File.ReadAllText(BlockedFile);
            hideMap.Deserialize(json);
        }
    }

    private void SaveBlockedMessages()
    {
        var json = hideMap.Serialize();
        File.WriteAllText(BlockedFile, json);
    }

    public static void DebugMessages()
    {
        new Message("Debug Title", "This is a debug message.").Show();
    }

    public void Show(Message message)
    {
        if (queued.Count > 0)
        {
            queued.Enqueue(message);
            return;
        }

        imess.Act(message);
    }

    private void Remove(Message message)
    {
        all.Remove(message);
        if (message.isRead)
        {
            unread--;
        }
    }

    private void Add(Message message)
    {
        all.Add(message);
        if (!message.isRead)
        {
            unread++;
        }
    }

    private void AddOrRemove(Message message, bool addToBlocked)
    {
        if (addToBlocked)
        {
            hideMap.Put(message.key, message.key);
        }
        else
        {
            hideMap.Remove(message.key);
        }
        SaveBlockedMessages();
    }

    private class List : Interrupter
    {
        private readonly Messages parent;
        private readonly GPanel panel = new GPanel().SetBig();
        private readonly GPanel innerPanel = new GPanel().SetBig();

        public List(Messages parent)
        {
            this.parent = parent;

            panel.Add(innerPanel);
            panel.SetTitle(C.¤¤Messages, UI.FONT().H2);
            panel.SetCloseAction(parent.hide);
            panel.Body().CenterIn(C.DIM());
            panel.Body().CenterX(C.WIDTH() / 2, C.WIDTH());

            innerPanel.Body().CenterIn(panel);
            innerPanel.MoveLastToBack();

            var deleteButton = new GButt.ButtPanel(C.¤¤Delete)
            {
                ClickAction = () =>
                {
                    for (int i = 0; i < parent.all.Count; i++)
                    {
                        if (parent.all[i].isRead)
                        {
                            parent.Remove(parent.all[i]);
                            i--;
                        }
                    }
                },
                RenderAction = () =>
                {
                    activeSet(parent.all.Count - parent.unread > 0);
                }
            };

            deleteButton.Body().CenterX(innerPanel).MoveY1(innerPanel.Body().Y2() + 10);
            innerPanel.Add(deleteButton);
        }

        public void Act()
        {
            Show(parent.manager);
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            panel.Hover(mCoo);
            return true;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                panel.Click();
            if (button == MButt.RIGHT)
                parent.hide();
        }

        protected override void Hide()
        {
            base.Hide();
        }

        protected override void HoverTimer(GBox text)
        {
            panel.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            panel.Render(r, ds);
            return true;
        }

        protected override bool Update(float ds)
        {
            return true;
        }
    }

    private class IMessage : Interrupter
    {
        private readonly Messages parent;
        private readonly GPanel panel = new GPanel().SetBig();
        private Message m;

        private readonly GButt.ButtPanel show = new GButt.ButtPanel(UI.FONT().S.GetText(C.¤¤Alert))
        {
            HoverInfoGet = text => text.Text(C.¤¤PauseD),
            RenderAction = () =>
            {
                selectedSet(!parent.hideMap.ContainsKey(m.key));
            },
            ClickAction = () =>
            {
                parent.AddOrRemove(m, !parent.hideMap.ContainsKey(m.key));
            }
        }.Icon(UI.icons().s.clock);

        private readonly ACTION close = new ACTION
        {
            Exe = () =>
            {
                Hide();
                if (m == null)
                    return;
                if (!m.isRead)
                    parent.unread--;
                m.isRead = true;
                if (parent.queued.Count > 0)
                {
                    Act(parent.queued.Dequeue());
                }
            }
        };

        public IMessage(Messages parent)
        {
            this.parent = parent;
            panel.SetCloseAction(close);
        }

        public void Act(Message m)
        {
            if (VIEW.b().IsActive())
                return;
            this.m = m;
            if (m.section == null)
                m.section = m.MakeSection();
            panel.Inner().Set(m.section);
            panel.Inner().SetWidth(Math.Max(panel.Inner().Width(), 500));
            panel.Inner().IncrH(20);
            panel.Body().CenterIn(C.DIM());
            m.section.Body().CenterX(panel.Inner());
            m.section.Body().MoveY1(panel.Inner().Y1());
            show.Body.MoveX2(m.section.Body().X2() - 8);
            show.Body.MoveY1(panel.Inner().Y2() + 2);

            if (m.title() != null && m.title().Length > 0)
                panel.SetTitle(m.title());
            else
                panel.SetTitle(C.¤¤Clear);

            Show(parent.manager);
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            panel.Hover(mCoo);
            if (m.section is HOVERABLE hoverableSection)
                hoverableSection.Hover(mCoo);
            show.Hover(mCoo);
            return true;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                panel.Click();
                if (m.section is CLICKABLE clickableSection)
                    clickableSection.Click();
                show.Click();
            }
            else if (button == MButt.RIGHT)
                close.Exe();
        }

        protected override void HoverTimer(GBox text)
        {
            if (panel.HoveredIs())
                panel.HoverInfoGet(text);
            if (m.section is HOVERABLE hoverableSection)
                hoverableSection.HoverInfoGet(text);
            if (show.HoveredIs())
                show.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            panel.Render(r, ds);
            m.section.Render(r, ds);
            show.Render(r, ds);
            return true;
        }

        protected override bool Update(float ds)
        {
            return false;
        }
    }
}