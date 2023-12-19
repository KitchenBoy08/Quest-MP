using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using Il2CppPuppetMasta;

using Il2CppSLZ.AI;

using UnityEngine;

namespace LabFusion.Syncables
{
    public class TriggerRefProxyExtender : PropComponentExtender<TriggerRefProxy>
    {
        public static FusionComponentCache<TriggerRefProxy, PropSyncable> Cache = new FusionComponentCache<TriggerRefProxy, PropSyncable>();

        protected override void AddToCache(TriggerRefProxy proxy, PropSyncable syncable)
        {
            Cache.Add(proxy, syncable);
        }

        protected override void RemoveFromCache(TriggerRefProxy proxy)
        {
            Cache.Remove(proxy);
        }
    }
}
