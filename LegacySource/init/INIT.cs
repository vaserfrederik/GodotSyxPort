using System;
using System.Collections.Generic;
using System.IO;
using game.boosting;
using game.save;
using init.constant;
using init.race;
using init.religion;
using init.resources;
using init.sprite.UI;
using init.structure;
using init.tech;
using init.trade;
using init.type;
using init.value;
using snake2d;
using snake2d.util.sets;

namespace init
{
    public class INIT
    {
        private static ArrayListGrower<Savable> savers = new ArrayListGrower<Savable>();

        private readonly ArrayListGrower<InitResource> resses = new ArrayListGrower<INIT.InitResource>();

        public INIT() throws IOException
        {
            savers.Clear();

            new Config(this);
            CORE.CheckIn();
            new UI(this);
            CORE.CheckIn();
            new GVALUES(this);
            CORE.CheckIn();
            new BOOSTING(this);
            CORE.CheckIn();
            new RACES();
            CORE.CheckIn();
            new TYPEINIT(this);
            CORE.CheckIn();
            new RESOURCES(this);
            CORE.CheckIn();
            new RELIGIONS(this);
            CORE.CheckIn();
            new TECHS();
            CORE.CheckIn();
            new STRUCTURES(this);
            CORE.CheckIn();
            new TR(this);
        }

        public static void AddSaver(Savable s)
        {
            savers.Add(s);
        }

        public LIST<Savable> Finish() throws IOException
        {
            foreach (InitResource ii in resses)
            {
                ii.FinishSetup();
            }

            return savers;
        }

        public static class InitResource
        {
            protected InitResource(INIT init)
            {
                init.resses.Add(this);
            }

            protected void FinishSetup() throws IOException
            {
            }
        }

        public static interface AfterInit
        {
            void Exe() throws IOException;
        }
    }
}