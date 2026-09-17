package settlement.room.main.throne;

import java.io.IOException;

import settlement.main.SETT;
import settlement.path.finders.SFinderRoomService;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprint;
import settlement.room.main.category.RoomCategorySub;
import settlement.room.main.util.RoomInitData;
import snake2d.util.color.COLOR;
import snake2d.util.color.ColorImp;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.datatypes.DIR;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.sets.LISTE;
import snake2d.util.sprite.SPRITE;
import util.info.INFO;
import view.sett.ui.room.UIRoomModule;
import view.tool.PLACABLE;

public class THRONE extends RoomBlueprint{

	private final Coo instance = new Coo(SETT.TWIDTH/2, SETT.THEIGHT/2);
	private int tile = instance.x()+instance.y()*SETT.TWIDTH;
	final Coo construction = new Coo(-1, -1);

	final Sprite sprite;
	public final INFO info;
	public final PLACABLE placer;
	public final Initer init;
	
	public THRONE(RoomInitData init, RoomCategorySub cat) throws IOException {
		super("_THRONE");
		init.init("_THRONE");
		info = new INFO(init.text());
		sprite = new Sprite(init);
		
		this.init = new Initer(this);
		placer = new Placer(this);
		clear();
	}
	
//	public COORDINATE getThrone() {
//		return instance;
//	}
	
	public static int tile() {
		return SETT.ROOMS().THRONE.tile;
	}
	
	public static COORDINATE coo() {
		return SETT.ROOMS().THRONE.instance;
	}
	
	public static DIR rot() {
		Room r = SETT.ROOMS().map.get(coo());
		if (r != null && r instanceof Instance) {
			return DIR.ORTHO.get(((Instance)r).rot);
		}
		return DIR.N;
	}
	
	@Override
	public Room get(int tx, int ty) {
		Room r = SETT.ROOMS().map.get(tx, ty);
		if (r != null && r instanceof Instance)
			return r;
		return null;
	}
	
	void setInstance(int tx, int ty) {
		instance.set(tx, ty);
		tile = instance.x()+instance.y()*SETT.TWIDTH;
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public SFinderRoomService service(int tx, int ty) {
		return null;
	}
	
	@Override
	public COLOR miniC(int tx, int ty) {
		return sprite.miniC;
	}
	

	@Override
	public COLOR miniCPimped(ColorImp origional, int tx, int ty, boolean northern, boolean southern) {
		return origional;
	}

	@Override
	protected void save(FilePutter saveFile) {
		instance.save(saveFile);
		construction.save(saveFile);
		
	}

	@Override
	protected void load(FileGetter saveFile) throws IOException {
		instance.load(saveFile);
		construction.load(saveFile);
		tile = instance.x()+instance.y()*SETT.TWIDTH;
	}

	@Override
	protected void clear() {
		setInstance(SETT.TWIDTH/2, SETT.THEIGHT/2);
	}

	@Override
	public void appendView(LISTE<UIRoomModule> mm) {
		// TODO Auto-generated method stub
		
	}



	public SPRITE icon() {
		return sprite.icon;
	}


	
}
