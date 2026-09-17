package settlement.room.law.guard;

import java.io.IOException;

import init.constant.C;
import init.sprite.SPRITES;
import init.sprite.UI.UI;
import settlement.overlay.Addable;
import snake2d.CORE;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.gui.clickable.CLICKABLE;
import snake2d.util.misc.ACTION;
import snake2d.util.sets.IntegerStack;
import util.gui.misc.GButt;
import util.rendering.RenderData;
import view.main.VIEW;
import view.sett.IDebugPanelSett;

public class Patrols implements SAVABLE{

	private final Patrol[] patrols = new Patrol[16];
	private boolean debug = false;
	
	private final IntegerStack free = new IntegerStack(patrols.length*Patrol.MAX);
	
	
	private final Addable a = new Addable(false, true) {
		
		@Override
		public void initAbove(RenderData data) {
			
			COLOR.RED100.bind();
			for (int pi = 0; pi < patrols.length; pi++) {
				Patrol p = patrols[pi];
				COLOR.UNIQUE.getC(pi).bind();
				for (int i = 0; i < p.posses(); i++) {
					Coo coo = p.pos(i);
					int px = data.transformGX(coo.x()-C.TILE_SIZEH);
					int py = data.transformGY(coo.y()-C.TILE_SIZEH);
					SPRITES.cons().BIG.outline.render(CORE.renderer(), 0, px, py);
				}
				
				
			}
			
			
			COLOR.unbind();
			super.initAbove(data);
			add();
		}
		
	};
	
	Patrols(){
		for (int i = 0; i < patrols.length; i++)
			patrols[i] = new Patrol();
		
		free.fill();
		
		
		IDebugPanelSett.add("show patrols", new ACTION() {
			
			@Override
			public void exe() {
				
				
				Coo coo = patrols[0].pos(0);
				VIEW.s().getWindow().centerer.set(coo.x(), coo.y());
				debug = true;
			}
		});
	}
	
	public CLICKABLE debugButt() {
		return new GButt.ButtPanel(UI.icons().m.crossair) {
			
			int pi = 0;

			
			@Override
			protected void clickA() {
				if (pi >= patrols.length)
					pi = 0;
				Patrol p = patrols[pi];
				Coo coo = p.pos(0);
				VIEW.s().getWindow().centerer.set(coo.x(), coo.y());
				debug = true;
			}
			
		};
	}
	
	void update(double ds) {
		
		for (Patrol p : patrols)
			p.update(ds);
		if (debug)
			a.add();
	}
	
	public int reservePosition() {
		if (!free.isEmpty()) {
			int p = free.pop();
			return p;
		}
		return -1;
	}
	
	public void returnPosition(int pos) {
		free.push(pos);
	}
	
	public Coo pos(int position) {
		int p = position/Patrol.MAX;
		int pp = position%Patrol.MAX;
		
		return patrols[p].pos(pp);
	}
	
	public DIR dir(int position) {
		int p = position/Patrol.MAX;
		int pp = position%Patrol.MAX;
		return patrols[p].dir(pp);
	}

	@Override
	public void save(FilePutter file) {
		free.save(file);
		for (Patrol p : patrols)
			p.save(file);
	}

	@Override
	public void load(FileGetter file) throws IOException {
		free.load(file);
		for (Patrol p : patrols)
			p.load(file);
	}

	@Override
	public void clear() {
		free.clear();
		free.fill();
		for (Patrol p : patrols)
			p.clear();
	}
	
}
