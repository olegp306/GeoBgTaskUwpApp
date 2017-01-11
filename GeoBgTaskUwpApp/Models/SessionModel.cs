using System;

namespace GeoBgTaskUwpApp.Models
{
    public class SessionModel
    {
        public Guid sessionId { get; set; }
        public Guid loginGuid { get; set; }
        public Guid employeeGuid { get; set; }
        public string employeeName { get; set; }
        public string employeePhone { get; set; }
        public string employeeAuto { get; set; }
        public int timezone { get; set; }
        public DateTime serverDateTime { get; set; }
        public bool admin { get; set; }
        public bool nfcMaster { get; set; }
        public bool geoTracking { get; set; }
    }
}
