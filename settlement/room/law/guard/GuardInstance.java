package settlement.room.law.guard;

import init.constant.C;
import settlement.main.SETT;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import snake2d.Renderer;
import snake2d.util.bit.Bit;
import snake2d.util.bit.Bits;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.COORDINATEE;
import snake2d.util.datatypes.DIR;
import snake2d.util.rnd.RND;
import util.rendering.RenderData;
import util.rendering.ShadowBatch;

public final class GuardInstance extends RoomInstance {
	
	private static final long serialVersionUID = 1L;
	
	final static Bits standOccupied = new Bits(0b01111);
	final static Bit standReserved = new Bit(0b01);
	
	private boolean search  = true;
	float eff = 0;
	
	int[] cdata;
	
	protected GuardInstance(ROOM_GUARD b, TmpArea area, RoomInit init) {
		super(b, area, init);
		
		for (COORDINATE c : body()) {
			if (!is(c))
				continue;
			if (SETT.ROOMS().fData.tileData.get(c) == Constructor.codeLight) {
				int off = SETT.ROOMS().fData.tileData.get(c.x()+1, c.y()) == Constructor.codeLight ? C.TILE_SIZEH-1 : 0;
				SETT.LIGHTS().torchBig(c.x(), c.y(), off);
			}
		}
		
		
	
		employees().maxSet((int)b.constructor.guards.get(this));
		employees().neededSet((int)b.constructor.guards.get(this));
		
		activate();
		
		blueprintI().finder.report(blueprintI().service.get(this), 1);
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderData.RenderIterator it) {
		it.lit();
		return super.render(r, shadowBatch, it);
	}

	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}
	
	@Override
	protected void dispose() {
		if (blueprintI().reporter.available(this))
			blueprintI().finder.report(blueprintI().service.get(this), -1);
		
	}

	@Override
	public ROOM_GUARD blueprintI() {
		return (ROOM_GUARD) blueprint();
	}
	
	@Override
	protected void updateAction(double updateInterval, boolean day) {
		if (day)
			search = true;
		float eff = (float) eff();
		if (Math.abs(eff -this.eff) > 0.1) {
			this.eff = eff;
			for (COORDINATE c : body()) {
				if (is(c)) {
					SETT.ENV().map.setChanged(c.x(), c.y());
				}
			}
			
		}
		super.updateAction(updateInterval, day);
	}
	
	public boolean guardSpot(COORDINATEE planTile, COORDINATE current) {
		if (!search)
			return false;
		if (is(current.x(), current.y()) && SETT.ROOMS().fData.tileData.is(current.x(), current.y(), Constructor.codeStand)) {
			int d = SETT.ROOMS().data.get(current.x(), current.y());
			if (!standReserved.is(d)) {
				d = standReserved.set(d);
				SETT.ROOMS().data.set(this, current.x(), current.y(), d);
				planTile.set(current);
				return true;
			}
		}
		
		int a = body().width()*body().height();
		int tx = body().x1() + RND.rInt(body().width());
		int ty = body().y1() + RND.rInt(body().height());
		while(a -- >= 0) {
			if (is(tx, ty) && SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand)) {
				
				int d = SETT.ROOMS().data.get(tx, ty);
				if (!standReserved.is(d)) {
					d = standReserved.set(d);
					SETT.ROOMS().data.set(this, tx, ty, d);
					planTile.set(tx, ty);
					return true;
				}
			}
			tx++;
			if (tx >= body().x2()) {
				tx = body().x1();
				ty++;
				if (ty >= body().y2()) {
					ty = body().y1();
				}
				
			}
			
		}
		search = false;
		return false;
	}
	
	public boolean hasPotentialSpots() {
		return search;
	}
	
	@Override
	public void upgradeSet(int upgrade) {
		// TODO Auto-generated method stub
		super.upgradeSet(upgrade);
		for (COORDINATE c : body()) {
			if (is(c)) {
				SETT.ENV().map.setChanged(c.x(), c.y());
			}
		}
		
	}
	
	public void guardSpotReturn(int tx, int ty) {
		search = true;
		if (!is(tx, ty) || !SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand)) {
			throw new RuntimeException(is(tx, ty) + " " + SETT.ROOMS().fData.tileData.is(tx, ty, Constructor.codeStand));
		}
		int d = SETT.ROOMS().data.get(tx, ty);
		d = standReserved.clear(d);
		SETT.ROOMS().data.set(this, tx, ty, d);
	}
	
	public DIR guardDir(int tx, int ty) {
		return blueprintI().constructor.gaurdDir(tx, ty);
	}
	
	public double eff() {
		return (1.0 - getDegrade()*0.5)*(upgrade()+1.0)*(employees().employed()/(double)employees().max());
	}

}
