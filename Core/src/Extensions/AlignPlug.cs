﻿using LabFusion.Utilities;

using SLZ.Interaction;

using System;

namespace LabFusion.Extensions
{
    public static class AlignPlugExtensions
    {
        public static void ForceEject(this AlignPlug plug)
        {
            try
            {
                if (plug._lastSocket && !plug._lastSocket.IsClearOnInsert)
                {
                    plug.EjectPlug();

                    while (plug._isExitTransition)
                        plug.Update();
                }
            }
            catch (Exception e) 
            {
#if DEBUG
                FusionLogger.LogException("running ForceEject on AlignPlug", e);
#endif
            }
        }
    }
}
