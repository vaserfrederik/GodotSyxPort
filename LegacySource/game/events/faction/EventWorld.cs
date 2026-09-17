using game.events.faction.player;

namespace game.events.faction
{
    public sealed class EventWorld
    {
        public readonly EventFactionExpand factionExpand = new EventFactionExpand();
        public readonly EventFactionCollapse factionBreak = new EventFactionCollapse();
        public readonly EventFactionPopup popup = new EventFactionPopup();
        public readonly EventFactionWar war = new EventFactionWar();
        public readonly EventFactionPeace warPeace = new EventFactionPeace();
        public readonly EventDiplomacy dip = new EventDiplomacy();
    }
}