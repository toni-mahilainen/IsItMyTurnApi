using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsItMyTurnApi.OtherModels
{
    public class NewShift
    {
        public int ApartmentId { get; set; }
        public DateTime Date { get; set; }
        public string FcmToken { get; set; }
    }
}
