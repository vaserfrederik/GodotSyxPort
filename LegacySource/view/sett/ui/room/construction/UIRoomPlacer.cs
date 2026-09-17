using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.gui.misc;
using util.text;

namespace view.sett.ui.room.construction
{
    public class UIRoomPlacer
    {
        private readonly State state;

        public UIRoomPlacer()
        {
            state = new State();
        }

        public void Init(RoomBlueprintImp b, int tx, int ty)
        {
            if (b.cat == SETT.ROOMS().CATS.DECOR)
                state.Init(b, b.cat);
            else
                state.Init(b, false);

            state.placement.placer.Init(b, 0);
            if (b.constructor().UsesArea())
                VIEW.s().tools.Place(state.placement.placer.Area(), state.config);
            else
            {
                if (b.constructor().IsSpecialAreaPlacable())
                {
                    if (b.constructor().Groups().Size() == 1 && b.constructor().Groups().Get(0).Rotations() == 1 && b.constructor().Groups().Size() == 1)
                    {
                        if (b.Employment() == null)
                        {
                            ipla.Set(b);
                            VIEW.s().tools.Place(ipla, null);
                            return;
                        }
                    }
                    else
                        throw new RuntimeException();
                }

                FurnisherItem it = SETT.ROOMS().fData.item.Get(tx, ty);
                if (it != null)
                {
                    PlacableFixed pp = state.placement.placer.Item(it.Group.Index());
                    pp.RotSet(it.Rotation);
                    int size = 0;
                    for (int i = 0; i < it.Group.Size(); i++)
                    {
                        if (it.Group.Item(i, it.Rotation) == it)
                        {
                            size = i;
                            break;
                        }
                    }
                    pp.SizeSet(size);
                    state.SetItem(it.Group().Index());
                    VIEW.s().tools.Place(pp, state.config);
                }
                else
                {
                    PlacableFixed pp = state.placement.placer.Item(state.Item());
                    VIEW.s().tools.Place(pp, state.config);
                }
            }
        }

        public void Init(RoomBlueprintImp b, RoomCategorySub bb)
        {
            state.Init(b, bb);

            state.placement.placer.Init(b, 0);
            if (b.constructor().UsesArea())
                VIEW.s().tools.Place(state.placement.placer.Area(), state.config);
            else
            {
                PlacableFixed pp = state.placement.placer.Item(state.Item());
                VIEW.s().tools.Place(pp, state.config);
            }
        }

        public void Init(RoomInstanceImp ins)
        {
            state.Init(ins.constructor().Blue(), true);

            RoomBlueprintImp b = ins.constructor().Blue();
            RoomState stat = ins.MakeState(ins.mX(), ins.mY(), false);
            int up = ins.Upgrade();
            int deg = ins.Degraded(ins.mX(), ins.mY()) == null ? 0 : ins.Degraded(ins.mX(), ins.mY()).GetData();
            TmpArea a = ins.Remove(ins.mX(), ins.mY(), false, this, false);
            if (a == null)
                return;
            if (a.Area() <= 0)
            {
                a.Clear();
                return;
            }

            SETT.ROOMS().placement.placer.Reconstruct(a, up, deg, stat, b);

            VIEW.s().tools.Place(state.placement.placer.Area(), state.config);
        }

        public void Init(int tx, int ty)
        {
            if (state.placement.CanReconstruct(tx, ty))
            {
                RoomInstanceImp r = (RoomInstanceImp)SETT.ROOMS().map.Get(tx, ty);
                Init(r);
            }
        }

        public bool IsActive(RoomBlueprintImp b)
        {
            return VIEW.s().tools.ConfigCurrent() == state.config && b == this.state.b;
        }

        public bool IsActive()
        {
            return VIEW.s().tools.ConfigCurrent() == state.config;
        }

        public RoomBlueprintImp Blue()
        {
            if (VIEW.s().tools.ConfigCurrent() == state.config)
            {
                return this.state.b;
            }
            return null;
        }

        private readonly PlacerItemSingleArea ipla = new PlacerItemSingleArea();

        private class PlacerItemSingleArea : PlacableMulti
        {
            private RoomBlueprintImp blueprint;
            private readonly ArrayList<CLICKABLE> li = new ArrayList<CLICKABLE>(5);

