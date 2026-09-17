using System;
using System.Collections.Generic;
using snake2d;
using util;

namespace view.ui.tech
{
    final class NodeCreator
    {
        final static int DIM = 32;

        public readonly LIST<RENDEROBJ> rows;
        private readonly MAP_OBJECT<RENDEROBJ> map;
        private readonly RECTANGLE bounds;

        NodeCreator(TechTree tree)
        {
            RENDEROBJ[][] res = Create(tree);
            bounds = new Rec(res[0].Length, res.Length);
            map = new MAP_OBJECT<RENDEROBJ>()
            {
                public RENDEROBJ get(int tx, int ty)
                {
                    if (bounds.holdsPoint(tx, ty))
                        return res[ty][tx];
                    return null;
                }

                public RENDEROBJ get(int tile)
                {
                    return null;
                }
            };
            Path();
            ArrayList<RENDEROBJ> rows = new ArrayList<RENDEROBJ>(res.Length);
            for (int i = 0; i < res.Length - 1; i += 2)
            {
                GuiSection s = new GuiSection();
                for (int x = 0; x < res[i].Length; x++)
                {
                    s.add(res[i][x], s.body().x2(), 0);
                    s.add(res[i + 1][x], s.getLast().x1(), s.getLastY2());
                }
                rows.add(s);
            }
            this.rows = rows;
        }

        private RENDEROBJ[][] Create(TechTree tree)
        {
            int w = 0;
            foreach (TECH[] n in tree.nodes)
                w = Math.Max(w, n.Length);
            NodeBoosts boosts = new NodeBoosts();
            RENDEROBJ[][] res = new RENDEROBJ[tree.nodes.Length * 2 + 1][w * 2];

            for (int y = 0; y < tree.nodes.Length; y++)
            {
                for (int x = 0; x < tree.nodes[y].Length; x++)
                {
                    if (tree.nodes[y][x] != null)
                        res[y * 2][x * 2] = new Node(tree.nodes[y][x], boosts);
                }
            }
            for (int y = 0; y < res.Length; y++)
            {
                for (int x = 0; x < res[y].Length; x++)
                {
                    if (res[y][x] == null)
                    {
                        int wi = (x & 1) == 1 ? DIM : Node.WIDTH;
                        int hi = (y & 1) == 1 ? DIM : Node.HEIGHT();
                        res[y][x] = new Edge(wi, hi);
                    }
                }
            }

            for (int y = 0; y < res.Length; y++)
            {
                bool clear = true;
                for (int x = 0; x < res[y].Length; x++)
                {
                    if (res[y][x] is Node)
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear)
                {
                    for (int x = 0; x < res[y].Length; x++)
                    {
                        RENDEROBJ o = res[y][x];
                        if (o.body().height() != DIM)
                            res[y][x] = new Edge(o.body().width(), DIM);
                    }
                }
            }

            for (int x = 0; x < res[0].Length; x++)
            {
                bool clear = true;
                for (int y = 0; y < res.Length; y++)
                {
                    if (res[y][x] is Node)
                    {
                        clear = false;
                        break;
                    }
                }
                if (clear)
                {
                    for (int y = 0; y < res.Length; y++)
                    {
                        RENDEROBJ o = res[y][x];
                        if (o.body().width() != DIM)
                            res[y][x] = new Edge(DIM, o.body().height());
                    }
                }
            }

            return res;
        }

