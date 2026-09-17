
package settlement.room.infra.inn;

import game.faction.FACTIONS;
import game.faction.FCredits.CTYPE;
import game.tourism.Review;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.SPRITE;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GGrid;
import util.gui.misc.GHeader;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.table.GRows;
import util.gui.table.GStaples;
import util.gui.table.GTableSorter.GTFilter;
import util.gui.table.GTableSorter.GTSort;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import util.text.DicTime;
import view.main.VIEW;
import view.sett.ui.room.UIRoomBulkApplier;
import view.sett.ui.room.UIRoomModule.UIRoomModuleImp;

class Gui extends UIRoomModuleImp<InnInstance, ROOM_INN> {

	private static CharSequence ¤¤Guestbook = "Guestbook";
	
	static {
		D.ts(Gui.class);
	}
	
	Gui(ROOM_INN s) {
		super(s);
	}

	@Override
	protected void appendPanel(GuiSection section, GGrid grid, GETTER<InnInstance> g, int x1, int y1) {
		
		GuiSection s = new GuiSection();
		
		s.add(new GStat() {
			
			@Override
			public void update(GText text) {
				GFORMAT.iIncr(text, g.get().earnings);
			}
			@Override
			public void hoverInfoGet(GBox b) {
				GText t = b.text();
				DicTime.setYears(t, -1);
				b.add(t);
				b.add(GFORMAT.iIncr(b.text(), g.get().earningsLast));
			};
			
		}.hv(Dic.¤¤Earnings));
		
		
		section.addRelBody(32, DIR.S, s);
		
		
		
		section.addRelBody(4, DIR.S, new GHeader(¤¤Guestbook));

		
		
		GRows gg = new GRows(2);
		
		for (int i = 0; i < 4; i++) {
			final int k = i;
			
			RENDEROBJ rr = new RENDEROBJ.RenderImp(800, 400) {
				
				@Override
				public void render(SPRITE_RENDERER r, float ds) {
					Review rev = g.get().reviews[k];
					if (rev != null && rev.has()) {
						rev.render(r, body.x1(), body.y1(), 800);
					}
				}
			};
			
			gg.add(new GButt.ButtPanel(new SPRITE.Imp(100, 48) {
				
				@Override
				public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
					Review rev = g.get().reviews[k];
					if (rev != null && rev.has()) {
						rev.renderScore(r, X1 + (X2-X1)/2, Y1+8);
						rev.renderCred(r, X1+24, Y1+8+20);
					}
				}
			}) {
				
				@Override
				protected void renAction() {
					Review rev = g.get().reviews[k];
					activeSet(rev != null && rev.has());
				}
				
				@Override
				protected void clickA() {
					Review rev = g.get().reviews[k];
					if (rev == null || !rev.has())
						return;
					VIEW.inters().popup.show(rr, this);
				}
				
			});
			
		}
		
		for (RENDEROBJ o : gg.rows()) {
			section.addRelBody(4, DIR.S, o);
		}
		
		

	}

	@Override
	protected void appendMain(GGrid grid, GGrid text, GuiSection sExtra) {
		
		GuiSection s = new GuiSection();
		
		final int am = FACTIONS.player().credits().get(CTYPE.TOURISM).IN.historyRecords();
		GStaples chart = new GStaples(am) {
			
			@Override
			protected void hover(GBox box, int stapleI) {
				int ago = am-1-stapleI;
				GText t = box.text();
				DicTime.setAgo(t, ago*FACTIONS.player().credits().get(CTYPE.TOURISM).IN.time().bitSeconds());
				box.textLL(t);
				box.NL();
				box.add(GFORMAT.iIncr(box.text(), FACTIONS.player().credits().get(CTYPE.TOURISM).IN.get(ago)));
				
			}
			
			@Override
			protected double getValue(int stapleI) {
				return FACTIONS.player().credits().get(CTYPE.TOURISM).IN.get(am-1-stapleI);
			}
		};
		chart.body().setDim(240, 58);
		
		s.add(chart);
		s.addRelBody(4, DIR.N, new GHeader(Dic.¤¤Earnings));
		
		s.addRelBody(4, DIR.S, new GButt.ButtPanel(Dic.¤¤Tourists) {
			
			@Override
			protected void clickA() {
				VIEW.UI().tourists.activate();
			};
			
		}.pad(4, 1));
		
		text.add(s);
		
	}

	@Override
	protected void appendTableButt(GuiSection s, GETTER<RoomInstance> ins) {


	}

	@Override
	protected void hover(GBox box, InnInstance i) {
		InnInstance ii = i;
		
		box.NL();
		box.textLL(Dic.¤¤Earnings);
		box.add(GFORMAT.iIncr(box.text(), ii.earnings));
		box.NL();
		
		
	}

	@Override
	protected void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters,
			LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers) {
		
	}

}
