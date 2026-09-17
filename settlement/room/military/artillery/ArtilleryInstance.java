package settlement.room.military.artillery;

import game.GAME;
import game.battle.Army;
import game.battle.div.Div;
import init.constant.C;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.path.finders.SFinderSoldierManning.FINDABLE_MANNING;
import settlement.path.finders.SFinderSoldierManning.FINDABLE_MANNING_INSTANCE;
import settlement.room.main.RoomInstance;
import settlement.room.main.TmpArea;
import settlement.room.main.util.RoomInit;
import settlement.stats.Induvidual;
import settlement.thing.projectiles.SProjectiles;
import settlement.thing.projectiles.Trajectory;
import snake2d.Renderer;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.misc.CLAMP;
import util.gui.misc.GBox;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;

public final class ArtilleryInstance extends RoomInstance implements FINDABLE_MANNING_INSTANCE{

	private static final long serialVersionUID = 1L;
	
	public boolean hovered, selected;
	
	private boolean enemy;
	boolean invisible;
	private final byte dir;
	private byte dirCurrent;
	byte men;
	
	private boolean mustered;
	private boolean fireAtWill;
	private final Coo cTarget = new Coo(-1, -1);
	private short dTarget = -1;
	private boolean bombard = false;
	
	private volatile boolean trajLock;
	private final Trajectory traj = new Trajectory();
	volatile boolean hasTrajectory = false;
	private float progress;
	private float skill;
	private float skillI;
	boolean isLoaded;
	private boolean targetIsUserSet = false;
	private static final Trajectory trajTmp = new Trajectory();
	
	ArtilleryInstance(ROOM_ARTILLERY b, TmpArea area, RoomInit init) {
		super(b, area, init);
		dir = (byte) (SETT.ROOMS().fData.item.get(mX(), mY()).rotation*2);
		dirCurrent = dir;
		activate();
	}

	@Override
	public ROOM_ARTILLERY blueprintI() {
		return (ROOM_ARTILLERY) blueprint();
	}

	void work(double amount, Humanoid hu) {
		
		invisible = false;
		
		if (!needsWork())
			return;
		
		double skill = (1.0 - 0.75*getDegrade())*blueprintI().bonus().get(hu.indu())/blueprintI().bonus().max(Induvidual.class);
		amount /= 6.0*blueprintI().projectile.reloadSeconds(skill);
		
		if (hasTrajectory) {
			DIR d = DIR.get(traj.vx(), traj.vy());
			if (d != dirCurrent()) {
				progress += amount*8;
				if (progress >= 1) {
					progress -= 1;
					if (d == dirCurrent().next(-2))
						dirCurrent = (byte) dirCurrent().next(-1).id();
					else if (d == dirCurrent().next(2))
						dirCurrent = (byte) dirCurrent().next(1).id();
					else 
						dirCurrent = (byte) d.id();
				}
				return;
			}
			
			if (isLoaded && (targetCooGet() != null || targetDivGet() != null)) {
				
				isLoaded = false;
				int h = SETT.TERRAIN().get(body().cX(), body().cY()).heightEnt(body().cX(), body().cY())*C.TILE_SIZE;
				h+= Trajectory.RELEASE_HEIGHT;
				getTrajectory(trajTmp);
				int fx = body().x1()*C.TILE_SIZE + body().width()*C.TILE_SIZE/2;
				int fy = body().y1()*C.TILE_SIZE + body().height()*C.TILE_SIZE/2;
				fx += C.TILE_SIZE*dir().x();
				fy += C.TILE_SIZE*dir().y();
				double ref = CLAMP.d(this.skill/skillI, 0, 1);
				skillI = 0;
				this.skill = 0;
				SETT.PROJS().launch(fx, fy, h, trajTmp, blueprintI().projectile, 1.0-blueprintI().projectile.accuracy(ref), ref, null);
				return;
			}
			
		}
		
		if (!isLoaded) {
			progress += amount;
			this.skill += skill;
			skillI ++;
			if (progress >= 1) {
				progress -= 1;
				isLoaded = true;
			}
		}
		
		
	}
	
	public void setVisible() {
		invisible = false;
	}
	
