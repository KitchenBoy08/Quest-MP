using LabFusion.Network;
using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Core.src.Network.Riptide
{
    public sealed class RiptideVoiceManager : VoiceManager
    {
        public override bool CanTalk => true;

        public override VoiceHandler GetVoiceHandler(PlayerId id)
        {
            if (TryGetHandler(id, out var handler))
                return handler;

            var newIdentifier = new RiptideVoiceHandler(id);
            VoiceHandlers.Add(newIdentifier);
            return newIdentifier;
        }
    }
}
