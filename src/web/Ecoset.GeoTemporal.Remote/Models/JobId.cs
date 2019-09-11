using System;

namespace Ecoset.GeoTemporal.Remote
{
    public class JobId
    {
        public JobId(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}