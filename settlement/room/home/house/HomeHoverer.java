package settlement.room.home.house;

import init.race.appearence.RPortrait;
import init.resources.RES_AMOUNT;
import init.sprite.UI.UI;
import init.type.HGROUP;
import init.type.HGROUP.HTypeBits;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.main.Room;
import settlement.stats.STATS;
import snake2d.SPRITE_RENDERER;
import snake2d.util.sprite.SPRITE;
import util.gui.misc.GBox;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.sett.ui.room.UIRoomModule;

final class HomeHoverer extends UIRoomModule{

	private static CharSequence ¤¤Residents = "¤{0}  ({1})";
	private static CharSequence ¤¤VacantFor = "¤Vacant for {0}:";
	private static CharSequence ¤¤Any = "¤Any species of any class.";
	private static CharSequence ¤¤None = "¤None";
	static {
		D.ts(HomeHoverer.class);
	}
	
	private PO[] pos = new PO[40];
	
	public HomeHoverer() {
		GText t = new GText(UI.FONT().S, 64);
		for (int i = 0; i < pos.length; i++)
			pos[i] = new PO(t);
	}
	
	@Override
	public void hover(GBox box, Room in, int rx, int ry) {
		
		HomeInstance h = SETT.ROOMS().HOME.getter.get(rx, ry);
		
		if (h == null)
			return;
		
		box.textL(Dic.¤¤Upgrade);
		box.tab(6);
		box.add(GFORMAT.iofkInv(box.text(), h.upgrade(), SETT.ROOMS().HOME.upgrades().max()));
		box.NL(8);
		
		if (h.occupants() > 0){
			GText t = box.text();
			t.add(¤¤Residents);
			t.insert(0, h.race().info.namePosessive);
			t.insert(1, h.occupant(0).indu().clas().names);
			box.NL();
			box.textLL(t);
			box.add(GFORMAT.iofk(box.text(), h.occupants(), h.occupantsMax()));
			box.NL();
			
			
			int ti = 0;
			for (int i = 0; i < h.occupants(); i++) {
				PO po = pos[i];
				po.h = h.occupant(i);
				box.add(po);
				if ((ti & 1) == 1)
					box.NL();
				ti++;
			}
			
			box.NL(8);
			
			int ri = 0;
			ti = 0;
			for (RES_AMOUNT ra : h.race().home().clas(h.occupant(0).indu().clas()).resources()) {
				box.tab(ti*2);
				box.add(ra.resource().icon());
				int curr = 0;
				int max = ra.amount()*h.occupants();
				for (int oi = 0; oi < h.occupants(); oi++) {
					curr += STATS.HOME().current(h.occupant(oi), ri);
				}
				ri++;
				box.add(GFORMAT.iofkInv(box.text(), curr, max));
				ti++;
				if (ti >= 4) {
					ti = 0;
					box.NL();
				}
			}	
			

			
		}else {
			
			GText t = box.text();
			box.NL();
			t.add(¤¤VacantFor);
			t.insert(0, h.occupantsMax());
			box.add(t);
			box.NL();
			
			HTypeBits s = h.setting();
			
			{
				int a = 0;
				
				for (HGROUP tt : HGROUP.all()) {
					if (s.is(tt))
						a++;
				}
				
				int am = 0;
				if (a == HGROUP.all().size()) {
					box.text(¤¤Any);
				}else if (a == 0) {
					box.text(¤¤None);
				}else if (a <= HGROUP.all().size()/2) {
					for (HGROUP tt : HGROUP.all()) {
						if (s.is(tt)) {
							box.add(tt.icon);
							am++;
							if (am > 10) {
								am = 0;
								box.NL();
							}
						}
					}
				}else {
					for (HGROUP tt : HGROUP.all()) {
						if (!s.is(tt)) {
							box.add(tt.icon);
							box.rewind(tt.icon.width());
							box.add(UI.icons().m.anti);
							am++;
							if (am> 10) {
								am = 0;
								box.NL();
							}
						}
					}
				}
			}
			
			
			
			
		}
		
		box.NL(8);
		box.textLL(Dic.¤¤Isolation);
		box.tab(5);
		box.add(GFORMAT.perc(box.text(), h.isolation()));
		
		super.hover(box, in, rx, ry);
	}
	
	private static class PO extends SPRITE.Imp{
		
		Humanoid h;
		private final GText t;
		
		
		PO(GText t){
			super(RPortrait.P_WIDTH*8, RPortrait.P_HEIGHT);
			this.t = t;
		}
		
		@Override
		public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
			STATS.APPEARANCE().portraitRender(r, h.indu(), X1, Y1, 1);
			t.clear().add(STATS.APPEARANCE().name(h.indu()));
			t.setMaxChars(22);
			t.lablifySub();
			t.render(r, X1 + RPortrait.P_WIDTH+ 4, Y1);
			t.clear().add(h.title());
			t.normalify();
			t.render(r, X1 + RPortrait.P_WIDTH+ 4, Y1+t.height()+2);
		}
		
	}
	
}
