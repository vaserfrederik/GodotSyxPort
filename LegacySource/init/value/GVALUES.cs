using System;
using game;
using game.faction;
using game.faction.royalty;
using init;
using settlement.stats;
using snake2d.util.misc;
using view.interrupter;
using view.main;
using view.ui.util;
using world.map.regions;

namespace init.value
{
    public class GVALUES : InitResource
    {
        public static readonly string KEY = "VALUE";
        public static readonly GValueCat<Induvidual> INDU = new GValueCat<Induvidual>("HUMAN");
        public static readonly GValueCat<Region> REGION = new GValueCat<Region>("REGION");
        public static readonly GValueCat<Faction> FACTION = new GValueCat<Faction>("FACTION");
        public static readonly GValueCat<Royalty> ROYALTY = new GValueCat<Royalty>("ROYALTY");

        static GVALUES()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    INDU.Clear();
                    REGION.Clear();
                    FACTION.Clear();
                    ROYALTY.Clear();
                }
            };
        }

        public GVALUES(INIT init) : base(init)
        {
            INDU.Clear();
            REGION.Clear();
            FACTION.Clear();
            ROYALTY.Clear();
        }

        protected override void FinishSetup()
        {
            GValuesInit.Init();
            INDU.Init();
            REGION.Init();
            FACTION.Init();
            ROYALTY.Init();
            IDebugPanel.Add("values", new ACTION
            {
                public override void Exe()
                {
                    GETTER<Faction> g = new GETTER<Faction>
                    {
                        public override Faction Get()
                        {
                            return FACTIONS.Player();
                        }
                    };
                    VIEW.Inters().Popup.Show(new UIValues<Faction>(FACTION, g), null);
                }
            });
        }
    }
}