        private void Path()
        {
            Bitmap1D m = new Bitmap1D(TECHS.ALL().size() * TECHS.ALL().size(), false);
            foreach (COORDINATE c in bounds)
            {
                RENDEROBJ o = map.get(c);
                if (o is Node)
                {
                    Node n = (Node)o;
                    m.set(n.tech.index() * TECHS.ALL().size() + n.tech.index(), true);
                }
            }

            Flooder f = GUTIL.flooder();

            outer:
            while (true)
            {
                f.init(this);

                Node start = null;
                fucker:
                foreach (COORDINATE c in bounds)
                {
                    RENDEROBJ o = map.get(c);
                    if (o is Node)
                    {
                        Node n = (Node)o;
                        for (int ri = 0; ri < n.tech.requiresNodes().size(); ri++)
                        {
                            TechRequirement r = n.tech.requiresNodes().get(ri);
                            if (!m.get(n.tech.index() * TECHS.ALL().size() + r.tech.index()))
                            {
                                start = n;
                                f.pushSloppy(c.x(), c.y(), 0);
                                f.setValue2(c.x(), c.y(), n.tech.index());
                                break fucker;
                            }
                        }
                    }
                }

                if (start == null)
                {
                    f.done();
                    return;
                }

                while (f.hasMore())
                {
                    PathTile t = f.pollSmallest();
                    RENDEROBJ ro = map.get(t);

                    if (ro is Node)
                    {
                        Node n = (Node)ro;
                        if (!m.get((int)(t.getValue2() * TECHS.ALL().size() + n.tech.index())))
                        {
                            PathTile root = t;
                            while (root.getParent() != null)
                                root = root.getParent();
                            Node node = (Node)map.get(root);
                            m.set(n.tech.index() * TECHS.ALL().size() + node.tech.index(), true);
                            m.set(node.tech.index() * TECHS.ALL().size() + n.tech.index(), true);
                            PathTile parent = t;
                            t = t.getParent();
                            while (t != null)
                            {
                                if (map.get(t) is Edge)
                                {
                                    Edge e = (Edge)map.get(t);

                                    DIR ori = DIR.get(t, parent);
                                    int mm = ori.mask();

                                    if (t.getParent() != null)
                                    {
                                        mm |= DIR.get(t, t.getParent()).mask();
                                    }
                                    if (t.getParent().getParent() == null)
                                        e.a |= DIR.get(t, t.getParent()).mask();
                                    e.m |= mm;

                                    node.addEdge(n, e, mm);
                                }
                                parent = t;
                                t = t.getParent();
                            }
                            m.set(n.tech.index(), false);
                            f.done();
                            continue outer;
                        }
                    }

                    foreach (DIR d in DIR.ORTHO)
                    {
                        if (d != DIR.S)
                            Push(t.x(), t.y(), d, t);
                    }
                }

                for (int ri = 0; ri < start.tech.requiresNodes().size(); ri++)
                {
                    TechRequirement r = start.tech.requiresNodes().get(ri);
                    if (!m.get(start.tech.index() * TECHS.ALL().size() + r.tech.index()))
                    {
                        m.set(start.tech.index() * TECHS.ALL().size() + r.tech.index(), true);
                        m.set(start.tech.index() + r.tech.index() * TECHS.ALL().size(), true);
                    }
                }
                f.done();
            }
        }

        private void Push(int dx, int dy, DIR d, PathTile parent)
        {
            if (!bounds.holdsPoint(dx, dy, d))
                return;
            double v = 0;
            if (map.get(dx, dy) is Edge)
            {
                Edge e = (Edge)map.get(dx, dy);
                if (d.x() != 0)
                    v += e.body().width() / 2;
                else
                    v += e.body().height() / 2;
                if ((e.m & d.mask()) != 0)
                    v *= 0.25;
            }
            else if (((Node)map.get(dx, dy)).tech.index() != parent.getValue2())
                return;
            dx += d.x();
            dy += d.y();
            if (map.get(dx, dy) is Edge)
            {
                Edge e = (Edge)map.get(dx, dy);
                if (d.x() != 0)
                    v += e.body().width() / 2;
                else
                    v += e.body().height() / 2;
                if ((e.m & d.mask()) != 0)
                    v *= 0.25;
            }
            else
            {
                Node n = (Node)map.get(dx, dy);
                if (!Req(parent, n))
                    return;
            }
            if (parent.getParent() != null)
            {
                if (DIR.get(parent.getParent(), parent) != d)
                    v += DIM / 4;
            }

            if (GUTIL.flooder().pushSmaller(dx, dy, v + parent.getValue(), parent) != null)
            {
                GUTIL.flooder().setValue2(dx, dy, parent.getValue2());
            }
        }

        private bool Req(PathTile t, Node node)
        {
            TECH p = TECHS.ALL().get((int)t.getValue2());
            for (int i = 0; i < p.requiresNodes().size(); i++)
            {
                if (p.requiresNodes().get(i).tech == node.tech)
                    return true;
            }
            return false;
        }
    }
}