using System;
using System.IO;
using game.GAME;
using game.debug;
using game.faction;
using game.faction.Faction;
using game.faction.npc;
using game.time;
using settlement.battle.invasion;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using view.interrupter;
using world;
using world.army;
using world.battle;
using world.entity.army;
using world.map.regions;

public sealed class RaidingCurrent : SAVABLE
{
    private enum STATE
    {
        WARNING,
        ALLY_HELP,
        WARNING_REJECTED,
        ARMY,
        INVADING,
        DEFEATED,
        VICTORY,
        STRANGENESS,
        ALLY_FIGHT,
        APPEAR_REGION
    }

    private Raider raider;
    private int currentArmy;
    private int ai;
    private double timer = 0;
    private STATE state;
    private int invadeRef;
    int iterration;
    private Coo appearCoo = new Coo();

    public override void Save(FilePutter file)
    {
        if (current() != null)
        {
            file.Bool(true);
            file.Object(raider);
            file.I(currentArmy);
            file.I(ai);
            file.D(timer);
            file.I((int)state);
            file.I(invadeRef);
            file.I(iterration);
            appearCoo.Save(file);
        }
        else
            file.Bool(false);
    }

    public override void Load(FileGetter file)
    {
        if (file.Bool())
        {
            raider = (Raider)file.Object(true);
            currentArmy = file.I();
            ai = file.I();
            timer = file.D();
            state = (STATE)file.I();
            invadeRef = file.I();
            iterration = file.I();
            appearCoo.Load(file);
        }
        else
        {
            raider = null;
        }
        if (raider == null)
            clear();
    }

    public RaidingCurrent()
    {
        new InvasionListener()
        {
            protected override void weirdness(int reference)
            {
                if (raider != null && state == STATE.INVADING && reference == invadeRef)
                {
                    state = STATE.STRANGENESS;
                    timer = 0;
                }
            }

            protected override void victory(int losses, int kills, int reference)
            {
                if (raider != null && state == STATE.INVADING && reference == invadeRef)
                {
                    GAME.raiders().defeat(raider);
                    state = STATE.DEFEATED;
                    timer = 0;
                }
            }

            protected override void defeat(int losses, int killsf, int reference)
            {
                if (raider != null && state == STATE.INVADING && reference == invadeRef)
                {
                    state = STATE.VICTORY;
                    timer = 0;
                }
            }

            protected override void register(WArmy a, int reference)
            {
                if (raider != null && a == army())
                {
                    currentArmy = -1;
                    state = STATE.INVADING;
                    invadeRef = reference;
                }
            }
        };

        new BattleListener()
        {
            public override void siege(Faction attacker, Region reg)
            {
            }

            public override void siege(WArmy attacker, Region reg)
            {
            }

            public override void battle(WArmy a, bool victory, int losses, int kills, Faction against)
            {
                if (a == army() && !victory && state == STATE.ARMY)
                {
                    GAME.raiders().defeat(raider);
                    state = STATE.DEFEATED;
                    timer = 0;
                }
            }

            public override void battle(Faction a, bool victory, int losses, int kills, Faction against)
            {
            }
        };

        IDebugPanel.Add("RAIDER next step", new ACTION()
        {
            public override void Exe()
            {
                timer += TIME.secondsPerDay() * 10;
            }
        });
    }

    public Raider current()
    {
        return raider;
    }

    public WArmy army()
    {
        if (raider == null)
            return null;
        if (currentArmy == -1)
            return null;
        WArmy a = WORLD.ENTITIES().armies.tryGet(currentArmy);
        if (a != null && a.added() && a.iteration() == ai)
        {
            return a;
        }
        return null;
    }

    private void set(Raider raider, STATE state, WArmy a)
    {
        if (this.raider != raider)
        {
            iterration++;
            raider.raids++;
        }

        timer = 0;
        this.raider = raider;
        this.state = state;
        if (a != null)
        {
            currentArmy = a.armyIndex();
            ai = a.iteration();
        }
        else
        {
            currentArmy = -1;
            ai = -1;
        }
    }

