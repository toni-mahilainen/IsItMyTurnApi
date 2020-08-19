using System;
using System.Collections.Generic;

namespace IsItMyTurnApi.Models
{
    public partial class Devices
    {
        public Devices()
        {
            FcmTokens = new HashSet<FcmTokens>();
        }

        public int DeviceId { get; set; }
        public string UniqueIdentifier { get; set; }

        public virtual ICollection<FcmTokens> FcmTokens { get; set; }
    }
}
