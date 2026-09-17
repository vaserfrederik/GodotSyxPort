using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Thing.HalfEntity.HalfCorpse
{
    using Init.Type;
    using Settlement.Entity.Humanoid;
    using Settlement.Thing.HalfEntity;
    using Snake2D.Util.File;
    using Snake2D.Util.Sets;

    public class MovingCorpseFactory : Factory<MovingCorpse>
    {
        public MovingCorpseFactory(LISTE<Factory<?>> all) : base(all)
        {
        }

        protected override void Save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Clear()
        {
            // TODO Auto-generated method stub
        }

        protected override MovingCorpse Make()
        {
            return new MovingCorpse();
        }

        public void Make(Humanoid h, bool gore, CAUSE_LEAVE l)
        {
            MovingCorpse e = Create();
            e.Init(h, gore, l);
        }
    }
}