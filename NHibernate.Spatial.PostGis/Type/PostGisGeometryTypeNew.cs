// Copyright 2007 - Ricardo Stuven (rstuven@gmail.com)
//
// This file is part of NHibernate.Spatial.
// NHibernate.Spatial is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// NHibernate.Spatial is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NHibernate.Spatial; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using GeoAPI.Geometries;
using NetTopologySuite.IO;
using System;
using System.Data;
using System.Data.Common;
using System.Text;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using Npgsql;
using NpgsqlTypes;
using NetTopologySuite.Geometries;

namespace NHibernate.Spatial.Type
{
    [Serializable]
    public class PostGisGeometryType : GeometryTypeBase<IGeometry>
    {
        private static readonly NullableType GeometryType = new GeometryType();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostGisGeometryType"/> class.
        /// </summary>
        public PostGisGeometryType()
            : base(GeometryType)
        {
        }

        /// <summary>
        /// Converts from GeoAPI geometry type to database geometry type.
        /// </summary>
        /// <param name="value">The GeoAPI geometry value.</param>
        /// <returns></returns>
        protected override IGeometry FromGeometry(object value)
        {
            var geometry = value as IGeometry;
            if (geometry != null)
            {
                SetDefaultSRID(geometry);
            }
            return geometry;

            //IGeometry geometry = value as IGeometry;
            //if (geometry == null)
            //{
            //    return null;
            //}
            //// PostGIS can't parse a WKB of any empty geometry other than GeomtryCollection
            //// (throws the error: "geometry requires more points")
            //// and parses WKT of empty geometries always as GeometryCollection
            //// (ie. "select AsText(GeomFromText('LINESTRING EMPTY', -1)) = 'GEOMETRYCOLLECTION EMPTY'").
            //// Force GeometryCollection.Empty to avoid the error.
            //if (!(geometry is IGeometryCollection) && geometry.IsEmpty)
            //{
            //    geometry = GeometryCollection.Empty;
            //}

            //this.SetDefaultSRID(geometry);

            //// Determine the ordinality of the geometry to ensure 3D and 4D geometries are
            //// correctly serialized by PostGisWriter (see issue #66)
            //// TODO: Is there a way of getting the ordinates directly from the geometry?
            //var ordinates = Ordinates.XY;
            //var interiorPoint = geometry.InteriorPoint;
            //if (!interiorPoint.IsEmpty && !double.IsNaN(interiorPoint.Z))
            //{
            //    ordinates |= Ordinates.Z;
            //}
            //if (!interiorPoint.IsEmpty && !double.IsNaN(interiorPoint.M))
            //{
            //    ordinates |= Ordinates.M;
            //}

            //var postGisWriter = new PostGisWriter
            //{
            //    HandleOrdinates = ordinates
            //};
            //byte[] bytes = postGisWriter.Write(geometry);
            //return bytes;
        }

        /// <summary>
        /// Converts to GeoAPI geometry type from database geometry type.
        /// </summary>
        /// <param name="value">The databse geometry value.</param>
        /// <returns></returns>
        protected override IGeometry ToGeometry(object value)
        {
            var geometry = (IGeometry)value;
            if (geometry != null)
            {
                SetDefaultSRID(geometry);
            }
            return geometry;

            //if (!(value is byte[] bytes))
            //{
            //    return null;
            //}
            //return new PostGisReader().Read(bytes);

            //string bytes = value as string;

            //if (string.IsNullOrEmpty(bytes))
            //{
            //    return null;
            //}

            //// Bounding boxes are not serialized as hexadecimal string (?)
            //const string boxToken = "BOX(";
            //if (bytes.StartsWith(boxToken))
            //{
            //    // TODO: Optimize?
            //    bytes = bytes.Substring(boxToken.Length, bytes.Length - boxToken.Length - 1);
            //    string[] parts = bytes.Split(',');
            //    string[] min = parts[0].Split(' ');
            //    string[] max = parts[1].Split(' ');
            //    string wkt = string.Format(
            //        "POLYGON(({0} {1},{0} {3},{2} {3},{2} {1},{0} {1}))",
            //        min[0], min[1], max[0], max[1]);
            //    return new WKTReader().Read(wkt);
            //}
        }

    }

    [Serializable]
    public class GeometryType : MutableType
    {
        internal GeometryType()
            // Pass any SqlType to base class.
            : base(new BinarySqlType())
        {
        }

        public override object Get(DbDataReader rs, int index, ISessionImplementor session)
        {
            return (IGeometry)rs[index];
        }

        public override object Get(DbDataReader rs, string name, ISessionImplementor session)
        {
            return Get(rs, rs.GetOrdinal(name), session);
        }

        public override string ToString(object val)
        {
            return ((IGeometry)val).AsText();
        }

        public override object FromStringValue(string xml)
        {
            // TODO: TEST
            return new PostGisReader().Read(Encoding.ASCII.GetBytes(xml));
        }

        public override System.Type ReturnedClass => typeof(IGeometry);

        public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = (NpgsqlParameter)cmd.Parameters[index];
            parameter.NpgsqlDbType = NpgsqlDbType.Geometry;
            parameter.Value = value;
        }

        public override string Name => "Geometry";

        public override object DeepCopyNotNull(object value)
        {
            var geometry = (IGeometry)value;
            return geometry.Clone();
        }
    }
}