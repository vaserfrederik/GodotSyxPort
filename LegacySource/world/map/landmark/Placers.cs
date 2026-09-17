using System;
using System.Collections.Generic;
using init.sprite.UI.Icons;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.text;
using view.main;
using view.subview;
using view.tool;
using world;

namespace world.map.landmark
{
    class Placers : ArrayListGrower<PLACABLE>
    {
        private static readonly long serialVersionUID = 1L;
        private readonly WorldLandmarks ll;
        private readonly INT.IntImp ii = new IntImp(1, WorldLandmarks.MAX);

        public Placers(WorldLandmarks ll, PlacerOverlay overlay)
        {
            this.ll = ll;

            LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();
            GSliderInt sl = new GSliderInt(ii, 100, true, true)
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                    GBox b = (GBox)text;
                    b.add(b.text().add(':').add(get().name));
                }
            };
            butts.add(sl);
            butts.add(new GButt.ButtPanel(Dic.¤¤name)
            {
                protected override void clickA()
                {
                    VIEW.inters().input.requestInput(new STRING_RECIEVER()
                    {
                        public void acceptString(CharSequence string)
                        {
                            if (string != null)
                                get().name.clear().add(string);
                        }
                    }, Dic.¤¤name);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(get().name);
                }
            });
            butts.add(new GButt.ButtPanel("...")
            {
                protected override void clickA()
                {
                    VIEW.inters().input.requestInput(new STRING_RECIEVER()
                    {
                        public void acceptString(CharSequence string)
                        {
                            if (string != null)
                                get().description.clear().add(string);
                        }
                    }, "description");
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.text(ll.getByIndex(ii.get()).description);
                }
            });

            butts.add(new GButt.ButtPanel(UI.icons().m.crossair)
            {
                protected override void clickA()
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                    {
                        if (ll.setter.get(c) != null && ll.setter.get(c).index() == ii.get())
                        {
                            VIEW.world().window.centererTile.set(c);
                            return;
                        }
                    }
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title("find landmark");
                }
            });

            butts.add(new GButt.ButtPanel(UI.icons().m.skull)
            {
                protected override void clickA()
                {
                    foreach (COORDINATE c in WORLD.TBOUNDS())
                    {
                        if (ll.setter.get(c) != null && ll.setter.get(c).index() == ii.get())
                            ll.setter.set(c, null);
                    }
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    text.title("remove whole landmark");
                }
            });

            PLACABLE undo = new PlacableMulti(Dic.¤¤remove + ": " + ll.name, "", UI.icons().m.place_ellispse.resized(IconS.L).twin(UI.icons().m.anti, DIR.C, 0))
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    ll.setter.set(tx, ty, null);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return get() != null ? null : E;
                }

                public override void updateRegardless(GameWindow window, AREA selected)
                {
                    overlay.hovered = get();
                }

                public override void placeInfo(GBox b, int oktiles, AREA a)
                {
                    hover(b, oktiles, a);
                }
            };

            PLACABLE p = new PlacableMulti(ll.name, "", UI.icons().m.place_ellispse.resized(IconS.L))
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    ll.setter.set(tx, ty, get());
                }

                public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    return null;
                }

                public override PLACABLE getUndo()
                {
                    return undo;
                }

                public override LIST<CLICKABLE> getAdditionalButt()
                {
                    return butts;
                }

                public override void updateRegardless(GameWindow window, AREA selected)
                {
                    overlay.hovered = get();
                }

                public override void placeInfo(GBox b, int oktiles, AREA a)
                {
                    hover(b, oktiles, a);
                }
            };

            add(p);
            add(undo);
        }

        private void hover(GBox b, int oktiles, AREA a)
        {
            if (a.area() == 1)
            {
                foreach (COORDINATE c in a.body())
                {
                    WorldLandmark m = ll.setter.get(c);
                    if (m != null)
                    {
                        b.NL();
                        b.add(b.text().add(Dic.¤¤Current).add(':').s().add(m.index()).s().add(m.name));
                        return;
                    }
                }
            }
        }

        private WorldLandmark get()
        {
            return ll.getByIndex(ii.get());
        }
    }
}