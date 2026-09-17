package settlement.room.law.guard;

import java.io.IOException;
import java.util.Arrays;

import settlement.entity.ENTITY;
import settlement.entity.humanoid.Humanoid;
import settlement.entity.humanoid.ai.main.AI;
import settlement.main.SETT;
import settlement.room.law.execution.ExecutionStation.Guard;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.rnd.RND;

public class CrimeReporter {

	private static int EE = 5;
	private int[] data;
	
	private static int tCrime = 0;
	private static int tExecution = 1;
	
	private final ROOM_GUARD b;
	
	CrimeReporter(ROOM_GUARD b){
		this.b = b;
		data = Alloc.ii(256*2);
	}
	
	void save(FilePutter file) {
		file.isE(data);
		
	}

	void load(FileGetter file) throws IOException {
		file.isE(data);
	}

	void clear() {
		Arrays.fill(data, 0);
	}

	int[] makeData() {
		return Alloc.ii(EE*2 + 2);
	}
	
	int[] data(GuardInstance ins) {
		if (ins.cdata == null || ins.cdata.length != EE*2+2)
			ins.cdata = Alloc.ii(EE*2+2);
		return ins.cdata;
	}
	
	public void reportCriminal(Humanoid a) {
		
		if (RND.rBoolean())
			return;
		report(tCrime, a.tc().x(), a.tc().y(), 90, a.id());
	}

	public void reportExecution(int tx, int ty) {
		int payload = ((tx << 16)&~0x0FFFF) | (ty&0x0FFFF);
		report(tExecution, tx, ty, 180, payload);
	}
	
	public int crimes(GuardInstance ins) {
		if (ins != null)
			return data(ins)[tCrime];
		return data[tCrime];
	}
	
	public int executions(GuardInstance ins) {
		if (ins != null)
			return data(ins)[tExecution];
		return data[tExecution];
	}
	
	private void report(int type, int sx, int sy, int radius, int payload) {
		
		COORDINATE c = b.finder.reserve(sx, sy, radius);
		
		if (c != null) {
			GuardInstance ins = b.getter.get(c);
			int[] data = data(ins);
			
			boolean av = available(data);
			push(type, payload, data);
			
			if (av && !available(data))
				b.finder.report(b.service.get(ins), -1);
		}else if (RND.oneIn(4)){
			push(type, payload, data);
		}
	}
	
	private boolean push(int stride, int payload, int data[]) {

		int length = (data.length-2)/2;
		int count = data[stride];
		if (count >= length)
			return false;
		data[2 + length*stride + count] = payload;
		data[stride]++;
		return true;
	}
	
	public Humanoid pollCriminal(GuardInstance ins) {
		
		if (ins != null) {
			
			boolean av = available(data);
			

			int id = pop(tCrime, data(ins));
			if (!av && available(data))
				b.finder.report(b.service.get(ins), 1);
			while (id >= 0) {
				ENTITY e = SETT.ENTITIES().getByID(id);
				if (e != null && e instanceof Humanoid && !e.isRemoved()) {
					Humanoid a = (Humanoid) e;
					if (AI.modules().isCriminal(a))
						return a;
				}
				id = pop(tCrime, ins.cdata);
			}
		}
		
		int id = pop(tCrime, data);
		while (id >= 0) {
			ENTITY e = SETT.ENTITIES().getByID(id);
			if (e != null && e instanceof Humanoid && !e.isRemoved()) {
				Humanoid a = (Humanoid) e;
				if (AI.modules().isCriminal(a))
					return a;
			}
			id = pop(tCrime, data);
		}
		return null;
	}
	
	public Guard pollExecution(GuardInstance ins) {
		
		if (ins != null) {
			
			boolean av = available(data);
			

			int id = pop(tExecution, data(ins));
			if (!av && available(data))
				b.finder.report(b.service.get(ins), 1);
			while (id >= 0) {
				int tx = (id >> 16)&0x0FFFF;
				int ty = id &0x0FFFF;

				Guard g = SETT.ROOMS().EXECUTION.stations.guard(tx, ty);
				if (g != null && g.active())
					return g;
				id = pop(tCrime, ins.cdata);
			}
		}
		
		int id = pop(tExecution, data);
		while (id >= 0) {
			int tx = (id >> 16)&0x0FF;
			int ty = id &0x0FF;
			
			Guard g = SETT.ROOMS().EXECUTION.stations.guard(tx, ty);
			if (g != null && g.active())
				return g;
			id = pop(tCrime, data);
		}
		return null;
	}
	
	public boolean available(GuardInstance ins) {
		return available(data(ins));
	}
	
	private boolean available(int data[]) {
		int length = (data.length-2)/2;
		if (data[tCrime] >= length || data[tExecution] >= length)
			return false;
		return true;
	}
	

	
	private int pop(int stride, int data[]) {
		int length = (data.length-2)/2;
		int count = data[stride];
		if (count == 0)
			return -1;
		data[stride]--;
		return data[2 + length*stride + count-1];
	}


	
}
