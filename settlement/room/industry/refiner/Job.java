package settlement.room.industry.refiner;

import init.resources.RESOURCE;
import settlement.main.SETT;
import settlement.misc.job.SETT_JOB;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.main.job.RoomResDeposit;
import settlement.room.main.job.RoomResStorage;
import snake2d.util.datatypes.COORDINATE;

class Job{


	final RoomResDeposit FETCH;
	final RoomResStorage storage;
	
	Job(ROOM_REFINER print, int store) {
		
		storage = new RoomResStorage(store) {
			
			@Override
			public RESOURCE resource() {
				ROOM_PRODUCER_INSTANCE ins = (ROOM_PRODUCER_INSTANCE) SETT.ROOMS().map.get(this);
				return ins.industry().outs().get(0).resource;
			}

			@Override
			protected boolean is(int tx, int ty) {
				return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_STORAGE;
			}
			
			@Override
			protected void changed(int tx, int ty) {
				if (hasRoom()) {
					RefinerInstance m = print.get(tx, ty);
					m.hasStorage = true;
					m.jobs.searchAgain();
				}
					
			}
			
		};
		FETCH = new RoomResDeposit(print) {
			
			@Override
			protected boolean is(int tx, int ty) {
				return SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.B_WORK;
			}

			@Override
			protected void hasCallback() {
				// TODO Auto-generated method stub
				
			}

			@Override
			protected boolean regularJobCanBeReserved(COORDINATE coo) {
				RefinerInstance ins = print.get(coo.x(), coo.y());
				return ins.hasStorage;
			}

			@Override
			protected void regularJobStore(COORDINATE coo, int am) {
				RefinerInstance ins = print.get(coo.x(), coo.y());
				int x1 = ins.sx;
				int y1 = ins.sy;
				RoomResStorage ss = storage.get(x1, y1, ins);
				
				while(ss != null && am > 0) {
					if (ss.hasRoom()) {
						ss.deposit();
						am--;
						continue;
					}
					
					RoomResStorage sss = storage.get(ss.x()+1, ss.y(), ins);
					if (sss == null)
						sss = storage.get(x1, ss.y()+1, ins);
					ss = sss;
				}
				if (am > 0)
					ins.hasStorage = false;
				
			}

		};
	}
	
	SETT_JOB init(int tx, int ty, RefinerInstance ins) {
		return FETCH.get(tx, ty, ins);
	}
	
}
