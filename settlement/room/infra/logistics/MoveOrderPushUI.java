package settlement.room.infra.logistics;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.path.components.finder.SCompFinder.SCompPath;
import settlement.room.infra.logistics.MoveOrderPush.MoveOrderPushInstance;
import settlement.room.main.Room;
import settlement.room.main.RoomInstance;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE;
import snake2d.util.gui.clickable.CLICKABLE;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sprite.SPRITE;
import util.colors.GCOLOR;
import util.data.GETTER;
import util.data.INT.INTE;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GHeader;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;
import view.main.VIEW;
import view.tool.PlacableSingle;

public class MoveOrderPushUI extends GuiSection{
	
	private static CharSequence ¤¤notSet = "not set";
	private static CharSequence ¤¤notSetC = "Click to set a storage site to push to.";
	private static CharSequence ¤¤setC = "Click to go to push destination.";
	private static CharSequence ¤¤issue = "Click to issue a push order, which will deliver resources to another storage site.";
	private static CharSequence ¤¤name = "¤Push Destination";
	private static CharSequence ¤¤Choose = "¤Choose a storage site to push to.";
	private static CharSequence ¤¤NoneSeclected = "¤No push destinations have been set.";
	private static CharSequence ¤¤Limit = "¤Push Limit";
	private static CharSequence ¤¤LimitD = "¤Only push when the destination storage utilization is below this limit.";

	private final GETTER<? extends MoveOrderPushInstance> source;
	private final GETTER<? extends RoomInstance> room;
	static {
		D.ts(MoveOrderPushUI.class);
	}
	
	public MoveOrderPushUI(GETTER<? extends MoveOrderPushInstance> source, GETTER<? extends RoomInstance> room, int orderAm){
		
		this.source = source;
		this.room = room;
		Placer placer = new Placer();
		Detail popup = new Detail(placer);
		
		for (int i = 0; i < orderAm; i++) {
			
			final int oi = i;
			
			
			
			CLICKABLE c = new CLICKABLE.ClickableAbs(48, 48) {
				
				@Override
				protected void render(SPRITE_RENDERER r, float ds, boolean isActive, boolean isSelected, boolean isHovered) {
					GButt.ButtPanel.renderBG(r, isActive, isSelected, isHovered, body);
					if (source.get().moveOrdersPush()[oi] == null) {
						UI.icons().m.storage_push.renderC(r, body.cX(), body.cY());
						GButt.ButtPanel.renderFrame(r, body);
						return;
					}
					
					MoveOrderPush o = source.get().moveOrdersPush()[oi];
					
					if (isHovered && o.destI() != null) {
						SETT.OVERLAY().add(o.destI().mX(), o.destI().mY());
					}
					
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
				}
				
				@Override
				protected void clickA() {
					if (source.get().moveOrdersPush()[oi] == null) {
						placer.activate(oi);
					}else {
						VIEW.inters().popup.show(popup.get(oi), this);
					}
				}
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤name);
					MoveOrderPush o = source.get().moveOrdersPush()[oi];
					if (o == null) {
						b.text(¤¤issue);
						return;
					}else {
						b.textLL(¤¤name);
						b.tab(6);
						if (o.destI() == null) {
							b.error(¤¤notSet);
						}else {
							b.text(o.destI().name());
						}
						b.NL();
						
						if (o.problem(source.get()) != null)
							b.error(o.problem(source.get()));
						else if (o.warning(source.get()) != null)
							b.add(b.text().warnify().add(o.warning(source.get())));
							
					}
				}
				
			};
			addGrid(c, i, 4, 0, 0);
		}
		
	}	

	private class Detail {
		private MoveOrderPush o;
		private final GuiSection section = new GuiSection();
		private int oi;
		
		Detail(Placer placer){
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
						if (o.dest() == null) {
							t.add(¤¤notSet);
							t.errorify();
						}else {
							t.add(o.destI().name());
						}
						t.adjustWidth();
						t.renderC(r, body);
						GButt.ButtPanel.renderFrame(r, body);
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						text.title(placer.name());
						if (o.dest() == null) {
							text.text(¤¤notSetC);
						}else {
							text.text(¤¤setC);
						}
						super.hoverInfoGet(text);
					}
					
					@Override
					protected void clickA() {
						if (o.dest() == null) {
							placer.activate(oi);
						}else {
							VIEW.s().getWindow().centererTile.set(o.destI().body().cX(), o.destI().body().cY());
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
			
			INTE ii = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return 50;
				}
				
				@Override
				public int get() {
					return o.limit/2;
				}
				
				@Override
				public void set(int t) {
					o.limit = (byte) (t*2);
				}
			};
			
			GSliderInt sl = new GSliderInt(ii, 160, false) {
				
				@Override
				public void hoverInfoGet(GUI_BOX text) {
					GBox b = (GBox) text;
					b.title(¤¤Limit);
					b.text(¤¤LimitD);
					

					b.NL();
					b.add(GFORMAT.perc(b.text(), o.limit/100.0));
					b.NL();
					
					if (o.dest() != null) {
						for (RESOURCE res : RESOURCES.ALL()) {
							if (o.dest().moveCapacity().has(res) && source.get().moveOrderPushAvailable().has(res)) {
								double am = o.dest().storedD(res);
								b.add(res.icon());
								b.add(GFORMAT.perc(b.text(), am));
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
					source.get().moveOrdersPush()[oi] = null;
					VIEW.inters().popup.close();
				}
				
			});
		}
		
		public GuiSection get(int oi) {
			this.oi = oi;
			this.o = source.get().moveOrdersPush()[oi];
			return section;
		}
		
	}
	
	public class Placer extends PlacableSingle{


		private Room hov;
		private int hx,hy;
		
		private int ii;
		public Placer() {
			super(¤¤name);
		}

		public void activate(int ii) {
			this.ii = ii;
			VIEW.s().tools.place(this);
		}
		
		@Override
		public void placeFirst(int tx, int ty) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			VIEW.s().tools.place(null);


			if (source.get().moveOrdersPush()[ii]  == null)
				source.get().moveOrdersPush()[ii]  = new MoveOrderPush((RoomInstance) r);
			else
				source.get().moveOrdersPush()[ii].destSet((RoomInstance) r);
			VIEW.s().ui.rooms.open(room.get());
		}
		
		@Override
		public CharSequence isPlacable(int tx, int ty) {
			
			if (tx == VIEW.s().getWindow().tile().x() && ty == VIEW.s().getWindow().tile().y()) {
				SCompPath pp = SETT.PATH().comps.pather.findDest(room.get().mX(), room.get().mY(), tx, ty);
				if (pp == null)
					return Dic.¤¤Unreachable;
//				if (pp.distance() > source.get().moveMaxRadius())
//					return ¤¤TooFar;
			}
			
			return pp(tx, ty);
			
		}
		
		private CharSequence pp(int tx, int ty) {
			Room r = SETT.ROOMS().map.get(tx, ty);
			hov = null;
			if (r != null && r != source.get() && r instanceof MoveJob.ROOM_MOVE_DEST) {
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
	}
	
	public static CharSequence problem(MoveOrderPushInstance i) {
		boolean isOk = false;
		boolean has = false;
		CharSequence prob = null;
		for (MoveOrderPush o : i.moveOrdersPush())
			if (o != null) {
				has = true;
				CharSequence p = o.problem(i);
				if (p != null) {
					prob = p;
				}else
					isOk = true;
			}
		
		if (!isOk) {
			return prob;
		}
		if (!has)
			return ¤¤NoneSeclected;
		
		return null;
	}
	
}
