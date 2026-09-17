package settlement.room.service.module;

import java.io.Serializable;

import settlement.misc.util.FSERVICE;

public class RoomServiceInstance implements Serializable{

	private static final long serialVersionUID = 1L;
	private short available;
	private short reserved = 0;
	private final short total;
	public byte currentHigh;
	public byte lastHigh;
	
	
	public RoomServiceInstance(int total, RoomService data) {
		this.total = (short) total;
		data.increServices(this.total, 0);
	}
	
	public void report(FSERVICE s, RoomService data, int delta) {
		report(s, data, delta, true);
	}
	
	public void report(FSERVICE s, RoomService data, int delta, boolean load) {
		
		
		
		if(s.findableReservedCanBe()) {
			available += delta;
			data.increServices(0, delta);
			if (delta < 0) {
				data.finder.report(s, -1);
			}else {
				data.finder.report(s, 1);
			}
		}else if (s.findableReservedIs())
			reserved += delta;
		
		if (load) {
			byte h = (byte) (127*(total()-available())/(double)total());
			if (h > currentHigh)
				currentHigh = h;
			if (h > lastHigh)
				lastHigh = h;
		}
	}
	
	
	public void report(FSERVICE s, RoomService data, int delta, int reservable, int reserved) {
			
		this.reserved += reserved*delta;
		available += reservable*delta;
		data.increServices(0, reservable*delta);
		
		if(s.findableReservedCanBe()) {
			if (delta < 0) {
				data.finder.report(s, -1);
			}else {
				data.finder.report(s, 1);
			}
		}
		
		
		
		byte h = (byte) (127*(total()-available())/(double)total());
		if (h > currentHigh)
			currentHigh = h;
		if (h > lastHigh)
			lastHigh = h;
	}
	
	public int available() {
		return available;
	}
	
	public int total() {
		return total;
	}
	
	public int reserved() {
		return reserved;
	}
	
	public double load() {
		return lastHigh/127.0;
	}
	
	public void updateDay() {
		lastHigh = currentHigh;
		currentHigh = 0;
	}
	
	public void clearLoad() {
		lastHigh = 0;
		currentHigh = 0;
	}

	public void dispose(RoomService data) {
		data.increServices(-this.total, -available);
		available = 0;
	}
	
}
