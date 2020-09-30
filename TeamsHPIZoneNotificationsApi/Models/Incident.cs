using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsHPIZoneNotificationsApi.Models
{
    public class Incident
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        //public string Status { get; set; }
        //public string Urgency { get; set; }
        //public string Category { get; set; }
        //public DateTime Opened { get; set; }
        //public DateTime Closed { get; set; }
        //public string OpenedBy { get; set; }
    }
}