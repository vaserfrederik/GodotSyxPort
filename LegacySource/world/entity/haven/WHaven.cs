using System;
using System.IO;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.rendering;
using world;
using world.entity;

namespace world.entity.haven
{
    public class WHaven : WEntity
    {
        private int ti = 0;
        private double size;
        private int ran;
        public readonly Str name = new Str(16);

        public WHaven() : base(24 * C.SCALE, 24 * C.SCALE) { }

        protected override void save(FilePutter file)
        {
            file.i(ti);
            file.d(size);
            file.i(ran);
            name.save(file);
        }

        protected override WEntity load(FileGetter file)
        {
            ti = file.i();
            ti = CLAMP.i(ti, 0, WORLD.camps().types.size() - 1);
            size = file.d();
            ran = file.i();
            name.load(file);
            return this;
        }

        protected override void renderAboveTerrain(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            WHavenType t = type();

            int off = (t.sheet.size() - 24 * C.SCALE) / 2;

            int ran = (int)((this.ran & 0b011) * 8);
            ran += (ran >> 2) & 1;
            int size = (int)(Math.Ceiling(this.size * 3)) * 2;
            t.cMask.bind();
            t.sheet.render(r, ran + size, x - off, y - off);
            s.setHeight(4).setDistance2Ground(0);
            t.sheet.render(s, ran + size, x - off, y - off);

            COLOR.unbind();
        }

        public void add(int tx, int ty, WHavenType type, double size, string name)
        {
            this.ti = type.index();
            this.ran = RND.rInt();
            this.name.clear().add(name);
            this.size = size;
            body().moveC(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH);

            add();
        }

        public WHavenType type()
        {
            return constructor().types.get(ti);
        }

        public int pop()
        {
            return (int)Math.Ceiling(type().popFrom + (type().popTo - type().popFrom) * size);
        }

        public double replenish()
        {
            return type().replenishMin + (type().replenishMax - type().replenishMin) * size;
        }

        protected override void renderBelowTerrain(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            // TODO Auto-generated method stub
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        protected override WHavens constructor()
        {
            return WORLD.ENTITIES().havens;
        }

        public void delete()
        {
            base.remove();
        }

        protected override void addAction()
        {
            Region r = WORLD.REGIONS().map.get(ctx(), cty());
            if (r != null)
                WORLD.ENTITIES().havens.setDirty(r.faction());
            base.addAction();
        }

        protected override void removeAction()
        {
            Region r = WORLD.REGIONS().map.get(ctx(), cty());
            if (r != null)
                WORLD.ENTITIES().havens.setDirty(r.faction());
            if (!constructor().free.isFull())
                constructor().free.push(this);
        }

        public override Faction faction()
        {
            Region r = WORLD.REGIONS().map.get(ctx(), cty());
            if (r != null)
                return r.faction();
            return null;
        }
    }
}