using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;
using Il2CppPuppetMasta;
using Il2CppSLZ.AI;

namespace LabFusion.Syncables
{
    public class BehaviourOmniwheelExtender : PropComponentExtender<BehaviourOmniwheel>
    {
        public static FusionComponentCache<BehaviourOmniwheel, PropSyncable> Cache = new FusionComponentCache<BehaviourOmniwheel, PropSyncable>();

        protected override void AddToCache(BehaviourOmniwheel wheel, PropSyncable syncable)
        {
            Cache.Add(wheel, syncable);
        }

        protected override void RemoveFromCache(BehaviourOmniwheel wheel)
        {
            Cache.Remove(wheel);
        }
    }
}
