using System;

namespace Settlement.Path
{
    using Game;
    using Game.Battle;

    public enum Availability
    {
        SOLID(-1, 0, true, 1, false),
        NOT_ACCESSIBLE(-1, 0, false, 0.5, false),
        NORMAL(1, 0, false, 1, true),
        AVOID_PASS(4, 5, false, 0.5, false),
        AVOID_LIKE_FUCK(32, 1, false, 0.25, false),

        ROAD0(0.5, 0, false, 1.0, true),
        ROAD1(0.5, 0, false, 1.05, true),
        ROAD2(0.5, 0, false, 1.10, true),
        ROAD3(0.5, 0, false, 1.15, true),
        ROAD4(0.5, 0, false, 1.20, true),
        PENALTY2(2, 0, false, 0.8, true),
        PENALTY3(3.0, 0, false, 0.5, true),
        PENALTY4(4, 1, false, 0.5, true),
        ROOM(1.2, 0, false, 1.0, false),

        ROOM_SOLID(-1, 0, true, 1, false),

        ENEMY(0.5, 0, true, 1.0, false, -1);

        public static readonly int Penalty = 2;
        public static readonly Availability[] Roads = new Availability[] {
            ROAD0,
            ROAD1,
            ROAD2,
            ROAD3,
            ROAD4
        };

        public readonly bool availableToStandOn;
        public readonly double from;
        public readonly double player;
        public readonly double enemy;
        public readonly bool tileCollide;
        public readonly double movementSpeed;
        public readonly double movementSpeedI;

        public readonly static Availability[] Values = Enum.GetValues(typeof(Availability)) as Availability[];

        private Availability(double player, double from, bool tileCollide, double movementBonus, bool available)
            : this(player, from, tileCollide, movementBonus, available, player)
        {
        }

        private Availability(double player, double from, bool tileCollide, double movementBonus, bool available, double enemy)
        {
            this.player = player;
            this.enemy = enemy;
            this.from = from;
            this.tileCollide = tileCollide;
            this.movementSpeed = movementBonus;
            this.movementSpeedI = 1.0 / movementSpeed;
            this.availableToStandOn = available;
        }

        public bool IsSolid(Army a)
        {
            if (a == GAME.ARMIES().player())
                return player < 0;
            return enemy < 0;
        }
    }
}