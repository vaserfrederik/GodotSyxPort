using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using Util.Info;
using Util.Keymap;
using Util.Spritecomposer;

namespace Settlement.Entity.Animal
{
    public class AnimalSpecies : INFO, MAPPED
    {
        public static readonly int SIZE = 24 * C.SCALE;
        private readonly double massMin;
        private readonly double heightOverGround;
        private readonly double acceleration;
        public readonly int hitboxSize;
        private readonly int spriteOff;
        public readonly Icon icon;
        private readonly int index;
        public readonly bool caravanable;
        public readonly COLOR color;
        private readonly LIST<RESOURCE> resources;
        public readonly RBIT rBit;
        private readonly double[] resAmounts;
        private readonly double[] climates;
        private readonly double[] terrains;
        private readonly string key;
        public readonly double[] damage = new double[BOOSTABLES.BATTLE().DAMAGES.Size];

        public readonly bool pack;
        public readonly bool grazes;
        public readonly COLOR blood = new ColorImp(127, 15, 15);

        public readonly double momTreshold;
        public readonly double momTresholdFly;
        public readonly double caveLiving;

        public readonly double danger;

        public readonly SoundRace sound;
        public readonly TILE_SHEET sheet;

        public AnimalSpecies(string key, int index, Json data, Json text, KeyMap<TILE_SHEET> sprites) : base(text, null)
        {
            this.key = key;
            this.index = index;
            icon = SPRITES.icons().Get(data);
            caravanable = data.Bool("CARAVAN");
            massMin = data.I("MASS", 1, 500);
            acceleration = data.I("SPEED", 1, 31) * C.TILE_SIZE;
            heightOverGround = data.I("HEIGHT", 0, 50);
            hitboxSize = 11 * C.SCALE;
            spriteOff = (32 * C.SCALE - hitboxSize) / 2;
            color = new ColorImp(data);
            resources = RESOURCES.map().ReadMany(data);
            resAmounts = data.Ds("RESOURCE_AMOUNT", resources.Size());

            RBITImp bb = new RBITImp();
            foreach (RESOURCE res in resources)
            {
                bb.Or(res);
            }
            this.rBit = bb;

            BOOSTABLES.BATTLE().DAMAGE_COLL.New(new KJson(data)
            {
                protected override void Process(BDamage s, Json j, string key, bool isWeak)
                {
                    damage[s.Index()] = j.D(key, 0, 10000);
                }
            });

            CLIMATES.MAP();
            climates = CLIMATES.MAP().ReadFill(data, 1);
            terrains = TERRAINS.MAP().ReadFill(data, 1);
            pack = data.Bool("PACK");
            grazes = data.Bool("GRAZES");
            danger = data.D("DANGER", 0, 1);
            momTreshold = acceleration * massMin * 1.5;
            momTresholdFly = acceleration * massMin * 2.0;
            caveLiving = data.D("LIVES_IN_CAVES", 0, 1);

            sound = AUDIO.race("ANIMAL_CALL_" + key);
            string sKey = data.Value("SPRITE");
            TILE_SHEET sheet;
            if (sprites.ContainsKey(sKey))
            {
                sheet = sprites[sKey];
            }
            else
            {
                new ComposerThings.IInit(PATHS.SPRITE().GetFolder("animal").Get(sKey), 164, 462);

                sheet = new ComposerThings.ITileSheet
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        s.singles.Init(0, 0, 1, 1, 2, 12, d.s32);
                        for (int i = 0; i < 12; i++)
                        {
                            s.singles.SetSkip(i * 2, 2).Paste(3, true);
                        }
                        return d.s32.SaveGame();
                    }
                }.Get();
            }
            this.sheet = sheet;
        }

        public double Occurence(CLIMATE c)
        {
            return climates[c.Index()];
        }

        public double Occurence(TERRAIN t)
        {
            return terrains[t.Index()];
        }

        public double Mass()
        {
            return massMin;
        }

        public double HeightOverGround()
        {
            return heightOverGround;
        }

        public double Acceleration()
        {
            return acceleration;
        }

        public int HitBoxSize()
        {
            return hitboxSize;
        }

        public int SpriteOff()
        {
            return spriteOff;
        }

        public override int Index()
        {
            return index;
        }

        public LIST<RESOURCE> Resources()
        {
            return resources;
        }

        public int ResAmount(int ri, double weight)
        {
            return (int)Math.Ceiling(resAmounts[ri] * weight * 0.3);
        }

        public override string Key()
        {
            return key;
        }
    }
}