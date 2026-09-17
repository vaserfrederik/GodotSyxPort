using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace game.tourism
{
    [Serializable]
    public class Review : ISerializable
    {
        private const long serialVersionUID = 1L;

        public Review()
        {
        }

        protected Review(SerializationInfo info, StreamingContext context)
        {
            name = (StringReusableSer)info.GetValue("name", typeof(StringReusableSer));
            rating = (StringReusableSer)info.GetValue("rating", typeof(StringReusableSer));
            attraction = (StringReusableSer)info.GetValue("attraction", typeof(StringReusableSer));
            service = (StringReusableSer)info.GetValue("service", typeof(StringReusableSer));
            inn = (StringReusableSer)info.GetValue("inn", typeof(StringReusableSer));
            score = info.GetDouble("score");
            credits = info.GetInt32("credits");
        }

        public StringReusableSer name = new StringReusableSer(64);
        public StringReusableSer rating = new StringReusableSer(128);
        public StringReusableSer attraction = new StringReusableSer(128);
        public StringReusableSer service = new StringReusableSer(128);
        public StringReusableSer inn = new StringReusableSer(128);
        public double score = 0;
        public int credits;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
            info.AddValue("rating", rating);
            info.AddValue("attraction", attraction);
            info.AddValue("service", service);
            info.AddValue("inn", inn);
            info.AddValue("score", score);
            info.AddValue("credits", credits);
        }

        public void Save(FileStream file)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, name);
            formatter.Serialize(file, rating);
            formatter.Serialize(file, attraction);
            formatter.Serialize(file, service);
            formatter.Serialize(file, inn);
            formatter.Serialize(file, score);
            formatter.Serialize(file, credits);
        }

        public void Load(FileStream file)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            name = (StringReusableSer)formatter.Deserialize(file);
            rating = (StringReusableSer)formatter.Deserialize(file);
            attraction = (StringReusableSer)formatter.Deserialize(file);
            service = (StringReusableSer)formatter.Deserialize(file);
            inn = (StringReusableSer)formatter.Deserialize(file);
            score = (double)formatter.Deserialize(file);
            credits = (int)formatter.Deserialize(file);
        }

        public void CopyOther(Review other)
        {
            name.Clear().Add(other.name);
            rating.Clear().Add(other.rating);
            attraction.Clear().Add(other.attraction);
            service.Clear().Add(other.service);
            inn.Clear().Add(other.inn);
            score = other.score;
            credits = other.credits;
        }

        public bool Has()
        {
            return name.Length > 0;
        }

        public void Clear()
        {
            name.Clear();
            score = 0;
        }

        public void Make(Induvidual indu, COORDINATE inn)
        {
            Inserter.SetRandom(RND.rLong());

            name.Clear();
            name.S().Add('/').S();
            name.Add(STATS.APPEARANCE().Name(indu));
            name.NL();
            name.Add(DicTime.SetDate(Str.TMP.Clear(), (int)TIME.CurrentSecond()));

            InsertData d = Text.dd;
            d.I = indu;
            d.Inn = inn;

            score = 0;
            Race race = indu.Race();
            credits = 0;
            Text da = race.Tourism().Data;

            {
                RoomBlueprintImp a = TOURISM.Attraction(indu);

                double s = CLAMP.D(a.Employment().Employed() / (double)Max(a), 0, 1);
                score += s;
                if (s == 0)
                    d.Rating = 0;
                else
                    d.Rating = 0.5 + s * 0.5 + RND.rFloat0(0.15);
                attraction.Clear().Add(da.Attraction.Get(d));
            }

            score += SetService(d, service.Clear());

            if (inn != null && SETT.ROOMS().INN.Get(inn) != null)
            {
                ROOM_SERVICER innService = (ROOM_SERVICER)SETT.ROOMS().INN.Get(inn);
                score += innService.Quality();
                d.Rating = 0.35 + 0.65 * innService.Quality();
                this.inn.Clear().Add(da.Inn.Get(d));
            }

            score /= 3;

            credits = (int)(score * race.Tourism().Credits * TOURISM.CREDITS * RND.rFloat1(0.2));
            score += RND.rFloat0(0.2);
            score = CLAMP.D(score, 0, 1);

            d.Rating = score;
            rating.Clear().Add(da.Rating.Get(d));
        }

        private double Max(RoomBlueprintImp a)
        {
            foreach (RoomExperienceBonus e in SETT.ROOMS().exp.ALL())
            {
                if (e.Blue == a)
                {
                    return e.MaxEmployed;
                }
            }
            return TOURISM.MAX_EMPLOYEES;
        }

        private double SetService(InsertData d, Str tmp)
        {
            Text da = d.I.Race().Tourism().Data;

            StatService ss = TOURISM.Service(d.I);

            if (ss != null)
            {
                d.Rating = ss.Total().Indu().GetD(d.I);
                if (d.Rating == 0)
                    d.Rating = 0;
                else
                    d.Rating = 0.5 + d.Rating * 0.5 + RND.rFloat0(0.15);

                tmp.Add(da.Service.Get(d));
                return d.Rating;
            }
            return 0;
        }

        public int Render(SPRITE_RENDERER r, int x1, int y1, int width)
        {
            {
                int icons = (int)(1 + 4 * score);
                int x = x1 + width / 2 - Icon.S * icons / 2;
                for (int i = 0; i < icons; i++)
                {
                    SPRITES.icons().S.star.Render(r, x, y1);
                    x += Icon.S;
                }

                Str.TMP.Clear().Add(credits);
                x = x1 + width - 128;
                SPRITES.icons().S.money.Render(r, x, y1);
                GCOLOR.T().H1.Bind();
                UI.FONT().S.Render(r, Str.TMP, x + 6 + Icon.M, y1);
                COLOR.Unbind();

                y1 += Icon.S + 4;
            }

            GCOLOR.T().NORMAL2.Bind();
            if (rating != null)
                y1 += 8 + UI.FONT().M.Render(r, rating, x1, y1, width, 1);
            if (attraction != null)
                y1 += 8 + UI.FONT().M.Render(r, attraction, x1, y1, width, 1);
            if (service != null)
                y1 += 8 + UI.FONT().M.Render(r, service, x1, y1, width, 1);
            if (inn != null)
                y1 += 8 + UI.FONT().M.Render(r, inn, x1, y1, width, 1);
            GCOLOR.T().H2.Bind();
            y1 += UI.FONT().S.Render(r, name, x1 + 30, y1, width - 30, 1);
            COLOR.Unbind();

            return y1;
        }

        public void RenderScore(SPRITE_RENDERER r, int cx, int y1)
        {
            {
                int icons = (int)(1 + 4 * score);
                int x = icons * Icon.S / 2;
                x = cx - x;
                for (int i = 0; i < icons; i++)
                {
                    SPRITES.icons().S.star.Render(r, x, y1);
                    x += Icon.S;
                }
            }
        }

        public void RenderCred(SPRITE_RENDERER r, int x1, int y1)
        {
            Str.TMP.Clear().Add(credits);
            SPRITES.icons().S.money.Render(r, x1, y1);
            GCOLOR.T().H1.Bind();
            UI.FONT().S.Render(r, Str.TMP, x1 + 6 + Icon.M, y1);
            COLOR.Unbind();
        }
    }
}