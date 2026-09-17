using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.trajectory;
using game.boosting;
using init.constant;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.file;
using GAME = game.GAME;

public sealed class DivSettings
{
    private readonly Div div;
    private bool mustering = false;
    public bool guard = true;
    private bool mopping = false;
    public bool charging = false;
    public bool running;
    public bool fireAtWill;
    public bool shouldNotMoveToFire;
    public short ammoI = 0;
    public DIV_FORMATION formation = DIV_FORMATION.TIGHT;

    private double speed = 0.4 * C.TILE_SIZE;
    public bool shouldFire;
    public bool shouldbreak = true;
    public bool chargeSpeed = false;

    private float chargeSound = 0;
    private float power;
    private double aref = 0;

    public DivSettings(Div div)
    {
        this.div = div;
    }

    public bool mustering()
    {
        return mustering;
    }

    public bool moppingUp()
    {
        return mopping;
    }

    public bool shouldFire()
    {
        return ammo() != null && BattleTrajectories.trajectories(div) > 0;
    }

    public void musteringSet(bool must)
    {
        div.current().init(div.menNrOf());
        mustering = must;
    }

    public void moppingSet(bool must)
    {
        if (must)
        {
            musteringSet(must);
        }
        mopping = must;
    }

    public EquipRange ammo()
    {
        if (STATS.EQUIP().RANGED().get(ammoI).stat().div().get(div) > 0 && STATS.EQUIP().RANGED().get(ammoI).ammoD(div) > 0)
            return STATS.EQUIP().RANGED().get(ammoI);
        aref = 0;
        for (int k = 0; k < STATS.EQUIP().RANGED().size(); k++)
        {
            EquipRange a = STATS.EQUIP().RANGED().get(k);
            if (a.stat().div().get(div) > 0 && a.ammoD(div) > 0)
            {
                setAmmoI(a.tIndex);
                return a;
            }
        }
        return null;
    }

    public void setBestAmmo()
    {
        int i = ThreadLocalRandom.Current.Next(STATS.EQUIP().RANGED().size());
        for (int k = 0; k < STATS.EQUIP().RANGED().size(); k++)
        {
            EquipRange a = STATS.EQUIP().RANGED().get(i + k);
            if (a.stat().div().get(div) > 0 && a.ammoD(div) > 0)
            {
                setAmmoI(a.tIndex);
            }
        }
    }

    void update()
    {
        if (div.menNrOf() == 0)
        {
            power = 0;
            aref = 0;
            speed = 0.4 * C.TILE_SIZE;
            return;
        }

        EquipRange a = ammo();
        if (a != null)
        {
            aref = a.ref(div);
        }
        else
        {
            aref = 0;
        }
        power = (float)GAME.battle().power.get(div);

        if (this.speed < 0.9)
        {
            chargeSound -= 1;
            if (chargeSound < 0)
                chargeSound = 0;
        }

        double speed = 0.4;

        if (charging || chargeSpeed)
        {
            if (chargeSound <= 0)
            {
                GAME.ARMIES().sound.chargeHorn.play(div.race(), div.centre().cX(), div.centre().cY());
                chargeSound = 10;
            }
            speed = 0.9;
        }
        else if (running)
            speed = 0.7;
        if (GAME.ARMIES().factors.shouldRun(div) || div.status().isFighting())
            speed *= 0.75;
        speed *= BOOSTABLES.PHYSICS().SPEED.get(div) * C.TILE_SIZE;

        this.speed = speed;
    }

    private void setAmmoI(int i)
    {
        if (ammoI != i)
        {
            ammoI = (short)i;
        }
    }

    public bool fireAtWill()
    {
        if (ammo() == null)
        {
            fireAtWill = false;
        }
        return fireAtWill;
    }

    public double ammoRef()
    {
        return aref;
    }

    void save(FilePutter file)
    {
        file.bool(running);
        file.bool(guard);
        file.b((byte)formation.ordinal());
        file.bool(fireAtWill);
        file.s(ammoI);
        file.bool(mustering);
        file.bool(mopping);
        file.bool(shouldFire);
        file.bool(shouldNotMoveToFire);
        file.bool(charging);
        file.bool(shouldbreak);
        file.bool(chargeSpeed);
        file.f(getPower());
        file.d(aref);
        file.d(speed);
    }

    void load(FileGetter file) throws IOException
    {
        running = file.bool();
        guard = file.bool();
        formation = DIV_FORMATION.all.get(file.b());
        fireAtWill = file.bool();
        ammoI = file.s();
        mustering = file.bool();
        mopping = file.bool();
        shouldFire = file.bool();
        shouldNotMoveToFire = file.bool();
        charging = file.bool();
        shouldbreak = file.bool();
        chargeSpeed = file.bool();
        power = file.f();
        aref = file.d();
        speed = file.d();
    }

    public void clear()
    {
        running = false;
        guard = true;
        fireAtWill = false;
        formation = DIV_FORMATION.LOOSE;
        ammoI = 0;
        mustering = false;
        mopping = false;
        shouldFire = false;
        shouldNotMoveToFire = false;
        charging = false;
        shouldbreak = false;
        chargeSpeed = false;
        power = 0;
        speed = 0.4 * C.TILE_SIZE;
    }

    public float getPower()
    {
        return power;
    }

    public double speed()
    {
        return speed;
    }
}