using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.spirte;

namespace settlement.entity.humanoid.ai.main
{
    public abstract class AISTATE : AIElement
    {
        private readonly string name;

        protected AISTATE(string key, string name) : base("STATE_" + key)
        {
            this.name = name;
        }

        public abstract HSprite Sprite(Humanoid a);
        protected abstract bool Update(Humanoid a, AIManager d, double ds);
        protected string Name() => name;

        public abstract class Custom : AISTATE
        {
            private readonly HSprite sprite;

            public Custom(string key, string name, HSprite sprite) : base("STATE_" + key, name)
            {
                this.sprite = sprite;
            }

            public override HSprite Sprite(Humanoid a)
            {
                return sprite;
            }
        }
    }
}