using System;
using System.Collections.Generic;

namespace IsItMyTurnApi.Models
{
    public partial class FcmTokens
    {
        public int TokenId { get; set; }
        public int DeviceId { get; set; }
        public string Token { get; set; }

        public virtual Devices Device { get; set; }
    }
}
