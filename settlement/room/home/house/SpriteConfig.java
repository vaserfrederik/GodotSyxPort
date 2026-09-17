package settlement.room.home.house;

import snake2d.util.map.MAP_OBJECT;

final class SpriteConfig {

	final Sprite[][] spri;
	private final Getter[] sprites = new Getter[4];
	
	SpriteConfig(Sprite[][] sprites){
		this.spri = sprites;
		this.sprites[0] = new Getter(sprites);
		for (int i = 1; i < 4; i++) {
			this.sprites[i] = new Getter(rotate(this.sprites[i-1].sp));
		}
	}
	
	private Sprite[][] rotate(Sprite[][] l) {
		final int M = l.length;
		final int N = l[0].length;
		Sprite[][] ret = new Sprite[N][M];
		for (int r = 0; r < M; r++) {
			for (int c = 0; c < N; c++) {
				ret[c][M - 1 - r] = l[r][c];
			}
		}
		return ret;
	}
	
	private static class Getter implements MAP_OBJECT<Sprite> {

		private final Sprite[][] sp;
		
		Getter(Sprite[][] sp){
			this.sp = sp;
		}

		@Override
		public Sprite get(int tile) {
			// TODO Auto-generated method stub
			return null;
		}

		@Override
		public Sprite get(int tx, int ty) {
			if (tx < 0 || tx >= sp[0].length)
				return null;
			if (ty < 0 || ty >= sp.length)
				return null;
			return sp[ty][tx];
		}
		
	}
	
	public MAP_OBJECT<Sprite> get(int rot){
		return sprites[rot];
	}
	

	
}
