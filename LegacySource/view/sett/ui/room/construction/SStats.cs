using System;
using System.Collections.Generic;
using game.time;
using init.constant;
using init.resources;
using init.sprite.UI;
using settlement.environment;
using settlement.main;
using settlement.maintenance;
using snake2d;
using snake2d.util.gui;
using util.colors;
using util.gui.misc;
using util.info;
using util.text;

namespace view.sett.ui.room.construction
{
    internal class SStats
    {
        private readonly HOVERABLE[] statStats = new HOVERABLE[8];
        private readonly HOVERABLE statResourcesStructure;
        private readonly HOVERABLE statResourcesCave;
        private readonly HOVERABLE[] statResources = new HOVERABLE[8];
        private readonly HOVERABLE foundation;
        private readonly GuiSection stats = new GuiSection();
        private readonly State s;

        private static readonly CharSequence ¤¤expense = "The room does not have enough support as indicated by yellow tiles. It can be built, but will require extra materials and maintenance. Shape the room thinner, or remove room tiles in the center to allow for more support and less costs.";
        private static readonly CharSequence ¤¤foundation = "This is a heavy room and relies on its foundation. Poor foundation will increase building and maintenance cost slightly, while good will decrease it.";
        private static readonly CharSequence ¤¤isolation = "The room will be poorly insulated, and maintenance need will be higher as a consequence. Toggle the automatic building of walls, and use as few doorways as possible to improve it.";

        static SStats()
        {
            D.ts(typeof(SStats));
        }

        internal SStats(State s)
        {
            this.s = s;
            statResourcesStructure = new HOVERABLE.HoverableAbs((int)(Icon.M * 2.5), Icon.M)
            {
                stat = new GStat()
                {
                    public override void update(GText text)
                    {
                        int am = s.placement.placer.structure.roofs() * s.placement.placer.structure.get().structure.resAmount;
                        am += s.placement.placer.structure.walls() * s.placement.placer.structure.get().structure.resAmount;
                        GFORMAT.i(text, am);
                    }
                },
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    if (s.placement.placer.structure.get().structure.resource != null)
                    {
                        s.placement.placer.structure.get().structure.resource.icon().render(r, body().x1(), body().y1());
                        stat.render(r, body().x1() + Icon.M + C.SG * 2, body().y1() + (body().height() - stat.height()) / 2);
                    }
                },
                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (s.placement.placer.structure.get().structure.resource != null)
                    {
                        text.text(s.placement.placer.structure.get().structure.resource.name);
                        text.NL();
                        text.text(s.placement.placer.structure.get().structure.nameCeiling);
                    }
                }
            };
            statResourcesCave = new HOVERABLE.HoverableAbs((int)(Icon.M * 2.5), Icon.M)
            {
                stat = new GStat()
                {
                    public override void update(GText text)
                    {
                        int am = s.placement.placer.structure.mountainWalls() * SETT.JOBS().clearss.caveFill.resAmount();
                        GFORMAT.i(text, am);
                    }
                },
                protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                {
                    if (s.placement.placer.structure.mountainWalls() == 0)
                        return;
                    RESOURCES.STONE().icon().render(r, body().x1(), body().y1());
                    stat.render(r, body().x1() + Icon.M + C.SG * 2, body().y1() + (body().height() - stat.height()) / 2);
                },
                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(RESOURCES.STONE().name);
                    text.NL();
                    text.text(SETT.JOBS().clearss.caveFill.placer().name());
                }
            };

            for (int i = 0; i < 8; i++)
            {
                int k = i;
                statResources[i] = new HOVERABLE.HoverableAbs((int)(Icon.M * 2.5), Icon.M)
                {
                    stat = new GStat()
                    {
                        public override void update(GText text)
                        {
                            double am = SETT.ROOMS().placement.placer.resNeeded(k);
                            GFORMAT.i(text, (int)Math.Ceiling(am));
                            if (SETT.ROOMS().placement.placer.cost().support() > 0)
                            {
                                text.warnify();
                                text.add(¤¤expense);
                            }
                        }
                    },
                    protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                    {
                        if (s.placement.placer.structure.get().structure.resource != null)
                        {
                            s.placement.placer.structure.get().structure.resource.icon().render(r, body().x1(), body().y1());
                            stat.render(r, body().x1() + Icon.M + C.SG * 2, body().y1() + (body().height() - stat.height()) / 2);
                        }
                    },
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.text(s.placement.placer.structure.get().structure.resource.name);
                        text.NL();
                        text.text(s.placement.placer.structure.get().structure.nameCeiling);
                    }
                };

                statStats[i] = new HOVERABLE.HoverableAbs(C.SG * 250, C.SG * 16)
                {
                    stat = new GStat()
                    {
                        public override void update(GText text)
                        {
                            s.b.constructor().stats().get(k).format(text, s.placement.placer.itemStats(k));
                        }
                    },
                    title = new GStat()
                    {
                        public override void update(GText text)
                        {
                            text.lablify().add(s.b.constructor().stats().get(k).name());
                        }
                    },
                    protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
                    {
                        stat.render(r, body().x1() + 190, body().y1());
                        title.render(r, body().x1(), body().y1());
                    },
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.text(s.b.constructor().stats().get(k).desc());
                    }
                };
            }

            GuiSection f = new GuiSection();
            f.add(new GButt.ButtPanel(UI.icons().s.eye)
            {
                protected override void renAction()
                {
                    selectedSet(SETT.ROOMS().placement.placer.showFoundation.is());
                },
                protected override void clickA()
                {
                    SETT.ROOMS().placement.placer.showFoundation.toggle();
                }
            });
            f.addRightC(6, new GText(UI.FONT().S, SETT.OVERLAY().FOUNDATION.name).lablify());

            f.addRightCAbs(190 - 32, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.percInc(text, -SETT.ROOMS().placement.placer.cost().foundation(), 2);
                }
            });
            f.hoverInfoSet(¤¤foundation);
            foundation = f;
        }

        internal GuiSection get()
        {
            stats.clear();
            int k = 0;

            for (int i = 0; i < s.b.constructor().stats().size(); i++)
            {
                stats.addDown(0, statStats[i]);
            }

            if (s.b.constructor().isHeavy())
            {
                stats.addDown(0, foundation);
            }

            int w = statResources[0].body().width();
            int h = statResources[0].body().height();
            int y1 = 84 + 18;

            for (int i = 0; i < s.b.constructor().resources(); i++)
            {
                stats.add(statResources[i], (k % 3) * w, y1 + (k / 3) * h);
                k++;
            }
            if (s.b.constructor().mustBeIndoors())
            {
                k++;
                stats.add(statResourcesStructure, (k % 3) * w, y1 + (k / 3) * h);
                k++;
                stats.add(statResourcesCave, (k % 3) * w, y1 + (k / 3) * h);
            }
            stats.body().incrH(16);
            return stats;
        }
    }
}