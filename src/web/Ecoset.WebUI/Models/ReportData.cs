using System.Collections.Generic;
using Ecoset.GeoTemporal.Remote;

namespace Ecoset.WebUI.Models
{
    public class ReportData
    {
        public string Title { get; set; }

        public string Description { get; set; }
        
        public float North { get; set; }

        public float South { get; set; }

        public float East { get; set; }

        public float West { get; set; }
        public List<RawData> RawResults { get; set; }
        public List<TableList> TableListResults { get; set; }

        public List<TableStats> TableStatsResults { get; set; }
    }

    public class TableList
    {
        public string Name { get; set; }
        public DataTableListResult Data { get; set; }
    }

    public class TableStats
    {
        public string Name { get; set; }
        public DataTableStatsResult Data { get; set; }
    }

    public class RawData
    {
        public string Name { get; set; }
        public RawDataResult Data { get; set; }
    }
}