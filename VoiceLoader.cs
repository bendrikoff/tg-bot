using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Misha_s_bot
{
    internal class VoiceLoader
    {
        public Dictionary<Voices, string> VoicesPath = new Dictionary<Voices, string>
        {
            {Voices.OhYes, "../../../voices/oh_yes.ogg" },
            {Voices.IgorFuck, "../../../voices/igor_fuck.ogg" },
            {Voices.KtoLohPoTelkam, "../../../voices/loh.ogg" },

        };

    }

    public enum Voices
    {
        OhYes,
        IgorFuck,
        KtoLohPoTelkam
    }
}
