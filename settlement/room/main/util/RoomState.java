package settlement.room.main.util;

import java.io.Serializable;

import game.faction.FACTIONS;
import init.type.WGROUP.HTypeBitsImp;
import settlement.room.industry.module.INDUSTRY_HASER;
import settlement.room.industry.module.ROOM_IDATA_INSTANCE;
import settlement.room.industry.module.ROOM_PRODUCER_INSTANCE;
import settlement.room.industry.module.consumption.RoomConsumption;
import settlement.room.industry.module.consumption.RoomConsumption.ROOM_CONSUMPTION_HASER;
import settlement.room.main.Room;
import settlement.room.main.RoomInstance;
import settlement.room.main.job.ROOM_EMPLOY_AUTO;
import settlement.room.main.job.ROOM_RADIUS.ROOM_RADIUS_INSTANCE;

public abstract class RoomState implements Serializable{
	
	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	public abstract void apply(Room r, int tx, int ty);
	public abstract void applyRepaired(Room r, int tx, int ty);
	
	public static class RoomStateInstance extends RoomState {

		private static final long serialVersionUID = 1L;
		private final int workersTarget;
		private final int industry;
		private boolean auto;
		private byte radius;
		private int consumptionMask;
		private String name;
		private final HTypeBitsImp pref = new HTypeBitsImp(false);
		
		public RoomStateInstance(RoomInstance ins) {
			this.name = ""+ins.name();
			
			pref.copy(ins.employees().preffered());
			
			this.workersTarget = ins.employees().hardTarget();
			if (ins.blueprintI() instanceof ROOM_EMPLOY_AUTO) {
				ROOM_EMPLOY_AUTO a = (ROOM_EMPLOY_AUTO) ins.blueprintI();
				auto = a.autoEmploy(ins);
			}
			if (ins instanceof ROOM_PRODUCER_INSTANCE) {
				industry = ((ROOM_PRODUCER_INSTANCE) ins).industryI();
			}else {
				industry = 0;
			}
			if (ins instanceof ROOM_RADIUS_INSTANCE)
				radius = ((ROOM_RADIUS_INSTANCE) ins).radiusRaw();
			if (ins.blueprintI() instanceof ROOM_CONSUMPTION_HASER) {
				RoomConsumption ii = ((ROOM_CONSUMPTION_HASER)ins.blueprintI()).consumption();
				for (int i = 0; i < ii.ins().size(); i++) {
					if (ii.enabled(ii.ins().get(i), (ROOM_IDATA_INSTANCE) ins)) {
						consumptionMask |= 1<<i;
					}
				}
				
			}
		}
		
		@Override
		public void apply(Room room, int tx, int ty) {
			if (!(room instanceof RoomInstance))
				return;
			
			RoomInstance ins = (RoomInstance) room;
			//ins.name().clear().add(name);
			if (ins.blueprintI().employment() != null) {
				ins.employees().neededSet(workersTarget);
			}
			if (ins.blueprintI() instanceof ROOM_EMPLOY_AUTO) {
				ROOM_EMPLOY_AUTO a = (ROOM_EMPLOY_AUTO) ins.blueprintI();
				a.autoEmploy(ins, auto);
			}
			if (ins instanceof ROOM_PRODUCER_INSTANCE && ins.blueprint() instanceof INDUSTRY_HASER) {
				INDUSTRY_HASER h = (INDUSTRY_HASER) ins.blueprint();
				if (industry >= 0 && industry < h.industries().size() && h.industries().getC(industry).lockable().passes(FACTIONS.player()))
					((ROOM_PRODUCER_INSTANCE) ins).setIndustry(industry);
			}
			if (ins instanceof ROOM_RADIUS_INSTANCE)
				((ROOM_RADIUS_INSTANCE) ins).radiusRawSet(radius);
			if (ins.blueprintI() instanceof ROOM_CONSUMPTION_HASER) {
				RoomConsumption ii = ((ROOM_CONSUMPTION_HASER)ins.blueprintI()).consumption();
				for (int i = 0; i < ii.ins().size(); i++) {
					boolean e = (consumptionMask & (1 << i)) != 0;
					if (e != ii.enabled(ii.ins().get(i), (ROOM_IDATA_INSTANCE) ins)) {
						ii.enabledToggle(ii.ins().get(i), (ROOM_IDATA_INSTANCE) ins, ins);
					}
				}
				
			}
			applyIns(ins);
		}
		
		protected void applyIns(RoomInstance ins) {
			
		}
		
		@Override
		public void applyRepaired(Room room, int tx, int ty) {
			if (!(room instanceof RoomInstance))
				return;
			RoomInstance ins = (RoomInstance) room;
			ins.name().clear().add(name);
			if (ins.blueprintI().employment() != null) {
				ins.employees().prefferedSet(pref);
			}
		}
		
	}
	
	public final static RoomState DUMMY = new RoomState() {
		
		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;

		@Override
		public void apply(Room r, int tx, int ty) {
			
		}

		@Override
		public void applyRepaired(Room r, int tx, int ty) {
			// TODO Auto-generated method stub
			
		}
	};
	
}