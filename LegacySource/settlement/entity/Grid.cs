using System;
using settlement.main;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.entity
{
    class Grid
    {
        /**
         * max tilesize of a hitbox
         */
        private const int max = 1;
        private readonly ENTITY[,] ents = new ENTITY[THEIGHT, TWIDTH];
        private readonly Resolver resolver = new Resolver();

        Grid()
        {

        }

        void Add(ENTITY e)
        {
            Add(e, true);
        }

        void Add(ENTITY e, bool collide)
        {
            if (e.gx != -1 && e.gy != -1)
                throw new RuntimeException();

            int tx = e.tc().X();
            int ty = e.tc().Y();

            if (collide)
                Collide(e);
            if (e.IsRemoved())
                return;

            e.prev = null;
            e.next = ents[ty, tx];
            if (e.next != null)
            {
                e.next.prev = e;
            }
            e.gx = (short)tx;
            e.gy = (short)ty;
            ents[ty, tx] = e;

            PATH().finders.entity.report(e, 1);
        }

        public void AddRaw(ENTITY e)
        {
            int tx = e.tc().X();
            int ty = e.tc().Y();

            e.prev = null;
            e.next = ents[ty, tx];
            if (e.next != null)
            {
                e.next.prev = e;
            }
            e.gx = (short)tx;
            e.gy = (short)ty;
            ents[ty, tx] = e;
            e.physics.initMoveCheck();
        }

        void Move(ENTITY e)
        {
            if (!e.physics.MoveCheck())
            {
                e.physics.initMoveCheck();

                int tx = e.tc().X();
                int ty = e.tc().Y();

                if (tx != e.gx || ty != e.gy)
                {
                    Remove(e);
                    Add(e, true);
                }
                else
                {
                    Collide(e);
                }
            }
        }

        private void Collide(ENTITY e)
        {
            int tx = e.tc().X();
            int ty = e.tc().Y();
            int tx1 = tx - max;
            int tx2 = tx + max;
            int ty1 = ty - max;
            int ty2 = ty + max;

            if (tx1 < 0)
                tx1 = 0;
            if (tx2 >= TWIDTH)
                tx2 = TWIDTH - 1;

            if (ty1 < 0)
                ty1 = 0;
            if (ty2 >= THEIGHT)
                ty2 = THEIGHT - 1;

            int x1 = e.body().x1();
            int x2 = e.body().x2();
            int y1 = e.body().y1();
            int y2 = e.body().y2();

            int death = 1000;

            for (int y = ty1; y <= ty2; y++)
            {
                for (int x = tx1; x <= tx2; x++)
                {
                    ENTITY c = ents[y, x];
                    while (c != null)
                    {
                        ENTITY next = c.next;
                        if (c != e)
                        {
                            if (((x1 < c.body().x2() && x2 > c.body().x1()) && (y1 < c.body().y2() && y2 > c.body().y1())))
                            {
                                resolver.resolveCollision(c, e);

                                if (e.IsRemoved())
                                    return;
                            }
                        }
                        c = next;

                        if (death-- <= 0)
                        {
                            throw new RuntimeException(e + " " + e.id() + c + " " + c.id());
                        }
                    }
                }
            }
        }

        void Remove(ENTITY e)
        {
            int ty = e.gy;
            int tx = e.gx;

            if (tx == -1 || ty == -1)
            {
                return;
            }

            PATH().finders.entity.report(e, -1);

            if (e.next != null)
            {
                e.next.prev = e.prev;
            }
            if (e.prev != null)
            {
                e.prev.next = e.next;
            }

            if (ents[ty, tx] == e)
            {
                ents[ty, tx] = e.next;
            }

            e.gy = -1;
            e.gx = -1;
            e.next = null;
            e.prev = null;
        }

        void Fill(RECTANGLE area, ADDABLE<ENTITY> result)
        {
            int tx1 = (area.x1() >> C.T_SCROLL) - max;
            int tx2 = (area.x2() >> C.T_SCROLL) + max + 1;
            int ty1 = (area.y1() >> C.T_SCROLL) - max;
            int ty2 = (area.y2() >> C.T_SCROLL) + max + 1;

            if (tx1 < 0)
                tx1 = 0;
            if (tx2 >= TWIDTH)
                tx2 = TWIDTH - 1;

            if (ty1 < 0)
                ty1 = 0;
            if (ty2 >= THEIGHT)
                ty2 = THEIGHT - 1;

            for (int y = ty1; y <= ty2; y++)
            {
                for (int x = tx1; x <= tx2; x++)
                {
                    ENTITY c = ents[y, x];
                    while (c != null)
                    {
                        if (c.body().touches(area))
                        {
                            result.tryAdd(c);
                        }
                        c = c.next;
                    }
                }
            }
        }

        void FillTile(int tx, int ty, ADDABLE<ENTITY> result)
        {
            if (tx < 0)
                return;
            if (tx >= TWIDTH)
                return;

            if (ty < 0)
                return;
            if (ty >= THEIGHT)
                return;

            ENTITY c = ents[ty, tx];
            while (c != null)
            {
                result.tryAdd(c);
                c = c.next;
            }
        }

        void Fill(int cx, int cy, ADDABLE<ENTITY> result)
        {
            int tx1 = (cx >> C.T_SCROLL) - max;
            int tx2 = (cx >> C.T_SCROLL) + max;
            int ty1 = (cy >> C.T_SCROLL) - max;
            int ty2 = (cy >> C.T_SCROLL) + max;

            if (tx1 < 0)
                tx1 = 0;
            if (tx2 >= TWIDTH)
                tx2 = TWIDTH - 1;

            if (ty1 < 0)
                ty1 = 0;
            if (ty2 >= THEIGHT)
                ty2 = THEIGHT - 1;

            for (int y = ty1; y <= ty2; y++)
            {
                for (int x = tx1; x <= tx2; x++)
                {
                    ENTITY c = ents[y, x];
                    while (c != null)
                    {
                        if (c.body().holdsPoint(cx, cy))
                        {
                            result.tryAdd(c);
                        }
                        c = c.next;
                    }
                }
            }
        }

        ENTITY GetFirst(int tx, int ty)
        {
            if (SETT.IN_BOUNDS(tx, ty))
                return ents[ty, tx];
            return null;
        }

        void Clear()
        {
            for (int y = 0; y < THEIGHT; y++)
            {
                for (int x = 0; x < TWIDTH; x++)
                {
                    ENTITY c = ents[y, x];
                    while (c != null)
                    {
                        ENTITY next = c.next;
                        c.gx = -1;
                        c.gy = -1;
                        c.prev = null;
                        c.next = null;
                        c = next;
                    }
                    ents[y, x] = null;
                }
            }
        }

        void Fill(ENTITY e, int radius, ADDABLE<ENTITY> result)
        {
            int tx1 = (e.body().cX() >> C.T_SCROLL) - max - radius;
            int tx2 = (e.body().cX() >> C.T_SCROLL) + max + 1 + radius;
            int ty1 = (e.body().cY() >> C.T_SCROLL) - max - radius;
            int ty2 = (e.body().cY() >> C.T_SCROLL) + max + 1 + radius;

            if (tx1 < 0)
                tx1 = 0;
            if (tx2 >= TWIDTH)
                tx2 = TWIDTH - 1;

            if (ty1 < 0)
                ty1 = 0;
            if (ty2 >= THEIGHT)
                ty2 = THEIGHT - 1;

            for (int y = ty1; y <= ty2; y++)
            {
                for (int x = tx1; x <= tx2; x++)
                {
                    ENTITY c = ents[y, x];
                    while (c != null)
                    {
                        if (c != e)
                        {
                            result.tryAdd(c);
                        }
                        c = c.next;
                    }
                }
            }
        }
    }
}