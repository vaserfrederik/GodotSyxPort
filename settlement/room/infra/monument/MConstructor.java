package settlement.room.infra.monument;

import java.io.IOException;

import settlement.environment.SettEnvMap;
import settlement.environment.SettEnvMap.SettEnv;
import settlement.environment.SettEnvMap.SettEnvValue;
import settlement.main.SETT;
import settlement.overlay.Addable;
import settlement.room.main.Room;
import settlement.room.main.RoomBlueprintImp;
import settlement.room.main.TmpArea;
import settlement.room.main.furnisher.Furnisher;
import settlement.room.main.furnisher.FurnisherItem;
import settlement.room.main.util.RoomInit;
import settlement.room.main.util.RoomInitData;
import snake2d.SPRITE_RENDERER;
import snake2d.util.datatypes.AREA;

class MConstructor extends Furnisher {

	private final ROOM_MONUMENT blue;

	MConstructor(ROOM_MONUMENT blue, RoomInitData init) throws IOException {
		super(init, init.data().jsons("ITEMS").length, 0);
		this.blue = blue;

	}

	@Override
	public boolean usesArea() {
		return false;
	}

	@Override
	public boolean mustBeIndoors() {
		return false;
	}

	@Override
	public Room create(TmpArea area, RoomInit init) {
		return blue.instance.place(area);
	}

	@Override
	public RoomBlueprintImp blue() {
		return blue;
	}

	@Override
	public void renderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item) {

		if (rx == 0 && ry == 0) {
			SETT.OVERLAY().monument(blue, item, tx, ty, 8);
		}
	}

	@Override
	public Addable overlay() {
		return SETT.OVERLAY().monument(blue);
	}

	@Override
	public boolean envValue(SettEnv e, SettEnvValue v, int tx, int ty) {
		if (envRadius[e.index()] != 0) {
			v.radius = (double)blue.radius(SETT.ROOMS().fData.item.get(tx, ty))/SettEnvMap.RADIUS;
			v.value = envValue[e.index()];
			return true;
		}
		return false;
	}

	@Override
	public void putFloor(int tx, int ty, int upgrade, AREA area) {

		super.putFloor(tx, ty, upgrade, area);
//			if (tree) {
//				SETT.FERTILITY().currentSetAbs(tx, ty, 0.7);
//			}
	}

}