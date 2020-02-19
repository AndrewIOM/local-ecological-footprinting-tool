using System.Threading.Tasks;
using Ecoset.GeoTemporal.Remote;

namespace Ecoset.WebUI.Services.Abstract
{    
    /// Converts data structures into file format for saving
    public interface IDataFormatter
    {
        string SpatialData(RawDataResult spatialData);
        string TableData(DataTableListResult tableData, System.IO.Stream saveLocation);

    }
}
