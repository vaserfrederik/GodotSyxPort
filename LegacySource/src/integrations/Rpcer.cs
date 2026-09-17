using System;

namespace Integrations
{
    public abstract class Rpcer
    {
        public abstract void Update(string state, string details);

        protected abstract void Dispose();
    }
}