    public override void clear()
    {
        if (raider != null)
        {
            if (state == STATE.INVADING)
            {
                SETT.INVADOR().cancel(invadeRef);
            }
            WArmy a = army();
            if (a != null)
                a.disband();
        }
        raider = null;
        timer = 0;
        currentArmy = -1;
    }

    public void raid(Raider raider)
    {
        clear();

        COORDINATE c = GAME.raiders().util.attackSpot(raider);
        if (c == null)
        {
            LOG.ln("no spots");
            return;
        }

        appearCoo.set(c);

        FactionNPC fa = RaidingMap.passThroughFaction(c);

        if (fa != null)
        {
            set(raider, STATE.ALLY_HELP, null);
            MessAlly.help(raider, fa);
            return;
        }

        set(raider, STATE.WARNING, null);
        new MessDemand(raider).send();
    }

    public void raid(Raider raider, CharSequence text)
    {
        clear();

        COORDINATE c = GAME.raiders().util.attackSpot(raider);
        if (c == null)
        {
            LOG.ln("no spots");
            return;
        }

        appearCoo.set(c);

        FactionNPC fa = RaidingMap.passThroughFaction(c);

        if (fa != null)
        {
            set(raider, STATE.ALLY_HELP, null);
            MessAlly.help(raider, fa);
            return;
        }

        set(raider, STATE.WARNING, null);
        new MessDemand(raider).send();
    }

    public void update(double delta)
    {
        timer += delta;

        switch (state)
        {
            case STATE.WARNING:
                if (timer > TIME.secondsPerDay())
                {
                    set(raider, STATE.WARNING_REJECTED, null);
                    new MessDemandRejected(raider).send();
                }
                break;
            case STATE.WARNING_REJECTED:
                if (timer > 30)
                {
                    Region reg = WORLD.REGIONS().map.get(appearCoo.x(), appearCoo.y());
                    if (reg == null || !isValid(appearCoo.x(), appearCoo.y()) || reg.capitol())
                    {
                        invadeRef = raider.army.invade(appearCoo.x(), appearCoo.y(), raider.indu);
                        set(raider, STATE.INVADING, null);
                    }
                    else
                    {
                        set(raider, STATE.APPEAR_REGION, null);
                        new MessArmySpotted(raider, appearCoo.x(), appearCoo.y()).send();
                    }
                }
                break;
            case STATE.APPEAR_REGION:
                if (timer > TIME.secondsPerDay() * 3)
                {
                    appear(raider, appearCoo.x(), appearCoo.y());
                }
                break;
            case STATE.ARMY:
                if (army() == null)
                {
                    new MessGoingAway(raider).send();
                    clear();
                }
                else
                {
                    WArmy a = army();
                    if (a == null)
                    {
                        new MessGoingAway(raider).send();
                        clear();
                        break;
                    }
                    AD.updateArmy(army());
                    if (a.state() == WArmyState.fortifying || a.state() == WArmyState.fortified)
                    {
                        if (timer > 60 * 4)
                        {
                            new MessGoingAway(raider).send();
                            clear();
                        }
                    }
                    else
                        timer = 0;
                }
                break;
            case STATE.DEFEATED:
                if (timer > 20)
                {
                    GAME.raiders().defeat(raider);
                    new MessDefeated(raider).send();
                    clear();
                }
                break;
            case STATE.INVADING:
                if (!SETT.INVADOR().invading())
                {
                    new MessGoingAway(raider).send();
                    clear();
                }
                break;
            case STATE.STRANGENESS:
                if (timer > 20)
                {
                    new MessGoingAway(raider).send();
                    clear();
                }
                break;
            case STATE.VICTORY:
                if (timer > 20)
                {
                    raider.hasAttacked = true;
                    new MessVictory(raider).send();
                    clear();
                }
                break;
            default:
                raider = null;
                break;
        }
    }

    private bool canPay(int iteration)
    {
        return raider != null && (state == STATE.WARNING || state == STATE.ALLY_HELP);
    }

    private void setAllyFight()
    {
        if (raider != null && (state == STATE.WARNING || state == STATE.ALLY_HELP))
        {
            set(raider, STATE.ALLY_FIGHT, null);
        }
    }
}