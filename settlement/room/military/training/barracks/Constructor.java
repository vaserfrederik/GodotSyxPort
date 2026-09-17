package settlement.room.military.training.barracks;

import static settlement.main.SETT.ROOMS;

import java.io.IOException;

import game.GAME;
import settlement.path.AVAILABILITY;
import settlement.room.main.Room;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.furnisher.FurnisherItemTile;
import settlement.room.main.furnisher.FurnisherStat;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import settlement.room.sprite.RoomSprite1x1;
import settlement.room.sprite.RoomSpriteBoxN;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.Json;
import util.gui.misc.GText;
import util.info.GFORMAT;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;


final class Constructor extends Furnisher{
	
	final FurnisherItemTile manikin;
	final FurnisherItemTile work;
	private final ROOM_BARRACKS blue;
	
	final FurnisherStat men = new FurnisherStat(this, 0) {
		
		@Override
		public double get(AREA area, double fromItems) {
			return fromItems;
		}
		
		@Override
		public GText format(GText t, double value) {
			return GFORMAT.i(t, (int)value);
		}
	};
	
	protected Constructor(ROOM_BARRACKS blue, RoomInitData init) throws IOException {
		super(init, 1, 1, 88, 44);
		this.blue = blue;
		Json js = init.data().json("SPRITES");
		RoomSpriteBoxN sPedi = new RoomSpriteBoxN(js, "PODEUM_BOX") {
			@Override
			protected boolean joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item) {
				return item.sprite(rx, ry) != null;
			}
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				super.render(r, s, data, it, degrade, false);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				return false;
			}
		};
		
		RoomSprite1x1 sMani = new RoomSprite1x1(js, "MANAKIN_A_1X1") {
			RoomSprite1x1 sMani2 = new RoomSprite1x1(js, "MANAKIN_B_1X1");
			
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sPedi.renderBelow(r, s, getData2(it), it, degrade);
			}
			
			@Override
			public boolean render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
					boolean isCandle) {
				int rot = (it.ran()>>4);
				if (blue.is(it.tile()) && BarracksThing.used.is(ROOMS().data.get(it.tile())))
					rot += GAME.intervals().get05();
				rot &= 0x07;
				if ((rot & 1) == 1) {
					return sMani2.render(r, s, rot>>1, it, degrade, false);
				}else 
					return super.render(r, s, rot>>1, it, degrade, false);
			}
			
			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sPedi.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		RoomSprite1x1 sCandle = new RoomSprite1x1(js, "TABLE_1X1") {
			@Override
			public void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade) {
				sPedi.renderBelow(r, s, getData2(it), it, degrade);
			}

			@Override
			public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan) {
				return sPedi.getData(tx, ty, rx, ry, item, itemRan);
			}
			
		};
		
		FurnisherItemTile ma = new FurnisherItemTile(this, false, sMani, AVAILABILITY.ROOM_SOLID, false);
		FurnisherItemTile ca = new FurnisherItemTile(this, false, sCandle, AVAILABILITY.ROOM_SOLID, true);
		FurnisherItemTile st = new FurnisherItemTile(this, true, sPedi, AVAILABILITY.AVOID_PASS, false);
		FurnisherItemTile ee = new FurnisherItemTile(this, false, sPedi, AVAILABILITY.AVOID_PASS, false);
		
		manikin = ma;
		work = st;
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ca},
					{st,ee}
				},
				1);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ca,ma},
					{st,ee,st}
				},
				2);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ca,ma,ma},
					{st,ee,st,st}
				},
				3.0);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ma,ca,ma,ma},
					{st,st,ee,st,st}
				},
				4.0);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ma,ma,ca,ma,ma},
					{st,st,st,ee,st,st}
				},
				5.0);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{ma,ma,ma,ca,ma,ma,ma},
					{st,st,st,ee,st,st,st}
				},
				6);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st},
					{ma,ma},
					{ma,ca},
					{st,ee}
				},
				3);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st,st},
					{ma,ma,ma},
					{ma,ca,ma},
					{st,ee,st}
				},
				5);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st,st,st},
					{ma,ma,ma,ma},
					{ma,ca,ma,ma},
					{st,ee,st,st}
				},
				7);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st,st,st,st},
					{ma,ma,ma,ma,ma},
					{ma,ma,ca,ma,ma},
					{st,st,ee,st,st}
				},
				9);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st,st,st,st,st},
					{ma,ma,ma,ma,ma,ma},
					{ma,ma,ma,ca,ma,ma},
					{st,st,st,ee,st,st}
				},
				11);
		
		new FurnisherItem(
				new FurnisherItemTile[][] {
					{st,st,st,st,st,st,st},
					{ma,ma,ma,ma,ma,ma,ma},
					{ma,ma,ma,ca,ma,ma,ma},
					{st,st,st,ee,st,st,st}
				},
				13);
		
		flush(1, 3);
	}

	@Override
	public boolean usesArea() {
		return true;
	}

	@Override
	public boolean mustBeIndoors() {
		return true;
	}
	
	@Override
	public ROOM_BARRACKS blue() {
		return blue;
	}
	
	@Override
	public Room create(TmpArea area, RoomInit init) {
		return new BarracksInstance(blue, area, init);
	}
	@Override
	public boolean isHeavy() {
		return true;
	}

}
