using System;
using System.Collections.Generic;
using settlement.main;
using settlement.room.main;
using snake2d.util.gui;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    internal class ModuleInstance : ModuleMaker
    {
        private static readonly string ¤¤ACTIVATE = "¤Activate";
        private static readonly string ¤¤DEACTIVATE = "¤Deactivate";
        private static readonly string ¤¤UNREACHABLE = "¤Room is not reachable and will not work properly. Make sure there is a clear path to your throne.";
        private static readonly string ¤¤DEACTIVATED = "¤Deactivated!";
        private static readonly string ¤¤boost = "Average boost from technology, race, and room properties of all your subjects working in this profession.";

        static ModuleInstance()
        {
            D.ts(typeof(ModuleInstance));
        }

        public ModuleInstance(Init init)
        {
        }

        public void Make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is RoomBlueprintIns<?>)
            {
                l.Add(new I((RoomBlueprintIns<?>) p));
            }
        }

        private sealed class I : UIRoomModule
        {
            private readonly RoomBlueprintIns<?> _blue;

            public I(RoomBlueprintIns<?> blue)
            {
                _blue = blue;
            }

            public override void AppendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
            {
                if (_blue.Employment != null || _blue == SETT.ROOMS().DUMP)
                {
                    appliers.Add(new UIRoomBulkApplier(¤¤ACTIVATE)
                    {
                        protected override void Apply(RoomInstance t)
                        {
                            t.Activate(true);
                        }
                    });

                    appliers.Add(new UIRoomBulkApplier(¤¤DEACTIVATE)
                    {
                        protected override void Apply(RoomInstance t)
                        {
                            t.Activate(false);
                        }
                    });
                }
            }

            public override void AppendManageScr(GGrid icons, GGrid text, GuiSection extra)
            {
                if (_blue.Bonus == null)
                    return;

                GStat s = new GStat()
                {
                    public override void Update(GText text)
                    {
                        GFORMAT.f1(text, SETT.RECIPES().player.Boost(_blue.Bonus), _blue.Bonus.BaseValue);
                    }
                };

                GButt.ButtPanel b = new GButt.ButtPanel(s)
                {
                    protected override void ClickA()
                    {
                        VIEW.UI().Tech.Activate();
                        VIEW.UI().Tech.Filter(_blue.Info.Name);
                    }

                    public override void HoverInfoGet(GUI_BOX text)
                    {
                        text.Text(¤¤boost);
                        base.HoverInfoGet(text);
                    }
                };

                b.Icon(_blue.Bonus.Icon);
                b.Body.IncrW(48);

                icons.Add(b);

                base.AppendManageScr(icons, text, extra);
            }

            public override void Hover(GBox box, Room room, int rx, int ry)
            {
            }

            public override void Problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry)
            {
                RoomInstance i = (RoomInstance) room;
                if (!i.Reachable())
                    errors.Add(¤¤UNREACHABLE);
                if (!i.Active())
                    errors.Add(¤¤DEACTIVATED);
            }

            public override void AppendPanel(GuiSection section, GETTER<RoomInstance> get, int x1, int y1)
            {
                // TODO Auto-generated method stub
            }
        }
    }
}