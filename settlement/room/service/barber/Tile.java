package settlement.room.service.barber;

import game.audio.SoundRace;
import init.resources.RBIT;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.misc.util.FSERVICE;
import settlement.room.main.util.RoomBits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;

final class Tile{

	
	private final Coo coo = new Coo();
	private Instance ins;
	private final ROOM_BARBER blue;
	
	private final int workTime;

	final RoomBits bWorked 		= new RoomBits(coo, 0b0000_0000_1111);
	final RoomBits bUses 		= new RoomBits(coo, 0b0000_0111_0000) {

		@Override
		protected void remove() {
			ins.service.report(service, ins.blueprintI().data, -1);
		};
		
		@Override
		protected void add() {
			ins.service.report(service, ins.blueprintI().data, 1);
		};
	};
	
	final RoomBits bWReserved 	= new RoomBits(coo, 0b0001_0000_0000);
	final RoomBits bSReserved 	= new RoomBits(coo, 0b0010_0000_0000) {
		
		@Override
		protected void remove() {
			ins.service.report(service, ins.blueprintI().data, -1);
		};
		
		@Override
		protected void add() {
			ins.service.report(service, ins.blueprintI().data, 1);
		};
	};
	
	Tile(ROOM_BARBER blue, int workTime){
		
		int t = workTime/16;
		this.workTime = t;
		this.blue = blue;
	}
	
	public SETT_JOB job(int tx, int ty) {
		if (init(tx, ty))
			return job;
		return null;
	}
	
	public FSERVICE service(int tx, int ty) {
		if (init(tx, ty))
			return service;
		return null;
	}
	
	private boolean init(int tx, int ty) {
		ins = blue.getter.get(tx, ty);
		
		if (ins != null && SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.IWORK) {
			coo.set(tx, ty);
			return true;
		}
		return false;
	}

	final SETT_JOB job = new SETT_JOB() {
		
		@Override
		public void jobReserve(RESOURCE r) {
			if (!jobReserveCanBe())
				throw new RuntimeException();
			bWReserved.set(ins, 1);
		}

		@Override
		public boolean jobReservedIs(RESOURCE r) {
			return 	bWReserved.get() == 1;
		}

		@Override
		public void jobReserveCancel(RESOURCE r) {
			bWReserved.set(ins, 0);
		}
		
		@Override
		public boolean jobReserveCanBe() {
			return bWReserved.get() == 0 && (bUses.get() < bUses.max() || bWorked.get() < bWorked.max());
		}

		@Override
		public RBIT jobResourceBitToFetch() {
			return null;
		}

		@Override
		public double jobPerformTime(Humanoid skill) {
			return workTime;
		}
		
		@Override
		public RESOURCE jobPerform(Humanoid skill, RESOURCE res, int ram) {
			if (!jobReservedIs(res))
				throw new RuntimeException();
			bWReserved.set(ins, 0);
			if (bWorked.get() == bWorked.max()) {
				bUses.inc(ins, 1);
				bWorked.set(ins, 0);
			}else {
				bWorked.inc(ins, 1);
			}
			return null;
		}

		@Override
		public COORDINATE jobCoo() {
			return coo;
		}

		private final static String name = "setting table";
		
		@Override
		public String jobName() {
			return name;
		}

		@Override
		public void jobStartPerforming() {
			// TODO Auto-generated method stub
			
		}

		@Override
		public boolean jobUseTool() {
			return false;
		}

		@Override
		public SoundRace jobSound() {
			return blue.employment().sound();
		}
		
		@Override
		public boolean jobUseHands() {
			return true;
		};


	};
	
	private final FSERVICE service = new FSERVICE() {
		
		@Override
		public int y() {
			return coo.y();
		}
		
		@Override
		public int x() {
			return coo.x();
		}
		
		@Override
		public boolean findableReservedIs() {
			return bSReserved.get() == 1;
		}
		
		@Override
		public boolean findableReservedCanBe() {
			return bSReserved.get() == 0 && bUses.get() > 0;
		}
		
		@Override
		public void findableReserveCancel() {
			bSReserved.set(ins, 0);
		}
		
		@Override
		public void findableReserve() {
			bSReserved.set(ins, 1);
		}
		
		@Override
		public void consume() {
			bSReserved.set(ins, 0);
			

			
			//create resource
		}
		
		@Override
		public void startUsing() {
			bUses.inc(ins, -1);
		};
		
		
	};



	
	
	
}
