package settlement.room.main.job;

import java.io.IOException;
import java.util.Arrays;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import snake2d.util.file.Alloc;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;

public class ResourceUnderflow implements SAVABLE{

	public int[] underflow = Alloc.ii(RESOURCES.ALL().size());
	
	@Override
	public void save(FilePutter file) {
		RESOURCES.map().saver().save(underflow, file);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		RESOURCES.map().loader().load(underflow, file, 0);
	}

	@Override
	public void clear() {
		Arrays.fill(underflow, 0);
	}
	
	public int withdraw(RESOURCE res, int target, int max) {
		if (target > max) {
			underflow[res.index()] += target-max;
			return max;
		}
		return target;
	}
	
	public int deposit(RESOURCE res, int amount) {
		int u = underflow[res.index()];
		if (u > 0) {
			int a = Math.min(amount, u);
			underflow[res.index()] -= a;
			return amount - a;
		}
		return amount;
	}
	

}
