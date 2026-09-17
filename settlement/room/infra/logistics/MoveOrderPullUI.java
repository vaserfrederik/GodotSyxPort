package settlement.room.infra.logistics;

import init.resources.RBIT;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.Icon;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.path.components.finder.SCompFinder.SCompPath;
import settlement.room.infra.logistics.MoveJob.ROOM_MOVE_SOURCE;
import settlement.room.infra.logistics.MoveOrderPull.MoveOrderPullInstance;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintIns;
import settlement.room.main.RoomInstance;
import snake2d.PathTile;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.color.OPACITY;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE;
import snake2d.util.gui.clickable.CLICKABLE;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.ArrayListResize;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.SPRITE;
import util.GUTIL;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GHeader;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.gui.table.GTableBuilder;
import util.gui.table.GTableBuilder.GRowBuilder;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.keyboard.KEYS;
import view.main.VIEW;
import view.tool.PLACER_TYPE;
import view.tool.PlacableMulti;
import view.tool.PlacableSingle;

public class MoveOrderPullUI extends GuiSection{


	private static CharSequence ¤¤name = "¤Pull Source";
	private static CharSequence ¤¤notSet = "not set";
	private static CharSequence ¤¤notSetC = "Click to set the pull source storage site.";
	private static CharSequence ¤¤setC = "Click to go to pull source.";
	private static CharSequence ¤¤issue = "Click to issue an order to pull from another storage site.";
	private static CharSequence ¤¤Choose = "¤Choose a storage site to pull from.";
	private static CharSequence ¤¤Limit = "¤Pull Limit";
	private static CharSequence ¤¤LimitD = "¤Only pull when the source storage exceeds this limit.";
	private static CharSequence ¤¤auto = "¤Set next order to closest source.";
	private static CharSequence ¤¤hold = "¤Hold {0} to move to source room. Click to select source room.";
	static CharSequence ¤¤paster = "Paste settings";
	static CharSequence ¤¤pasterD = "Paste current setting onto other room of the same type.";
	
	private final GETTER<? extends MoveOrderPullInstance> source;
	private final GETTER<? extends RoomInstance> room;
	private int placerII;
	
	
	static {
		D.ts(MoveOrderPullUI.class);
	}
	
