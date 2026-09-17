using init.constant;
using init.settings;
using init.sprite.UI;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data.INT;
using util.gui.misc;
using util.gui.panel;
using util.info;
using util.text;
using view.interrupter;
using view.main;

namespace view.ui.family
{
    public sealed class UIFamilyTree : Interrupter
    {
        private const int MAX_REFS = 1024 * 2;

        private bool remove = false;
        private readonly UIFamilyTreeRefs refs = new UIFamilyTreeRefs();
        private readonly UIFamilyTreeAligner aligner = new UIFamilyTreeAligner();
        private readonly UIFamilyTreeDrawer drawer = new UIFamilyTreeDrawer();

        private readonly GuiSection buttons = new GuiSection();
        private int currentRef;

        static UIFamilyTree()
        {
            // D.ts(UIFamilyTree.class);
        }

        public UIFamilyTree()
        {
            if (S.Get().developer)
            {
                buttons.AddRightC(0, new GStat
                {
                    Update = text =>
                    {
                        int am = 0;
                        for (int i = 0; i < STATS.REL().References(); i++)
                        {
                            if (STATS.REL().IsRef(i))
                                am++;
                        }
                        GFORMAT.Iofk(text, am, STATS.REL().References());
                    }
                });

                INTE ii = new INTE
                {
                    Min = () => 0,
                    Max = () => 300,
                    Get = () => aligner.maxIterations,
                    Set = t =>
                    {
                        aligner.maxIterations = t;
                        Init(currentRef);
                    }
                };
                buttons.AddRightC(100, new GInputInt(ii, true, true));

                buttons.AddRightC(0, new GButt.ButtPanel(UI.Icons().M.Plus)
                {
                    ClickA = () =>
                    {
                        STATS.REL().DebugPopulate();
                        Init(currentRef);
                    }
                }.HoverInfoSet("populate"));

                buttons.AddRightC(0, new GButt.ButtPanel(UI.Icons().M.Cancel)
                {
                    ClickA = () => remove = !remove,
                    RenAction = () => selectedSet(remove)
                }.HoverInfoSet("toggleRemove"));

                buttons.AddRightC(0, new GButt.ButtPanel(UI.Icons().M.Disease)
                {
                    ClickA = () =>
                    {
                        STATS.REL().DebugPrune();
                        Init(currentRef);
                    }
                }.HoverInfoSet("prune"));
            }

            buttons.AddRightC(0, new GButt.ButtPanel(UI.Icons().M.Exit)
            {
                ClickA = hide
            }.HoverInfoSet(Dic.¤¤Close));

            GPanel p = new GPanel(buttons.Body());
            p.SetButt();
            buttons.Add(p);
            buttons.MoveLastToBack();

            buttons.Body().MoveX2(C.WIDTH() - 20);
            buttons.Body().MoveY1(20);
        }

        public void Show(Induvidual indu)
        {
            Show(STATS.REL().Reference(indu));
        }

        public void Show(int refId)
        {
            if (STATS.REL().IsRef(refId))
            {
                Init(refId);
                Show(VIEW.Inters().Manager);
            }
        }

        private void Init(int refId)
        {
            if (STATS.REL().IsRef(refId))
            {
                refs.Init(refId);
                aligner.Init(refs);
                drawer.Init(refId, refs, aligner);
                currentRef = refId;
            }
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            buttons.Hover(mCoo);
            return true;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.RIGHT)
            {
                hide();
            }
            else if (button == MButt.LEFT)
            {
                if (buttons.HoveredIs())
                {
                    buttons.Click();
                }
                else
                {
                    int hovered = drawer.Hovered();
                    if (!STATS.REL().IsRef(hovered))
                        drawer.Drag();
                    else if (remove)
                    {
                        if (hovered == currentRef)
                        {
                            for (int i = 0; i < refs.Max(); i++)
                            {
                                if (refs.Get(i) != hovered)
                                {
                                    STATS.REL().RemoveRef(hovered);
                                    Init(refs.Get(i));
                                }
                            }
                            return;
                        }
                        STATS.REL().RemoveRef(hovered);
                        Init(currentRef);
                    }
                    else
                    {
                        if (hovered == currentRef && STATS.REL().Human(hovered) != null)
                        {
                            Humanoid h = STATS.REL().Human(hovered);

                            hide();
                            VIEW.S().Activate();
                            if (VIEW.S().UI.Subjects.CanShow(h))
                            {
                                VIEW.S().UI.Subjects.Show(h);
                            }
                            else
                            {
                                VIEW.S().GetWindow().CenterAt(h.Body().CX(), h.Body().CY());
                            }
                        }
                        else
                        {
                            Init(hovered);
                        }
                    }
                }
            }
        }

        protected override void HoverTimer(GBox text)
        {
            if (buttons.HoveredIs())
            {
                buttons.HoverInfoGet(text);
            }
            else
            {
                STATS.REL().Hover(text, drawer.Hovered());
            }
        }

        protected override bool Render(Renderer r, float ds)
        {
            drawer.Draw(ds, refs, aligner, currentRef);
            buttons.Render(r, ds);
            return false;
        }

        protected override bool Update(float ds)
        {
            if (!STATS.REL().IsRef(currentRef))
                hide();
            return false;
        }
    }
}