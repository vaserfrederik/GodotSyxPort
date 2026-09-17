package settlement.room.infra.transport;

import java.io.Serializable;

import game.time.TIME;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.path.AVAILABILITY;
import settlement.room.infra.logistics.MoveOrderPull;
import snake2d.util.datatypes.COORDINATE;

class Cart implements Serializable{

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;

	public final static int PREPARATION_TIME = (int) TIME.workSeconds();
	
	
	private short delivering; 
	private short resource = -1;
	private short loaded;
	private short unloaded;
	private short unloadedSpots;
	private double preparation;
	
	Cart(){
		
	}
	
	public int stored() {
		return loaded;
	}
	
	public void store(int am) {
		loaded += am;
	}
	
	void unloadedInc(int am){
		unloaded += am;
	}
	
	public boolean needsPrep() {
		return preparation < PREPARATION_TIME;
	}
	
	public double prepD() {
		return preparation /PREPARATION_TIME;
	}
	
	public void prep(double time) {
		preparation += time;			
	}
	
	public int unloaded() {
		return unloaded;
	}
	void unloadedSpotsInc(int am){
		unloadedSpots += am;
	}
	
	int unloadedSpots(){
		return unloadedSpots;
	}
	
	public RESOURCE resource() {
		if (resource == -1)
			return null;
		return RESOURCES.ALL().get(resource);
	}
	
	protected void loadFix() {
		resource = (byte) RESOURCES.map().loader().fix(resource, -1);
	};
	
	void empty() {
		loaded = 0;
	}
	
	void clear() {
		empty();
	}
	
	void coco() {
		preparation -= PREPARATION_TIME;
		if (preparation < 0)
			preparation = 0;
	}
	
	void go() {
		delivering += loaded;
		clear();
		coco();
		
	}
	
	void deliver(int am) {
		delivering -= am;
	}
	
	public int delivering() {
		return delivering;
	}
	
	public void deliverIncrease(int am) {
		delivering += am;
	}
	
	public void resourceSet(RESOURCE res, TransportInstance ins) {
		
		RESOURCE old = resource();
		if (old == res) {
			return;
		}
		int am = stored();
		
		empty();
		preparation = 0;
		ins.blueprintI().job.remove(ins);
		resource = res == null ? -1 : (short) res.index();
		ins.blueprintI().job.add(ins);
		for (MoveOrderPull o : ins.pullOrders) {
			if (o != null)
				o.resbits.clearSet(ins.moveCapacity());
		}
		
		if (old != null && am > 0) {
			for (COORDINATE c : ins.body()) {
				if (ins.is(c) && SETT.PATH().availability.get(c) == AVAILABILITY.ROOM) {
					SETT.THINGS().resources.create(c, old, am);
					return;
				}
			}
		}
		
	}
	
	public boolean canGo() {
		if (resource() == null)
			return false;
		if (preparation < PREPARATION_TIME)
			return false;
		if (loaded >= ROOM_TRANSPORT.MAX_LOAD)
			return true;
		return false;
		
	}
	
	public boolean cartVisible() {
		return preparation >= PREPARATION_TIME || loaded > 0;
	}
	
	public boolean oxVisible() {
		return preparation >= PREPARATION_TIME;
	}
	
}
