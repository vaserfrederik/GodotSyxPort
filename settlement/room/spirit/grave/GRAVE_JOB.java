package settlement.room.spirit.grave;

import init.resources.RESOURCE;
import settlement.entity.humanoid.Humanoid;
import settlement.misc.job.SETT_JOB;
import settlement.thing.ThingsCorpses.Corpse;

public interface GRAVE_JOB extends SETT_JOB{

	@Override
	default RESOURCE jobPerform(Humanoid skill, RESOURCE r, int rAm) {
		throw new RuntimeException();
	}
	public void buryAndPerform(Corpse c);
}
