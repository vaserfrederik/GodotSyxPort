using System.Collections.Generic;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.sets;
using util.keymap;
using util.text;

public sealed class HTYPES
{
    private readonly ArrayListGrower<HTYPE> all = new ArrayListGrower<HTYPE>();

    static HTYPES()
    {
        D.gInit(new HTYPES());
    }

    private readonly HTYPE SUBJECT = new HTYPE(all, "CITIZEN",
            HCLASSES.CITIZEN(),
            D.g("Citizen"), D.g("Citizens"),
            D.g("CitizenD", "Citizens are the bulk of your population and will carry out your wishes."),
            new ColorImp(3, 1, 19),
            UI.icons().s.typeCitizen.createColored(new ColorImp(50, 255, 255)));
    private readonly HTYPE RETIREE = new HTYPE(all, "RETIREE",
            HCLASSES.CITIZEN(),
            D.g("Retiree"), D.g("Retirees"),
            D.g("RetireeD", "Retired people are citizens that have served you for many years and are now entitled to some relaxation their final years. They do not work."),
            new ColorImp(new ColorImp(8, 20, 20)),
            UI.icons().s.typeRetire.createColored(new ColorImp(255, 128, 0)));
    private readonly HTYPE GUARD = new HTYPE(all, "GUARD",
            HCLASSES.CITIZEN(),
            D.g("Guard"), D.g("Guards"),
            D.g("GuardD", "Guards are soldiers on guard duty. They spend their days guarding your city, catch criminals and make your citizens obedient"),
            new ColorImp(new ColorImp(20, 8, 8)),
            UI.icons().s.typeGuard.createColored(new ColorImp(255, 90, 90)));
    private readonly HTYPE RECRUIT = new HTYPE(all, "RECRUIT",
            HCLASSES.CITIZEN(),
            D.g("Recruit"), D.g("Recruits"),
            D.g("RecruitD", "Recruits are citizens either training their combat skills for a place in a division, or honing these skills towards the limit you've set for said division."),
            new ColorImp(20, 8, 16),
            UI.icons().s.typeRecruit.createColored(new ColorImp(50, 255, 128)));
    private readonly HTYPE STUDENT = new HTYPE(all, "STUDENT",
            HCLASSES.CITIZEN(),
            D.g("Student"), D.g("Students"),
            D.g("StudentD", "Students are citizens currently attending university. They do not count towards your workforce."),
            new ColorImp(20, 8, 16),
            UI.icons().s.typeStudent.createColored(new ColorImp(50, 128, 255)));
    private readonly HTYPE PRISONER = new HTYPE(all, "PRISONER",
            HCLASSES.OTHER(),
            D.g("Prisoner"), D.g("Prisoners"),
            D.g("PrisonerD", "Prisoners are caught criminals, or POWs. Prisoners will spend their time in your dungeons. They can be used as sacrifices in temples, or gladiators. They can also be enslaved, or executed."),
            new ColorImp(20, 20, 8),
            UI.icons().s.typePrison.createColored(new ColorImp(200, 200, 200)));
    private readonly HTYPE TOURIST = new HTYPE(all, "TOURIST",
            HCLASSES.OTHER(),
            Dic.¤¤Tourist, Dic.¤¤Tourists,
            D.g("TouristD", "Tourists are foreigners visiting your city in search of a spectacle. Treat them well, and they will show their appreciation by tossing you some coins."),
            new ColorImp(20, 20, 8),
            UI.icons().s.typeTourist.createColored(new ColorImp(128, 128, 255)));
    private readonly HTYPE SOLDIER = new HTYPE(all, "SOLDIER",
            HCLASSES.CITIZEN(),
            D.g("Soldier"), D.g("Soldiers"),
            D.g("SoldierD", "Soldiers are men on the battlefield."),
            new ColorImp(3, 1, 19),
            UI.icons().s.typeSoldier.createColored(new ColorImp(50, 128, 255)));
    private readonly HTYPE ENEMY = new HTYPE(all, "ENEMY",
            HCLASSES.OTHER(),
            D.g("Enemy"), D.g("Enemies"),
            D.g("EnemyD", "Enemies are hostile peoples, bent on destroying your rule"),
            new ColorImp(30, 1, 1),
            UI.icons().s.typeSoldier.createColored(new ColorImp(255, 50, 50)));

