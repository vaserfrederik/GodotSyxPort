using System;
using game.battle.div;
using game.boosting;
using init.constant;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using settlement.thing.projectiles;
using snake2d.util.datatypes;
using snake2d.util.rnd;

public sealed class FightingUtil
{
    private readonly VectorImp vec = new VectorImp();
    private ECollision coll = new ECollision();

    private readonly double CHANCE_MIN = 1.0 / Config.battle().DAMAGE_REDUCTION;
    private readonly double CHANCE_SPAN = Config.battle().DAMAGE_REDUCTION - CHANCE_MIN;

    public void Attack(Humanoid a, ENTITY enemy)
    {
        DIR od = enemy.speed.Dir();
        vec.Set(a.body(), enemy.body());
        coll.Other = a;

        double dot = (1 - od.xN() * vec.nX() - od.yN() * vec.nY()) * 0.5;

        coll.norX = vec.nX();
        coll.norY = vec.nY();
        coll.dirDot = dot;
        coll.dirDotOther = dot;
        coll.SpeedHasChanged = false;
        coll.TileMomentum = 0;

        if (Dodge(BOOSTABLES.BATTLE().OFFENCE.Get(a.indu()), enemy.GetDefenceSkill(dot, coll.norX, coll.norY)))
        {
            coll.damagetileStrength = 0;
            coll.TileMomentum = 0;

            enemy.Collide(coll);
            return;
        }

        SetDamage(coll, a);

        if (enemy is Humanoid)
        {
            Humanoid e = (Humanoid)enemy;
            if (!DoesNotBlock(BOOSTABLES.BATTLE().DEXTERITY.Get(a.indu()), e, dot, coll.norX, coll.norY))
            {
                BlockDamage(coll, e);
            }
        }

        AI.Modules().Battle.SoundSword.Rnd(a);

        double nY = enemy.speed.Y() + vec.nY() * coll.TileMomentum * enemy.physics.GetMassI();
        double nX = enemy.speed.X() + vec.nX() * coll.TileMomentum * enemy.physics.GetMassI();
        coll.SpeedHasChanged = true;
        enemy.speed.SetRaw(nX, nY);

        enemy.Collide(coll);
        if (enemy.IsRemoved())
        {
            STATS.BATTLE().MakeAKill(a);
        }
    }

    private bool Dodge(double attackSpeed, double defenceAgility)
    {
        if (defenceAgility <= 0)
            return false;

        if (attackSpeed <= 0)
            return true;

        double r = attackSpeed / (defenceAgility * (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN));
        if (r > RND.rFloat())
            return false;
        return true;
    }

    private bool DoesNotBlock(double attackSkill, Humanoid enemy, double dot, double adx, double ady)
    {
        if (attackSkill <= 0)
            return false;

        double def = HPoll.Handler.ParrySkill(enemy, dot, adx, ady);

        if (def <= 0)
            return true;

        if (enemy.Division() == null)
            def *= 0.25;

        double r = attackSkill / (def * (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN));
        if (r > RND.rFloat())
            return true;
        return false;
    }

    private double FormationValue(Humanoid a, double ax, double ay)
    {
        Div div = a.Division();
        if (div != null)
        {
            DIR dd = div.Position().Dir();
            double dot = ax * dd.xN() + ay * dd.yN();
            dot = (1 + dot) * 0.5;
            dot = (int)(dot * 4) * 0.25;

            if (dot > 0)
            {
                return dot * BOOSTABLES.BATTLE().FORMATION.Get(a.Division());
            }
        }
        return 0;
    }

    private void SetDamage(ECollision e, Humanoid a)
    {
        double h = 1.0 + SETT.TERRAIN().Get(a.tc()).HeightEnt(a.tc().X(), a.tc().Y()) / 10.0;
        e.damagetileStrength = BOOSTABLES.BATTLE().BLUNT_ATTACK.Get(a.indu()) * h;

        for (int i = 0; i < e.damage.Length; i++)
        {
            double da = BOOSTABLES.BATTLE().DAMAGES.Get(i).attack.Get(a.indu());
            e.damage[i] = da;
        }
        coll.TileMomentum = C.TILE_SIZE * h * BOOSTABLES.BATTLE().BLUNT_ATTACK.Get(a.indu()) * (0.5 + RND.rFloat() * 2);
    }

    private void BlockDamage(ECollision e, Humanoid blocker)
    {
        e.damagetileStrength /= BOOSTABLES.BATTLE().BLUNT_DEFENCE_DIR.Get(blocker.indu());
        for (int i = 0; i < e.damage.Length; i++)
        {
            e.damage[i] /= 1 + BOOSTABLES.BATTLE().DAMAGES.Get(i).defenceDir.Get(blocker.indu());
        }
    }

