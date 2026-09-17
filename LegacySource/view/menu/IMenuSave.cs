using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.save;
using init.constant;
using init.paths;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;

namespace view.menu
{
    class IMenuSave : GuiSection, STRING_RECIEVER
    {
        private readonly CLICKABLE overwrite;
        private readonly CLICKABLE delete;
        private ACTION successfullAction;

        private SaveFile[] saves = new SaveFile[0];

        private int selectedSave = -1;

        private readonly ACTION overwriteAction;

        private static readonly CharSequence ¤¤¤nameYour = "¤Name your save-game";
        private static readonly CharSequence ¤¤failed = "¤failed to be overwritten";
        private static readonly CharSequence ¤¤success = "¤successfully overwritten";
        private static readonly CharSequence ¤¤overwrite = "¤overwrite";
        private static readonly CharSequence ¤¤successSave = "{0} successfully saved!";
        private static readonly CharSequence ¤¤charsAllowed = "Only characters: {0} are allowed!";
        private static readonly CharSequence ¤¤fail = "Save failed. See error report!";

        static IMenuSave()
        {
            D.ts(typeof(IMenuSave));
        }

        public IMenuSave(IMenu m, Font font, Font small, ACTION successfullAction)
        {
            MenuScreen sc = new MenuScreen(Dic.¤¤save, GCOLOR.T().H1)
            {
                protected override void back()
                {
                    m.setMain();
                }
            };

            this.successfullAction = successfullAction;

            SaveEntry[] entries = new SaveEntry[14];
            for (int i = 0; i < 14; i++)
            {
                entries[i] = new SaveEntry();
            }

            GScrollable scroll = new GScrollable(entries)
            {
                public override int nrOFEntries()
                {
                    return saves.Length;
                }
            };

            scroll.getView().body().centerIn(C.DIM());
            add(scroll.getView());

            //NEW
            CLICKABLE newButt = new MenuScreen.ScreenButton(Dic.¤¤new)
            {
                protected override void clickA()
                {
                    string name = FACTIONS.player().name + "-";
                    KeyMap<string> m = new KeyMap<string>();
                    foreach (SaveFile f in saves)
                    {
                        if (f.name.StartsWith(name))
                        {
                            string n = f.name.Substring(name.Length, f.name.Length - name.Length);
                            m.putReplace(n, n);
                        }
                    }

                    string ph = "";

                    for (int i = 0; i < 512; i++)
                    {
                        string k = "" + i;
                        if (!m.containsKey(k))
                        {
                            ph = name + k;
                            break;
                        }
                    }

                    VIEW.inters().input.requestInput(this, ¤¤¤nameYour, ph);
                }
            };
            sc.addButt(newButt);

            //OVERWRITE
            GButt yes = new GButt.Glow(Dic.¤¤confirm);
            yes.clickActionSet(new ACTION()
            {
                public void exe()
                {
                    PATHS.local().save().delete(saves[selectedSave].fullName);
                    if (GAME.saver().save(SaveFile.stamp(saves[selectedSave].name)) == null)
                    {
                        VIEW.inters().fullScreen.activate(saves[selectedSave].name + " " + ¤¤failed, COLOR.RED100, null);
                    }
                    else
                    {
                        VIEW.inters().fullScreen.activate(saves[selectedSave].name + " " + ¤¤success, COLOR.WHITE100, successfullAction);
                    }
                    m.setMain();
                }
            });
            GButt no = new GButt.Glow(Dic.¤¤cancel);

            overwriteAction = new ACTION()
            {
                public void exe()
                {
                    VIEW.inters().fullScreen.activate(¤¤overwrite + " " + saves[selectedSave].name + "?", COLOR.WHITE100, null, yes, no);
                }
            };
            overwrite = new MenuScreen.ScreenButton(¤¤overwrite)
            {
                protected override void renAction()
                {
                    activeSet(selectedSave != -1);
                }
            };
            overwrite.clickActionSet(overwriteAction);
            sc.addButt(overwrite);

            //DELETE
            GButt yes2 = new GButt.Glow(Dic.¤¤confirm);
            yes2.clickActionSet(new ACTION()
            {
                public void exe()
                {
                    PATHS.local().save().delete(saves[selectedSave].fullName);
                    VIEW.inters().fullScreen.activate(saves[selectedSave].name + " deleted!", COLOR.WHITE100, null);
                    populateSaves();
                }
            });

            delete = new MenuScreen.ScreenButton(Dic.¤¤delete)
            {
                protected override void clickA()
                {
                    VIEW.inters().fullScreen.activate(Dic.¤¤delete + " " + saves[selectedSave].name, COLOR.WHITE100, null, yes2, no);
                }

                protected override void renAction()
                {
                    activeSet(selectedSave != -1);
                }
            };
            sc.addButt(delete);

            add(sc);
            moveLastToBack();

            populateSaves();
        }