    private readonly HTYPE RIOTER = new HTYPE(all, "RIOTER",
            HCLASSES.OTHER(),
            D.g("Rioter"), D.g("Rioters"),
            D.g("RioterD", "Rioters are former citizens, who have had enough of your rule and express their disappointment by burning your city to ashes."),
            new ColorImp(30, 1, 1),
            UI.icons().s.typeRioter.createColored(new ColorImp(255, 50, 50)));
    private readonly HTYPE DERANGED = new HTYPE(all, "DERANGED",
            HCLASSES.OTHER(),
            D.g("Deranged"), D.g("Derangeds", "Deranged"),
            D.g("DerangedD", "Deranged are people who have gone insane. They will do no work, and wander around your city doing erratic things. Can be cured in an asylum."),
            new ColorImp(30, 30, 1),
            UI.icons().s.typeCrazy.createColored(new ColorImp(255, 255, 50)));
    private readonly HTYPE NOBILITY = new HTYPE(all, "NOBILITY",
            HCLASSES.NOBLE(),
            D.g("Nobility"), D.g("Nobles"),
            D.g("NobilityD", "The nobility are above the common plebs. Do not work in a traditional sense, but are part of the social structure."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeNobility.createColored(new ColorImp(255, 255, 255)));
    private readonly HTYPE SLAVE = new HTYPE(all, "SLAVE",
            HCLASSES.SLAVE(),
            D.g("Slave"), D.g("Slaves"),
            D.g("SlaveD", "Slaves are forced to work and do not have rights."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeSlave.createColored(new ColorImp(0, 0, 0)));
    private readonly HTYPE CHILD = new HTYPE(all, "CHILD",
            HCLASSES.CITIZEN(),
            D.g("Child"), D.g("Children"),
            D.g("ChildD", "Children are young citizens who are not yet old enough to work."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeChild.createColored(new ColorImp(255, 255, 255)));
    private readonly HTYPE GUARD = new HTYPE(all, "GUARD",
            HCLASSES.CITIZEN(),
            D.g("Guard"), D.g("Guards"),
            D.g("GuardD", "Guards are citizens tasked with maintaining order and protecting the city."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeGuard.createColored(new ColorImp(255, 255, 255)));
    private readonly HTYPE CHILD_SLAVE = new HTYPE(all, "CHILD_SLAVE",
            HCLASSES.SLAVE(),
            D.g("ChildSlave"), D.g("ChildSlaves"),
            D.g("ChildSlaveD", "ChildSlaves are young slaves who are forced to work."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeChildSlave.createColored(new ColorImp(0, 0, 0)));
    private readonly HTYPE PARENT = new HTYPE(all, "PARENT",
            HCLASSES.CITIZEN(),
            D.g("Parent"), D.g("Parents"),
            D.g("ParentD", "Parents are citizens who take care of their children."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeParent.createColored(new ColorImp(255, 255, 255)));
    private readonly HTYPE PARENT_SLAVE = new HTYPE(all, "PARENT_SLAVE",
            HCLASSES.SLAVE(),
            D.g("ParentSlave"), D.g("ParentSlaves"),
            D.g("ParentSlaveD", "ParentSlaves are slaves who take care of their child slaves."),
            new ColorImp(0, 0, 0),
            UI.icons().s.typeParent.createColored(new ColorImp(0, 0, 0)));

    private readonly RMAPS<HTYPE> map;

    private static HTYPES self;

    private HTYPES(HCLASSES cls)
    {
        self = this;

        ENEMY.hostile = true;
        RIOTER.hostile = true;

        SOLDIER.visible = false;

        SUBJECT.works = true;
        SLAVE.works = true;

        PARENT.child = CHILD;
        PARENT_SLAVE.child = CHILD_SLAVE;

        CHILD.parent = PARENT;
        CHILD_SLAVE.parent = SLAVE;

        KeyMap<HTYPE> mm = new KeyMap<HTYPE>();
        foreach (HTYPE h in all)
            mm.put(h.key, h);

        map = new RMAPS<HTYPE>("HTYPE", all);
    }

    public static RMAPS<HTYPE> MAP()
    {
        return self.map;
    }

    public static LIST<HTYPE> ALL()
    {
        return self.all;
    }

    public static HTYPE SUBJECT()
    {
        return self.SUBJECT;
    }

    public static HTYPE RETIREE()
    {
        return self.RETIREE;
    }

    public static HTYPE RECRUIT()
    {
        return self.RECRUIT;
    }

    public static HTYPE STUDENT()
    {
        return self.STUDENT;
    }

    public static HTYPE PRISONER()
    {
        return self.PRISONER;
    }

    public static HTYPE TOURIST()
    {
        return self.TOURIST;
    }

    public static HTYPE SOLDIER()
    {
        return self.SOLDIER;
    }

    public static HTYPE ENEMY()
    {
        return self.ENEMY;
    }

    public static HTYPE RIOTER()
    {
        return self.RIOTER;
    }

    public static HTYPE DERANGED()
    {
        return self.DERANGED;
    }

    public static HTYPE NOBILITY()
    {
        return self.NOBILITY;
    }

    public static HTYPE SLAVE()
    {
        return self.SLAVE;
    }

    public static HTYPE CHILD()
    {
        return self.CHILD;
    }

    public static HTYPE GUARD()
    {
        return self.GUARD;
    }

    public static HTYPE CHILD_SLAVE()
    {
        return self.CHILD_SLAVE;
    }

    public static HTYPE PARENT()
    {
        return self.PARENT;
    }

    public static HTYPE PARENT_SLAVE()
    {
        return self.PARENT_SLAVE;
    }

    public static HTYPE child(HCLASS cl)
    {
        if (cl == HCLASSES.SLAVE())
            return CHILD_SLAVE();
        else
            return CHILD();
    }
}