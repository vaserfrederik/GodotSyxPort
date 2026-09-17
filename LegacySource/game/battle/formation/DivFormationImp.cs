using System;
using System.IO;

namespace game.battle.formation
{
    public class DivFormationImp : DivPositionImp, DivFormation, Copyable<DivFormationImp>
    {
        private DIR faceDir = DIR.N;
        private readonly Rec bounds = new Rec();
        readonly Coo start = new Coo();
        private double dx;
        private double dy;
        private int width;

        private DIV_FORMATION ts = DIV_FORMATION.TIGHT;
        private int centreI = -1;
        private bool hasExtraRoom = false;
        private bool isNotCoherent = false;
        private readonly byte[] dirMasks;

        public DivFormationImp()
            : this(Config.battle().MEN_PER_DIVISION)
        {
        }

        private DivFormationImp(int maxMen)
            : base(maxMen)
        {
            dirMasks = new byte[maxMen];
        }

        public override DIR dir()
        {
            return faceDir;
        }

        public override void save(FilePutter file)
        {
            base.save(file);
            file.i(faceDir.id());
            bounds.save(file);
            start.save(file);
            file.i(centreI);
            file.d(dx);
            file.d(dy);
            file.i((int)ts);
            file.bs(dirMasks);
            file.bool(hasExtraRoom);
            file.bool(isNotCoherent);
            file.i(width);
        }

        public override void load(FileGetter file)
        {
            base.load(file);
            faceDir = DIR.ALL.get(file.i());
            bounds.load(file);
            start.load(file);
            centreI = file.i();
            dx = file.d();
            dy = file.d();
            ts = (DIV_FORMATION)file.i();
            file.bs(dirMasks);
            hasExtraRoom = file.bool();
            isNotCoherent = file.bool();
            width = file.i();
        }

        public override void clear()
        {
            base.clear();
            bounds.set(SETT.PWIDTH + 1, -1, SETT.PHEIGHT + 1, -1);
            ts = DIV_FORMATION.TIGHT;
            hasExtraRoom = false;
            isNotCoherent = false;
            centreI = -1;
        }

        public void deployInit(DIR face, int x1, int y1, double dx, double dy, DIV_FORMATION ts, int width)
        {
            clear();
            this.start.set(x1, y1);
            this.dx = dx;
            this.dy = dy;
            this.faceDir = face;
            this.ts = ts;
            this.width = width;
        }

        public void move(int dx, int dy)
        {
            this.start.increment(dx, dy);
            bounds.incr(dx, dy);

            for (int i = 0; i < deployed(); i++)
            {
                int x = px(i) + dx;
                int y = py(i) + dy;
                set(i, x, y);
            }
        }

        public void deploy(int x, int y, DIV_SPEC spec)
        {
            int t = ts.sizeH(spec);
            bounds.unify(x - t, y - t);
            bounds.unify(x + t, y - t);
            bounds.unify(x - t, y + t);
            bounds.unify(x + t, y + t);

            set(deployed(), x, y);
            init(deployed() + 1);
        }

        public void deploy(int x, int y, int pi, DIV_SPEC spec)
        {
            int t = ts.sizeH(spec);
            bounds.unify(x - t, y - t);
            bounds.unify(x + t, y - t);
            bounds.unify(x - t, y + t);
            bounds.unify(x + t, y + t);

            set(pi, x, y);
            init(deployed() + 1);
        }

        public void deployFinish(Filler f, DIV_SPEC spec)
        {
            isNotCoherent = false;
            if (deployed() == 0)
                return;

            int xx = 0;
            int yy = 0;
            for (int i = 0; i < deployed(); i++)
            {
                COORDINATE p = pixel(i);
                xx += p.x();
                yy += p.y();
            }

            xx /= deployed();
            yy /= deployed();

            int cx = xx;
            int cy = yy;

            double dist = double.MaxValue;
            int distI = -1;
            for (int i = 0; i < deployed(); i++)
            {
                COORDINATE p = pixel(i);
                int dx = p.x() - cx;
                int dy = p.y() - cy;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d < dist)
                {
                    dist = d;
                    distI = i;
                }
            }

            if (distI == -1)
                throw new RuntimeException();

            this.centreI = distI;

            setDirs(f, spec);
        }

        public void setHasExtraRoom()
        {
            hasExtraRoom = true;
        }

        public override void copy(DivFormationImp o)
        {
            base.copyposition(o);
            faceDir = o.faceDir;
            bounds.set(o.bounds);
            start.set(o.start);
            dx = o.dx;
            dy = o.dy;
            centreI = o.centreI;
            hasExtraRoom = o.hasExtraRoom;
            isNotCoherent = o.isNotCoherent;
            width = o.width;
            Array.Copy(o.dirMasks, dirMasks, dirMasks.Length);
        }

        public override RECT body()
        {
            return base.body();
        }

        public override int deployed()
        {
            return base.deployed();
        }

        public override void init(int num)
        {
            base.init(num);
        }

        public override int px(int i)
        {
            return base.px(i);
        }

        public override int py(int i)
        {
            return base.py(i);
        }

        public override void set(int i, int x, int y)
        {
            base.set(i, x, y);
        }

        public override COORDINATE pixel(int i)
        {
            return base.pixel(i);
        }

        public override COORDINATE tile(int i)
        {
            return base.tile(i);
        }

        public override int size()
        {
            return base.size();
        }

        public override void swap(int a, int b)
        {
            if (a == centreI)
                centreI = b;
            else if (b == centreI)
                centreI = a;

            byte dm = dirMasks[a];
            int x = px(a);
            int y = py(a);
            set(a, px(b), py(b));
            set(b, x, y);
            dirMasks[a] = dirMasks[b];
            dirMasks[b] = dm;
        }

        public override bool isCoherent()
        {
            return !isNotCoherent;
        }

        public void coherentSetNot()
        {
            isNotCoherent = true;
        }

        public override string ToString()
        {
            return start.x() + " " + start.y() + " | > " + dx + " " + dy + " | " + width + " " + deployed() + " | " + ts + " | " + centrePixel();
        }

        public override COORDINATE centreTile()
        {
            if (deployed() == 0)
                return null;
            return tile(centreI);
        }

        public override COORDINATE centrePixel()
        {
            if (deployed() == 0)
                return null;
            return pixel(centreI);
        }

        public int centreI()
        {
            return centreI;
        }

        public override bool isSameAs(DivFormationImp o)
        {
            return start.isSameAs(o.start) && dx == o.dx && dy == o.dy && deployed() == o.deployed() && width == o.width && ts == o.ts && centrePixel().isSameAs(o.centrePixel());
        }

        public override bool isSameAs(DivFormationImp o, Div div)
        {
            if (!start.isSameAs(o.start))
                return false;
            if (dx != o.dx || dy != o.dy)
                return false;
            if (ts != o.ts || !centrePixel().isSameAs(o.centrePixel()))
                return false;
            if (deployed() != o.deployed())
                return false;
            int w1 = width / ts.size(div);
            int w2 = o.width / ts.size(div);

            if (w1 != w2)
                return false;
            return true;
        }
    }
}