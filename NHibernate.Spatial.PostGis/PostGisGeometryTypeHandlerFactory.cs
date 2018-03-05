using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NHibernate.Spatial.TypeHandlers;
using Npgsql;
using Npgsql.TypeHandling;

namespace NHibernate.Spatial
{
    public class PostGisGeometryTypeHandlerFactory : NpgsqlTypeHandlerFactory<IGeometry>
    {
        private readonly Func<NpgsqlConnection, PostGisGeometryTypeHandler> _factoryMethod;

        public PostGisGeometryTypeHandlerFactory(Func<NpgsqlConnection, PostGisGeometryTypeHandler> factoryMethod = null)
        {
            _factoryMethod = factoryMethod;
        }

        protected override NpgsqlTypeHandler<IGeometry> Create(NpgsqlConnection conn)
        {
            return _factoryMethod != null
                ? _factoryMethod(conn)
                : new PostGisGeometryTypeHandler(new PostGisWriter(), new PostGisReader());
        }
    }
}
