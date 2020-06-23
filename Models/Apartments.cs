using System;
using System.Collections.Generic;

namespace IsItMyTurnApi.Models
{
    public partial class Apartments
    {
        public Apartments()
        {
            CompletedShifts = new HashSet<CompletedShifts>();
        }

        public int ApartmentId { get; set; }
        public string Apartment { get; set; }

        public virtual ICollection<CompletedShifts> CompletedShifts { get; set; }
    }
}
