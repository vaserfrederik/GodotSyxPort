using System;
using init.constant;
using snake2d.util.file;

namespace init.race
{
    public sealed class Physics
    {
        private readonly double heightOverGround;
        private readonly int hitboxSize;
        public readonly int childDays;
        public readonly int babyDays;
        public readonly int adultDay;
        public readonly bool decays;
        public readonly bool sleeps;
        public readonly double slaveprice;
        public readonly double slavePRriceRecovery;
        public readonly double raiding;

        public Physics(Json json)
        {
            json = json.json("PROPERTIES");

//            acceleration = json.d("ACCELERATION", 0, 1000) * C.TILE_SIZE;
//            topSpeed = json.d("TOP_SPEED", 1, 15) * C.TILE_SIZE;
            heightOverGround = json.i("HEIGHT", 0, 200);
            hitboxSize = json.i("WIDTH", 5, 15) * C.SCALE;
            childDays = json.i("CHILD_DAYS");
            babyDays = json.i("BABY_DAYS");
            decays = json.bool("CORPSE_DECAY");
            sleeps = json.bool("SLEEPS");
            slaveprice = json.d("SLAVE_PRICE", 0, int.MaxValue);
            slavePRriceRecovery = json.d("SLAVE_PRICE_RECOVERY", 0, 10);
            raiding = json.d("RAID_MERCINARY", 0, 100000);
            adultDay = babyDays + childDays;
        }

        public double height()
        {
            return heightOverGround;
        }

//        public double acceleration()
//        {
//            return acceleration;
//        }
//
//        public double topSpeed()
//        {
//            return topSpeed;
//        }

        public int hitBoxsize()
        {
            return hitboxSize;
        }
    }
}