	double progress() {
		return progress;
	}
	
	public boolean needsWork() {
		return hasTrajectory || !isLoaded;
	}
	
	@Override
	protected void activateAction() {
		
	}

	@Override
	protected void deactivateAction() {
		
	}
	
	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		if (invisible)
			return false;
		return super.render(r, shadowBatch, i);
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		if (invisible)
			return false;
		return super.renderAbove(r, shadowBatch, i);
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		if (invisible)
			return false;
		return super.renderBelow(r, shadowBatch, i);
	}
	
	public DIR dirCurrent() {
		return DIR.ALL.get(dirCurrent);
	}
	
	public DIR dir() {
		return DIR.ALL.get(dir);
	}
	
	@Override
	protected void dispose() {
		muster(false);
	}
	

	public Army army() {
		return enemy ? GAME.ARMIES().enemy() : GAME.ARMIES().player();
	}
	
	
	public boolean inService() {
		return true;
	}
	
	public boolean mustered() {
		return mustered;
	}
	
	public void muster(boolean muster) {
		if (mustered == muster)
			return;
		
		men = 0;
		if (!muster) {
			for (COORDINATE c : body()) {
				if (is(c))
					blueprintI().service.deactivate(c.x(), c.y());
			}
			mustered = muster;
		}
		
		else {
			mustered = muster;
			for (COORDINATE c : body()) {
				if (is(c))
					blueprintI().service.activate(c.x(), c.y());
			}
		}
	}

	public boolean fireAtWill() {
		return fireAtWill;
	}

	public void fireAtWill(boolean fire) {
		this.fireAtWill = fire;
	}
	
	public boolean isFiring() {
		return mustered() && menMustering() > 0 && hasTrajectory && (targetCooGet() != null || targetDivGet() != null);
	}

	public void clearTarget() {
		cTarget.set(-1, -1);
		dTarget = -1;
	}
	
	public void targetDivSet(Div div, boolean userSet) {
		clearTarget();
		dTarget = div.index();
	}
	
	public Div targetDivGet() {
		if (dTarget == -1)
			return null;
		Div d = GAME.ARMIES().division(dTarget);
		
		return d;
	}
	
	public COORDINATE targetCooGet() {
		if (cTarget.x() == -1)
			return null;
		return cTarget;
	}
	

	
	public void targetCooSet(int tx, int ty, boolean bombard, boolean userSet) {
		clearTarget();
		cTarget.set(tx, ty);
		this.bombard = bombard;
		
	}
	
	void getTrajectory(Trajectory t) {
		lockTrajectory();
		t.set(traj.vx(), traj.vy(), traj.vz());
		trajLock = false;
	}
	
	public void setTrajectory(Trajectory t) {
		lockTrajectory();
		if (t == null) {
			hasTrajectory = false;
		}else {
			traj.set(t.vx(), t.vy(), t.vz());
			hasTrajectory = true;
		}
		
		trajLock = false;
		
	}
	
	private synchronized void lockTrajectory() {
		while(trajLock)
			;
		trajLock = true;
	}
	
	public void hover(GUI_BOX box) {
		
		box.add(blueprintI().iconBig());
		box.text(name());
		box.NL();
		Hoverer.hover((GBox) box, this);
		
	}
	
	public COORDINATE centre() {
		int fx = body().x1()*C.TILE_SIZE + body().width()*C.TILE_SIZE/2;
		int fy = body().y1()*C.TILE_SIZE + body().height()*C.TILE_SIZE/2;
		fx += C.TILE_SIZE*dir().x();
		fy += C.TILE_SIZE*dir().y();
		Coo.TMP.set(fx, fy);
		return Coo.TMP;
	}
	
	public boolean bombarding() {
		return bombard;
	}
	
	public int rangeMin() {
		return (int) blueprintI().projectile.velocity(blueprintI().ref());
	}
	
	public int rangeMax() {
		return (int) Trajectory.range(0, blueprintI().projectile.maxAngle(Short.MAX_VALUE), blueprintI().projectile.velocity(blueprintI().ref()));
	}
	
	public boolean targetIsUserSet() {
		return targetIsUserSet;
	}
	
	private boolean testTarget(int tx, int ty) {
		
		{
			int fx = body().cX()*C.TILE_SIZE + C.TILE_SIZEH;
			int fy = body().cY()*C.TILE_SIZE + C.TILE_SIZEH;
			fx += C.TILE_SIZE*dir().x();
			fy += C.TILE_SIZE*dir().y();
			
			int dx = tx-fx;
			int dy = ty-fy;
			double l = Math.sqrt(dx*dx+dy*dy);
			
			double min = rangeMin();
			double max = rangeMax();
			if (l < min) {
				return false;
			}
			if (l > max) {
				return false;
			}
			
			{
				DIR d = dir().next(-1);
				double Ax = fx + min*d.xN();
				double Ay = fy + min*d.yN();
				double Bx = fx + max*d.xN();
				double By = fy + max*d.yN();
				if (dd(Ax, Ay, Bx, By, tx, ty) < 0) {
					return false;
				}
			}
			

			{
				DIR d = dir().next(1);
				double Ax = fx + min*d.xN();
				double Ay = fy + min*d.yN();
				double Bx = fx + max*d.xN();
				double By = fy + max*d.yN();
				if (dd(Ax, Ay, Bx, By, tx, ty) > 0) {
					return false;
				}
			}
		}
		return true;
		
	}
	
	public CharSequence testTarget(int px, int py, Trajectory traj, boolean entity) {
		
		if (!testTarget(px, py))
			return SProjectiles.¤¤OUT_OF_RANGE;
		
		int fx = body().x1()*C.TILE_SIZE + body().width()*C.TILE_SIZE/2;
		int fy = body().y1()*C.TILE_SIZE + body().height()*C.TILE_SIZE/2;
		fx += C.TILE_SIZE*dir().x();
		fy += C.TILE_SIZE*dir().y();
		
		int h = SETT.TERRAIN().get(body().cX(), body().cY()).heightEnt(body().cX(), body().cY())*C.TILE_SIZE;
		h+= Trajectory.RELEASE_HEIGHT;
		
		int ttx = px >> C.T_SCROLL;
		int tty = px >> C.T_SCROLL;
		if (entity)
			h -= SETT.TERRAIN().get(ttx, tty).heightEnt(ttx, tty) + Trajectory.HIT_HEIGHT/2;
		else
			h -= SETT.TERRAIN().get(ttx, tty).heightStart(ttx, tty) + (SETT.TERRAIN().get(ttx, tty).heightEnd(ttx, tty)- SETT.TERRAIN().get(ttx, tty).heightStart(ttx, tty))/2;
		
		CharSequence problem = SProjectiles.¤¤OUT_OF_RANGE;
		
		if (traj.calcLow(h, fx, fy, px, py, blueprintI().projectile.maxAngle(Short.MAX_VALUE), blueprintI().projectile.velocity(blueprintI().ref()))) {
			
			problem = SProjectiles.trajectoryProblem(army(), traj, fx, fy);
			if (problem == null)
				return null;
		}
		
		if (traj.calcHigh(h, fx, fy, px, py, blueprintI().projectile.maxAngle(Short.MAX_VALUE), blueprintI().projectile.velocity(blueprintI().ref()))) {
			problem = SProjectiles.trajectoryProblem(army(), traj, fx, fy);
			if (problem == null)
				return null;
		}
		return problem;
		
	}
	
	private double dd(double Ax, double Ay, double Bx, double By, double Cx, double Cy) {
		return (Bx - Ax) * (Cy - Ay) - (By - Ay) * (Cx - Ax);
	}
	
	public void setEnemy() {
		muster(false);
		this.enemy = true;
		invisible = true;
		muster(true);
	}

	@Override
	public FINDABLE_MANNING getManning(int tx, int ty) {
		return blueprintI().service.get(tx, ty);
	}

	public double menMustering() {
		return men/6.0;
	}
	
	@Override
	public void destroyTile(int tx, int ty) {
		if (enemy) {
			SETT.THINGS().gore.debris(centre().x(), centre().y(), 0, 0);
			remove(tx, ty, false, this, true).clear();;
		}
		else
			super.destroyTile(tx, ty);
	}
	
	
}
