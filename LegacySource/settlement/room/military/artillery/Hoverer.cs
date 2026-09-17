using System;
using settlement.room.military.artillery;
using init.sprite;
using settlement.thing.projectiles;
using util.gui.misc;
using util.info;
using util.text;

class Hoverer
{
    public static void Hover(GBox box, ArtilleryInstance i)
    {
        ArtilleryInstance ins = (ArtilleryInstance)i;

        if (ins.Mustered())
        {
            box.TextL(Dic.¤¤Musterd);
            box.NL();
            if (i.IsLoaded)
                box.TextL(Dic.¤¤ReadyFire);
            else
            {
                box.TextL(Dic.¤¤Reloading);
                box.Tab(5);
                box.Add(GFORMAT.Perc(box.Text(), i.Progress()));
            }
            box.NL(4);
        }

        if (i.HasTrajectory && ins.Mustered())
        {
            box.TextL(Dic.¤¤Attacking);
        }

        box.Add(SPRITES.Icons().S.Human);
        box.Add(GFORMAT.IofkInv(box.Text(), ins.Men, 6));
        box.NL(6);
        box.Sep();
        i.BlueprintI().Projectile.Hover(box, i.BlueprintI().Info.Name, i.BlueprintI().Ref() * i.GetDegrade(), Trajectory.RELEASE_HEIGHT);
    }
}