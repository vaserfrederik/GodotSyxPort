using System;
using System.Collections.Generic;
using settlement.main;
using settlement.tilemap;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using util;
using world;

class GeneratorRiverSmall {

    private readonly HeightMap height;
    private readonly TileMap m;
    private readonly int width;
    private readonly GeneratorUtil util;
    private readonly Polymap polly;
    
    private readonly PathGame.PathFancy p = new PathGame.PathFancy(5000);

    public GeneratorRiverSmall(CapitolArea area, GeneratorUtil util) {

        util.checker.init();
        
        this.util = util;
        polly = util.polly;
        height = util.height;
        this.m = TILE_MAP();
        width = 1;
        for (int i = 0; i < GRID.tiles().size(); i++) {
            SettlementGrid.Tile ut = GRID.tile(i);
            build(area.ts().get(i), ut);
        }
        
        GUTIL.flooder().init(this);
        
        foreach (COORDINATE c in SETT.TILE_BOUNDS) {
            if (util.checker.is(c)) {
                GUTIL.flooder().pushSloppy(c, 0);
                double h = 1 + util.fer.get(c);
                if (h < 0) {
                    h = 0;
                }
                    
                h = Math.Sqrt(h / width);
                GUTIL.flooder().setValue2(c, h * 4);
                //util.fer.set(c, 0);
                
            }
        }
        
        while (GUTIL.flooder().hasMore()) {
            PathTile t = GUTIL.flooder().pollSmallest();
            TERRAIN().WATER.SHALLOW.placeRaw(t.x(), t.y());
            //GROUND().fertilityGet(0.5).placeRaw(t.x(), t.y());
            
            double v = t.getValue();
            

            if (v >= t.getValue2()) {
                continue;
            }
            
            for (int i = 0; i < DIR.ALL.size(); i++) {
                
                DIR d = DIR.ALL.get(i);
                int x = d.x() + t.x();
                int y = d.y() + t.y();
                
                if (!IN_BOUNDS(x, y))
                    continue;
                
                v = d.tileDistance();
                v *= 1 + Math.Pow(height.get(x, y), 4) * 1.5;
                v += t.getValue();
                if (TERRAIN().MOUNTAIN.is(x, y))
                    v += 2;
                if (v >= t.getValue2())
                    continue;
                if (GUTIL.flooder().pushSmaller(x, y, (float)v) != null) {
                    GUTIL.flooder().setValue2(x, y, t.getValue2());
                }

            }
        }
        
        GUTIL.flooder().done();
        
    }
    
    private void build(COORDINATE wtt, SettlementGrid.Tile ut) {

        if (!WORLD.WATER().RIVER_SMALL.is(wtt))
            return;

        for (int i = 0; i < DIR.ORTHO.size(); i++) {
            DIR dStart = DIR.ORTHO.get(i);
            if (WORLD.WATER().has.is(wtt, dStart)) {
                int startX = ut.coo(dStart).x();
                int startY = ut.coo(dStart).y();

                for (int j = i + 1; j < DIR.ORTHO.size(); j++) {
                    DIR dEnd = DIR.ORTHO.get(j);
                    if (WORLD.WATER().has.is(wtt, dEnd)) {
                        int endX = ut.coo(dEnd).x();
                        int endY = ut.coo(dEnd).y();
                        pave(positions(startX, startY, dStart, false), positions(endX, endY, dEnd, false), ut.bounds, 1, 1.0);
                    }
                }
            }

        }

    }

    private LinkedList<Coo> positions(int x, int y, DIR d, bool delta) {

        if (x < 0) {
            x = 0;
        }
        if (x >= TWIDTH) {
            x = TWIDTH - 1;
        }
        if (y < 0) {
            y = 0;
        }
        if (y >= THEIGHT) {
            y = THEIGHT - 1;
        }
        
        int start = delta ? QUAD_QUATER : 0;
        int width = this.width;
        if (delta)
            width *= 2;
        
        LinkedList<Coo> res = new LinkedList<Coo>();
        if (d.x() != 0) {
            for (int i = start; i < QUAD_SIZE * 2 && res.size() < width; i++) {
                if (IN_BOUNDS(x, y + i) && polly.isEdge(x, y + i)) {
                    res.add(new Coo(x, y + i));
                }
                if (IN_BOUNDS(x, y - i) && polly.isEdge(x, y - i)) {
                    res.add(new Coo(x, y - i));
                }
            }
        } else {
            for (int i = start; i < QUAD_SIZE * 2 && res.size() < width; i++) {
                if (IN_BOUNDS(x + i, y) && polly.isEdge(x + i, y)) {
                    res.add(new Coo(x + i, y));
                }
                if (IN_BOUNDS(x - i, y) && polly.isEdge(x - i, y)) {
                    res.add(new Coo(x - i, y));
                }
            }
        }
        return res;
        
        
    }
    
    private void pave(LinkedList<Coo> starts, LinkedList<Coo> ends, RECTANGLE bounds, double startWidth, double endWidth) {

        PathGame.COST cm = new PathGame.COST() {

            public double getCost(int fromX, int fromY, int toX, int toY) {

                if (!bounds.holdsPoint(toX, toY))
                    return 50;

                if (m.topology.MOUNTAIN.is(toX, toY))
                    return 20;

                if (m.topology.ROCK.is(toX, toY))
                    return 20;

                if (!polly.isEdge(toX, toY))
                    return 2.0;

                if (TERRAIN().WATER.SHALLOW.is(toX, toY))
                    return 0.5;
                
                double mul = util.checker.is(toX, toY) ? 1.0 : 1.0;
                
                return mul; 

            }
        };
        
        LinkedList<Coo> flood = new LinkedList<>();
        
        while(!starts.isEmpty()) {
            
            Coo s = starts.removeFirst();
            Coo e = ends.removeFirst();
            ends.add(e);
            if (GUTIL.astar().getShortest(p, cm, s.x(), s.y(), e.x(), e.y())) {
//                double dW = (endWidth - startWidth) / p.getLength();
//                if (endWidth == startWidth)
//                    dW = 0;
//                double w = startWidth;
                do {
                    if (!util.checker.is(p)) {
                        //util.fer.set(p, 1.0);
                        util.checker.set(p, true);
                    } else if (!TERRAIN().WATER.SHALLOW.is(p)){
                        //util.fer.increment(p, w); 
                    }
                    //w += dW;
                    
                } while (p.setNext());
            } else {
                LOG.ln("nono");
            }
            flood.add(s);
            
        }
        
        
        GUTIL.flooder().init(this);
        
        while(!flood.isEmpty()) {
            
            Coo s = flood.removeFirst();
            GUTIL.flooder().pushSloppy(s, 0);
            
        }

        while (GUTIL.flooder().hasMore()) {
            PathTile t = GUTIL.flooder().pollSmallest();
            TERRAIN().WATER.SHALLOW.placeRaw(t.x(), t.y());
            double v = t.getValue();

            if (v >= t.getValue2()) {
                continue;
            }

            for (int i = 0; i < DIR.ALL.size(); i++) {
                
                DIR d = DIR.ALL.get(i);
                int x = d.x() + t.x();
                int y = d.y() + t.y();
                
                if (!IN_BOUNDS(x, y))
                    continue;
                
                if (util.checker.is(t, d)) {
                    GUTIL.flooder().pushSmaller(x, y, 0);
                }

            }
        }
        
        GUTIL.flooder().done();
        

    }
}