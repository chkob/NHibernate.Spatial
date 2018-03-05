using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NHibernate.Spatial.TypeHandlers;
using Npgsql;
using Npgsql.TypeMapping;
using NpgsqlTypes;

namespace NHibernate.Spatial
{
    public static class NpgsqlPostGisExtensions
    {
        public static INpgsqlTypeMapper UsePostGis(this INpgsqlTypeMapper mapper, Func<NpgsqlConnection, PostGisGeometryTypeHandler> typeHandlerFactory = null)
        {
            // NpgsqlConnection.GlobalTypeMapper
            mapper.RemoveMapping("geometry");
            mapper.AddMapping(new NpgsqlTypeMappingBuilder
            {
                PgTypeName = "geometry",
                NpgsqlDbType = NpgsqlDbType.Geometry,
                ClrTypes = new[]
                {
                    typeof(IGeometry),
                    typeof(Point),
                    typeof(MultiPoint),
                    typeof(LineString),
                    typeof(MultiLineString),
                    typeof(Polygon),
                    typeof(MultiPolygon),
                    typeof(GeometryCollection)
                },
                TypeHandlerFactory = new PostGisGeometryTypeHandlerFactory(typeHandlerFactory)
            }.Build());

            return mapper;
        }
    }
}
