using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoBgTaskUwpApp.Models
{
    public class GeoData
    {
        public string Guid { get; set; }
        public string ObjectGuid { get; set; }
        public string DepartmentGuid { get; set; }
        public DateTime DateTime { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        public decimal Latitude { get; set; }
        /// <summary>
        /// Долгота
        /// </summary>
        public decimal Longitude { get; set; }
    }
}
