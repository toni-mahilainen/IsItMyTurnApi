using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsItMyTurnApi.OtherModels
{
    public class FCMBody
    {
        public string[] registration_ids { get; set; }
        public FCMNotification notification { get; set; }
    }

    public class FCMNotification
    {
        public string body { get; set; }
        public string title { get; set; }
    }
}
