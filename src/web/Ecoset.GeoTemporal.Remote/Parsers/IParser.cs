using Newtonsoft.Json.Linq;

namespace Ecoset.GeoTemporal.Remote
{
    public interface IParser<T>
    {
        T TryParse(JToken raw);
    }
}