	public MoveOrderPullUI(GETTER<? extends MoveOrderPullInstance> source, GETTER<? extends RoomInstance> room, LIST<RESOURCE> resources, int orderAm){

		this.source = source;
		this.room = room;
		Placer placer = new Placer();
		Detail popup = new Detail(placer, resources);
		
		GButt.ButtPanel cc = new GButt.ButtPanel(UI.icons().s.cog) {
			@Override
			protected void clickA() {
				GUTIL.flooder().init(this);
				GUTIL.flooder().pushSloppy(room.get().mX(), room.get().mY(), 0);
				while(GUTIL.flooder().hasMore()) {
					PathTile t = GUTIL.flooder().pollSmallest();
					
					Room r = SETT.ROOMS().map.get(t);
					if (r != null && r != source.get() && r.mX(t.x(), t.y()) == t.x() && r.mY(t.x(), t.y()) == t.y()) {
						if (r instanceof MoveJob.ROOM_MOVE_SOURCE) {
							MoveJob.ROOM_MOVE_SOURCE ss = (ROOM_MOVE_SOURCE) r;
							if (ss.moveCapacity().has(source.get().moveOrderPullAccepted())) {
								for (int ii = 0; ii < source.get().moveOrdersPull().length; ii++) {
									MoveOrderPull o = source.get().moveOrdersPull()[ii];
									if (o == null || o.source() == null || o.source() == ss) {
										source.get().moveOrdersPull()[ii]  = new MoveOrderPull((RoomInstance) r, source.get().moveOrderPullAccepted());
										source.get().moveOrdersPull()[ii].pullLimit = 0;
										break;
									}
										
								}
								GUTIL.flooder().done();
							}
						}
					}
					
					for (DIR d : DIR.ALL) {
						if (SETT.IN_BOUNDS(t, d) && SETT.PATH().coster.player.getCost(t.x(), t.y(), t.x()+d.x(), t.y()+d.y()) > 0) {
							GUTIL.flooder().pushSmaller(t.x()+d.x(), t.y()+d.y(), t.getValue()+d.tileDistance());
						}
					}
				}
				GUTIL.flooder().done();
				super.clickA();
			}
			
			@Override
			protected void renAction() {
				activeSet(true);
				
				for (MoveOrderPull o : source.get().moveOrdersPull()) {
					if (o == null || o.source() == null) {
						return;
					}
						
				}
				activeSet(false);
			}
			
		};
		cc.hoverInfoSet(¤¤auto);
		cc.body.setHeight(48);
		addRightC(0, cc);
		
		
		for (int i = 0; i < orderAm; i++) {
			
			final int oi = i;
			
			addRightC(0, new CLICKABLE.ClickableAbs(48, 48) {
				
				@Override
				protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected, boolean isHovered) {
					GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
					if (source.get().moveOrdersPull()[oi] == null) {
						UI.icons().m.storage_pull.renderC(r, body.cX(), body.cY());
						GButt.ButtPanel.renderFrame(r, body);
						return;
					}
					
					MoveOrderPull o = source.get().moveOrdersPull()[oi];
					
					if (o.problem(source.get()) != null) {
						GCOLOR.UI().BAD.hovered.bind();
					}else if (o.warning(source.get()) != null) {
						GCOLOR.UI().SOSO.hovered.bind();
					}else {
						GCOLOR.UI().GOOD.hovered.bind();
					}
					UI.icons().s.alert.renderC(r, body().cX(), body.cY());
					COLOR.unbind();
					GButt.ButtPanel.renderFrame(r, body);
					
					if (isHovered && source.get().moveOrdersPull()[oi].source() != null) {
						RoomInstance ins = (RoomInstance) source.get().moveOrdersPull()[oi].source();
						SETT.OVERLAY().add(ins.mX(), ins.mY());
					}
				}
				
				@Override
				protected void clickA() {
					if (source.get().moveOrdersPull()[oi] == null) {
						placer.activate(oi);
					}else {
						VIEW.inters().popup.show(popup.get(oi), this);
					}
				}
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤name);
					MoveOrderPull o = source.get().moveOrdersPull()[oi];
					if (o == null) {
						b.text(¤¤issue);
						return;
					}else {
						b.textLL(¤¤name);
						b.tab(6);
						if (o.sourceI() == null) {
							b.error(¤¤notSet);
						}else {
							b.text(o.sourceI().name());
						}
						b.NL();
						
						if (o.problem(source.get()) != null)
							b.error(o.problem(source.get()));
						else if (o.warning(source.get()) != null)
							b.add(b.text().warnify().add(o.warning(source.get())));
							
					}
				}
				
			});
			
		}
		
		cc = new GButt.ButtPanel(UI.icons().s.copy) {
			Paster pp = new Paster();
			
			@Override
			protected void clickA() {
				pp.current = source.get();
				VIEW.s().tools.place(pp);
				super.clickA();
			}
		};
		cc.hoverInfoSet(¤¤pasterD);
		cc.body.setHeight(48);
		addRightC(0, cc);
		
		
		body().incrW(cc.body.width());
		
	}	

	private class Detail {
		private MoveOrderPull o;
		private final GuiSection section = new GuiSection();
		private int oi;
		
		Detail(Placer placer, LIST<RESOURCE> resources){
			{
				section.add(new HOVERABLE.Sprite(placer.getIcon()).hoverInfoSet(placer.name()),  0, section.body().y2()+4);
				section.addRightCAbs(48, new CLICKABLE.ClickableAbs(200, 32) {
					
					final GText t = new GText(UI.FONT().S, 24);
					
					@Override
					protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected, boolean isHovered) {
						GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
						t.setMaxWidth(180);
						t.setMultipleLines(false);
						t.clear();
						t.normalify();
						if (o.source() == null) {
							t.add(¤¤notSet);
							t.errorify();
						}else {
							t.add(o.sourceI().name());
						}
						t.adjustWidth();
						t.renderC(r, body);
						GButt.ButtPanel.renderFrame(r, body);
						if (source.get().moveOrdersPull().length >= oi)
							return;
						if (isHovered && source.get().moveOrdersPull()[oi].source() != null) {
							RoomInstance ins = (RoomInstance) source.get().moveOrdersPull()[oi].source();
							SETT.OVERLAY().add(ins.mX(), ins.mY());
						}
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						text.title(placer.name());
						if (o.source() == null) {
							text.text(¤¤notSetC);
						}else {
							text.text(¤¤setC);
						}
						super.hoverInfoGet(text);
					}
					
					@Override
					protected void clickA() {
						if (o.source() == null) {
							placer.activate(oi);
						}else {
							VIEW.s().getWindow().centererTile.set(o.sourceI().body().cX(), o.sourceI().body().cY());
						}
					}
				});
			}
			
			{	
				RENDEROBJ oo = new RENDEROBJ.RenderImp(300, 64) {
					GText t = new GText(UI.FONT().S, 128);
					
					@Override
					public void render(SPRITE_RENDERER r, float ds) {
						t.clear();
						t.setMultipleLines(true);
						t.setMaxWidth(280);
						if (o.problem(source.get()) != null) {
							t.add(o.problem(source.get()));
							t.errorify();
						}else if(o.warning(source.get()) != null) {
							t.add(o.warning(source.get()));
							t.warnify();
						}
						t.adjustWidth();
						t.render(r, body.x1(), body.y1());
					}
				};
				section.add(oo, 0, section.body().y2()+4);
			}
			
			if (resources != null){
				int i = 0;
				int rows = 8;
				GuiSection s = new GuiSection();
				
				s.addGrid(new GButt.ButtPanel(UI.icons().m.ok) {
					@Override
					protected void clickA() {
						o.resbits.or(RBIT.ALL);
					}
					
				}, i++, rows, 0, 0);
				
				s.addGrid(new GButt.ButtPanel(UI.icons().m.cancel) {
					
					@Override
					protected void clickA() {
						o.resbits.clear();
					}
					
				}, i++, rows, 0, 0);
				
				for (RESOURCE r : resources) {
					
					s.addGrid(new GButt.ButtPanel(r.icon()) {
						
						@Override
						protected void renAction() {
							selectedSet(o.resbits.has(r));
						}
						
						@Override
						protected void clickA() {
							o.resbits.toggle(r);
						}
						
						@Override
						protected void render(SPRITE_RENDERER re, float ds, boolean isActive, boolean isSelected,
								boolean isHovered) {
							super.render(re, ds, isActive, isSelected, isHovered);
							if (o.source() == null || !o.source().moveCapacity().has(r.bit) || !source.get().moveOrderPullAccepted().has(r.bit)) {
								OPACITY.O50.bind();
								COLOR.BLACK.render(re, body, -4);
								OPACITY.unbind();
							}
						}
						
					}, i++, rows, 0, 0);
					
				}
				section.addRelBody(32, DIR.E, s);
			}
			
			INTE ii = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return 100;
				}
				
				@Override
				public int get() {
					return o.pullLimit;
				}
				
				@Override
				public void set(int t) {
					o.pullLimit = (byte) t;
				}
			};
			
			GSliderInt sl = new GSliderInt(ii, 160, true) {
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤Limit);
					b.text(¤¤LimitD);
					

					b.NL();
					b.add(GFORMAT.perc(b.text(), o.pullLimit/100.0));
					b.NL();
					
					if (o.source() != null) {
						for (RESOURCE res : RESOURCES.ALL()) {
							if (o.source().moveCapacity().has(res) && o.resbits.has(res)) {
								int am = o.source().moveCapacityAm(res);
								b.add(res.icon());
								b.add(GFORMAT.i(b.text(), (long) (o.pullLimit*am/100.0)));
								b.NL();
							}
						}
					}
				}
				
			};
			
			sl.addRelBody(8, DIR.W, new GHeader(¤¤Limit));
			
			section.addRelBody(6, DIR.S, sl);
			
			
			section.addRelBody(8, DIR.S, new GButt.ButtPanel(Dic.¤¤remove) {
				
				@Override
				protected void clickA() {
					source.get().moveOrdersPull()[oi] = null;
					VIEW.inters().popup.close();
				}
				
			});
		}
		
		public GuiSection get(int oi) {
			this.oi = oi;
			this.o = source.get().moveOrdersPull()[oi];
			return section;
		}
		
	}
	
	private static final ArrayListResize<MoveJob.ROOM_MOVE_SOURCE> prooms = new ArrayListResize<>(256);
	private static final int placerUIrows = 4;
	private class PlacerUI extends GuiSection {
		
		
		
		
		PlacerUI(){
			GTableBuilder bu = new GTableBuilder() {
				
				@Override
				public int nrOFEntries() {
					return (int) Math.ceil((double)prooms.size()/placerUIrows);
				}
			};
			
			bu.column(null, 100*placerUIrows, new GRowBuilder() {
				
				@Override
				public RENDEROBJ build(GETTER<Integer> ier) {
					GuiSection s = new GuiSection();
					for (int i = 0; i < placerUIrows; i++)
						s.addRight(0, new PlacerUIButt(ier, i));
					return s;
				}
			});
			
			add(bu.create(4, false));
		}
		
		@Override
		public void render(SPRITE_RENDERER r, float ds) {
			prooms.clearSoft();
			for (RoomBlueprintIns<?> i : SETT.ROOMS().ins()) {
				if (i.instancesSize() > 0 && i.getInstance(0) instanceof MoveJob.ROOM_MOVE_SOURCE) {
					for (int ii = 0; ii < i.instancesSize(); ii++) {
						if (i.getInstance(ii) != room.get())
							prooms.add((ROOM_MOVE_SOURCE) i.getInstance(ii));
					}
					
				}
			}
			SETT.OVERLAY().PULL.add(source.get());
			super.render(r, ds);
		}
		
	}
	
	private class PlacerUIButt extends GuiSection {

		private final GETTER<Integer> ier;
		private final int off;
		
		public PlacerUIButt(GETTER<Integer> ier, int off) {
			this.ier = ier;
			this.off = off;
			body().setWidth(100).setHeight(24);
		}
		
		@Override
		public void render(SPRITE_RENDERER r, float ds) {
			int i = ier.get()*placerUIrows + off;
			if (i < 0 || i >= prooms.size())
				return;
			MoveJob.ROOM_MOVE_SOURCE s = prooms.get(i);
			
			boolean sel = s.moveCapacity().has(source.get().moveOrderPullAccepted());
			
			
			GButt.ButtPanel.renderBG(r, sel, false, hoveredIs(), body());
			GButt.ButtPanel.renderFrame(r, body());
			
			
			
			if (s instanceof RoomInstance) {
				RoomInstance ins = (RoomInstance) s;
				ins.blueprintI().icon.small.render(r, body().x1()+4, body().y1()+4);
				if (hoveredIs() && KEYS.MAIN().MOD.isPressed()) {
					VIEW.s().getWindow().centererTile.set(ins.body().cX(), ins.body().cY());
					
				}
				SETT.OVERLAY().add(ins.mX(), ins.mY());
			}
			
			int x1 = body().x1()+32;
			
			for (RESOURCE res : RESOURCES.ALL()) {
				if (s.moveCapacity().has(res) && source.get().moveOrderPullAccepted().has(res)) {
					res.icon().small.render(r, x1, body().y1()+4);
					x1 += Icon.S;
					if (x1+Icon.S > body().x2())
						break;
				}
			}
			
			
			super.render(r, ds);
		}
		
		@Override
		public void hoverInfoGet(GUI_BOX text) {
			int i = ier.get()*placerUIrows + off;
			if (i < 0 || i >= prooms.size())
				return;
			
			GBox b = (GBox) text;
			{
				GText t = b.text();
				t.add(¤¤hold);
				t.insert(0, KEYS.MAIN().MOD.repr());
				t.warnify();
				b.add(t);
				b.sep();
			}

			MoveJob.ROOM_MOVE_SOURCE s = prooms.get(i);
			if (s instanceof RoomInstance) {
				RoomInstance ins = (RoomInstance) s;
				VIEW.s().ui.rooms.hover(b, ins, ins.mX(), ins.mY());
				
				
			}
			
			super.hoverInfoGet(text);
		}
		
		@Override
		protected void clickA() {
			int i = ier.get()*placerUIrows + off;
			if (i < 0 || i >= prooms.size())
				return;
			MoveJob.ROOM_MOVE_SOURCE s = prooms.get(i);
			if (source.get().moveOrdersPull()[placerII]  == null)
				source.get().moveOrdersPull()[placerII]  = new MoveOrderPull((RoomInstance) s, source.get().moveOrderPullAccepted());
			else
				source.get().moveOrdersPull()[placerII].destSet((RoomInstance) s);
			VIEW.s().tools.place(null);
			VIEW.s().ui.rooms.open(room.get());
			
			
			
			
			super.clickA();
		}
		
	}
	
	private class Placer extends PlacableSingle{

		private final ArrayListGrower<CLICKABLE> ebutts = new ArrayListGrower<>();
		private Room hov;
		private int hx,hy;
		
		
		public Placer() {
			super(¤¤name);
			
			ebutts.add(new PlacerUI());

		}

		public void activate(int ii) {
			placerII = ii;
			VIEW.s().tools.place(this);
		}
		
		@Override
		public void placeFirst(int tx, int ty) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			VIEW.s().tools.place(null);


			if (source.get().moveOrdersPull()[placerII]  == null)
				source.get().moveOrdersPull()[placerII]  = new MoveOrderPull((RoomInstance) r, source.get().moveOrderPullAccepted());
			else
				source.get().moveOrdersPull()[placerII].destSet((RoomInstance) r);
			VIEW.s().ui.rooms.open(room.get());
			

			
		}
		
		@Override
		public CharSequence isPlacable(int tx, int ty) {
			
			if (tx == VIEW.s().getWindow().tile().x() && ty == VIEW.s().getWindow().tile().y()) {
				SCompPath pp = SETT.PATH().comps.pather.findDest(room.get().mX(), room.get().mY(), tx, ty);
				if (pp == null)
					return Dic.¤¤Unreachable;
			}
			
			return pp(tx, ty);
			
		}
		
		
		private CharSequence pp(int tx, int ty) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			if (r != null && r != source.get() && r instanceof MoveJob.ROOM_MOVE_SOURCE) {
				hov = r;
				hx = tx;
				hy = ty;
				return null;
			}
			return ¤¤Choose;
		}
		
		@Override
		public void placeInfo(GBox b, int tiles) {
			if (hov != null) {
				VIEW.s().ui.rooms.hover(b, hov, hx, hy);
				hov = null;
			}
		}
		
		@Override
		public SPRITE getIcon() {
			return UI.icons().m.crossair;
		}
		
		@Override
		public boolean expandsTo(int fromX, int fromY, int toX, int toY) {
			if (pp(fromX,fromY) == null && SETT.ROOMS().map.get(fromX, fromY) == SETT.ROOMS().map.get(toX, toY))
				return true;
			return false;
		}
		
		@Override
		public LIST<CLICKABLE> getAdditionalButt() {
			return ebutts;
		}
	}
	
	private static class Paster extends PlacableMulti {

		MoveOrderPullInstance current;
		
		public Paster() {
			super(¤¤paster);
		}

		@Override
		public CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type) {
			Room i = SETT.ROOMS().map.get(tx, ty);
			return i != null && i != current && i.blueprint() == ((Room)current).blueprint() ? null : E;
		}

		@Override
		public void place(int tx, int ty, AREA area, PLACER_TYPE type) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			MoveOrderPullInstance i = (MoveOrderPullInstance) r;
			
			i.copyFrom(current);
			
			if (r.mX(tx, ty) != tx && r.mY(tx, ty) != ty)
				return;
			for (int oi = 0; oi < current.moveOrdersPull().length && oi < i.moveOrdersPull().length;  oi++) {
				i.moveOrdersPull()[oi] = null;
				MoveOrderPull o = current.moveOrdersPull()[oi];
				
				if (o != null && o.source() != null) {
					i.moveOrdersPull()[oi] = new MoveOrderPull(o.sourceI(), i.moveOrderPullAccepted());
					i.moveOrdersPull()[oi].pullLimit = o.pullLimit;
				}
			}
		}
		
		@Override
		public boolean expandsTo(int fromX, int fromY, int toX, int toY) {
			Room i = SETT.ROOMS().map.get(fromX, fromY);
			return i != null && i.isSame(fromX, fromY, toX, toY) && i != current && i.blueprint() == ((Room)current).blueprint();
		}
		
		
	}
	

}
