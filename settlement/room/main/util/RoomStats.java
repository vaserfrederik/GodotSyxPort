package settlement.room.main.util;

import java.io.IOException;

import settlement.main.SETT;
import settlement.room.main.ROOMS.RoomResource;
import settlement.room.main.Room;
import snake2d.util.datatypes.COORDINATE;
import snake2d.util.datatypes.Coo;
import snake2d.util.file.FileGetter;
import snake2d.util.file.FilePutter;
import snake2d.util.file.SAVABLE;
import snake2d.util.sets.QueueInteger;

public class RoomStats extends RoomResource{

	private RoomStatsList list = new RoomStatsList(256);
	private RoomStatsList broken = new RoomStatsList(512);

	public RoomStatsList finished() {
		return list;
	}
	
	public RoomStatsList broken() {
		return broken;
	}

	@Override
	protected void save(FilePutter file) {
		list.save(file);
		broken.save(file);
	}

	@Override
	protected void load(FileGetter file) throws IOException {
		list.load(file);
		broken.load(file);
	}

	@Override
	protected void clear() {
		list.clear();
		broken.clear();
	}
	
	public static class RoomStatsList implements SAVABLE{
		
		private QueueInteger list;
		private QueueInteger listTemp;
		
		private RoomStatsList(int size){
			list = new QueueInteger(size);
			listTemp = new QueueInteger(size);
		}
		
		public void add(int mx, int my) {
			if (!list.hasRoom())
				list.poll();
			int ii = mx | (my<<16);
			list.push(ii);		
		}
		
		public void remove(int mx, int my) {
			listTemp.clear();
			while(list.hasNext()) {
				int i = list.poll();
				int tx = i & 0x0FFFF;
				int ty = (i >> 16) & 0x0FFFF;
				if (mx == tx && my == ty) {
					continue;
				}
				listTemp.push(i);
			}
			QueueInteger l = list;
			list = listTemp;
			listTemp = l;
		}
		
		public int amount() {
			return list.size();
		}
		
		private final Coo tmp = new Coo();
		
		public COORDINATE poll() {
			if (!list.hasNext())
				return null;
			int i = list.poll();
			int tx = i & 0x0FFFF;
			int ty = (i >> 16) & 0x0FFFF;
			Room r = SETT.ROOMS().map.get(tx, ty);
			if (r != null) {
				tmp.set(tx, ty);
				return tmp;
			}
			return null;
		}

		@Override
		public void save(FilePutter file) {
			list.save(file);
		}

		@Override
		public void load(FileGetter file) throws IOException {
			list.load(file);
		}

		@Override
		public void clear() {
			list.clear();
		}
	}

	@Override
	protected void update(double ds) {
		// TODO Auto-generated method stub
		
	}
	
	
}
