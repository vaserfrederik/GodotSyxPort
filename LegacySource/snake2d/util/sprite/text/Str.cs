using System;
using System.IO;
using System.Text;

namespace Snake2D.Util.Sprite.Text
{
    public class Str : ICharSequence
    {
        public static readonly Str TMP = new Str(64);
        public static readonly Str TMP2 = new Str(64);
        private static readonly Str TMP3 = new Str(64);

        protected char[] chars;
        protected int last = 0;
        private static readonly StringBuilder builder = new StringBuilder(1024);
        private static readonly string boolT = "true";
        private static readonly string boolF = "false";

        public Str(int size)
        {
            chars = new char[size];
        }

        public Str(ICharSequence s)
        {
            chars = new char[s.Length()];
            Add(s);
        }

        protected Str()
        {
        }

        public char CharAt(int index)
        {
            if (index >= last)
                throw new IndexOutOfRangeException(index.ToString());
            return chars[index];
        }

        public Str SetMaxChars(int max)
        {
            if (last > max)
            {
                last = max;
                chars[max - 1] = '.';
                chars[max - 2] = '.';
            }
            return this;
        }

        public int Length()
        {
            return last;
        }

        public ICharSequence SubSequence(int start, int end)
        {
            return new string(chars, start, end - start);
        }

        public Str Add(ICharSequence str)
        {
            Add(str, str.Length());
            return this;
        }

        public Str Add(ICharSequence str, int length)
        {
            if (str.Length() == 0)
                return this;

            if (length > str.Length())
                length = str.Length();

            if (last + length > chars.Length)
                Resize(last + length);

            int i = 0;
            while (i < length)
            {
                chars[last++] = str.CharAt(i++);
            }
            return this;
        }

        public Str Add(ICharSequence str, int start, int end)
        {
            if (str.Length() == 0)
                return this;

            if (last + end - start > chars.Length)
                Resize(last + str.Length());

            int i = start;
            if (end > str.Length())
                end = str.Length();
            while (i < end)
            {
                chars[last++] = str.CharAt(i++);
            }
            return this;
        }

        public Str Add(long i)
        {
            Add(i, false);
            return this;
        }

        public Str Add(long i, bool format)
        {
            if (i < 0)
            {
                Add('-');
                i = -i;
            }

            if (i == 0)
                return Add('0');

            builder.Clear();

            int ii = 0;

            while (i > 0)
            {
                builder.Append((char)('0' + i % 10));
                i /= 10;
                if (format && ii++ >= 2 && i > 0)
                {
                    builder.Append('.');
                    ii = 0;
                }
            }

            builder.Reverse();

            for (int j = 0; j < builder.Length; j++)
            {
                chars[last++] = builder[j];
            }

            return this;
        }

        public Str Add(double v, int decimals)
        {
            builder.Clear();
            builder.Append(v.ToString($"F{decimals}"));
            for (int j = 0; j < builder.Length; j++)
            {
                chars[last++] = builder[j];
            }
            return this;
        }

        public Str Insert(int index, char v)
        {
            TMP3.Clear().Add(v);
            return Insert(index, TMP3);
        }

        public Str Insert(int index, int v)
        {
            TMP3.Clear().Add(v);
            return Insert(index, TMP3);
        }

        public Str Insert(int index, double v, int decimals)
        {
            TMP3.Clear().Add(v, decimals);
            return Insert(index, TMP3);
        }

        public Str InsertD(int index, double v, int maxDecimals)
        {
            double vv = v;
            for (int i = 0; i < maxDecimals; i++)
            {
                if ((int)vv == vv)
                    return Insert(index, v, i);
                vv *= 10;
            }
            return Insert(index, v, maxDecimals);
        }

        public Str Insert(int index, ICharSequence v)
        {
            builder.Clear();
            for (int i = 0; i < Length(); i++)
            {
                if (i == index)
                {
                    builder.Append(v.ToString());
                    i += v.Length();
                }
                else
                {
                    builder.Append(CharAt(i));
                }
            }

            Clear();
            Add(builder.ToString());
            return this;
        }

        private Str Insert(ICharSequence v)
        {
            builder.Clear();
            for (int i = 0; i < Length(); i++)
            {
                if (Matches(this, TMPMATCH, i))
                {
                    builder.Append(v.ToString());
                    i += TMPMATCH.Length();
                    for (; i < Length(); i++)
                        builder.Append(CharAt(i));
                    Clear();
                    Add(builder.ToString());
                    return this;
                }
                else
                {
                    builder.Append(CharAt(i));
                }
            }
            return this;
        }

