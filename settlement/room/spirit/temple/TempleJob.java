package settlement.room.spirit.temple;

import game.audio.SoundRace;
import game.faction.FACTIONS;
import game.faction.FResources.RTYPE;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;

public abstract class TempleJob {

	protected final ROOM_TEMPLE blue;
	protected TempleInstance ins;
	protected final Coo coo = new Coo();
	protected TempleAltar altar;
	
	private TempleJob(ROOM_TEMPLE blue){
		this.blue = blue;
	}
	
	TempleJob get(int tx, int ty) {
		ins = blue.get(tx, ty);
		if (ins != null) {
			if (SETT.ROOMS().fData.tile.is(tx, ty, blue.constructor.ap)) {
				coo.set(tx, ty);
				for (int di = 0; di < DIR.ORTHO.size(); di++) {
					int dx = tx + DIR.ORTHO.get(di).x();
					int dy = ty + DIR.ORTHO.get(di).y();
					if (ins.is(dx, dy) && blue.altar.get(dx, dy) != null)
						altar = blue.altar.get(dx, dy);
				}
				
				return this;
			}
		}
		return null;
	}
	
	public abstract void jobReserve();

	public abstract boolean jobReservedIs();
	public abstract void jobReserveCancel();

	public abstract RBIT jobResourceBitToFetch();

	public abstract void jobStartPerforming();

	public abstract void jobPerform(Humanoid skill, int r);

	public SoundRace jobSound() {
		return blue.employment().sound();
	}

	public COORDINATE coo() {
		return coo;
	}
	
	public COORDINATE faceCoo() {
		return altar.coo();
	}

	public CharSequence jobName() {
		return blue.employment().verb;
	}
	
	public boolean shouldKill() {
		return false;
	}
	
	public void kill() {
		
	}
	
	public void reportMissingResource() {
		ins.resHas = false;
	}
	
	

	static final class Resources extends TempleJob{
		

		private final RoomBits reserved = new RoomBits(coo,	 	0b0000_0000_0000_0000_0000_0000_0000_0001);
		
		private final RESOURCE res;
		
		Resources(ROOM_TEMPLE blue, RESOURCE resources){
			super(blue);
			this.res = resources;
		}

		@Override
		public void jobReserve() {
			reserved.set(ins, 1);
		}

		@Override
		public boolean jobReservedIs() {
			return reserved.get() == 1;
		}

		@Override
		public void jobReserveCancel() {
			reserved.set(ins, 0);
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			if (altar.resourceNeeds()) {
				return res.bit;
			}
			return null;
		}

		@Override
		public void jobStartPerforming() {
			
		}

		@Override
		public void jobPerform(Humanoid skill, int res) {
			jobReserveCancel();
			if (res > 0) {
				altar.resourceInc(res);
				FACTIONS.player().res().inc(this.res, RTYPE.CONSUMED, -res);
			}
		}
		
		@Override
		public boolean shouldKill() {
			return altar.shouldKill();
		}
		
		@Override
		public void kill() {
			altar.kill();
		}


		@Override
		public SoundRace jobSound() {
			return blue.employment().sound();
		}
		
	}
	
	static final class None extends TempleJob{
		

		private final RoomBits reserved = new RoomBits(coo,	 	0b0000_0000_0000_0000_0000_0000_0000_0001);
		
		None(ROOM_TEMPLE blue){
			super(blue);
		}

		@Override
		public void jobReserve() {
			reserved.set(ins, 1);
		}

		@Override
		public boolean jobReservedIs() {
			return reserved.get() == 1;
		}

		@Override
		public void jobReserveCancel() {
			reserved.set(ins, 0);
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public void jobStartPerforming() {
			
		}

		@Override
		public void jobPerform(Humanoid skill, int res) {
			jobReserveCancel();
		}
		
		@Override
		public boolean shouldKill() {
			return altar.shouldKill();
		}
		
		@Override
		public void kill() {
			altar.kill();
		}


		@Override
		public SoundRace jobSound() {
			return blue.employment().sound();
		}
		
	}
	



}
