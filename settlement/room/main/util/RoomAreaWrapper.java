package settlement.room.main.util;

import settlement.main.SETT;
import settlement.room.main.ROOMA;
import settlement.room.main.Room;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.RECTANGLE;
import snake2d.util.datatypes.Rec;

public class RoomAreaWrapper {

	private Rec body = new Rec();
	private Room room;
	private int mx, my;
	private int area;
	private boolean used;
//	private StackTraceElement[] uu;
	
	public RoomAreaWrapper() {
		
	}
	
	public ROOMA init(Room r, int x, int y) {
		use();
		if (r instanceof ROOMA)
			return (ROOMA)r;
		if (room != r || !a.is(x, y) || true) {
			room = r;
			mx = r.mX(x, y);
			my = r.mY(x, y);
			body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
			body.setDim(r.width(mx, my), r.height(mx, my));
			area = r.area(mx, my);
		}
		return a;
	}
	
	private void use() {
		if (used) {
//			for (StackTraceElement ee : uu)
//				System.err.println(ee);
			throw new RuntimeException();
		}
		used = true;
//		uu = new RuntimeException().getStackTrace();
	}
	
	public boolean changedAndInit(Room r, int x, int y) {
		use();
		if (room != r || !a.is(x, y)) {
			
			room = r;
			mx = r.mX(x, y);
			my = r.mY(x, y);
			body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
			body.setDim(r.width(mx, my), r.height(mx, my));
			area = r.area(mx, my);
			return true;
		}
		return false;
		
	}
	
	public ROOMA init(Room r, COORDINATE c) {
		return init(r, c.x(), c.y());
	}
	
	public ROOMA area() {
		return a;
	}
	
	public void done() {
		used = false;
	}
	
	private ROOMA a = new ROOMA() {
		
		@Override
		public RECTANGLE body() {
			return body;
		}

		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}

		@Override
		public boolean is(int tx, int ty) {
			return room != null && SETT.ROOMS().map.get(tx, ty) == room && room.isSame(mx, my, tx, ty);
		}

		@Override
		public int area() {
			return area;
		}

		@Override
		public int index() {
			return room.index();
		}

		@Override
		public int mX() {
			return mx;
		}

		@Override
		public int mY() {
			return my;
		}
	};

	public void clear() {
		room = null;
	}
	
	public static class RoomWrap implements ROOMA{
		
		private Rec body = new Rec();
		private Room room;
		private int mx, my;
		private int area;
		
		public boolean init(Room r, int x, int y) {
			if (room != r || !is(x, y)) {
				room = r;
				mx = r.mX(x, y);
				my = r.mY(x, y);
				body.moveX1Y1(r.x1(mx, my), r.y1(mx, my));
				body.setDim(r.width(mx, my), r.height(mx, my));
				area = r.area(mx, my);
				return true;
			}
			return false;
		}
		
		@Override
		public RECTANGLE body() {
			return body;
		}

		@Override
		public boolean is(int tile) {
			return is(tile%SETT.TWIDTH, tile/SETT.TWIDTH);
		}

		@Override
		public boolean is(int tx, int ty) {
			return SETT.ROOMS().map.get(tx, ty) == room && room.isSame(mx, my, tx, ty);
		}

		@Override
		public int area() {
			return area;
		}

		@Override
		public int index() {
			return room.index();
		}

		@Override
		public int mX() {
			return mx;
		}

		@Override
		public int mY() {
			return my;
		}
	}
	

}
