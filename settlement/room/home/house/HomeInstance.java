package settlement.room.home.house;

import static settlement.main.SETT.ROOMS;

import game.GAME;
import init.race.Race;
import init.resources.RES_AMOUNT;
import init.type.HGROUP;
import init.type.HGROUP.HTypeBits;
import init.type.HGROUP.HTypeBitsImp;
import settlement.entity.ENTITY;
import settlement.entity.humanoid.Humanoid;
import settlement.main.SETT;
import settlement.maintenance.ROOM_DEGRADER;
import settlement.path.AVAILABILITY;
import settlement.room.home.HOME;
import settlement.room.main.Room;
import settlement.room.main.Room.RoomInstanceImp;
import settlement.room.main.TmpArea;
import settlement.room.main.construction.ConstructionInit;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomState;
import settlement.room.sprite.RoomSprite;
import settlement.stats.STATS;
import snake2d.Renderer;
import snake2d.util.bit.Bits;
import snake2d.util.bit.BitsLong;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.RecShort;
import snake2d.util.file.Alloc;
import snake2d.util.rnd.RND;
import snake2d.util.sprite.SPRITE;
import util.rendering.RenderData.RenderIterator;
import util.rendering.ShadowBatch;
import util.text.Dic;

public final class HomeInstance extends RoomInstanceImp implements HOME {

	/**
	 * 
	 */
	private static final long serialVersionUID = 1L;
	private final RecShort tiles;
	private long resourceData;
	private final HTypeBitsImp egroup = new HTypeBitsImp(true);
	private final int[] occupants;
	private byte litTimer;
	private final byte random;
	private byte isolation;
	private byte am;
	private byte amOdd;
	private byte renderTimer;
	private byte bitData;
	private final byte sx,sy;
	private static final Bits lit = 				new Bits(0b0000_0000_0000_0000_0000_0000_0000_0001);
	private static final Bits upgrade = 			new Bits(0b0000_0000_0000_0000_0000_0000_0000_1110);
	private static BitsLong[] resources = new BitsLong[8];
	static {
		for (int i = 0; i < resources.length; i++) {
			int m = 0x0F << (i)*4;
			resources[i] = new BitsLong(m);
		}
	}
	
	
	protected HomeInstance(ROOM_HOME p, TmpArea a) {
		super(ROOMS(), p, false);
		
		
		
		FurnisherItem it = SETT.ROOMS().fData.item.get(a.mx(), a.my());		
		int sx = 0;
		int sy = 0;
		for (int y = 0; y < it.height(); y++) {
			for (int x = 0; x < it.width(); x++) {
				if (it.get(x, y) == SETT.ROOMS().HOME.constructor.tOpening) {
					sx = x;
					sy = y;
				}
			}
				
		}
		
		this.sx = (byte) sx;
		this.sy = (byte) sy;
		
		tiles = new RecShort(a.body());
		random = (byte) RND.rInt();
		int[] mo = blueprintI().constructor.maxOccupants[it().group.index()];
		occupants = Alloc.ii(mo[mo.length-1]);
		
		a.replaceAndClear(this);
		for (COORDINATE c : tiles) {
			if (is(c))
				SETT.ROOMS().extraBit.set(c, 0);
		}
		SETT.ROOMS().extraBit.set(serviceX(), serviceY(), 1);

		for (COORDINATE c : body()) {
			if (is(c)) {
				int m = 0;
				Sprite s = sprite(c.x(), c.y());
				
				if (s != null) {
					s.house = this;
					m = s.getData(c.x(), c.y(), c.x()-body().x1(), c.y()-body().y1(), it(), random);
				}
				SETT.ROOMS().fData.spriteData.set(c.x(), c.y(), m);
				SETT.PATH().availability.updateAvailability(c.x(), c.y());
				SETT.ROOMS().extraBit.set(c, 0);
			}
		}
		
		isolationSet(SETT.ROOMS().isolation.getProspect(blueprint(), this, null));
		blue().odd.update(serviceX(), serviceY());
		add();
	}

	@Override
	public SPRITE icon() {
		return blueprintI().icon;
	}
	
