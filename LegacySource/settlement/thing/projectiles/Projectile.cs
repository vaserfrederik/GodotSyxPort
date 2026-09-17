using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Settlement.Thing.Projectiles
{
    public static class Projectile
    {
        static List<Projectile> ALL = new List<Projectile>(8) { null };
        static Projectile()
        {
            GAME.AddDisposable(() =>
            {
                ALL.Clear();
            });
        }

        private static string ¤¤name = "¤Projectile";
        private static string ¤¤splashDamage = "¤Splash Damage (tiles)";
        private static string ¤¤Range = "¤Range (tiles)";
        private static string ¤¤Accuracy = "¤Accuracy";
        private static string ¤¤Reload = "¤Reload (seconds)";
        private static string ¤¤Arch = "¤Max angle (degrees)";

        static Projectile()
        {
            D.ts(typeof(Projectile));
        }

        public readonly short Index;

        public Projectile()
        {
            if (ALL.Count >= short.MaxValue)
                throw new Exception("Too many projectiles declared! max is " + short.MaxValue);
            Index = (short)ALL.Add(this);
        }

        public int Index() => Index;

        public double Range(int height, double reference)
        {
            return Trajectory.Range(height, MaxAngle(reference), Velocity(reference));
        }

        public abstract double Velocity(double reference);
        public abstract double MaxAngle(double reference);

        public double BluntDamage(double reference)
        {
            return Mass(reference) * Velocity(reference) * C.ITILE_SIZE;
        }

        public abstract double ReloadSeconds(double reference);
        public abstract double Accuracy(double reference);
        public abstract double Skill(double reference);
        public abstract double Mass(double reference);
        public abstract double Damage(int battleIndex, double reference);
        public abstract double AreaAttack(double reference);
        public abstract SoundRace SoundRelease();
        public abstract SoundRace SoundHit();
        public abstract ProjectileSprite Sprite();

        public static void RenderArrow(Renderer r, ShadowBatch s, double x, double y, int h, int ran, double dx, double dy, double dz, int zoomOut)
        {
            if (zoomOut < 2)
            {
                double l = Math.Sqrt(dx * dx + dy * dy + dz * dz * 4);
                dx /= l;
                dy /= l;
                dx *= C.SCALE;
                dy *= C.SCALE;
                for (int k = 0; k < 8; k++)
                {
                    r.RenderParticle((int)x, (int)y);
                    x += dx;
                    y += dy;
                }
            }
            s.SetHeight(0);
            s.SetDistance2Ground(h / 4);
            SPRITES.icons().s.dot.RenderC(s, (int)x, (int)y);
        }

        private static readonly Rectangle pixels = new Rectangle();
        private static readonly Vector2 tVec = new Vector2();
        private static readonly Vector2 sVec = new Vector2();

        public void Impact(double reference, double cx, double cy, double dx, double dy, double dz)
        {
            double areaAttack = AreaAttack(reference);
            if (areaAttack <= 0)
                return;

            if (dx == 0 && dy == 0 && dz == 0)
                return;

            double mass = Mass(reference);
            double speed = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double mom = mass * speed;

            SETT.THINGS().gore.Debris((int)cx, (int)cy, dx * 0.5, dy * 0.5);
            SETT.GRASS().current.Increment((int)cx, (int)cy, -0.5);

            sVec.Set(dx, dy);
            dx = sVec.NX();
            dy = sVec.NY();

            pixels.SetDim(areaAttack * 2);
            pixels.MoveC(cx, cy);

            foreach (ENTITY e in SETT.ENTITIES().Fill(pixels))
            {
                double l = tVec.Set(cx, cy, e.body().cX(), e.body().cY());
                if (l > areaAttack)
                    continue;
                l = 1.0 - (l / areaAttack);
                if (l < 0)
                    continue;

                tVec.Set(tVec.NX() + sVec.NX() * (0.1 + RND.rFloat()) + RND.rFloat0(0.1), tVec.NY() + sVec.NY() * (0.1 + RND.rFloat()) + RND.rFloat0(0.1));

                GAME.Battle().Fight.ProjectileAttack(e, dx, dy, speed, this, reference);
            }

            for (int tdy = (int)-areaAttack; tdy <= areaAttack; tdy += C.TILE_SIZE)
            {
                for (int tdz = (int)-areaAttack; tdz <= areaAttack; tdz += C.TILE_SIZE)
                {
                    pixels.SetDim(areaAttack * 2);
                    pixels.MoveC(cx + tdz, cy + tdy);

                    foreach (ENTITY e in SETT.ENTITIES().Fill(pixels))
                    {
                        double l = tVec.Set(cx + tdz, cy + tdy, e.body().cX(), e.body().cY());
                        if (l > areaAttack)
                            continue;
                        l = 1.0 - (l / areaAttack);
                        if (l < 0)
                            continue;

                        tVec.Set(tVec.NX() + sVec.NX() * (0.1 + RND.rFloat()) + RND.rFloat0(0.1), tVec.NY() + sVec.NY() * (0.1 + RND.rFloat()) + RND.rFloat0(0.1));

                        GAME.Battle().Fight.ProjectileAttack(e, dx, dy, speed, this, reference);
                    }
                }
            }
        }

        public static class ProjectileSpec
        {
            public double maxAngle;
            public double velocity;
            public double accuracy;
            public double dexterity;
            public double reloadSpeed;

            public double mass, areaAttack;
            public double[] damage;

            public ProjectileSpec(Json json)
            {
                if (json.Has("PROJECTILE"))
                    json = json.Json("PROJECTILE");
                mass = json.d("MASS", 0.01, 100000);
                velocity = json.d("TILE_SPEED", 0.5, 250) * C.TILE_SIZE;

                reloadSpeed = json.d("RELOAD_SECONDS", 0.01, 10000);

                accuracy = json.d("ACCURACY", 0.01, 1);
                dexterity = json.d("DEXTERITY", 0, 10000000);
                BOOSTABLES.BATTLE().DAMAGE_COLL.ReadFill(damage, json, 0, 100000);
                areaAttack = json.dTry("TILE_RADIUS_DAMAGE", 0, 10000, 0) * C.TILE_SIZE;
                maxAngle = json.d("MAX_ARCH_ANGLE_DEGREES", 0, 75);
            }
        }

        public class ProjectileImp : Projectile
        {
            private readonly ProjectileSpec from;
            private readonly ProjectileSpec delta;
            private readonly ProjectileSprite sprite;
            public readonly SoundRace soundRelease;
            public readonly SoundRace soundHit;

            public ProjectileImp(Json data, string key) : base()
            {
                if (data.Has("PROJECTILE"))
                    data = data.Json("PROJECTILE");

                sprite = ProjectileSprite.Get(data);
                soundRelease = AUDIO.race("PROJECTILE_RELEASE_" + key);
                soundHit = AUDIO.race("PROJECTILE_HIT_" + key);
                from = new ProjectileSpec(data.Json("FROM"));
                delta = new ProjectileSpec(data.Json("TO"));

                delta.accuracy -= from.accuracy;
                delta.dexterity -= from.dexterity;
                delta.areaAttack -= from.areaAttack;
                delta.mass -= from.mass;
                delta.maxAngle -= from.maxAngle;
                delta.reloadSpeed -= from.reloadSpeed;
                delta.velocity -= from.velocity;

                foreach (BDamage d in BOOSTABLES.BATTLE().DAMAGES)
                    delta.damage[d.Index()] -= from.damage[d.Index()];
            }

            public override double Mass(double reference)
            {
                return from.mass + delta.mass * reference;
            }

            public override double Damage(int battleI, double reference)
            {
                return from.damage[battleI] + delta.damage[battleI] * reference;
            }

            public override double AreaAttack(double reference)
            {
                return from.areaAttack + delta.areaAttack * reference;
            }

            public override double Velocity(double reference)
            {
                return from.velocity + delta.velocity * reference;
            }

            public override double MaxAngle(double reference)
            {
                reference = Math.Clamp(reference, 0, 1);
                return from.maxAngle + delta.maxAngle * reference;
            }

            public override double ReloadSeconds(double reference)
            {
                reference = Math.Clamp(reference, 0, 1);
                return from.reloadSpeed + delta.reloadSpeed * reference;
            }

            public override double Accuracy(double reference)
            {
                return Math.Clamp(from.accuracy + delta.accuracy * reference, 0, 1);
            }

            public override double Skill(double reference)
            {
                return from.dexterity + delta.dexterity * reference;
            }

            public override SoundRace SoundRelease()
            {
                return soundRelease;
            }

            public override SoundRace SoundHit()
            {
                return soundHit;
            }

            public override ProjectileSprite Sprite()
            {
                return sprite;
            }
        }
    }
}