using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;

namespace util
{
    public interface Debugger
    {
        Str debug(CharSequence name);
        void debug(CharSequence name, ACTION a);
        void title(CharSequence name);
        void debugObject(CharSequence name, object o);
        void debugObject(object o);
    }

    public interface Debuggable
    {
        void debug(CharSequence name, Debugger d);
    }

    public static class DebuggerDummy : Debugger
    {
        private readonly Str tmp = new Str(128);

        public void title(CharSequence name)
        {
        }

        public Str debug(CharSequence name)
        {
            tmp.clear();
            return tmp;
        }

        public void debugObject(CharSequence name, object o)
        {
            // TODO Auto-generated method stub
        }

        public void debug(CharSequence name, ACTION a)
        {
            // TODO Auto-generated method stub
        }
    }

    public class DebuggerLive : Debugger
    {
        public ArrayListGrower<Row> all = new ArrayListGrower<Row>();
        public int size = 0;
        private int indent = 0;

        public DebuggerLive()
        {
        }

        public Str debug(CharSequence name)
        {
            if (size >= 2048 * 8)
            {
                all.get(size).name = "overflow...";
                return Str.TMP.clear();
            }
            if (size >= all.size())
                all.add(new Row());
            all.get(size).name = name;
            all.get(size).a = null;
            all.get(size).indent = indent;
            Str s = all.get(size).str;
            size++;
            return s.clear();
        }

        public void debug(CharSequence name, ACTION a)
        {
            debug(name);
            all.get(size - 1).a = a;
        }

        public void title(CharSequence name)
        {
            debug(null).add(name);
        }

        public void debugObject(CharSequence name, object o)
        {
            if (o == null)
            {
                debug(name).add("null");
                return;
            }
            if (indent > 10)
            {
                debug(name).add("overflow...");
                return;
            }
            else
            {
                debug(name).add('>');
            }

            Type clazz = o.GetType();
            indent++;
            foreach (FieldInfo field in clazz.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.IsStatic)
                {
                    continue;
                }
                field.IsPublic = true;

                try
                {
                    object value = field.GetValue(o);

                    if (value == null)
                    {
                        debug(field.Name).add("null");
                    }
                    else if (value is Debuggable dbg)
                    {
                        debugObject(field.Name, dbg);
                    }
                    else
                    {
                        debug(field.Name).add(value.ToString());
                    }
                }
                catch (IllegalAccessException e)
                {
                    debug(field.Name).add("inaccessible");
                }
            }
            indent--;
        }

        public void clear()
        {
            size = 0;
            indent = 0;
        }
    }

    public class Row
    {
        public CharSequence name;
        public readonly Str str = new Str(64);
        public int indent = 0;
        public ACTION a;
    }

    public abstract class DebuggerSection : GuiSection
    {
        private readonly ArrayListResize<Row> filtered = new ArrayListResize<Row>(128);
        public readonly DebuggerLive debugger = new DebuggerLive();
        private readonly GInput sp = new GInput(new StringInputSprite(24, UI.FONT().S));

        public DebuggerSection(int height)
        {
            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () => filtered.size(),
                click = index =>
                {
                    if (filtered.get(index).a != null)
                        filtered.get(index).a.exe();
                }
            };

            bu.column(null, 800, new GRowBuilder
            {
                build = ier =>
                {
                    return new RENDEROBJ.RenderImp(800, 24)
                    {
                        render = (r, ds) =>
                        {
                            Row row = filtered.get(ier.get());
                            int x1 = body.x1() + row.indent * 10;
                            if (row.name == null)
                            {
                                GCOLOR.T().H1.bind();
                                UI.FONT().S.renderCY(r, x1, body.cY(), row.str);
                            }
                            else
                            {
                                GCOLOR.T().H2.bind();
                                UI.FONT().S.renderCY(r, x1, body.cY(), row.name);
                                COLOR.unbind();
                                UI.FONT().S.renderCY(r, x1 + 400, body.cY(), row.str);
                            }
                            COLOR.unbind();
                        }
                    };
                }
            });

            add(bu.createHeight(height - 8 - sp.body().height(), true));

            addRelBody(8, DIR.N, sp);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
            debugger.clear();
            fill(debugger);
            filtered.clearSoft();

            if (sp.text() == null || sp.text().Length == 0)
            {
                filtered.add(debugger.all);
            }
            else
            {
                foreach (Row rr in debugger.all)
                {
                    if (sp.text() == null || sp.text().Length == 0)
                    {
                        filtered.add(rr);
                    }
                    else if (rr.name != null && Str.containsText(rr.name, sp.text()))
                    {
                        filtered.add(rr);
                    }
                    else if (Str.containsText(rr.str, sp.text()))
                    {
                        filtered.add(rr);
                    }
                }
            }
        }

        protected abstract void fill(Debugger d);
    }
}