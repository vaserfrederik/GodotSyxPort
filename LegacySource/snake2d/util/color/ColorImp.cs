using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Snake2D.Util.Color
{
    [Serializable]
    public class ColorImp : COLOR, ISerializable, SAVABLE
    {
        public static readonly ColorImp TMP = new ColorImp();

        private static readonly TextureCoords texture = new TextureCoords();
        private static short width;
        private static short height;

        public static void SetSPRITE(int wX1, int wY1, int w, int h)
        {
            texture.Get(wX1, wY1, w, h);
            width = (short)w;
            height = (short)h;
        }

        private byte red;
        private byte green;
        private byte blue;

        public ColorImp()
            : this(127, 127, 127)
        {
        }

        public ColorImp(int red, int green, int blue)
        {
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
        }

        public ColorImp(Json json)
            : this(json, "COLOR")
        {
        }

        public static LIST<ColorImp> Cols(Json json)
        {
            return Cols(json, "COLOR");
        }

        public static LIST<ColorImp> Cols(Json json, string key)
        {
            if (!json.Has(key))
                throw new RuntimeException();
            if (json.JsonIs(key))
            {
                json = json.Json(key);
                if (json.Has("R") && json.Has("B") && json.Has("G"))
                    return new ArrayList(new ColorImp[] { new ColorImp(json.Get<int>("R"), json.Get<int>("G"), json.Get<int>("B")) });
                else
                {
                    LIST<ColorImp> colors = new ArrayList();
                    int generateCount = json.Get<int>("GenerateCount");
                    for (int i = 0; i < generateCount; i++)
                    {
                        colors.Add(new ColorImp(RND.rInt(255), RND.rInt(255), RND.rInt(255)));
                    }
                    return colors;
                }
            }
            else
            {
                LIST<ColorImp> colors = new ArrayList();
                int count = json.Get<int>("Count");
                for (int i = 0; i < count; i++)
                {
                    colors.Add(new ColorImp(json.Get<int>($"Color{i}")));
                }
                return colors;
            }
        }

        public ColorImp(this.Json json, string key)
        {
            Set(json.Get<int>(key));
        }

        public override void Bind()
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Free()
        {
            throw new NotImplementedException();
        }

        public override void Get(object data)
        {
            throw new NotImplementedException();
        }

        public override void Give(object data)
        {
            throw new NotImplementedException();
        }

        public override void Load(FileGetter file)
        {
            int i = file.i();
            red = (byte)(i & 0x0FF);
            green = (byte)((i >> 8) & 0x0FF);
            blue = (byte)((i >> 16) & 0x0FF);
        }

        public override void Randomize(double d)
        {
            red = (byte)CLAMP.i((int)(red + RND.rFloat() * d * 255), 0, 255);
            green = (byte)CLAMP.i((int)(green + RND.rFloat() * d * 255), 0, 255);
            blue = (byte)CLAMP.i((int)(blue + RND.rFloat() * d * 255), 0, 255);
        }

        public override void Save(FilePutter file)
        {
            file.i((red & 0x0FF) | ((green << 8) & 0x0FF00) | ((blue << 16) & 0x0FF0000));
        }

        public override void SetBrightnessSelf(double shade)
        {
            int hi = red & 0x0FF;
            hi = Math.Max(hi, green & 0x0FF);
            hi = Math.Max(hi, blue & 0x0FF);

            int sh = (int)(shade * 127);
            sh = CLAMP.i(sh, 0, 255);

            double d = 1.0 + (sh - hi) / 255.0;

            SetRed(CLAMP.i((int)((red & 0x0FF) * d), 0, 255));
            SetGreen(CLAMP.i((int)((green & 0x0FF) * d), 0, 255));
            SetBlue(CLAMP.i((int)((blue & 0x0FF) * d), 0, 255));
        }

        public override void SetMinBrightnessSelf(double shade)
        {
            SetBrightnessSelf(shade);

            int r = red & 0x0FF;
            int g = green & 0x0FF;
            int b = blue & 0x0FF;

            double tot = r + g + b;
            tot /= 127 * 3;

            if (tot >= shade)
                return;

            shade -= tot;
            shade *= 127 * 3;

            if (r < 127)
            {
                int am = (int)(shade / 3);
                am = (int)CLAMP.d(am, 0, 127 - r);
                r += am;
                shade -= am;
            }

            if (g < 127)
            {
                int am = (int)(shade / 2);
                am = (int)CLAMP.d(am, 0, 127 - g);
                g += am;
                shade -= am;
            }

            int am = (int)(shade);
            am = (int)CLAMP.d(am, 0, 127 - b);
            b += am;

            Set(r, g, b);

            SetRed(CLAMP.i(r, 0, 255));
            SetGreen(CLAMP.i(g, 0, 255));
            SetBlue(CLAMP.i(b, 0, 255));
        }

        public override void SetSaturationSelf(double amount)
        {
            double r = (red & 0x0FF);
            double g = (green & 0x0FF);
            double b = (blue & 0x0FF);

            double min = 255.0;
            double max = 0;

            if (r < min) min = r;
            if (r > max) max = r;
            if (g < min) min = g;
            if (g > max) max = g;
            if (b < min) min = b;
            if (b > max) max = b;

            double lum = (min + max) / 2.0;

            int red = (int)(lum + (r - lum) * amount);
            int green = (int)(lum + (g - lum) * amount);
            int blue = (int)(lum + (b - lum) * amount);
            Set(red, green, blue);
        }

        public override void SetShadeSelf(double shade)
        {
            Set((int)((red & 0x0FF) * shade), (int)((green & 0x0FF) * shade), (int)((blue & 0x0FF) * shade));
        }

        public override void SetTexture(TextureCoords texture)
        {
            throw new NotImplementedException();
        }

        public override void SetTexture(string texturePath)
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((red & 0x0FF) | ((green << 8) & 0x0FF00) | ((blue << 16) & 0x0FF0000));
        }

        public override void Read(BinaryReader reader)
        {
            int i = reader.ReadInt32();
            red = (byte)(i & 0x0FF);
            green = (byte)((i >> 8) & 0x0FF);
            blue = (byte)((i >> 16) & 0x0FF);
        }

        public override void WriteObject(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("red", red);
            info.AddValue("green", green);
            info.AddValue("blue", blue);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            red = info.GetByte("red");
            green = info.GetByte("green");
            blue = info.GetByte("blue");
        }

        private static readonly double ii = 1.0 / 0x0FF;

        public override double R()
        {
            return (red & 0x0FF) * ii;
        }

        public override double G()
        {
            return (green & 0x0FF) * ii;
        }

        public override double B()
        {
            return (blue & 0x0FF) * ii;
        }
    }
}