    public double ValueDefenceSkill(Humanoid a, double attackDot, double angleOfAttackX, double angleOfAttackY)
    {
        double def = BOOSTABLES.BATTLE().DEFENCE.Get(a.indu());
        double res = (0.1 + 0.9 * attackDot) * def;
        res += FormationValue(a, angleOfAttackX, angleOfAttackY);
        return res;
    }

    public double ValueParrySkill(Humanoid a, double attackDot, double angleOfattackX, double angleOfAttackY)
    {
        double def = BOOSTABLES.BATTLE().PARRY.Get(a.indu());
        def = attackDot * def;
        def += FormationValue(a, angleOfattackX, angleOfAttackY);
        return def;
    }

    public double GetDamageDone(ECollision coll, Humanoid a)
    {
        double dam = 1;
        dam /= 1.0 + SETT.TERRAIN().Get(a.tc()).HeightEnt(a.tc().X(), a.tc().Y()) / 10.0;

        for (int i = 0; i < BOOSTABLES.BATTLE().DAMAGES.Size(); i++)
        {
            dam += coll.damage[i] / (1 + BOOSTABLES.BATTLE().DAMAGES.Get(i).defence.Get(a.indu()));
        }

        dam *= coll.damagetileStrength;
        dam /= (CHANCE_MIN + RND.rFloat() * CHANCE_SPAN) * BOOSTABLES.BATTLE().BLUNT_DEFENCE.Get(a.indu());

        double ch = RND.rFloat();
        if (a.Division() == null)
            ch *= 0.25;
        if (dam > ch)
        {
            return dam;
        }

        return 0;
    }

    private double bb = 0;
    private double am = 0;

    public bool ProjectileAttack(ENTITY e, double angleX, double angleY, double speed, Projectile type, double refVal)
    {
        int sp = (int)(speed);
        DIR od = e.speed.Dir();

        double dot = od.xN() * angleX + od.yN() * angleY;
        dot = (1 + dot) * 0.5;

        if (Dodge(sp, e.GetDefenceSkill(dot, angleX, angleY)))
        {
            return false;
        }

        coll.norX = angleX;
        coll.norY = angleY;
        coll.dirDot = dot;
        coll.dirDotOther = dot;
        coll.SpeedHasChanged = false;
        coll.TileMomentum = 0;

        if (Dodge(BOOSTABLES.BATTLE().DEXTERITY.Get(a.indu()) + speed, coll.Other.GetDefenceSkill(coll.dirDotOther, coll.norX, coll.norY)))
        {
            return false;
        }

        SetDamage(coll, a);

        speed = speed * C.ITILE_SIZE - 2;
        if (speed < 0)
            speed = 0;

        STATS.NEEDS().EXHAUSTION.indu().IncD(a.indu(), -speed * 0.25);

        double bonus = speed * coll.dirDot;
        bonus *= BOOSTABLES.BATTLE().CHARGE.Get(a.indu());

        coll.damagetileStrength *= bonus;

        if (coll.Other is Humanoid)
        {
            Humanoid e = (Humanoid)coll.Other;
            if (!DoesNotBlock(BOOSTABLES.BATTLE().OFFENCE.Get(a.indu()), e, coll.dirDotOther, coll.norX, coll.norY))
            {
                BlockDamage(coll, e);
            }
        }

        e.Collide(coll);

        return true;
    }

    public void SetImpactDamage(Humanoid a, ECollision coll, ECollision damage)
    {
        double speed = a.speed.Magnitude();

        damage.damagetileStrength = 0;

        if (Dodge(BOOSTABLES.BATTLE().DEXTERITY.Get(a.indu()) + speed, coll.Other.GetDefenceSkill(coll.dirDotOther, coll.norX, coll.norY)))
        {
            return;
        }

        SetDamage(damage, a);

        speed = speed * C.ITILE_SIZE - 2;
        if (speed < 0)
            speed = 0;

        STATS.NEEDS().EXHAUSTION.indu().IncD(a.indu(), -speed * 0.25);

        double bonus = speed * coll.dirDot;
        bonus *= BOOSTABLES.BATTLE().CHARGE.Get(a.indu());

        damage.damagetileStrength *= bonus;

        if (coll.Other is Humanoid)
        {
            Humanoid e = (Humanoid)coll.Other;
            if (!DoesNotBlock(BOOSTABLES.BATTLE().OFFENCE.Get(a.indu()), e, coll.dirDotOther, coll.norX, coll.norY))
            {
                BlockDamage(damage, e);
            }
        }
    }
}