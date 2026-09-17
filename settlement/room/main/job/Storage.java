package settlement.room.main.job;

import static settlement.main.SETT.PATH;

import java.io.Serializable;

import game.boosting.Boostable;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.main.SETT;
import settlement.misc.util.RESOURCE_TILE;
import settlement.misc.util.TILE_STORAGE;
import settlement.path.path.SPath;
import settlement.room.main.ROOMA;
import snake2d.Errors;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.DIR;

public abstract class Storage implements RESOURCE_TILE, TILE_STORAGE{

	final static int noRes = 0x0;
	private int tx, ty;
	private final int max;
	private StorageData data;
	
	protected Storage(int max){
		this.max = max;
		if (RESOURCES.ALL().size() +1 > 0x0FF) {
			throw new Errors.GameError("Too many resources are declared: " + RESOURCES.ALL().size());
		}
	}
	
	public StorageData[] make(ROOMA room) {
		int am = 0;
		for (COORDINATE c : room.body()) {
			if (room.is(c) && is(c.x(), c.y())) {
				SETT.ROOMS().data.set(room, c, am);
				am++;
			}
		}
		StorageData[] res = new StorageData[am];
		for (int i = 0; i < res.length; i++) {
			res[i] = new StorageData();
		}
		return res;
	}
	
	public Storage get(int tx, int ty, StorageData[] data) {
		if (is(tx, ty)) {
			int i = SETT.ROOMS().data.get(tx, ty);
			this.tx = tx;
			this.ty = ty;
			this.data = data[i];
			return this;
		}
		return null;
	}
	
	protected abstract boolean is(int tx, int ty);
	
	protected abstract int max();
	
	@Override
	public void storageDeposit(int amount) {
		if (resource() == null || amount() + amount > max)
			throw new RuntimeException(resource() + " " + amount() + " " + amount + " " + max);
		reservedSpaceSet(reservedSpace()-amount);
		amountSet(amount()+amount);
	}

	@Override
	public int storageReserved() {
		if (resource() == null)
			return 0;
		return data.bReservedSpace;
	}

	@Override
	public int storageReservable() {
		if (resource() == null)
			return 0;
		return max - (amount() + storageReserved());
	}

	@Override
	public void storageReserve(int amount) {
		if (storageReservable() < amount)
			throw new RuntimeException(storageReservable() + " " + amount);
			
		reservedSpaceSet(reservedSpace()+amount);
	}

	@Override
	public void storageUnreserve(int amount) {
		if (storageReserved() < amount)
			amount = storageReserved();
		reservedSpaceSet(storageReserved()-amount);
	}
	
	@Override
	public RESOURCE resource() {
		int i = data.res;
		if (i == noRes)
			return null;
		return RESOURCES.ALL().get(i-1);
	}
	
	public void resourceSet(RESOURCE res) {
		if (resource() != null) {
			throw new RuntimeException();
		}
		data.res = (short) (res.index()+1);
		add();
	}
	
	public void remove() {
		RESOURCE r = resource();
		if (r != null) {
			count(r.bIndex(), -1, -data.bAmount, -(data.bAmount - data.bReserved), -data.bReservedSpace);
			if (findableReservedCanBe())
				PATH().finders.resource.reportAbsence(this);
			if (storageReservable() > 0)
				PATH().finders.storage.reportAbsence(this);
		}
	}
	
	public void add() {
		RESOURCE r = resource();
		if (r != null) {
			count(r.bIndex(), 1, data.bAmount, data.bAmount - data.bReserved, data.bReservedSpace);
			if (findableReservedCanBe()) {
				PATH().finders.resource.reportPresence(this);
			}
			if (storageReservable() > 0) {
				PATH().finders.storage.reportPresence(this);
			}
		}
	}
	
	protected abstract void count(int res, int crates, int amountTot, int amountUnres, int spaceRes);

	@Override
	public int amount() {
		return data.bAmount;
	}
	
	public void amountSet(int am) {
		remove();
		data.bAmount = (short) am;
		add();
	}
	
	public int reserved() {
		return data.bReserved;
	}
	
	public void reservedSet(int r) {
		remove();
		data.bReserved = (short) r;
		add();
	}
	
	public int reservedSpace() {
		return data.bReservedSpace;
	}
	
	private void reservedSpaceSet(int r) {
		remove();
		data.bReservedSpace = (short) r;
		add();
	}
	
	@Override
	public int y() {
		return ty;
	}
	
	@Override
	public int x() {
		return tx;
	}
	
	@Override
	public boolean findableReservedIs() {
		return data.bReserved > 0;
	}
	
	@Override
	public boolean findableReservedCanBe() {
		return data.bReserved < data.bAmount;
	}
	
	@Override
	public void findableReserveCancel() {
		if (reserved() > 0)
			reservedSet(reserved()-1);
	}
	
	@Override
	public void findableReserve() {
		reservedSet(reserved()+1);
	}
	
	@Override
	public void resourcePickup() {
		findableReserveCancel();
		amountSet(amount()-1);
	}
	
	@Override
	public int reservable() {
		return Storage.this.amount()-Storage.this.reserved();
	}
	
	public void clear() {
		if (resource() == null)
			return;
		int am = amount();
		remove();
		if (am > 0) {
			for (DIR dd : DIR.ORTHO) {
				if (!PATH().solidity.is(this, dd)) {
					SETT.THINGS().resources.create(x()+dd.x(), y()+dd.y(), resource(), am);
					break;
				}
			}
		}
		data.res = noRes;
	}
	
	public void dispose(){
		RESOURCE res = resource();
		if (res == null)
			return;
		int am = amount();
		remove();
		if (am > 0)
			SETT.THINGS().resources.create(tx, ty, res, am);
		
		data.res = noRes;
	}

	@Override
	public boolean isFindable() {
		return true;
	}

	public interface STORAGE_CRATE_HASSER {
		
		public TILE_STORAGE job(COORDINATE start, SPath path);
		public TILE_STORAGE job(int tx, int ty);
		
		public boolean getsMaximum(RESOURCE res);
		
		public default boolean fetchesFromEveryone(RESOURCE res) {
			return false;
		}
		
		public Boostable carryBonus();
		
	}

	public class StorageData implements Serializable{

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		short res;
		short bAmount;
		short bReserved;
		short bReservedSpace;
		
		private StorageData() {
			
		}

	}
	
}