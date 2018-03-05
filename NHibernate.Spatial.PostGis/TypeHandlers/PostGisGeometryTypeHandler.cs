using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using Npgsql.BackendMessages;
using Npgsql.TypeHandling;

namespace NHibernate.Spatial.TypeHandlers
{
    public class PostGisGeometryTypeHandler : NpgsqlTypeHandler<IGeometry>,
        INpgsqlTypeHandler<Point>, INpgsqlTypeHandler<MultiPoint>,
        INpgsqlTypeHandler<LineString>, INpgsqlTypeHandler<MultiLineString>,
        INpgsqlTypeHandler<Polygon>, INpgsqlTypeHandler<MultiPolygon>,
        INpgsqlTypeHandler<GeometryCollection>
    {
        private readonly PostGisWriter _writer;
        private readonly PostGisReader _reader;
        private static readonly Func<PostGisWriter, IGeometry, int, int> GetGeometrySize;

        static PostGisGeometryTypeHandler()
        {
            var getSizeMethod = typeof(PostGisWriter).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(o => o.Name == "GetByteStreamSize" && o.GetParameters().First().ParameterType == typeof(IGeometry));
            var param1 = Expression.Parameter(typeof(PostGisWriter), "writer");
            var param2 = Expression.Parameter(typeof(IGeometry), "geometry");
            var param3 = Expression.Parameter(typeof(int), "coordinateSpace");
            var methodCall = Expression.Call(param1, getSizeMethod, param2, param3);
            var lambda = Expression.Lambda<Func<PostGisWriter, IGeometry, int, int>>(methodCall, param1, param2, param3);
            GetGeometrySize = lambda.Compile();
        }

        public PostGisGeometryTypeHandler(PostGisWriter writer, PostGisReader reader)
        {
            _writer = writer;
            _reader = reader;
        }

        #region Read

        public override ValueTask<IGeometry> Read(NpgsqlReadBuffer buf, int len, bool async,
            FieldDescription fieldDescription = null)
        {
            var bytes = new byte[len];
            buf.ReadBytes(bytes, 0, len);
            return new ValueTask<IGeometry>(_reader.Read(bytes));
        }

        async ValueTask<Point> INpgsqlTypeHandler<Point>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (Point)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<MultiPoint> INpgsqlTypeHandler<MultiPoint>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (MultiPoint)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<LineString> INpgsqlTypeHandler<LineString>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (LineString)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<MultiLineString> INpgsqlTypeHandler<MultiLineString>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (MultiLineString)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<Polygon> INpgsqlTypeHandler<Polygon>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (Polygon)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<MultiPolygon> INpgsqlTypeHandler<MultiPolygon>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (MultiPolygon)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        async ValueTask<GeometryCollection> INpgsqlTypeHandler<GeometryCollection>.Read(NpgsqlReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            return (GeometryCollection)await Read(buf, len, async, fieldDescription).ConfigureAwait(false);
        }

        #endregion

        #region Write

        public override Task Write(IGeometry value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter, bool async)
        {
            buf.WriteBytes(_writer.Write(value));
            return Task.CompletedTask;
        }

        public Task Write(Point value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter, bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(MultiPoint value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter,
            bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(LineString value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter,
            bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(MultiLineString value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter,
            bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(Polygon value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter, bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(MultiPolygon value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter,
            bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        public Task Write(GeometryCollection value, NpgsqlWriteBuffer buf, NpgsqlLengthCache lengthCache, NpgsqlParameter parameter,
            bool async)
        {
            return Write((IGeometry)value, buf, lengthCache, parameter, async);
        }

        #endregion

        #region ValidateAndGetLength

        public override int ValidateAndGetLength(IGeometry value, ref NpgsqlLengthCache lengthCache,
            NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(Point value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(MultiPoint value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(LineString value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(MultiLineString value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(Polygon value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(MultiPolygon value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        public int ValidateAndGetLength(GeometryCollection value, ref NpgsqlLengthCache lengthCache, NpgsqlParameter parameter)
        {
            return GetByteSize(value);
        }

        #endregion

        #region Size calculation

        private int GetByteSize(IGeometry geometry)
        {
            var coordinateSpace = 8 * OrdinatesUtility.OrdinatesToDimension(_writer.HandleOrdinates);
            return GetGeometrySize(_writer, geometry, coordinateSpace);
        }
        
        #endregion
    }

}