        private void populateSaves()
        {
            saves = SaveFile.list();

            selectedSave = -1;
            overwrite.activeSet(false);
            delete.activeSet(false);
        }

        public void acceptString(CharSequence str)
        {
            if (str == null)
                return;

            if (!FileManager.NAME.okName(str))
            {
                Str.TMP.clear().add(¤¤charsAllowed).insert(0, FileManager.NAME.legalChars);
                VIEW.inters().fullScreen.activate(Str.TMP, COLOR.RED100, null);
                return;
            }

            for (int i = 0; i < saves.Length; i++)
            {
                if (saves[i].name.Equals(str))
                {
                    selectedSave = i;
                    overwriteAction.exe();
                    return;
                }
            }

            if (GAME.saver().save(SaveFile.stamp(str)) == null)
            {
                VIEW.inters().fullScreen.activate(¤¤fail, COLOR.RED100, null);
                return;
            }
            else
            {
                VIEW.inters().fullScreen.activate(¤¤successSave, COLOR.WHITE100, successfullAction);
            }

            VIEW.inters().menu.setMain();
            Str.TMP.clear().add(¤¤successSave).insert(0, str);
            VIEW.inters().fullScreen.activate(Str.TMP, COLOR.WHITE100, successfullAction);
        }

        private class SaveEntry : Savebutt
        {
            public SaveEntry()
            {
            }

            protected override void clickA()
            {
                selectedSave = index;
                overwrite.activeSet(true);
                delete.activeSet(true);
                if (MButt.LEFT.isDouble())
                {
                    overwriteAction.exe();
                }
            }

            protected override bool selected(int index)
            {
                return index == selectedSave;
            }

            protected override SaveFile save(int index)
            {
                return saves[index];
            }
        }

        private abstract class Savebutt : CLICKABLE.ClickableAbs, ScrollRow
        {
            private static readonly GText version = new GText(UI.FONT().M, 16);
            int index = -1;

            public Savebutt()
            {
                body().size.set(820, 20);
            }

            protected override void render(GUIBox text)
            {
                SaveFile s = save(index);

                if (s == null)
                    return;

                version.clear();
                version.add(VERSION.versionMajor(s.version));
                version.add('.');
                version.add(VERSION.versionMinor(s.version));
                if (VERSION.VERSION_MAJOR != VERSION.versionMajor(s.version))
                {
                    COLOR.RED100.bind();
                }
                else if (s.problem() != null)
                {
                    COLOR.YELLOW100.bind();
                }
                else
                {
                    if (selected(index))
                    {
                        GCOLOR.T().SELECTED.bind();
                    }
                    else if (isHovered)
                    {
                        GCOLOR.T().HOVERED.bind();
                    }
                }
                font().render(text.g, version, body().x1(), body().y1());

                if (selected(index))
                {
                    GCOLOR.T().SELECTED.bind();
                }
                else if (isHovered)
                {
                    GCOLOR.T().HOVERED.bind();
                }
                else
                {
                    GCOLOR.T().CLICKABLE.bind();
                }

                font().render(text.g, s.name, body().x1() + 60, body().y1());

                version.clear().add('p').s();
                GFORMAT.i(version, s.pop);
                font().render(text.g, version, body().x1() + 740, body().y1());

                font().render(text.g, s.ago, body().x1() + 820, body().y1());
                COLOR.unbind();
            }

            public void hoverInfoGet(GUI_BOX text)
            {
                SaveFile s = save(index);
                if (s != null)
                {
                    CharSequence p = s.problem();
                    if (p != null)
                        ((GBox)text).error(p);
                }
            }

            protected abstract bool selected(int index);
            protected abstract SaveFile save(int index);
        }
    }
}