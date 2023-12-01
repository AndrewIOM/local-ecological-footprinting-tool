using System;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobId(Guid id)
    {
        public Guid Id { get; private set; } = id;
    }
}