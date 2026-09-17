package settlement.room.industry.module;

import game.GameDisposable;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;
import world.map.regions.Region;

public abstract class IndustryRegion {

	private static ArrayListGrower<IndustryRegion> all = new ArrayListGrower<IndustryRegion>();
	static {
		new GameDisposable() {
			
			@Override
			protected void dispose() {
				all.clear();
			}
		};
	}
	
	public final int index;
	public final Industry ins;
	public final double rarity;
	
	public IndustryRegion(Industry ins, double rarity){
		this.ins = ins;
		this.index = all.add(this);
		this.rarity = rarity;
		ins.reg = this;
	}
	
	public abstract double occurence(Region reg);
	
	public static LIST<IndustryRegion> ALL(){
		return all;
	}
}
