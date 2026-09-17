package settlement.room.military.artillery;

import init.sprite.SPRITES;
import settlement.thing.projectiles.Trajectory;
import util.gui.misc.GBox;
import util.info.GFORMAT;
import util.text.Dic;

class Hoverer {

	public static void hover(GBox box, ArtilleryInstance i) {
		ArtilleryInstance ins = (ArtilleryInstance) i;
		
		if (ins.mustered()) {
			box.textL(Dic.¤¤Musterd);
			box.NL();
			if (i.isLoaded)
				box.textL(Dic.¤¤ReadyFire);
			else {
				box.textL(Dic.¤¤Reloading);
				box.tab(5);
				box.add(GFORMAT.perc(box.text(), i.progress()));
			}
				box.NL(4);
		}
		
		if (i.hasTrajectory && ins.mustered()) {
			box.textL(Dic.¤¤Attacking);
		}
		
		box.add(SPRITES.icons().s.human);
		box.add(GFORMAT.iofkInv(box.text(), ins.men, 6));
		box.NL(6);
		box.sep();
		i.blueprintI().projectile.hover(box, i.blueprintI().info.name, i.blueprintI().ref()*i.getDegrade(), Trajectory.RELEASE_HEIGHT);
		
	}
	
}
