using System.Collections.Generic;
using snake2d.util.sets;
using view.sett;
using view.tool;

namespace settlement.misc.placers
{
    public class ComplexPlacers
    {
        public readonly ArrayList<PLACABLE> ALL;
        public readonly PlacableFixed landingParty;

        public ComplexPlacers()
        {
            new PlacerLanding();
            landingParty = PlacerLanding.get();
            ALL = new ArrayList<PLACABLE>(landingParty);

            IDebugPanelSett.add("complex", ALL);
        }
    }
}