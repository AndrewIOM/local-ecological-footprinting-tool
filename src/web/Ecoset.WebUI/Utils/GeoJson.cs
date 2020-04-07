using System.IO;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Ecoset.WebUI.Utils {

    public static class GeoJson {

        public static bool BoxIntersects(string geojsonFileName, double latNorth, double latSouth, double lonEast, double lonWest) 
        {
            var json = File.ReadAllText(geojsonFileName); 
            var feature = new GeoJsonReader().Read<FeatureCollection>(json);
            var geometryFactory = new GeometryFactory();
            var rectCoords = new[] {
                new Coordinate(lonWest, latNorth),
                new Coordinate(lonWest, latSouth),
                new Coordinate(lonEast, latSouth),
                new Coordinate(lonEast, latNorth),
                new Coordinate(lonWest, latNorth)
            };

            var bbox = geometryFactory.CreatePolygon(rectCoords);
            var intersects = feature.FirstOrDefault(f => {
                return f.Geometry.Intersects(bbox);
            });
            return intersects != null;
        }

    }
}