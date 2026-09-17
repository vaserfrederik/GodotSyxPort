using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake2D.Util.File
{
    abstract class JsonValue
    {
        public readonly string Key;
        public readonly int Line;
        public readonly string ErrorPath;

        private JsonValue(string key, int line, string errorPath)
        {
            Key = key;
            Line = line;
            ErrorPath = errorPath;
        }

        public virtual string Text()
        {
            return null;
        }

        public virtual string[] Texts()
        {
            return null;
        }

        public virtual string Value()
        {
            return null;
        }

        public virtual string[] Values()
        {
            return null;
        }

        public virtual string[][] Values2()
        {
            return null;
        }

        public virtual JsonValueJson Json()
        {
            return null;
        }

        public virtual JsonValueJson[] Jsons()
        {
            return null;
        }

        protected void ThrowError(string error)
        {
            string m = $"Error parsing line: {Line}. key: {Key} {error}";
            throw new Errors.DataError(m, ErrorPath);
        }

        public abstract string ToString(int indent);

        public abstract void Overwrite(JsonValue v, bool arrayAdd, bool jsonAdd);

        public class JsonValueString : JsonValue
        {
            public string Data;

            public JsonValueString(string key, int line, string data, string errorPath) : base(key, line, errorPath)
            {
                Data = data.Replace("[\\n\\r]+", "\n").Replace("[\\t]+", "").Replace("%r%", "\n");
            }

            public override string Text()
            {
                return Data;
            }

            public override void Overwrite(JsonValue v, bool arrayAdd, bool jsonAdd)
            {
                if (v is JsonValueString vv)
                {
                    Data = vv.Data;
                }
                else
                {
                    v.ThrowError("Entry is of another type");
                }
            }

            public override string ToString(int indent)
            {
                string s = "";
                for (int i = 0; i < indent; i++)
                    s += "\t";
                if (Key != null)
                    s += $"{Key}: \"{Data}\",\n";
                else
                    s += $"\"{Data}\",\n";
                return s;
            }
        }

        public class JsonValueArray : JsonValue
        {
            public List<JsonValue> Datas = new List<JsonValue>();

            public JsonValueArray(string key, int line, string errorPath) : base(key, line, errorPath)
            {
            }

            public override string ToString(int indent)
            {
                string s = "";
                for (int i = 0; i < indent; i++)
                    s += "\t";
                s += "[\n";
                foreach (var item in Datas)
                {
                    s += item.ToString(indent + 1);
                }
                for (int i = 0; i < indent; i++)
                    s += "\t";
                s += "],\n";
                return s;
            }

            public override void Overwrite(JsonValue v, bool arrayAdd, bool jsonAdd)
            {
                if (v is JsonValueArray vv)
                {
                    if (arrayAdd)
                    {
                        Datas.AddRange(vv.Datas);
                    }
                    else
                    {
                        Datas = vv.Datas;
                    }
                }
                else
                {
                    v.ThrowError("Entry is not a json array");
                }
            }
        }

        public class JsonValueJson : JsonValue
        {
            public Dictionary<string, JsonValue> Map = new Dictionary<string, JsonValue>();
            public List<string> Keys = new List<string>();

            public JsonValueJson(string key, int line, string errorPath, string content) : base(key, line, errorPath)
            {
                var reader = new Reader(content, this);
                while (reader.NextChar())
                {
                    string k = reader.GetKey();
                    var v = reader.GetValue(k);
                    if (Map.ContainsKey(k))
                        ThrowError("Duplicate key: " + k);
                    Map[k] = v;
                    Keys.Add(k);
                }
            }

            public override JsonValueJson Json()
            {
                return this;
            }

            public override void Overwrite(JsonValue v, bool arrayAdd, bool jsonAdd)
            {
                if (v is JsonValueJson vv)
                {
                    foreach (var jv in vv.Map.Values)
                    {
                        if (Map.ContainsKey(jv.Key))
                        {
                            if (jsonAdd)
                                Map[jv.Key].Overwrite(jv, arrayAdd, jsonAdd);
                            else
                                Map[jv.Key] = jv;
                        }
                        else
                        {
                            Map[jv.Key] = jv;
                            Keys.Add(jv.Key);
                        }
                    }
                }
                else
                {
                    v.ThrowError("Entry is not a json object");
                }
            }

            public void ThrowError(string error, string key)
            {
                JsonValue v = Map[key];
                if (v != null)
                {
                    string m = $"Error parsing line: {v.Line}, key: {key}. {error}";
                    throw new Errors.DataError(m, ErrorPath);
                }
                else
                {
                    throw new Errors.DataError($"{error}. Error parsing key {key}", ErrorPath);
                }
            }

            public string GetError(string error, string key)
            {
                JsonValue v = Map[key];
                if (v != null)
                {
                    return $"{error} Key: {key} line: {v.Line} {ErrorPath}";
                }
                else
                {
                    return $"{error}. Error parsing key {key} {ErrorPath}";
                }
            }

            public override string ToString()
            {
                return ToString(Key == null ? -1 : 0);
            }

            public override string ToString(int indent)
            {
                string s = "";
                for (int i = 0; i < indent; i++)
                    s += "\t";
                if (indent >= 0)
                {
                    if (Key != null)
                        s += $"{Key}: ";
                    s += "{\n";
                }
                foreach (var k in Keys)
                {
                    s += Map[k].ToString(indent + 1);
                }
                for (int i = 0; i < indent; i++)
                    s += "\t";
                if (indent >= 0)
                    s += "},\n";
                return s;
            }
        }

        private class Reader
        {
            private readonly JsonValue Abs;
            private readonly string Content;
            private int LineCurrent = 0;
            private int I = 0;

            public Reader(string content, JsonValue abs)
            {
                Content = content;
                Abs = abs;
                LineCurrent = abs.Line;
            }

            public bool IsNewline()
            {
                char c = Content[I];

                if (c == '\r' && I < Content.Length - 2 && Content[I + 1] == '\n')
                {
                    LineCurrent++;
                    I += 2;
                    return true;
                }
                else if (c == '\r')
                {
                    LineCurrent++;
                    I += 1;
                    return true;
                }
                else if (c == '\n')
                {
                    LineCurrent++;
                    I += 1;
                    return true;
                }
                return false;
            }

            public bool NextChar()
            {
                bool comment = false;
                while (true)
                {
                    if (I >= Content.Length - 1)
                        return false;

                    if (Content[I] == '*' && I < Content.Length - 1 && Content[I + 1] == '*')
                    {
                        comment = true;
                        I++;
                        continue;
                    }

                    if (IsNewline())
                    {
                        comment = false;
                        continue;
                    }

                    char c = Content[I];

                    if (comment)
                    {
                        I++;
                    }
                    else if (c == ' ' || c == '\t')
                    {
                        I++;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            public string GetKey()
            {
                int i2 = I;
                int l = LineCurrent;
                while (Content[i2] != ':')
                {
                    if (i2 >= Content.Length - 1)
                        ThrowError("Expecting a keyword followed by a ':' after line: " + l);
                    i2++;
                }
                string key = Content.Substring(I, i2 - I).Trim();
                I = i2 + 1;
                NextChar();
                return key;
            }

            public JsonValue GetValue(string key)
            {
                char c = Content[I];
                int l = LineCurrent;
                if (c == '\"')
                {
                    string s = FindValue('\"', '\"');
                    return new JsonValueString(key, l, s, Abs.ErrorPath);
                }
                else if (c == '{')
                {
                    string s = FindValue('{', '}');
                    return new JsonValueJson(key, l, Abs.ErrorPath, s);
                }
                else if (c == '[')
                {
                    string s = FindValue('[', ']');
                    JsonValueArray a = new JsonValueArray(key, l, Abs.ErrorPath);
                    a.Datas.Add(new JsonValue.JsonValueValue(null, l, s, Abs.ErrorPath));
                    return a;
                }
                else
                {
                    int start = I;
                    while (++I < Content.Length)
                    {
                        if (IsNewline())
                            ThrowError("Expecting: ','");
                        if (Content[I] == ',')
                        {
                            I++;
                            string s = Content.Substring(start, I - 1);
                            return new JsonValue.JsonValueValue(key, l, s, Abs.ErrorPath);
                        }
                    }
                    int i2 = I - 10 >= 0 ? I - 10 : 0;
                    string after = i2 < I - 1 ? Content.Substring(i2, I - 1) : " ";
                    ThrowError("Expecting: ',' after: '" + after + "'." + " | " + Content);
                    return null;
                }
            }

            public string FindValue(char open, char close)
            {
                int nesting = 0;
                int start = I;
                int l = LineCurrent;
                I++;

                while (true)
                {
                    if (I >= Content.Length)
                    {
                        ThrowError("Expecting a close : " + close + " followed by a ',' after line " + l);
                    }
                    if (IsNewline())
                    {
                        continue;
                    }
                    char c = Content[I];

                    if (c == close)
                    {
                        if ((I == Content.Length - 1 || Content[I + 1] == ',') && nesting == 0)
                        {
                            string s = Content.Substring(start + 1, I - start - 1);
                            I += 2;
                            return s;
                        }
                        nesting--;
                    }
                    if (c == open)
                    {
                        nesting++;
                    }

                    I++;
                }
            }

            public void ThrowError(string error)
            {
                string m = $"Error parsing line: {LineCurrent}. {error}";
                throw new Errors.DataError(m, Abs.ErrorPath);
            }
        }
    }
}