            private readonly CLICKABLE bOverlay = new GButt.ButtPanel(UI.icons().s.eye.Sized(Icon.M))
            {
                protected override void ClickA()
                {
                    SETT.ROOMS().placement.placer.ShowOverlay.Toggle();
                }

                protected override void RenAction()
                {
                    SelectedSet(SETT.ROOMS().placement.placer.ShowOverlay.Is());
                }

                public override void HoverInfoGet(GUI_BOX text)
                {
                    text.Title(Dic.¤¤Overlay);
                    SETT.ROOMS().placement.placer.Structure.Get();
                    if (blueprint.constructor().Overlay() != null && blueprint.constructor().Overlay().Desc != null)
                    {
                        text.Text(blueprint.constructor().Overlay().Desc);
                    }
                }
            };

            private readonly CLICKABLE bFoundation = new GButt.ButtPanel(SETT.OVERLAY().FOUNDATION.icon.Resized(Icon.M))
            {
                protected override void ClickA()
                {
                    SETT.ROOMS().placement.placer.ShowFoundation.Toggle();
                }

                protected override void RenAction()
                {
                    SelectedSet(SETT.ROOMS().placement.placer.ShowFoundation.Is());
                }

                public override void HoverInfoGet(GUI_BOX text)
                {
                    text.Title(SETT.OVERLAY().FOUNDATION.name);
                    text.Text(SETT.OVERLAY().FOUNDATION.desc);
                }
            };

            public PlacerItemSingleArea() : base("")
            {
            }

            public void Set(RoomBlueprintImp b)
            {
                this.blueprint = b;
            }

            public override CharSequence Name()
            {
                return blueprint.info.names;
            }

            public override CharSequence IsPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                return Init().Placable(tx, ty, 0, 0);
            }

            public override void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Init().Place(tx, ty, 0, 0);
            }

            public override PLACABLE GetUndo()
            {
                return Init().GetUndo();
            }

            public override void PlaceInfo(GBox b, int oktiles, AREA a)
            {
                for (int i = 0; i < blueprint.constructor().Resources(); i++)
                {
                    if (blueprint.constructor().Groups().Get(0).Item(0, 0).Cost2(i, 0) > 0)
                    {
                        b.SetResource(blueprint.constructor().Resource(i), oktiles * Math.Ceiling(blueprint.constructor().Groups().Get(0).Item(0, 0).Cost2(i, 0)));
                        b.Space();
                    }
                }
                base.PlaceInfo(b, oktiles, a);
            }

            private PlacableFixed Init()
            {
                state.placement.placer.Init(blueprint, 0);
                PlacableFixed pp = state.placement.placer.Item(0);
                return pp;
            }

            public override void UpdateRegardless(GameWindow window, AREA selected)
            {
                if (blueprint.constructor().Overlay() != null && state.placement.placer.ShowOverlay.Is())
                    blueprint.constructor().Overlay().Add();
                if (blueprint.constructor().IsHeavy() && state.placement.placer.ShowFoundation.Is())
                    SETT.OVERLAY().FOUNDATION.Add();
            }

            public override void RenderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA area,
                PLACER_TYPE type, bool isPlacable, bool areaIsPlacable)
            {
                base.RenderPlaceHolder(r, mask, x, y, tx, ty, area, type, isPlacable, areaIsPlacable);
                blueprint.constructor().RenderExtra(r, x, y, tx, ty, 0, 0, blueprint.constructor().Groups().Get(0).Item(0, 0));
            }

            public override LIST<CLICKABLE> GetAdditionalButt()
            {
                li.ClearSloppy();
                if (blueprint.constructor().Overlay() != null)
                    li.Add(bOverlay);
                if (blueprint.constructor().IsHeavy())
                    li.Add(bFoundation);
                return base.GetAdditionalButt();
            }
        }

        public void Reconstruct(RoomInstance r)
        {
            Init(r);
        }

        public void Reconstruct(int tx, int ty)
        {
            Init(tx, ty);
        }

        public void Reconstruct(RoomBlueprintImp b)
        {
            Init(b, -1, -1);
        }
    }
}