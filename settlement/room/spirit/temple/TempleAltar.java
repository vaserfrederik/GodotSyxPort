package settlement.room.spirit.temple;

import init.constant.C;
import init.race.RACES;
import init.race.Race;
import init.resources.RESOURCE;
import settlement.entity.animal.AnimalSpecies;
import settlement.main.SETT;
import settlement.room.main.util.RoomBits;
import settlement.thing.THINGS.Thing;
import settlement.thing.ThingsCadavers.Cadaver;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.misc.CLAMP;
import snake2d.util.rnd.RND;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public abstract class TempleAltar {

	protected final ROOM_TEMPLE blue;
	protected TempleInstance ins;
	protected Coo coo = new Coo();
	
	protected final RoomBits resources = new RoomBits(coo,	 	0b0000_0000_0000_0000_0000_0000_1111_1111);

	
	private TempleAltar(ROOM_TEMPLE blue){
		this.blue = blue;
	}
	
	TempleAltar get(int tx, int ty) {
		ins = blue.get(tx, ty);
		if (ins != null) {
			if (SETT.ROOMS().fData.tile.is(tx, ty, blue.constructor.es)) {
				coo.set(tx, ty);
				return this;
			}
		}
		return null;
	}
	
	void updateday(int tx, int ty) {
		if (get(tx, ty) == null)
			return;
		double d = blue.STIME;
		int am = (int) d;
		if (RND.rFloat() < (d-am))
			am++;
		
		updateDay(am);
	}
	
	abstract void updateDay(int sac);
	
	abstract void dispose(int tx, int ty);
	protected abstract void render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it);
	
	public void resourceInc(int am) {
		resources.inc(ins, am);
	}
	
	public boolean resourceNeeds() {
		return ins.resHas && resources.get() < CLAMP.i((int)Math.ceil(blue.STIME*3), 0, 10);
	}
	
	public COORDINATE coo() {
		return coo;
	}
	
	abstract boolean shouldKill();
	
	abstract void kill();

	static final class Resource extends TempleAltar{

		private final RESOURCE res;
		
		Resource(ROOM_TEMPLE blue, RESOURCE resources){
			super(blue);
			this.res = resources;
		}

		@Override
		protected void render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it) {
			int am = resources.get();
			
			if (am > 0) {
				res.renderLaying(r, it.x(), it.y(), it.ran(), am);
			}
			
		}

		@Override
		public void dispose(int tx, int ty) {
			
		}

		@Override
		void updateDay(int am) {
			ins.sacrificesTotal += am;
			int rr = resources.get();
			am = CLAMP.i(am, 0, rr);
			resources.inc(ins, -am);
			ins.sacrifices += am;
			ins.consumed += am;
			blue.consumed += am;
		}

		@Override
		boolean shouldKill() {
			return false;
		}

		@Override
		void kill() {
			// TODO Auto-generated method stub
			
		}
		
	}
	
	static final class Prisoner extends TempleAltar{
		
		protected final RoomBits needs =	new RoomBits(coo,	 		0b0000_0000_0000_0000_0000_0000_0000_0001);
		protected final RoomBits reserved =	new RoomBits(coo,	 		0b0000_0000_0000_0000_0000_0000_0000_0010);
		protected final RoomBits ready =	new RoomBits(coo,	 		0b0000_0000_0000_0000_0000_0000_0000_0100);
		protected final RoomBits kills = new RoomBits(coo,				0b0000_0000_0000_0000_0000_0000_0111_0000);
		protected final RoomBits race = new RoomBits(coo,				0b0000_0000_1111_1111_1111_0000_0000_0000);
		
		Prisoner(ROOM_TEMPLE blue){
			super(blue);
		}

		@Override
		protected void render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it) {
			long ran = it.bigRan();
			int a = kills.get();
			COLOR col = RACES.all().get(race.get()).appearance().colors.blood;
			col.bind();
			if (a > 0) {
				int cx = it.x()+C.TILE_SIZEH;
				int cy = it.y()+C.TILE_SIZEH;
				for (int i = 0; i < a; i++) {
					int xx = (int) (cx + (-4 + (ran&0x07))*C.SCALE);
					ran = ran >> 3;
					int yy = (int) (cy + (-4 + (ran&0x07))*C.SCALE);
					ran = ran >> 3;
					SETT.THINGS().sprites.bloodPool.render(r, (int) (ran&0x0F), xx, yy);
					ran = ran >> 4;
				}
			}
			COLOR.unbind();
			
		}

		@Override
		public void dispose(int tx, int ty) {
			
		}
		
		@Override
		boolean shouldKill() {
			return (ready.get() == 1 && kills.get() < kills.max());
		}
		
		@Override
		void kill() {
			if (!shouldKill())
				return;
			SETT.THINGS().gore.gore(coo.x()*C.TILE_SIZE+C.TILE_SIZEH, coo.y()*C.TILE_SIZE+C.TILE_SIZEH, RACES.all().get(race.get()).appearance().colors.blood);
			kills.inc(ins, 1);
		}

		@Override
		void updateDay(int am) {
			am = CLAMP.i(am, 0, 1);
			if (am == 0)
				return;
			if (reserved.get() == 1) {
				return;
			}
			
			if (needs.get() == 1)
				ins.sacrificesTotal += am;
			else
				ins.sacrificesRequired += 1;
			needs.set(ins, 1);
		}
		
		public void sacrificeReserve(Race r) {
			if (reserved.get() == 0) {
				reserved.set(ins, 1);
				race.set(ins, r.index());
				ins.sacrificesRequired -= 1;
			}
		}
		
		public void sacrificeUnreserve() {
			if (reserved.get() == 1) {
				ins.sacrificesRequired += 1;
			}
			reserved.set(ins, 0);
			if (ready.get() == 1) {
				ins.sacrificesTotal += 1;
				ins.sacrifices += 1;
				ins.sacrificesRequired -= 1;
				ins.consumed += 1;
				blue.consumed += 1;
				needs.set(ins, 0);
				reserved.set(ins, 0);
				ready.set(ins, 0);
			}
			
		}
		
		public boolean sacrificeReservable() {
			return needs.get() == 1 && reserved.get() == 0;
		}
		
		public boolean sacrificeReserved() {
			return reserved.get() == 1;
		}
		
		
		public void sacrificeReady() {
			if (reserved.get() == 1) {
				ready.set(ins, 1);
				kills.set(ins, 0);
			}
		}
		
		public double sacrificeKillAmount() {
			return kills.getD();
		}
		
	}
	
	static final class Animal extends TempleAltar{

		protected final RoomBits hasSacrifice =	new RoomBits(coo,	 		0b0000_0000_0000_0000_0000_0001_0000_0000);
		protected final RoomBits kills =			new RoomBits(coo,	 	0b0000_0000_0000_0000_0000_1110_0000_0000);
		
		Animal(ROOM_TEMPLE blue){
			super(blue);
		}

		@Override
		protected void render(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it) {
			long ran = it.bigRan();
			int a = kills.get();
			if (a > 0) {
				int cx = it.x()+C.TILE_SIZEH;
				int cy = it.y()+C.TILE_SIZEH;
				for (int i = 0; i < a; i++) {
					int xx = (int) (cx + (-4 + (ran&0x07))*C.SCALE);
					ran = ran >> 3;
					int yy = (int) (cy + (-4 + (ran&0x07))*C.SCALE);
					ran = ran >> 3;
					SETT.THINGS().sprites.bloodPool.render(r, (int) (ran&0x0F), xx, yy);
					ran = ran >> 4;
				}
			}else {
				int am = resources.get();
				
				if (am > 0) {
					blue.resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
				}
			}
			
			
			
		}

		@Override
		public void dispose(int tx, int ty) {
			
		}
		
		@Override
		boolean shouldKill() {
			if (hasSacrifice.get() == 1 && kills.get() < kills.max()) {
				cadaver();
				return true;
			}
			return false;
		}
		
		@Override
		void kill() {
			if (!shouldKill())
				return;
			
			kills.inc(ins, 1);
			Cadaver c = cadaver();
			if (c == null)
				return;
			SETT.THINGS().gore.gore(c.body().cX(), c.body().cY(), c.spec().blood);
			c.setInjuries(kills.getD());
		}

		@Override
		void updateDay(int am) {
			am = CLAMP.i(am, 0, 1);
			ins.sacrificesTotal += am;
			int rr = resources.get();
			am = CLAMP.i(am, 0, rr);
			resources.inc(ins, -am);
			ins.sacrifices += am;
			
			hasSacrifice.set(ins, am);
			
			ins.consumed += am;
			blue.consumed += am;
			
			kills.set(ins, 0);
			Thing t = SETT.THINGS().getFirst(coo.x(), coo.y());
			while (t != null) {
				t.remove();
				t = SETT.THINGS().getFirst(coo.x(), coo.y());
			}
			
			
			
		}
		
		private Cadaver cadaver() {
			Cadaver c = SETT.THINGS().cadavers.tGet.get(coo);
			if (c == null) {
				AnimalSpecies s = SETT.ANIMALS().sett().get(RND.rInt(SETT.ANIMALS().sett().size()));
				c = SETT.THINGS().cadavers.normal(coo.x()*C.TILE_SIZE+C.TILE_SIZEH, coo.y()*C.TILE_SIZE+C.TILE_SIZEH, 0, 0, s, 0);
			}
			return c;
		}
		
	}


}
