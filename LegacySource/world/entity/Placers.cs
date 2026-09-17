using System;
using System.Collections.Generic;
using init.sprite;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.data.GETTER;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using view.tool;
using world;
using world.entity.haven;

namespace world.entity
{
    internal sealed class Placers : ArrayListGrower<PLACABLE>
    {
        private static readonly long serialVersionUID = 1L;

        public Placers(LIST<WHavenType> types)
        {
            {
                final PlacableSimpleTile undo = new PlacableSimpleTile("camps remove")
                {
                    public override void Place(int tx, int ty)
                    {
                        foreach (WHaven h in WORLD.ENTITIES().havens.FillTile(tx, ty))
                        {
                            h.Delete();
                        }
                    }

                    public override CharSequence IsPlacable(int tx, int ty)
                    {
                        return WORLD.ENTITIES().havens.FillTile(tx, ty).Size() > 0 ? null : E;
                    }
                };

                GETTER_IMP<WHavenType> type = new GETTER_IMP<WHavenType>(types.Get(0));

                GuiSection s = new GuiSection();
                foreach (WHavenType t in types)
                {
                    s.AddRightC(0, new GButt.ButtPanel(SPRITES.icons().m.cancel)
                    {
                        protected override void ClickA()
                        {
                            type.Set(t);
                        }

                        protected override void RenAction()
                        {
                            label = t.race.appearance().icon;
                            SelectedSet(type.Get() == t);
                        }
                    }.HoverInfoSet(t.race.info.names));
                }

                INTE inte = new INTE()
                {
                    int i = 0;

                    public override int Min()
                    {
                        return 0;
                    }

                    public override int Max()
                    {
                        return 8;
                    }

                    public override int Get()
                    {
                        return i;
                    }

                    public override void Set(int t)
                    {
                        i = t;
                    }
                };

                s.AddRelBody(2, DIR.S, new GAllocator(COLOR.RED100, inte, 6, 16));

                ArrayList<CLICKABLE> ss = new ArrayList<CLICKABLE>(s);

                Add(new PlacableSimpleTile("camps")
                {
                    public override CharSequence IsPlacable(int tx, int ty)
                    {
                        return null;
                    }

                    int ni = 0;

                    public override void Place(int tx, int ty)
                    {
                        CharSequence nn = type.Get().names.GetC(ni++);

                        Str.TMP.Clear().Add(nn);
                        Str.TMP.Insert(0, type.Get().race.appearance().lastNamesNoble.GetC(RND.rInt(0x0FFFF)));

                        WORLD.ENTITIES().havens.Create(tx, ty, type.Get(), inte.GetD(), nn);
                    }

                    public override LIST<CLICKABLE> GetAdditionalButt()
                    {
                        return ss;
                    }

                    public override PLACABLE GetUndo()
                    {
                        return undo;
                    }

                    public override SPRITE GetIcon()
                    {
                        return WORLD.ENTITIES().havens.types.Get(0).race.appearance().icon;
                    }
                });
            }
        }
    }
}