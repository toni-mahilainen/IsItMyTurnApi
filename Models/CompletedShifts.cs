using System;
using System.Collections.Generic;

namespace IsItMyTurnApi.Models
{
    public partial class CompletedShifts
    {
        public int ShiftId { get; set; }
        public int ApartmentId { get; set; }
        public DateTime Date { get; set; }

        public virtual Apartments Apartment { get; set; }
    }
}