        private static bool Matches(ICharSequence s, ICharSequence n, int i)
        {
            if (i + n.Length() > s.Length())
                return false;

            char c = s.CharAt(i);
            if (n.CharAt(0) == c)
            {
                for (int k = 0; k < n.Length(); k++)
                {
                    int ki = k + i;
                    if (n.CharAt(k) != s.CharAt(ki))
                        return false;
                }
                return true;
            }
            return false;
        }

        private static bool MatchesText(ICharSequence s, ICharSequence n, int i)
        {
            if (i + n.Length() > s.Length())
                return false;
            if (i >= s.Length())
                return false;
            char c = char.ToLower(s.CharAt(i));
            if (char.ToLower(n.CharAt(0)) == c)
            {
                for (int k = 0; k < n.Length(); k++)
                {
                    int ki = k + i;
                    if (char.ToLower(n.CharAt(k)) != char.ToLower(s.CharAt(ki)))
                        return false;
                }
                return true;
            }
            return false;
        }

        public Str ToUpper()
        {
            for (int i = 0; i < last; i++)
            {
                if (chars[i] >= 'a' && chars[i] <= 'z')
                {
                    chars[i] -= 32;
                }
            }
            return this;
        }

        public Str ToLower()
        {
            for (int i = 0; i < last; i++)
            {
                if (chars[i] >= 'A' && chars[i] <= 'Z')
                {
                    chars[i] += 32;
                }
            }
            return this;
        }

        public bool StartsWithIgnoreCase(ICharSequence other)
        {
            return StartsWithIgnoreCase(this, other);
        }

        public static bool StartsWithIgnoreCase(ICharSequence a, ICharSequence b)
        {
            if (a.Length() == 0)
                return true;
            if (b.Length() == 0)
                return true;
            if (b.Length() > a.Length())
                return false;
            for (int i = 0; i < b.Length(); i++)
            {
                if (char.ToLower(b.CharAt(i)) != char.ToLower(a.CharAt(i)))
                    return false;
            }
            return true;
        }

        public void Save(FilePutter f)
        {
            f.WriteInt(last);
            for (int i = 0; i < last; i++)
            {
                f.WriteInt(chars[i]);
            }
        }

        public void Load(FileGetter f) throws IOException
        {
            last = f.ReadInt();
            if (last > chars.Length)
            {
                chars = new char[last];
            }

            for (int i = 0; i < last; i++)
            {
                chars[i] = (char)f.ReadInt();
            }
        }

        public Str Trim()
        {
            char[] chars = new char[Length()];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = this.chars[i];
            this.chars = chars;
            return this;
        }

        public Str NL()
        {
            Add(Font.nl);
            return this;
        }

        public Str TAB()
        {
            Add(Font.tab);
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is ICharSequence cc)
            {
                if (cc.Length() != Length())
                    return false;
                for (int i = 0; i < cc.Length(); i++)
                {
                    if (cc.CharAt(i) != CharAt(i))
                        return false;
                }
                return true;
            }
            return false;
        }

        public bool ContainsText(ICharSequence text)
        {
            for (int i = 0; i < Length() - text.Length() + 1; i++)
                if (MatchesText(this, text, i))
                    return true;
            return false;
        }

        public static bool ContainsText(ICharSequence a, ICharSequence text)
        {
            if (text.Length() == 0 && a != null)
                return true;

            for (int i = 0; i < a.Length() - text.Length() + 1; i++)
                if (MatchesText(a, text, i))
                    return true;
            return false;
        }

        public static bool IsSame(ICharSequence tName, ICharSequence tName2)
        {
            if (tName.Length() != tName2.Length())
            {
                return false;
            }
            for (int i = 0; i < tName.Length(); i++)
            {
                if (tName.CharAt(i) != tName2.CharAt(i))
                    return false;
            }
            return true;
        }

        private void Resize(int newSize)
        {
            Array.Resize(ref chars, newSize);
        }
    }

    public interface ICharSequence
    {
        int Length();
        char CharAt(int index);
    }

    public static class Font
    {
        public static readonly char nl = '\n';
        public static readonly char tab = '\t';
    }

    [Serializable]
    public class StringReusableSer : Str, ISerializable
    {
        public StringReusableSer(int size) : base(size)
        {
        }

        protected StringReusableSer(SerializationInfo info, StreamingContext context)
        {
            chars = (char[])info.GetValue("chars", typeof(char[]));
            last = info.GetInt32("last");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("chars", chars);
            info.AddValue("last", last);
        }
    }
}