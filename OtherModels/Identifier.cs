using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsItMyTurnApi.OtherModels
{
    public class Identifier
    {
        public string DeviceId { get; set; }
        public string OldToken { get; set; }
        public string NewToken { get; set; }
    }
}
