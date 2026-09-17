using System;
using System.Collections.Generic;
using System.Linq;
using game.faction;
using game.time;
using init.sprite;
using settlement.main;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.main;
using view.sett.ui.room;

namespace settlement.room.food.farm
{
    class Gui : UIRoomModuleImp<FarmInstance, ROOM_FARM>
    {
        private static readonly string ¤¤estimated = "¤Estimated Harvest (year)";
        private static readonly string ¤¤daysToHarvest = "¤Days until harvest";
        private static readonly string ¤¤baseValue = "¤Base Value";
        private static readonly string ¤¤workValue = "¤Work Value";
        private static readonly string ¤¤workValueD = "¤Each day of the year a farm needs tending to. Workers failing to tend to all the tiles will lead to low yields.";

        private static readonly string ¤¤HarvestYear = "¤This Year";
        private static readonly string ¤¤HarvestPrev = "¤Last Year";

        private static readonly string ¤¤skill = "¤Bonus";
        private static readonly string ¤¤skillD = "¤The average bonus accumulated during the year. This determines the output of the harvest.";

        private static readonly string ¤¤skillCurrent = "¤Bonus (current)";
        private static readonly string ¤¤skillCurrentD = "¤The bonus that is currently being added to the Farm.";

        private static readonly string ¤¤cycle = "¤Cycle";
        private static readonly string ¤¤reseed = "¤reseed";
        private static readonly string ¤¤reseedD = "¤Reseed the farm with another crop. New farm must still be constructed.";

        private static readonly string ¤¤Farmer = "¤Farmers have no storage nearby to store their harvest.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_FARM s) : base(s)
        {
        }

        private GuiSection rebuilds = new GuiSection();

        public override void hover(GBox box, FarmInstance i)
        {
            base.hover(box, i);
            box.NL();
            box.text(blueprint.constructor.fertility.name());
            box.add(GFORMAT.perc(box.text(), blueprint.constructor.fertility.get(i)));

            box.NL();
            box.text(¤¤estimated);
            box.add(GFORMAT.i(box.text(), (int)Math.Ceiling(Util.prospect(i))));

            box.space();
        }

        protected override void problem(FarmInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            if (i.tData.shouldStore() && i.storeTimeout)
            {
                warnings.add(¤¤Farmer);
            }
            base.problem(i, free, errors, warnings);
        }

        protected override void appendMain(GGrid icons, GGrid text, GuiSection sExtra)
        {
            IndustryResource res = blueprint.industries()[0].outs()[0];

            text.add(new GHeader(Dic.¤¤Production));

            GuiSection s = new GuiSection()
            {
                public void hoverInfoGet(GUI_BOX text)
                {
                    text.title(¤¤estimated);
                }
            };

            s.add(res.resource.icon(), 0, 0);

            s.addRightC(4, new GStat()
            {
                public void update(GText text)
                {
                    int am = 0;
                    for (int i = 0; i < blueprint.instancesSize(); i++)
                    {
                        FarmInstance ins = blueprint.getInstance(i);
                        am += (int)Math.Ceiling(Util.prospect(ins));
                    }
                    GFORMAT.i(text, am);
                }
            });

            s.addRightC(64, new GStat()
            {
                public void update(GText text)
                {
                    int prev = 0;
                    int am = 0;
                    for (int i = 0; i < blueprint.instancesSize(); i++)
                    {
                        FarmInstance ins = blueprint.getInstance(i);
                        prev += Util.prevHarvest(ins);
                        am += (int)Math.Ceiling(Util.prospect(ins));
                    }
                    GFORMAT.iIncr(text, am - prev);
                }
            });

            text.add(s);

            GStaples st = new GStaples(res.history().historyRecords())
            {
                protected override void hover(GBox box, int stapleI)
                {
                    int i = res.history().historyRecords() - 1 - stapleI;
                    int am = res.history().get(i);
                    GText t = box.text();
                    DicTime.setDaysAgo(t, i);
                    box.add(t);
                    box.NL(2);
                    box.add(GFORMAT.i(box.text(), am));
                }

                protected override double getValue(int stapleI)
                {
                    return res.history().get(stapleI);
                }
            };

            text.add(st);
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }

        protected override void appendTableSorts(LISTE<GTSort<RoomInstance>> sorts)
        {
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters)
        {
        }

        protected override void appendTableAppliers(LISTE<UIRoomBulkApplier> appliers)
        {
        }

        protected override void appendTableColumns(LISTE<ITableColumn<RoomInstance>> columns)
        {
        }

        protected override void appendTableHeaders(LISTE<ITableHeader> headers)
        {
        }

        protected override void appendTable(LISTE<RoomInstance> instances, GuiTable table)
        {
        }

        protected override void appendTable(GuiTable table)
        {
        }

        protected override void appendTable(GuiPanel panel)
        {
        }

        protected override void appendTable(GuiSection section)
        {
        }

        protected override void appendTable(GuiButton button)
        {
        }

        protected override void appendTable(GuiLabel label)
        {
        }

        protected override void appendTable(GuiText text)
        {
        }

        protected override void appendTable(GuiBox box)
        {
        }

