using System;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using util.gui.misc;
using view.keyboard;
using view.main;

namespace view.sett.ui.bottom
{
    final class Delete : SPanel
    {
        protected Delete()
        {
            {
                CharSequence name = JOBS().tool_remove_smartl.name();

                ACTION a = new ACTION()
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(JOBS().tool_remove_smartl);
                    }
                };
                CLICKABLE c = new BButt(JOBS().tool_remove_smartl.getIcon(), name)
                {
                    protected void clickA()
                    {
                        a.exe();
                    }

                    public void hoverInfoGet(GUI_BOX text)
                    {
                        JOBS().tool_remove_smartl.hoverDesc((GBox)text);
                    }
                };

                c = KeyButt.wrap(a, c, KEYS.SETT(), "REMOVE_SMART", name, "");
                c = SearchToolPanel.add(c, name, "");
                addDownC(0, c);
            }

            {
                CharSequence name = JOBS().tool_remove_all.name();

                ACTION a = new ACTION()
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(JOBS().tool_remove_all);
                    }
                };
                CLICKABLE c = new BButt(JOBS().tool_remove_all.getIcon(), name)
                {
                    protected void clickA()
                    {
                        a.exe();
                    }

                    public void hoverInfoGet(GUI_BOX text)
                    {
                        JOBS().tool_remove_all.hoverDesc((GBox)text);
                    }
                };
                c = KeyButt.wrap(a, c, KEYS.SETT(), "REMOVE_ALL", name, "");
                c = SearchToolPanel.add(c, name, "");
                addDownC(0, c);
            }

            {
                CharSequence name = JOBS().clearss.road.placer().name();

                ACTION a = new ACTION()
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(JOBS().clearss.road.placer());
                    }
                };
                CLICKABLE c = new BButt(JOBS().clearss.road.placer().getIcon(), name)
                {
                    protected void clickA()
                    {
                        a.exe();
                    }

                    public void hoverInfoGet(GUI_BOX text)
                    {
                        JOBS().clearss.road.placer().hoverDesc((GBox)text);
                    }
                };
                c = KeyButt.wrap(a, c, KEYS.SETT(), "REMOVE_ROADS", name, "");
                c = SearchToolPanel.add(c, name, "");
                addDownC(0, c);
            }

            {
                CharSequence name = JOBS().tool_clear.name();

                ACTION a = new ACTION()
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(JOBS().tool_clear);
                    }
                };
                CLICKABLE c = new BButt(JOBS().tool_clear.getIcon(), name)
                {
                    protected void clickA()
                    {
                        a.exe();
                    }

                    public void hoverInfoGet(GUI_BOX text)
                    {
                        JOBS().tool_clear.hoverDesc((GBox)text);
                    }
                };
                c = KeyButt.wrap(a, c, KEYS.SETT(), "REMOVE_JOB", name, "");
                c = SearchToolPanel.add(c, name, "");
                addDownC(0, c);
            }

            {
                CharSequence name = ROOMS().DELETE.name();
                ACTION a = new ACTION()
                {
                    public void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(ROOMS().DELETE);
                    }
                };
                CLICKABLE c = new BButt(ROOMS().DELETE.getIcon(), name)
                {
                    protected void clickA()
                    {
                        a.exe();
                    }

                    public void hoverInfoGet(GUI_BOX text)
                    {
                        ROOMS().DELETE.hoverDesc((GBox)text);
                    }
                };
                c = KeyButt.wrap(a, c, KEYS.SETT(), "REMOVE_ROOM", name, "");
                c = SearchToolPanel.add(c, name, "");
                addDownC(0, c);
            }

            pad(3, 8);
        }
    }
}