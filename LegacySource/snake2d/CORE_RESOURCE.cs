using System;

namespace snake2d
{
    /**
     * Listen up! implementing this means that one has strange stuff in memory that needs proper disposal
     * @author mail__000
     *
     */
    public abstract class CORE_RESOURCE
    {
        public CORE_RESOURCE()
        {
        }
        
        /**
         * remove all strange stuff from GPU's memory
         */
        public abstract void dis();
    }
}