	@Override
	public Furnisher constructor() {
		return blueprintI().constructor;
	}

	@Override
	public int mX() {
		return tiles.x1()+ it().firstX();
	}

	@Override
	public int mY() {
		return tiles.y1()+ it().firstY();
	}

	@Override
	public int area() {
		return it().area;
	}

	@Override
	public RECTANGLE body() {
		return tiles;
	}

	@Override
	public boolean is(int tile) {
		return SETT.ROOMS().map.indexGetter.get(tile) == roomI;
	}

	@Override
	protected boolean render(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			return s.render(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()), ROOMS().fData.candle.is(i.tile()));
		return false;
	}
	
	@Override
	protected boolean renderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null) {
			s.renderAbove(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()));
		}
		return false;
	}
	
	@Override
	protected boolean renderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i) {
		RoomSprite s = ROOMS().fData.sprite.get(i.tile());
		if (s != null)
			s.renderBelow(r, shadowBatch, ROOMS().fData.spriteData.get(i.tile()), i, getDegrade(i.tx(), i.ty()));
		return false;
	}

	@Override
	public CharSequence name(int tx, int ty) {
		return Dic.empty;
	}
	
	

	@Override
	protected AVAILABILITY getAvailability(int tile) {
		AVAILABILITY a = ROOMS().fData.availability.get(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		if (a == AVAILABILITY.ROOM) {
			int tx = tile%SETT.TWIDTH;
			int ty = tile/SETT.TWIDTH;
			Sprite s = sprite(tx, ty);
			if (s != null && s.solid)
				return AVAILABILITY.NOT_ACCESSIBLE;
		}
		return a;
	}

	@Override
	public TmpArea remove(int tx, int ty, boolean scatter, Object user, boolean forced) {
		dispose();
		SETT.ROOMS().stats.broken().remove(mX(), mY());
		TmpArea a = delete(tx, ty, user);
		return a;
	}

	@Override
	public boolean destroyTileCan(int tx, int ty) {
		return ROOMS().fData.availability.get(tx, ty).player < 0 || ROOMS().fData.availability.get(tx, ty).enemy < 0;
	}
	
	@Override
	public void destroyTile(int tx, int ty) {
		ConstructionInit init = new ConstructionInit(this, tx, ty, true);
		TmpArea a = remove(tx, ty, false, this, true);
		ROOMS().construction.breakIt(a, init, tx, ty);
	}



	@Override
	public ROOM_DEGRADER degrader(int tx, int ty) {
		return null;
	}

	@Override
	public int resAmount(int ri, int upgrade) {
		return (int) it().group.cost(ri, upgrade);
	}

	public ROOM_HOME blueprintI() {
		return SETT.ROOMS().HOME;
	}
	
	@Override
	public RoomState makeState(int tx, int ty, boolean broken) {
		return new State(this);
	};
	
	void dispose() {
		for (int i = 0; i < occupants(); i++) {
			Humanoid a = occupant(i);
			STATS.HOME().GETTER.set(a, null);
			i--;
		}
		remove();
	}
	
	@Override
	public HOME vacate(Humanoid h) {
		remove();
		for (int oi = 0; oi < occupants(); oi++) {
			Humanoid o = occupant(oi);
			if (o == h) {
				occupants[oi] = 0;
				am --;
				if (STATS.WORK().EMPLOYED.get(o) == null)
					amOdd --;
				for (int k = oi+1; k < occupantsMax(); k++) {
					occupants[k-1] = occupants[k];
				}
				add();
				if (am == 0)
					turnOffLight();
				SETT.ROOMS().HOME.odd.update(serviceX(), serviceY());
				return this;
			}
		}
		
		throw new RuntimeException(h + " " + am + " " + occupantsMax());
	}
	
	@Override
	public HOME occupy(Humanoid h) {
		
		if (am >= occupantsMax()) {
			if (amOdd == 0)
				throw new RuntimeException(h + " " + am + " " + occupantsMax());
			vacateOddjobber();
		}
		remove();
		if (h != SETT.ENTITIES().getByID(h.id()))
			throw new RuntimeException(""+h.id());
		occupants[am] = h.id();
		am++;
		if (STATS.WORK().EMPLOYED.get(h) == null)
			amOdd++;
		add();
		return this;
	}
	
	private void vacateOddjobber() {
		if (amOdd == 0)
			return;
		
		for (int i = 0; i < am; i++) {
			Humanoid o = occupant(i);
			if (STATS.WORK().EMPLOYED.get(o) == null) {
				STATS.HOME().GETTER.set(o, null);
				return;
			}
		}
		throw new RuntimeException(am + " " + amOdd);
	}
	
	public HOME use() {
		litTimer = (byte) ((GAME.updateI()>>8)&0x0FF);
		if (lit.get(bitData) == 1) {
			return this;
		}
		bitData = (byte) lit.set(bitData, 1);
		
		for (COORDINATE c : tiles) {
			if (is(c)) {
				RoomSprite s = sprite(c.x(), c.y());
				if (s != null && s == blue().constructor.sp.nSta && !SETT.LIGHTS().is(c.x(), c.y()))
					SETT.LIGHTS().candle(c.x(), c.y(), 1);
			}
		}
		
		return this;
	}
	

	
	private void turnOffLight() {
		bitData = (byte) lit.set(bitData, 0);
		for (COORDINATE c : tiles) {
			if (is(c)) {
				RoomSprite s = sprite(c.x(), c.y());
				if (s != null && s == blue().constructor.sp.nSta && SETT.LIGHTS().is(c.x(), c.y()))
					SETT.LIGHTS().remove(c.x(), c.y());
			}
			
		}
	}
	
	@Override
	public Humanoid occupant(int oi) {
		if (oi < am) {
			int i = occupants[oi];
			ENTITY e = SETT.ENTITIES().getByID(i);
			if (e != null && e instanceof Humanoid) {
				return (Humanoid) e;
			}else
				throw new RuntimeException(oi + " " + tiles + " " + e);
		}
		return null;
	}
	
	@Override
	public int occupants() {
		return am;
	}
	
	public int occupantsOdd() {
		return amOdd;
	}
	
	@Override
	public int serviceX() {
		return tiles.x1() + sx;
	}
	
	@Override
	public int serviceY() {
		return tiles.y1() + sy;
	}
	
	Sprite sprite(int tx, int ty){
		int ri = random&0x0FF;
		
		ri %= blue().constructor.sp.sp.sprites[it().group.index()].length;
		
		SpriteConfig sp = blue().constructor.sp.sp.sprites[it().group.index()][ri];
		int dx = tx-tiles.x1();
		int dy = ty-tiles.y1();
		return sp.get(it().rotation).get(dx, dy);
	}

	public DIR dir() {
		return DIR.ORTHO.get(it().rotation);
	}
	
	public HTypeBits availability() {
		if (am-amOdd >= occupantsMax())
			return null;
		return psetting();
	}
	
	public void settingSet(HTypeBits bits) {
		for (int i = 0; i < am; i++) {
			Humanoid o = occupant(i);
			if (!bits.is(o)) {
				i--;
				STATS.HOME().GETTER.set(o, null);
			}
		}
		remove();
		egroup.copy(bits);
		add();
	}
	
	@Override
	public boolean canOccupy(Humanoid h) {
		return availability() != null && availability().is(h);
	}
	
	private HTypeBits psetting() {
		if (occupants() > 0)
			return HTypeBitsImp.specific(HGROUP.get(occupant(0)));
		return setting();
	}
	
	public HTypeBits setting() {
		return egroup;
	}
	


	private void remove() {

		SETT.ROOMS().HOME.report(-am, -occupantsMax(), psetting());
		if (availability() != null) {
			SETT.PATH().comps.data.home.reportAbsence(serviceX(), serviceY(), availability());
			
		}

	}
	
	private void add() {
		SETT.ROOMS().HOME.report(am, occupantsMax(), psetting());
		if (availability() != null) {
			SETT.PATH().comps.data.home.reportPresence(serviceX(), serviceY(), availability());
			
		}
	}
	
	private static final DirCoo dcoo = new DirCoo();
	
	public static class DirCoo extends Coo {
		
		private static final long serialVersionUID = 1L;
		public DIR dir;
		public boolean isLay;
		
	}

	@Override
	public int occupantsMax() {
		return blueprintI().constructor.maxOccupants[it().group.index()][upgrade.get(bitData)];
	}
	

	@Override
	public int resourceAm(int ri) {
		resCount();
		return resources[ri].get(resourceData);
		
	}
	
	private void resCount() {
		unuse();
		if (occupants() == 0)
			return;
		if ((renderTimer & 0x0FF) != ((GAME.updateI() >> 8)&0x0FF)) {
			renderTimer = (byte) ((GAME.updateI() >> 8)&0x0FF);
			int ri = 0;
			for (@SuppressWarnings("unused") RES_AMOUNT a : occupant(0).race().home().clas(occupant(0)).resources()) {
				double am = 0;
				for (int i = 0; i < occupants(); i++) {
					am += STATS.HOME().current(occupant(i), ri);
				}
				resourceData = resources[ri].set(resourceData, (int)Math.ceil(am/occupants()));
				ri++;
			}
		}
	}
	
	private void unuse() {
		if (lit.get(bitData) == 0) {
			return;
		}
		
		if (Bits.getDistance(litTimer, GAME.updateI()>>8, 0x0FF) > 0x0F){
			turnOffLight();
			
		}
		litTimer = (byte) ((GAME.updateI()>>8)&0x0FF);
	}
	
	public DirCoo getService(int tx, int ty) {
		Sprite s = sprite(tx, ty);
		if (s != null && s.service) {
			dcoo.dir = s.dir(SETT.ROOMS().fData.spriteData.get(tx, ty));
			dcoo.set(tx, ty);
			dcoo.isLay = s == blue().constructor.sp.bedS;
			return dcoo;
		}
		return null;
	}
	

	
	public DirCoo findService(Humanoid h) {
		
		int rx = body().x1() + RND.rInt(it().width());
		int ry = body().y1() + RND.rInt(it().height());
		
		for (int y = 0; y < it().height(); y++) {
			for (int x = 0; x < it().width(); x++) {
				
				DirCoo c = getService(rx, ry);
				if (c != null && SETT.ENTITIES().getAtTileSingle(rx, ry) == null) {
					return c;
				}
				rx++;
				if (rx >= tiles.x1() + it().width()) {
					rx = tiles.x1();
					ry++;
					if (ry >= tiles.y1()+it().height()) {
						ry = tiles.y1();
					}
				}
			}
		}
		return null;
		
	}


	
	@Override
	public double isolation() {
		return (double)(isolation&0x0FF) / 0x0FF;
	}
	
	@Override
	public void isolationSet(int tx, int ty, double isolation) {
		isolationSet(isolation);
	}
	
	public HomeInstance isolationSet(double am) {
		isolation = (byte) (am*0x0FF);
		return this;
	}
	
	public static ROOM_HOME blue() {
		return SETT.ROOMS().HOME;
	}

	public Race race() {
		return occupant(0).race();
	}

	@Override
	public void upgradeSet(int upgrade) {
		if (upgrade <= upgrade())
			return;
		

		remove();
		
		
		
		bitData = (byte) HomeInstance.upgrade.set(bitData, upgrade);
		add();
	}
	
	
	@Override
	public int upgrade() {
		return upgrade.get(bitData);
	}

	public FurnisherItem it() {
		return SETT.ROOMS().fData.item.get(serviceX(), serviceY());
	}
	
	

	public static class State extends RoomState {

		/**
		 * 
		 */
		private static final long serialVersionUID = 1L;
		public final HTypeBitsImp egroup;
		
		State(HomeInstance h){
			egroup = new HTypeBitsImp(true);
			if (h != null)
				egroup.copy(h.egroup);
		}
		
		
		@Override
		public void apply(Room r, int tx, int ty) {
			HomeInstance h = (HomeInstance) r;
			h.settingSet(egroup);

		}

		@Override
		public void applyRepaired(Room r, int tx, int ty) {
			// TODO Auto-generated method stub
			
		}
		
	}
	
	@Override
	public CharSequence typeName(int tx, int ty) {
		return SETT.ROOMS().fData.item.get(tx, ty).group.name;
	}

}