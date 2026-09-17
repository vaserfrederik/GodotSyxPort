using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Settlement.Room.Main.Copy
{
    public class SavedPrints
    {
        private readonly List<List<SavedPrint>> all;
        private static readonly string fileName = "SavedPrints71";

        public SavedPrints(ROOMS rooms)
        {
            all = new List<List<SavedPrint>>(rooms.AMOUNT_OF_BLUEPRINTS);
            for (int i = 0; i < rooms.AMOUNT_OF_BLUEPRINTS; i++)
            {
                all.Add(new List<SavedPrint>());
            }

            try
            {
                string filePath = Path.Combine(PATHS.Local().PROFILE.Get(fileName), "BLUEPRINTS.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    List<JsonElement> jsons = JsonConvert.DeserializeObject<List<JsonElement>>(json);

                    foreach (var j in jsons)
                    {
                        SavedPrint p = new SavedPrint(rooms, j);
                        if (p.Blue != null && p.Blue.Constructor() != null && p.Blue.Constructor().CanBeCopied() && p.Check.Equals(SavedPrint.Check(p.Blue)))
                        {
                            Add(p);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                Save();
            }
        }

        public void Save()
        {
            List<JsonElement> allJsons = new List<JsonElement>();

            foreach (var ll in all)
            {
                foreach (var l in ll)
                {
                    allJsons.Add(l.Save());
                }
            }

            string json = JsonConvert.SerializeObject(allJsons, Formatting.Indented);
            string filePath = Path.Combine(PATHS.Local().PROFILE.Get(fileName), "BLUEPRINTS.json");

            try
            {
                if (!Directory.Exists(PATHS.Local().PROFILE.Get(fileName)))
                {
                    Directory.CreateDirectory(PATHS.Local().PROFILE.Get(fileName));
                }

                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                e.printStackTrace(Console.Out);
                all.Clear();
            }
        }

        public SavedPrint Push(Room ins, int mx, int my)
        {
            if (CanAdd(ins))
            {
                SavedPrint p = new SavedPrint(ins, mx, my);
                Add(p);
                Save();
                return p;
            }
            return null;
        }

        public void Remove(SavedPrint print)
        {
            all[print.Blue.Index()].Remove(print);
            Save();
        }

        private void Add(SavedPrint p)
        {
            all[p.Blue.Index()].Add(p);
        }

        public List<SavedPrint> All(RoomBlueprint p)
        {
            return all[p.Index()];
        }

        public bool CanAdd(Room ins)
        {
            if (ins.Constructor() == null)
            {
                return false;
            }
            return CanAdd(ins.Constructor().Blue());
        }

        public bool CanAdd(RoomBlueprint b)
        {
            if (b is RoomBlueprintIns)
            {
                RoomBlueprintImp bb = (RoomBlueprintImp)b;
                if (bb.Constructor() == null)
                {
                    return false;
                }
                if (!bb.Constructor().UsesArea())
                {
                    return false;
                }
                if (!bb.Constructor().CanBeCopied())
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public class SavedPrint
        {
            public string Name;
            public readonly RoomBlueprintImp Blue;
            public readonly string Check;
            public readonly int Width, Height;
            public readonly Structure Structure;
            private readonly int[] Data;
            private static readonly RoomWrap Wrap = new RoomWrap();

            public SavedPrint(ROOMS rr, JsonElement json)
            {
                Name = json["NAME"].ToString();
                RoomBlueprint pp = rr.Collection.TryGet(json["ROOM"].GetInt32());
                Blue = pp is RoomBlueprintImp ? (RoomBlueprintImp)pp : null;
                Check = json["CHECK"]?.GetString() ?? "";
                Width = json["WIDTH"].GetInt32();
                Height = json["HEIGHT"].GetInt32();
                Data = json["DATA"].GetInt32Array();
                Structure = json["STRUCTURE"]?.GetString() == "_" ? null : STRUCTURES.Map().ReadTry("STRUCTURE", json);
            }

            public JsonElement Save()
            {
                JsonElement j = new JsonElement();
                j.Add("NAME", Name);
                j.Add("ROOM", Blue.Key);
                j.Add("STRUCTURE", Structure == null ? "_" : Structure.Key);
                j.Add("CHECK", Check);
                j.Add("WIDTH", Width);
                j.Add("HEIGHT", Height);
                j.Add("DATA", Data);

                return j;
            }

            private static string Check(RoomBlueprintImp blue)
            {
                string ch = "";
                Furnisher f = blue.Constructor();
                ch += f.Groups().Count;
                foreach (FurnisherItemGroup g in f.Groups())
                {
                    ch += g.Min;
                    ch += g.Max;
                    ch += g.Rotations();
                    ch += g.Size();

                    for (int i = 0; i < g.Size(); i++)
                    {
                        FurnisherItem it = g.Item(i, 0);
                        ch += it.Width();
                        ch += it.Height();
                        for (int h = 0; h < it.Height(); h++)
                        {
                            for (int w = 0; w < it.Width(); w++)
                            {
                                ch += h + " " + w + (it.Get(h, w) == null ? "n" : it.Get(h, w).Availability.Player);
                            }
                        }
                    }
                }
                return ch;
            }

            public SavedPrint(string name, SavedPrint other)
            {
                Name = name;
                Blue = other.Blue;
                Check = other.Check;
                Width = other.Width;
                Height = other.Height;
                Data = other.Data;
                Structure = other.Structure;
            }

            public SavedPrint(Room ins, int mx, int my)
            {
                Wrap.Init(ins, mx, my);
                Name = ins.Name(mx, my).ToString();
                Blue = ins.Constructor().Blue();
                Check = Check(Blue);
                Width = Wrap.Body().Width() + 2;
                Height = Wrap.Body().Height() + 2;
                Data = Alloc.Ii(Width * Height);
                TBuilding b = ConstructionInit.FindStructure(mx, my);
                Structure = b == null ? null : b.Structure;

                for (int dy = 0; dy < Height; dy++)
                {
                    for (int dx = 0; dx < Width; dx++)
                    {
                        int di = dx + dy * Width;
                        int x = dx + Wrap.Body().X1() - 1;
                        int y = dy + Wrap.Body().Y1() - 1;
                        if (Wrap.Is(x, y))
                        {
                            this.Data[di] |= 0b1;
                            if (SETT.Terrain().Get(x, y).RoofIs())
                                this.Data[di] |= 0b100;
                            if (SETT.Path().Availability.Get(x, y) != AVAILABILITY.Room)
                            {
                                this.Data[di] |= 0b01000;
                            }
                            if (SETT.Rooms().FData.IsMaster.Is(x, y))
                            {
                                FurnisherItem it = SETT.Rooms().FData.Item.Get(x, y);
                                this.Data[(dx) + (dy) * Width] |= (it.Index() + 1) << 4;
                            }
                        }
                        else
                        {
                            bool is = false;
                            foreach (DIR d in DIR.ALL)
                            {
                                if (Wrap.Is(x, y, d))
                                {
                                    is = true;
                                    break;
                                }
                            }
                            if (is)
                            {
                                if (UtilWallPlacability.OpeningIsReal.Is(x, y))
                                    this.Data[di] |= 0b100;
                                else if (UtilWallPlacability.WallisReal.Is(x, y))
                                    this.Data[di] |= 0b010;
                            }
                        }
                    }
                }
            }

            public bool IsRoom(int rx, int ry)
            {
                return (Data[rx + ry * Width] & 1) != 0;
            }

            public bool IsWall(int rx, int ry)
            {
                return (Data[rx + ry * Width] & 0b10) != 0;
            }

            public bool IsRoof(int rx, int ry)
            {
                return (Data[rx + ry * Width] & 0b100) != 0;
            }

            public bool IsSolid(int rx, int ry)
            {
                return (Data[rx + ry * Width] & 0b1000) != 0;
            }

            public FurnisherItem Item(int rx, int ry, RoomBlueprintImp blue)
            {
                int i = (Data[rx + ry * Width] & 0x0FFFF0) >> 4;
                if (i > 0)
                {
                    return blue.Constructor().Item(i - 1);
                }
                return null;
            }
        }
    }
}