        protected override void appendTable(GuiSlider slider)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiTree tree)
        {
        }

        protected override void appendTable(GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiImage image)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiTable table)
        {
        }

        protected override void appendTable(GuiSection section, GuiTable table)
        {
        }

        protected override void appendTable(GuiButton button, GuiTable table)
        {
        }

        protected override void appendTable(GuiLabel label, GuiTable table)
        {
        }

        protected override void appendTable(GuiText text, GuiTable table)
        {
        }

        protected override void appendTable(GuiBox box, GuiTable table)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiTable table)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiTable table)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiTable table)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiTable table)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiTable table)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiTable table)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiTable table)
        {
        }

        protected override void appendTable(GuiTree tree, GuiTable table)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiTable table)
        {
        }

        protected override void appendTable(GuiImage image, GuiTable table)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiSection section)
        {
        }

        protected override void appendTable(GuiSection section, GuiSection section2)
        {
        }

        protected override void appendTable(GuiButton button, GuiSection section)
        {
        }

        protected override void appendTable(GuiLabel label, GuiSection section)
        {
        }

        protected override void appendTable(GuiText text, GuiSection section)
        {
        }

        protected override void appendTable(GuiBox box, GuiSection section)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiSection section)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiSection section)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiSection section)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiSection section)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiSection section)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiSection section)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiSection section)
        {
        }

        protected override void appendTable(GuiTree tree, GuiSection section)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiSection section)
        {
        }

        protected override void appendTable(GuiImage image, GuiSection section)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiButton button)
        {
        }

        protected override void appendTable(GuiSection section, GuiButton button)
        {
        }

        protected override void appendTable(GuiButton button1, GuiButton button2)
        {
        }

        protected override void appendTable(GuiLabel label, GuiButton button)
        {
        }

        protected override void appendTable(GuiText text, GuiButton button)
        {
        }

        protected override void appendTable(GuiBox box, GuiButton button)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiButton button)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiButton button)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiButton button)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiButton button)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiButton button)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiButton button)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiButton button)
        {
        }

        protected override void appendTable(GuiTree tree, GuiButton button)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiButton button)
        {
        }

        protected override void appendTable(GuiImage image, GuiButton button)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiLabel label)
        {
        }

        protected override void appendTable(GuiSection section, GuiLabel label)
        {
        }

        protected override void appendTable(GuiButton button, GuiLabel label)
        {
        }

        protected override void appendTable(GuiLabel label1, GuiLabel label2)
        {
        }

        protected override void appendTable(GuiText text, GuiLabel label)
        {
        }

        protected override void appendTable(GuiBox box, GuiLabel label)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiLabel label)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiLabel label)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiLabel label)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiLabel label)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiLabel label)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiLabel label)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiLabel label)
        {
        }

        protected override void appendTable(GuiTree tree, GuiLabel label)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiLabel label)
        {
        }

        protected override void appendTable(GuiImage image, GuiLabel label)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiText text)
        {
        }

        protected override void appendTable(GuiSection section, GuiText text)
        {
        }

        protected override void appendTable(GuiButton button, GuiText text)
        {
        }

        protected override void appendTable(GuiLabel label, GuiText text)
        {
        }

        protected override void appendTable(GuiText text1, GuiText text2)
        {
        }

        protected override void appendTable(GuiBox box, GuiText text)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiText text)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiText text)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiText text)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiText text)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiText text)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiText text)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiText text)
        {
        }

        protected override void appendTable(GuiTree tree, GuiText text)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiText text)
        {
        }

        protected override void appendTable(GuiImage image, GuiText text)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiBox box)
        {
        }

        protected override void appendTable(GuiSection section, GuiBox box)
        {
        }

        protected override void appendTable(GuiButton button, GuiBox box)
        {
        }

        protected override void appendTable(GuiLabel label, GuiBox box)
        {
        }

        protected override void appendTable(GuiText text, GuiBox box)
        {
        }

        protected override void appendTable(GuiBox box1, GuiBox box2)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiBox box)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiBox box)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiBox box)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiBox box)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiBox box)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiBox box)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiBox box)
        {
        }

        protected override void appendTable(GuiTree tree, GuiBox box)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiBox box)
        {
        }

        protected override void appendTable(GuiImage image, GuiBox box)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiSection section, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiButton button, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiLabel label, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiText text, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiBox box, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiSlider slider1, GuiSlider slider2)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiTree tree, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiImage image, GuiSlider slider)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiSection section, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiButton button, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiLabel label, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiText text, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiBox box, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar1, GuiProgressBar progressBar2)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiTree tree, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiImage image, GuiProgressBar progressBar)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiSection section, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiButton button, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiLabel label, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiText text, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiBox box, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox1, GuiCheckbox checkbox2)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiTree tree, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiImage image, GuiCheckbox checkbox)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiSection section, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiButton button, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiLabel label, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiText text, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiBox box, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton1, GuiRadioButton radioButton2)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiTree tree, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiImage image, GuiRadioButton radioButton)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiSection section, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiButton button, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiLabel label, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiText text, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiBox box, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiDropdown dropdown1, GuiDropdown dropdown2)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiTree tree, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiImage image, GuiDropdown dropdown)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiSection section, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiButton button, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiLabel label, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiText text, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiBox box, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiTextInput textInput1, GuiTextInput textInput2)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiTree tree, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiImage image, GuiTextInput textInput)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiSection section, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiButton button, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiLabel label, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiText text, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiBox box, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiListBox listBox1, GuiListBox listBox2)
        {
        }

        protected override void appendTable(GuiTree tree, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiImage image, GuiListBox listBox)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiTree tree)
        {
        }

        protected override void appendTable(GuiSection section, GuiTree tree)
        {
        }

        protected override void appendTable(GuiButton button, GuiTree tree)
        {
        }

        protected override void appendTable(GuiLabel label, GuiTree tree)
        {
        }

        protected override void appendTable(GuiText text, GuiTree tree)
        {
        }

        protected override void appendTable(GuiBox box, GuiTree tree)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiTree tree)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiTree tree)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiTree tree)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiTree tree)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiTree tree)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiTree tree)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiTree tree)
        {
        }

        protected override void appendTable(GuiTree tree1, GuiTree tree2)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiTree tree)
        {
        }

        protected override void appendTable(GuiImage image, GuiTree tree)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiSection section, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiButton button, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiLabel label, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiText text, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiBox box, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiTree tree, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiCanvas canvas1, GuiCanvas canvas2)
        {
        }

        protected override void appendTable(GuiImage image, GuiCanvas canvas)
        {
        }

        protected override void appendTable(GuiPanel panel, GuiImage image)
        {
        }

        protected override void appendTable(GuiSection section, GuiImage image)
        {
        }

        protected override void appendTable(GuiButton button, GuiImage image)
        {
        }

        protected override void appendTable(GuiLabel label, GuiImage image)
        {
        }

        protected override void appendTable(GuiText text, GuiImage image)
        {
        }

        protected override void appendTable(GuiBox box, GuiImage image)
        {
        }

        protected override void appendTable(GuiSlider slider, GuiImage image)
        {
        }

        protected override void appendTable(GuiProgressBar progressBar, GuiImage image)
        {
        }

        protected override void appendTable(GuiCheckbox checkbox, GuiImage image)
        {
        }

        protected override void appendTable(GuiRadioButton radioButton, GuiImage image)
        {
        }

        protected override void appendTable(GuiDropdown dropdown, GuiImage image)
        {
        }

        protected override void appendTable(GuiTextInput textInput, GuiImage image)
        {
        }

        protected override void appendTable(GuiListBox listBox, GuiImage image)
        {
        }

        protected override void appendTable(GuiTree tree, GuiImage image)
        {
        }

        protected override void appendTable(GuiCanvas canvas, GuiImage image)
        {
        }

        protected override void appendTable(GuiImage image1, GuiImage image2)
        {
        }

        public override void Draw()
        {
            ImGui.Begin("Settings");
            ImGui.Text("Settings");
            ImGui.Separator();
            
            if (ImGui.BeginTabBar("SettingsTabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    DrawGeneralSettings();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Audio"))
                {
                    DrawAudioSettings();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Video"))
                {
                    DrawVideoSettings();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Advanced"))
                {
                    DrawAdvancedSettings();
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        private void DrawGeneralSettings()
        {
            ImGui.Text("General Settings");
            ImGui.Separator();
            
            ImGui.Checkbox("Enable Notifications", ref Settings.EnableNotifications);
            ImGui.Checkbox("Auto Update", ref Settings.AutoUpdate);
            ImGui.SliderInt("Language", ref Settings.Language, 0, 2, "Language: {0}");
        }

        private void DrawAudioSettings()
        {
            ImGui.Text("Audio Settings");
            ImGui.Separator();
            
            ImGui.Checkbox("Enable Sound Effects", ref Settings.EnableSoundEffects);
            ImGui.SliderFloat("Volume", ref Settings.Volume, 0.0f, 1.0f, "Volume: {0:0.00}");
            ImGui.Checkbox("Mute Music", ref Settings.MuteMusic);
        }

        private void DrawVideoSettings()
        {
            ImGui.Text("Video Settings");
            ImGui.Separator();
            
            ImGui.Checkbox("Fullscreen", ref Settings.Fullscreen);
            ImGui.SliderInt("Resolution", ref Settings.Resolution, 0, 3, "Resolution: {0}");
            ImGui.SliderInt("Brightness", ref Settings.Brightness, 0, 100, "Brightness: {0}");
            ImGui.SliderInt("Contrast", ref Settings.Contrast, 0, 100, "Contrast: {0}");
        }

        private void DrawAdvancedSettings()
        {
            ImGui.Text("Advanced Settings");
            ImGui.Separator();
            
            ImGui.Checkbox("Enable Developer Mode", ref Settings.EnableDeveloperMode);
            ImGui.Checkbox("Enable Debug Logs", ref Settings.EnableDebugLogs);
            ImGui.SliderInt("Debug Level", ref Settings.DebugLevel, 0, 5, "Debug Level: {0}");
            ImGui.Checkbox("Enable Experimental Features", ref Settings.EnableExperimentalFeatures);
        }
    }
}