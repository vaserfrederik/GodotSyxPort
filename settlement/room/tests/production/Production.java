package settlement.room.tests.production;

import init.resources.RESOURCE;
import init.resources.RESOURCES;
import settlement.room.food.hunter.ROOM_HUNTER;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import snake2d.util.file.Alloc;
import snake2d.util.sets.ArrayListGrower;
import snake2d.util.sets.LIST;

public final class Production{
	
	private final Li all = new Li();
	private final Li[] resMap = new Li[RESOURCES.ALL().size()];
	private final Li[] insMap = new Li[Industry.all().size()];
	
	public Production(){
		
		int[] industrycount = Alloc.ii(Industry.all().size());
		
		for (RESOURCE res : RESOURCES.ALL()) {
			resMap[res.index()] = new Li();
		}
		
		
		Li all = new Li();
		for (Industry ins : Industry.all()) {
			insMap[ins.index()] = new Li();
			if (ins.outs().size() == 0)
				continue;
			if (ins.blue instanceof ROOM_HUNTER)
				continue;
			
			for (IndustryResource o : ins.outs()){
				TestRecipe r = new TestRecipe(industrycount[ins.index()]++, ins, o);
				all.add(r);
				resMap[r.res.index()].add(r);
			}
		}
		
		for (TestRecipe r : all) {
			populateInputs(r, resMap, industrycount);
		}
		
		for (RESOURCE res : RESOURCES.ALL()) {
			this.all.add(resMap[res.index()]);
		}
		
		for (TestRecipe r : all) {
			insMap[r.ins.index()].add(r);
		}
		
	}
	
	private void populateInputs(TestRecipe r, Li[] resMap, int[] industrycount) {
		for (IndustryResource in : r.ins.ins()) {
			for (TestRecipe prod : resMap[in.resource.index()]) {
				populateInputs(prod, resMap, industrycount);
			}
		}
		
		if (r.inputs.size() > 0)
			return;
		
		
		for (IndustryResource in : r.ins.ins()) {
			Input i = new Input(in.rate, resMap[in.resource.index()].get(0));
			r.inputs.add(i);
		}
		
		int extras = 1;
		for (IndustryResource in : r.ins.ins()) {
			extras *= resMap[in.resource.index()].size();
		}
		
		ArrayListGrower<TestRecipe> newRecs = new ArrayListGrower<TestRecipe>();
		
		for (int i = 1; i < extras; i++) {
			int k = i;
			
			ArrayListGrower<Input> inputs = new ArrayListGrower<Input>();

			for (IndustryResource in2 : r.ins.ins()) {
				int ri = k%resMap[in2.resource.index()].size();
				
				TestRecipe rr = resMap[in2.resource.index()].get(ri);
				inputs.add(new Input(in2.rate, rr));
				
				k /= resMap[in2.resource.index()].size();
			}
			
			TestRecipe res = new TestRecipe(industrycount[r.ins.index()]++, r);
			res.inputs.add(inputs);
			newRecs.add(res);
		}
		
		resMap[r.res.index()].add(newRecs);
		
	}

	public LIST<TestRecipe> all(){
		return all;
	}
	
	public LIST<TestRecipe> get(RESOURCE res){
		return resMap[res.index()];
	}
	
	public TestRecipe best(RESOURCE resource, ProductionSpec ibonuses){
		LIST<TestRecipe> rs = get(resource);
		TestRecipe r = null;
		double best = Double.MAX_VALUE;
		for (int ri = 0; ri < rs.size(); ri++) {
			TestRecipe r2 = rs.get(ri);
			double w = r2.wPerItem(ibonuses);
			if (w < best) {
				best = w;
				r = r2;
			}
		}
		return r;
	}
	
	public LIST<TestRecipe> get(Industry res){
		return insMap[res.index()];
	}
	
	public double price(RESOURCE res, ProductionSpec ibonuses) {
		LIST<TestRecipe> rs = get(res);
		double min = Double.MAX_VALUE;
		
		for (TestRecipe rr : rs) {
			double t = rr.pricePerItem(ibonuses);
			if (t < min) {
			
				min = t;
			}	
		}
		return min;
	}
	
	private static class Li extends ArrayListGrower<TestRecipe> {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		
	}
	
}