using System;
using System.Reflection;
using init.paths;
using init.settings;
using snake2d;
using snake2d.util.file;

namespace util.text
{
    public static class D
    {
        private static Json currentJson;
        private static string currentClass;
        private static Json dd;
        private static bool first = true;

        static D()
        {
        }

        public static void Init()
        {
            dd = new Json(PATHS.TEXT().GetFolder("dictionary").Get("Dic"));
        }

        public static void GInit(Type clazz)
        {
            if (dd == null)
                return;
            if (!dd.Has(clazz.Name))
            {
                if (S.Get().Debug)
                {
                    LOG.Err("No mapping for class: " + clazz.Name);
                    if (first)
                    {
                        new Exception().PrintStackTrace();
                        first = false;
                    }
                }
                currentJson = null;
                currentClass = null;
            }
            else
            {
                currentClass = clazz.Name;
                currentJson = dd.Json(clazz.Name);
            }
        }

        public static void GInit(object clazz)
        {
            GInit(clazz.GetType());
        }

        public static string G(string key, string def)
        {
            if (dd == null)
                return def;
            return G(key);
        }

        public static string G(string defKey)
        {
            if (dd == null)
                return defKey;
            if (currentJson == null || !currentJson.Has(defKey))
            {
                if (S.Get().Debug || S.Get().Developer)
                {
                    string ss = "";
                    foreach (StackTraceElement e in new StackTrace().GetFrames())
                    {
                        if (!e.GetMethod().DeclaringType.FullName.Equals(typeof(D).FullName) && e.GetMethod().DeclaringType.FullName.IndexOf(".Thread") < 0)
                        {
                            ss = "(" + e.GetMethod().DeclaringType.FullName + ".java:" + e.GetFileLineNumber() + ")";
                            break;
                        }
                    }
                    LOG.Err("No mapping " + currentClass + " " + defKey + " " + ss);
                }
                return defKey;
            }
            return currentJson.Text(defKey);
        }

        public static void T(object clazz)
        {
            T(clazz.GetType(), clazz);
        }

        public static void T(Type clazz)
        {
            T(clazz, null);
        }

        private static string old;

        public static string Ts(Type clazz)
        {
            string c = currentClass;
            T(clazz);
            if (c != null && dd != null)
            {
                currentClass = c;
                if (dd.Has(c))
                    currentJson = dd.Json(c);
                else
                    currentJson = null;
            }
            return "";
        }

        public static void Spush(Type clazz)
        {
            old = currentClass;
            T(clazz);
        }

        public static void Spop()
        {
            if (old != null && dd != null)
            {
                currentClass = old;
                currentJson = dd.Json(old);
            }
        }

        public static void T(Type clazz, object o)
        {
            GInit(clazz);
            if (currentJson == null)
                return;

            foreach (FieldInfo f in clazz.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (typeof(CharSequence).IsAssignableFrom(f.FieldType))
                {
                    string s = f.Name;
                    if (s.Length > 1 && s[0] == '¤' && s[1] == '¤')
                    {
                        f.SetValue(o, G(s.Substring(2)));
                    }
                }
            }
        }
    }
}