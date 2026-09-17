using game.faction.FACTIONS;
using game.faction.diplomacy.DIP;
using game.faction.diplomacy.deal;
using game.faction.npc;
using game.faction.royalty.opinion;
using init.race.KingMessages;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using view.ui.diplomacy;
using view.ui.message;
using world.map.regions;

class WarMessages
{
    private static string ¤¤War = "War!";
    private static string ¤¤WarD = "The enemy has shown itself. Let us muster and fight!";
    public static void start(FactionNPC f)
    {
        Message m = f.king().induvidual.race().kingMessage().WAR_NORMAL;
        if (DIP.VASSAL().is(f))
        {
            m = f.king().induvidual.race().kingMessage().WAR_VASSAL;
        }
        else if (DIP.get(f).ally)
        {
            m = f.king().induvidual.race().kingMessage().WAR_ALLY;
        }
        new UIDipMess(¤¤War, m.get(f), ¤¤WarD, f).send();
    }

    private static string ¤¤Crusade = "More enemies";
    private static string ¤¤CrusadeD = "This faction joins our enemies.";
    private static string ¤¤CrusadeD2 = "This faction of {0} and {1} have now banded together against you. They call themselves: {1}.";
    public static void join(FactionNPC f)
    {
        Message m = f.king().induvidual.race().kingMessage().WAR_JOIN_NORMAL;
        if (DIP.VASSAL().is(f))
        {
            m = f.king().induvidual.race().kingMessage().WAR_JOIN_VASSAL;
        }
        else if (DIP.get(f).ally)
        {
            m = f.king().induvidual.race().kingMessage().WAR_JOIN_ALLY;
        }

        string dd = ¤¤CrusadeD;
        if (DIP.WAR().all(FACTIONS.player()).size() == 2)
        {
            string name1 = DIP.WAR().all(FACTIONS.player()).get(0).name;
            string name2 = DIP.WAR().all(FACTIONS.player()).get(1).name;
            string teamName = DIP.WAR_PLAYER().teamName;
            dd = string.Format(¤¤CrusadeD2, name1, name2, teamName);
        }

        new UIDipMess(¤¤Crusade, m.get(f), dd, f).send();
    }

    private static string ¤¤WarByProxy = "Proxy war!";
    private static string ¤¤WarByProxyD = "My lord, even in distant places they manage to hate our freedom and way of life. The distant faction {0} has aided our mortal enemy, {1}, bolstering their armies!";
    public static void proxy(FactionNPC atWar, FactionNPC supplier, int am)
    {
        new UIDipMess(¤¤WarByProxy, string.Format(¤¤WarByProxyD, supplier.name, atWar.name), null, atWar).send();
    }

    private static string ¤¤rumour = "¤Bad Rumours";
    private static string ¤¤rumourD = "¤It has come to our attention that a rumour has been spread. That you {0}. Now, while those close to you know this isn't true, it has affected your standing amongst the other factions of Syx significantly. We do not know who started the rumours, but we suspect the ruler of {1} might have had something to do with it. On the bright side, we now have more international support should we choose to attack this faction.";
    public static void poision(FactionNPC source)
    {
        new MessageText(¤¤rumour).paragraph(string.Format(¤¤rumourD, source.race().kingMessage().RUMOUR.rnd(), source.name)).send();
    }

    private static string ¤¤terror = "¤Terrorists";
    private static string ¤¤terrorD = "¤In the region of {0} there has emerged an organization calling themselves {1}. These are violently opposing your rule and destabilizing the affiliation of the region. Apparently, they have received backing and funds from elsewhere, we suspect the ruler of {2}. On the bright side, we now have more international support should we choose to attack this faction.";
    public static void ngo(FactionNPC source, Region target)
    {
        new MessageText(¤¤terror).paragraph(string.Format(¤¤terrorD, target.info.name(), source.race().kingMessage().NGO.rnd(), source.name)).send();
    }

    public static string ¤¤teachings = "¤Bad Teachings";
    private static string ¤¤teachingsD = "¤Someone has been spreading leaflets about the teachings of the Burnt Prophet, filling the heads of our subjects with ideas such as all are created equal, pacifism, self rule and other rubbish! As a result, our subjects loyalty will be diminished for some time. We do not know who is responsible to this, but the paper smells like it's from the faction on {0}. On the bright side, we now have more international support should we choose to attack this faction.";
    public static void teachings(FactionNPC source)
    {
        new MessageText(¤¤teachings).paragraph(string.Format(¤¤teachingsD, source.name)).send();
    }

    private static string ¤¤Demand = "Demand";
    private static string ¤¤DemandD = "Failure to comply with this request can have unforeseen consequences.";
    public static void warn(FactionNPC f)
    {
        if (DIP.VASSAL().is(f))
        {
            new VassalRequest(f).send();
        }
        else
        {
            Deal d = DIP.TMP();
            d.setFactionAndClear(f);
            DealDrawfter.draft(d, -d.player.offerableWorth() * (0.2 + RND.rFloat() * 0.2), true, true);

            if (d.hasDeal())
            {
                Message m = f.king().induvidual.race().kingMessage().THREAT_NORMAL;
                if (DIP.get(f).transit)
                    m = f.king().induvidual.race().kingMessage().THREAT_ALLY;

                double v = ROPINION.GIFTS().getGenerosityNeededForPeace(f);
                new UIDipMessDeal(¤¤Demand, m.get(f), ¤¤DemandD, d, v, -0.5).send();
            }
        }
    }

    private static string ¤¤breakTitle = "¤Freedom request";
    private static string ¤¤breakBody = "¤This faction asks that you release them from their bounds. The faction will become your colleague, and the faction will be very grateful should you accept. Decline and they might get bad ideas.";
    private class VassalRequest : UIDipMessAction
    {
        public VassalRequest(FactionNPC f) : base(¤¤breakTitle, f.king().induvidual.race().kingMessage().THREAT_VASSAL.get(f), ¤¤breakBody, f, f, 1, -1)
        {
        }

        protected override void accept(FactionNPC f, FactionNPC o)
        {
            DIP.PACT().set(f);
            ROPINION.OTHER().liberate(f);
        }

        protected override bool valid(FactionNPC f, FactionNPC o)
        {
            return DIP.VASSAL().is(f);
        }
    }

    static WarMessages()
    {
        D.ts(typeof(WarMessages));
    }
}