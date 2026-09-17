using System;
using System.IO;
using System.Linq;

namespace Game.Battle.Thread.General.Offence
{
    class ContextLines : SAVABLE
    {
        private int lineI;
        private Line[] all = new Line[0];
        private static readonly VectorImp vec = new VectorImp();

        ContextLines()
        {
        }

        public class Line : SAVABLE
        {
            public int sx;
            public int sy;
            public int length;
            public double dx;
            public double dy;
            public byte mark;
            public int back;
            public int blobID;

            private Line()
            {
            }

            public int cx()
            {
                return (int)(sx + dx * length / 2);
            }

            public int cy()
            {
                return (int)(sy + dy * length / 2);
            }

            public DivFormation deploy(StrategosUtil util, Div d)
            {
                int m = d.menNrOf();
                if (m == 0)
                    return null;
                int w = (int)Math.Sqrt(m);
                if (w == 0)
                    return null;

                int x = sx;
                int y = sy;

                vec.set(dx, dy);
                vec.rotate90();
                x += back * vec.nX();
                y += back * vec.nY();

                DivFormation f = util.divDeployer.deploy(d, x, y, length, dx, dy);

                if (f != null)
                {
                    back += d.settings().formation.size(d) * Math.Ceiling(m /= (length / d.settings().formation.size(d))) + C.TILE_SIZE;
                }
                return f;
            }

            public override void save(FilePutter file)
            {
                file.i(sx);
                file.i(sy);
                file.i(length);
                file.d(dx);
                file.d(dy);
                file.i(back);
                file.b(mark);
                file.i(blobID);
            }

            public override void load(FileGetter file)
            {
                sx = file.i();
                sy = file.i();
                length = file.i();
                dx = file.d();
                dy = file.d();
                back = file.i();
                mark = file.b();
                blobID = file.i();
            }

            public override void clear()
            {
                sx = 0;
                sy = 0;
                length = 1;
                dx = 1;
                dy = 0;
                back = 0;
                mark = 0;
                blobID = 0;
            }
        }

        public Line get(int index)
        {
            if (index >= lineI)
                throw new RuntimeException();
            return all[index];
        }

        public void remove(int index)
        {
            Line l = all[index];
            all[index] = all[lineI - 1];
            all[lineI - 1] = l;
            lineI--;
        }

        public int lines()
        {
            return lineI;
        }

        public Line makeNew()
        {
            if (lineI >= all.Length)
            {
                Line[] lines2 = new Line[all.Length + 256];
                all.CopyTo(lines2, 0);
                for (int i = all.Length; i < lines2.Length; i++)
                {
                    lines2[i] = new Line();
                }
                all = lines2;
            }
            Line l = all[lineI];
            l.clear();
            lineI++;
            return l;
        }

        public override void save(FilePutter file)
        {
            file.i(all.Length);
            file.i(lineI);

            for (int i = 0; i < lineI; i++)
            {
                Line l = all[i];
                l.save(file);
            }
        }

        public override void load(FileGetter file)
        {
            all = new Line[file.i()];
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = new Line();
            }
            lineI = file.i();

            for (int i = 0; i < lineI; i++)
            {
                Line l = all[i];
                l.load(file);
            }
        }

        public override void clear()
        {
            lineI = 0;
        }
    }
}