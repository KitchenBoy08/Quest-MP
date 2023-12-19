using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;
using Il2CppPuppetMasta;
using Il2CppSLZ.AI;
using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BehaviourBaseNav))]
    public static class BehaviourBaseNavPatches
    {
        public static bool IgnorePatches = false;
    }
}
