using System;
using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;

namespace view.keyboard
{
    internal sealed class IChange : Interrupter, KeyPoller
    {
        private readonly GuiSection section = new GuiSection();
        private Key key;
        private int codeMod;
        private int codeKey;
        private int triedcodeMod;
        private int triedcodeKey;
        private readonly List<Key> errorKey = new List<Key>(64);
        private double timer = 0;

        private static readonly CharSequence ¤¤Pick = "¤Pick a hotkey for:";
        private static readonly CharSequence ¤¤Explanation = "¤Either press a single key, or press and hold a key to use as a modulator, then press another key. Hit escape to exit.";

        private static readonly CharSequence ¤¤Sucess = "¤Hotkey {0} successfully mapped to {1}!";
        private static readonly CharSequence ¤¤Fail = "¤Hotkey {0} is already in use by {1}, pick another one.";
        private static readonly CharSequence ¤¤Overwritten = "¤{0} is now without a hotkey!";
        private readonly GText text = new GText(UI.FONT().M, 120);

        static IChange()
        {
            D.ts(typeof(IChange));
        }

        public IChange()
        {
            text.setMaxWidth(600);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            section.hover(mCoo);
            return true;
        }

        protected override void mouseClick(MButt button)
        {
            section.click();
        }

        protected override void hoverTimer(GBox text)
        {
            section.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            int cx = C.DIM().cX();
            int y1 = C.DIM().cY() - 200;

            text.setFont(UI.FONT().H1);
            text.lablify();
            text.set(¤¤Pick);
            text.renderC(r, cx, y1);
            y1 += text.height();

            text.setFont(UI.FONT().H2);
            text.lablifySub();
            text.set(key.name);
            text.renderC(r, cx, y1);
            y1 += text.height() + 8;

            text.setFont(UI.FONT().M);
            text.normalify();
            text.set(key.desc);
            text.renderC(r, cx, y1);
            y1 += text.height() + 16;

            if (timer > 0)
            {
                text.clear();
                text.setFont(UI.FONT().H2);
                text.normalify2();
                text.add(¤¤Sucess);
                text.insert(0, key.name);
                text.insert(1, key.repr());
                text.adjustWidth();
                text.renderC(r, cx, y1);
                int y = y1 + 32;
                foreach (Key e in errorKey)
                {
                    text.clear();
                    text.setFont(UI.FONT().M);
                    text.errorify();
                    text.add(¤¤Overwritten);
                    text.insert(0, e.name);
                    text.adjustWidth();
                    text.renderC(r, cx, y);
                    y += 32;
                }
            }
            else if (codeMod == -1 && triedcodeKey != -1)
            {
                if (errorKey.Count > 0)
                {
                    int y = y1 + 32;
                    foreach (Key e in errorKey)
                    {
                        text.setFont(UI.FONT().M);
                        text.errorify();
                        text.clear().add(¤¤Fail);
                        text.insert(0, e.repr());
                        text.insert(1, e.name);
                        text.adjustWidth();
                        text.renderC(r, cx, y);
                        y += 32;
                    }
                }
            }
            else
            {
                if (codeMod != -1)
                {
                    text.setFont(UI.FONT().H2);
                    text.normalify();
                    text.set(KEYS.names.getCode(codeMod));
                    text.renderC(r, cx, y1);
                }
                else
                {
                    text.setFont(UI.FONT().M);
                    text.color(COLOR.WHITE702WHITE100);
                    text.normalify();
                    text.set(¤¤Explanation);
                    text.renderC(r, cx, y1 + 32);
                }
            }

            return false;
        }

        protected override bool update(float ds)
        {
            if (timer > 0)
            {
                timer -= ds;
                if (timer <= 0 || MButt.LEFT.consumeClick())
                {
                    hide();
                }
                VIEW.setKeyPoller(this);
                return true;
            }
            VIEW.setKeyPoller(this);

            if (codeKey != -1)
            {
                triedcodeKey = codeKey;
                triedcodeMod = codeMod;
                this.codeKey = -1;
                this.codeMod = -1;
                this.errorKey.Clear();

                if (KEYS.MAIN().get(triedcodeMod, triedcodeKey) != null && KEYS.MAIN().get(triedcodeMod, triedcodeKey) != key)
                    errorKey.Add(KEYS.MAIN().get(triedcodeMod, triedcodeKey));
                if (key.page().get(triedcodeMod, triedcodeKey) != null && key.page().get(triedcodeMod, triedcodeKey) != key)
                    errorKey.Add(key.page().get(triedcodeMod, triedcodeKey));

                if (key.page() == KEYS.MAIN())
                {
                    foreach (KeyPage p in KEYS.pages())
                    {
                        if (p == key.page() || p == KEYS.MAIN())
                            continue;
                        if (p.get(triedcodeMod, triedcodeKey) != null && p.get(triedcodeMod, triedcodeKey) != key)
                            errorKey.Add(p.get(triedcodeMod, triedcodeKey));
                    }
                }

                if (key.assign(triedcodeMod, triedcodeKey))
                {
                    KEYS.get().save();
                    timer = 5;
                }
            }

            if (MButt.RIGHT.consumeClick())
            {
                hide();
            }
            if (timer > 0 && MButt.LEFT.consumeClick())
            {
                hide();
            }

            return false;
        }

        internal void show(Key key)
        {
            this.key = key;
            this.codeKey = -1;
            this.codeMod = -1;
            this.triedcodeMod = -1;
            this.triedcodeKey = -1;
            timer = 0;
            VIEW.inters().manager.add(this);
        }

        public void poll(LIST<KeyEvent> keys)
        {
            if (timer > 0)
            {
                foreach (KeyEvent e in keys)
                {
                    if (e.action() == KEYACTION.PRESS)
                    {
                        hide();
                        return;
                    }
                }
            }

            foreach (KeyEvent e in keys)
            {
                if (e.action() == KEYACTION.PRESS)
                {
                    if (e.code() == KEYCODES.KEY_ESCAPE)
                    {
                        hide();
                        return;
                    }

                    if (codeMod == -1)
                    {
                        codeMod = e.code();
                    }
                    else
                    {
                        codeKey = e.code();
                    }
                }
                else if (e.action() == KEYACTION.RELEASE)
                {
                    if (e.code() == codeMod)
                    {
                        codeKey = codeMod;
                        codeMod = -1;
                    }
                }
            }
        }
    }
}