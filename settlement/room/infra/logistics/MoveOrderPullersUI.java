package settlement.room.infra.logistics;

import game.GAME;
import game.GameDisposable;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.infra.logistics.MoveOrderPull.MoveOrderPullInstance;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.ArrayList;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.SPRITE;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.table.GTableBuilder;
import util.gui.table.GTableBuilder.GRowBuilder;
import util.text.D;
import view.main.VIEW;

public final class MoveOrderPullersUI extends GButt.ButtPanel {

	private static ArrayList<RoomInstance> ins;
	
	private static CharSequence ¤¤pullers = "Pullers";
	
	
	private final GuiSection pop;
	
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				ins = null;
			}
		};
		D.ts(MoveOrderPullersUI.class);
		
		
	}

	private final GETTER<? extends RoomInstance> g;

	public MoveOrderPullersUI(GETTER<? extends RoomInstance> g) {
		super(UI.icons().m.storage_pullers);
		this.g = g;
		body.setDim(48);
		
		GTableBuilder bu = new GTableBuilder() {
			
			@Override
			public int nrOFEntries() {
				return all().size();
			}
		};
		
		bu.column(400, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				
				SPRITE sp = new SPRITE.Imp(400, Icon.L) {
					
					@Override
					public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) {
						RoomInstance ins = all().get(ier.get());
						if (ins != null) {
							ins.icon().render(r, X1, Y1);
							GCOLOR.T().H1.bind();
							UI.FONT().H2.render(r, ins.name(), X1+48, Y1+Icon.L/2-UI.FONT().H2.height()/2);
							
						}
						
					}
				};
				return new GButt.ButtPanel(sp) {
					@Override
					protected void clickA() {
						RoomInstance ins = all().get(ier.get());
						if (ins != null)
						VIEW.s().getWindow().centererTile.set(ins.body().cX(), ins.body().cY());
					}
				};
				
				
			}
		}, DIR.NW);
		
		pop = new GuiSection();
		pop.add(bu.create(20, false));
	}

	@Override
	protected void renAction() {
		activeSet(!all().isEmpty());
	}
	
	@Override
	protected void clickA() {
		VIEW.inters().popup.show(pop, this, true);
	}
	
	@Override
	public void hoverInfoGet(GUI_BOX text) {
		GBox b = (GBox) text;
		b.title(¤¤pullers);
		for (RoomInstance ins : all()) {
			b.add(ins.icon());
			b.text(ins.name());
			b.NL();
		}
		
		super.hoverInfoGet(text);
	}
	
	
	private int upI = -1;
	
	private LIST<RoomInstance> all(){
		if (ins == null) {
			ins = new ArrayList<>(100);
		}
		if (upI == GAME.updateI())
			return ins;
		
		ins.clearSloppy();
		RoomInstance tar = g.get();
		for (RoomBlueprint b : SETT.ROOMS().all()) {
			if (b instanceof RoomBlueprintIns<?>) {
				RoomBlueprintIns<?> bb = (RoomBlueprintIns<?>) b;
				if (bb.instancesSize() > 0 && bb.getInstance(0) instanceof MoveOrderPullInstance) {
					for (int i = 0; i < bb.instancesSize(); i++) {
						RoomInstance ii = bb.getInstance(i);
						if (ii instanceof MoveOrderPullInstance) {
							MoveOrderPullInstance pi = (MoveOrderPullInstance) ii;
							for (MoveOrderPull p : pi.moveOrdersPull()) {
								if (p != null && p.source() == tar) {
									ins.add(ii);
									if (!ins.hasRoom())
										return ins;
								}
									
							}
							
						}
					}
					
					
				}
			}
		}
		return ins;
	}

}
