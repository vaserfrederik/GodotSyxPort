using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using snake2d;
using snake2d.util.bit;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.text;

namespace settlement.stats.colls
{
    public sealed class StatsAppearance : StatCollection
    {
        public const int NAME_MAX = 0x0000_0FFF;

        public readonly INT_OE<Induvidual> nameData;
        public readonly NamePart nameFirst = new NamePart(true);
        public readonly NamePart nameLast = new NamePart(false);

        public readonly INT_OE<Induvidual> gender;
        public readonly LIST<INT_OE<Induvidual>> all;
        public readonly INT_OE<Induvidual> favo;

        public readonly STAT dead;

        public StatsAppearance(StatsInit init) : base(init, "APPEARANCE", "", "")
        {
            nameData = init.count.new DataInt("APPEARENCE_NAME");
            gender = init.count.new DataNibble("APPEARENCE_GENDER");

            init.savers.put("APPEARENCE_SCRAP", new SAVABLE
            {
                Save = file =>
                {
                    nameFirst.Save(file);
                    nameLast.Save(file);
                },
                Load = file =>
                {
                    nameFirst.Load(file);
                    nameLast.Load(file);
                },
                Clear = () =>
                {
                    nameFirst.Clear();
                    nameLast.Clear();
                }
            });

            dead = new STATInduOnly("DEAD", init, init.count.new DataBit("APPEARENCE_DEAD"));

            LinkedList<INT_OE<Induvidual>> tt = new LinkedList<INT_OE<Induvidual>>();
            tt.Add(nameData);
            tt.Add(gender);
            favo = init.count.new DataBit("FFAVOURITE");
            tt.Add(favo);

            all = new ArrayList<INT_OE<Induvidual>>(tt);

            init.copier.Add(all);

            init.onConstruct.Add(new StatInitable
            {
                Init = h =>
                {
                    double ri = RND.rFloat(h.race().appearance().tMax);
                    int gi = 0;
                    foreach (RType t in h.race().appearance().types)
                    {
                        ri -= t.spec.occurrence;
                        if (ri <= 0)
                        {
                            gi = CLAMP.i(gi, 0, h.race().appearance().types.size() - 1);
                            gender.Set(h, gi);
                            break;
                        }
                        gi++;
                    }
                    nameFirst.Randmoize(h);
                    nameLast.Randmoize(h);
                }
            });
        }

        private RType Get(Induvidual i)
        {
            if (i.hType() == HTYPES.CHILD())
                return i.race().appearance().child;
            return i.race().appearance().types.Get(gender.Get(i));
        }

        public COLOR ColorSkin(Race race, int gender, int nameData)
        {
            return race.appearance().types.Get(gender).names.Get(nameData);
        }

        public CharSequence Name(Induvidual i)
        {
            return Name(i.race(), i.hType(), gender.Get(i), nameData.Get(i));
        }

        public CharSequence Name(Race r, HTYPE t, int gender, int nameData)
        {
            ss.Clear();
            ss.Add(nameFirst.Name(r, t, gender, nameData));
            ss.S();
            ss.Add(nameLast.Name(r, t, gender, nameData));
            return ss;
        }

        public void SetCustomName(Induvidual i, string name)
        {
            CharSequence first = Dic.empty;
            CharSequence last = Dic.empty;
            if (name.IndexOf(' ') > 0)
            {
                first = name.Substring(0, name.IndexOf(' '));
                last = name.Substring(name.IndexOf(' ') + 1);
            }
            else
            {
                first = name;
                last = Dic.empty;
            }
            nameFirst.SetCustom(i, first);
            nameLast.SetCustom(i, last);
        }

        public void PortraitRender(SPRITE_RENDERER r, Induvidual a, int x, int y, int scale)
        {
            if (a.hType() == HTYPES.CHILD() || a.hType() == HTYPES.CHILD_SLAVE())
            {
                a.race().appearance().child.portrait.Render(r, x, y, a, scale);
            }
            else
            {
                a.race().appearance().types.Get(gender.Get(a)).portrait.Render(r, x, y, a, scale);
            }
        }

        public class NamePart
        {
            private readonly Bits bData;
            private readonly Bit bC;
            private readonly ArrayList<Str> allNames = new ArrayList<Str>(NAME_MAX + 1);
            private int kk = 1;
            private readonly Str ss = new Str(1024);
            private bool first;

            public NamePart(bool first)
            {
                int scroll = first ? 0 : 1;
                bData = new Bits(NAME_MAX << (16 * scroll));
                bC = new Bit(0x0000_1000 << (16 * scroll));
                allNames.Add(new Str(1));
                this.first = first;
            }

            public CharSequence Name(Race r, HTYPE t, int gender, int nameData)
            {
                if (bC.Is(nameData))
                {
                    return allNames.Get(bData.Get(nameData));
                }
                ss.Clear();
                if (first)
                {
                    ss.Add(r.appearance().types.Get(gender).names.firstNames.Get(bData.Get(nameData)));
                }
                else
                {
                    ss.Add(r.appearance().types.Get(gender).names.lastNames.Get(bData.Get(nameData)));
                }
                return ss;
            }

            public CharSequence Name(Induvidual i)
            {
                return Name(i.race(), i.hType(), gender.Get(i), nameData.Get(i));
            }

            public void Randmoize(Induvidual i)
            {
                int g = gender.Get(i);
                int max = 0;
                if (first)
                {
                    max = i.race().appearance().types.Get(g).names.firstNames.size();
                }
                else
                {
                    max = i.race().appearance().types.Get(g).names.lastNames.size();
                }
                int data = nameData.Get(i);
                data = bC.Set(data, false);
                data = bData.Set(data, RND.rInt(max));
                nameData.Set(i, data);
            }

            public void Copy(Induvidual i, int nameDataToCopy)
            {
                if (bC.Is(nameDataToCopy))
                {
                    CharSequence nn = allNames.Get(bData.Get(nameDataToCopy));
                    SetCustom(i, nn);
                }
                else
                {
                    int data = bData.Set(nameData.Get(i), bData.Get(nameDataToCopy));
                    nameData.Set(i, data);
                }
            }

            public void SetCustom(Induvidual i, CharSequence cc)
            {
                if (cc.Length == 0)
                {
                    int data = nameData.Get(i);
                    data = bData.Set(data, 0);
                    data = bC.Set(data, true);
                    nameData.Set(i, data);
                    return;
                }

                if (kk >= allNames.Max())
                    kk = 1;
                while (kk >= allNames.Size())
                    allNames.Add(new Str(32));

                allNames.Get(kk).Clear().Add(cc);

                int data = nameData.Get(i);
                data = bData.Set(data, kk);
                data = bC.Set(data, true);
                nameData.Set(i, data);
                kk++;
            }

            public void Save(FilePutter file)
            {
                file.i(kk);
                file.i(allNames.Size());
                for (int i = 0; i < allNames.Size(); i++)
                    allNames.Get(i).Save(file);
            }

            public void Load(FileGetter file)
            {
                kk = file.i();
                if (kk == 0)
                    kk = 1;
                int am = file.i();
                allNames.ClearSloppy();
                for (int i = 0; i < am; i++)
                {
                    allNames.Add(new Str(32));
                    allNames.Get(allNames.Size() - 1).Load(file);
                }
            }

            public void Clear()
            {
                kk = 1;
            }
        }
    }
}