package settlement.room.industry.module;

import java.io.IOException;

import game.time.TIME;
import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.INDEXED;
import util.data.DOUBLE_O.DOUBLE_OE;
import util.data.DataOSimple;
import util.data.INT_O.INT_OE;
import util.statistics.HISTORY_INT;
import util.statistics.HistoryInt;

public abstract class IndustryResource implements INDEXED, SAVABLE{
	
	public final RESOURCE resource;
	public final double rate;
	public final double AIRate;
	public final double AIRecovery;
	public final double rateSeconds;
	private final int index;

	protected final HistoryInt history = new HistoryInt(48, TIME.days(), false);
	public final INT_OE<ROOM_IDATA_INSTANCE> year; 
	public final INT_OE<ROOM_IDATA_INSTANCE> yearPrev; 
	public final DOUBLE_OE<ROOM_IDATA_INSTANCE> day; 
	public final INT_OE<ROOM_IDATA_INSTANCE> dayPrev; 
	
	public IndustryResource(DataOSimple<ROOM_IDATA_INSTANCE> data, int li, RESOURCE res, double rate, double AIRate, double AIRecovery) {
		this.resource = res;
		this.rate = rate;
		this.AIRate = AIRate;
		this.AIRecovery = AIRecovery;
		rateSeconds = Humanoid.WORK_PER_DAYI*rate/TIME.secondsPerDay();
		this.index = li;
		year = data.new DataInt(); 
		yearPrev = data.new DataInt(); 
		day = data.new DataFloat();
		dayPrev = data.new DataInt();
	}
	
	public HISTORY_INT history() {
		return history;
	}
	
	@Override
	public void save(FilePutter file) {
		history.save(file);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		history.load(file);
	}

	@Override
	public void clear() {
		history.clear();
	}
	
	public int inc(ROOM_IDATA_INSTANCE r, double amount) {
		return inc(r, amount, true);
	}
	
	public abstract int inc(ROOM_IDATA_INSTANCE r, double amount, boolean record);
	
	public int work(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds) {
		double e = getEffort(skill, r, workSeconds);
		int a = inc(r, e);
		return a;
	}
	
	public int incDay(ROOM_IDATA_INSTANCE r) {
		return inc(r, rate);
	}
	
	protected abstract double getEffort(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds);

	
	@Override
	public int index() {
		return index;
	}
	
}