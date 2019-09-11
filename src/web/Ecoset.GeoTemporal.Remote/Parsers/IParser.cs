namespace Ecoset.GeoTemporal.Remote
{
    public interface IParser<T>
    {
        T TryParse(string raw);
    }
}