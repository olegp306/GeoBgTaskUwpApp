using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoBgTaskUwpApp.Models
{
    public class GeoDataForApi
    {
        public GeoDataForApi()
        {
            Points = new List<PointShort>();
        }

        public string UserGuid { get; set; }
        public List<PointShort> Points { get; set; }
    }

    public class PointShort
    {
        public DateTime Tsmp { get; set; }
        public decimal Lng { get; set; }
        public decimal Lat { get; set; }
    }

    public class EmployerPoint : PointShort
    {
        public string userGuid { get; set; }
    }

    public class FindSparePartsRequestModel
    {
        public Range Range { get; set; }
        public Viewport Viewport { get; set; }
        public IEnumerable<Sparepart> Spareparts { get; set; }
    }

    public class Range
    {
        /* отступ, для диапазонной загрузки */
        public string Offset { get; set; }
        /* количество сотрудников для отображение */
        public string Fetch { get; set; }
    }

    public class Viewport
    {
        /* опорные координаты, от которых будет производиться расчет расстояния */
        public string Center { get; set; }
    }

    public class Sparepart
    {
        public string Guid { get; set; }
    }

    public class FindSparepartsResponseModel : EmployerPoint
    {
        public int DistanceMeters { get; set; }
    }

  
}
