using System;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using util.text;
using view.main;
using view.world.generator.tools;
using world;

namespace view.world.generator
{
    class StageTerrain
    {
        static readonly CharSequence ¤¤title = "Generate Terrain";
        static readonly CharSequence ¤¤warning = "Regenerating terrain will reset your current world. Proceed?";

        static StageTerrain()
        {
            D.ts(typeof(StageTerrain));
        }

        public StageTerrain(WorldViewGenerator stages)
        {
            stages.Reset();
            GuiSection s = new UIWorldGenerateTerrain(WORLD.GEN());
            s.Body().CenterIn(C.DIM());

            GuiSection ss = new GuiSection();

            ACTION generate = new ACTION()
            {
                public void Exe()
                {
                    if (WORLD.GEN().playerX != -1)
                    {
                        StageCapitol.Clear();
                    }

                    stages.Reset();

                    WORLD.TERRAIN().Saver().Generate(WorldViewGenerator.loadPrint);
                    WORLD.LANDMARKS().Saver().Generate(WorldViewGenerator.loadPrint);
                    WorldViewGenerator.loadPrint.Exe();
                    MINIMAP().Repaint();
                    WorldViewGenerator.loadPrint.Exe();

                    WORLD.GEN().hasGeneratedTerrain = true;
                    stages.Set();
                }
            };

            ss.AddRightC(2, new GButt.ButtPanel(WorldViewGenerator.¤¤generate)
            {
                protected override void ClickA()
                {
                    if (WORLD.GEN().playerX != -1)
                    {
                        VIEW.inters().yesNo.Activate(¤¤warning, generate, ACTION.NOP, true);
                    }
                    else
                    {
                        generate.Exe();
                    }
                }
            });

            if (WORLD.GEN().hasGeneratedTerrain)
            {
                ss.AddRightC(2, new GButt.ButtPanel(Dic.¤¤cancel)
                {
                    protected override void ClickA()
                    {
                        stages.Set();
                    }
                });
            }

            s.AddRelBody(8, DIR.S, ss);

            stages.dummy.Add(s, UIWorldGenerateTerrain.¤¤MapType);
        }
    }
}