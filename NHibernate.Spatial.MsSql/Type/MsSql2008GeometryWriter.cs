// Copyright 2008 - Ricardo Stuven (rstuven@gmail.com)
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

using NetTopologySuite.Geometries;
using Microsoft.SqlServer.Types;
using System;

namespace NHibernate.Spatial.Type
{
    internal class MsSql2008GeometryWriter
    {
        private readonly SqlGeometryBuilder builder = new SqlGeometryBuilder();

        public SqlGeometry Write(Geometry geometry)
        {
            builder.SetSrid(geometry.SRID);
            AddGeometry(geometry);
            return builder.ConstructedGeometry;
        }

        private void AddGeometry(Geometry geometry)
        {
            if (geometry is Point)
            {
                AddPoint(geometry);
            }
            else if (geometry is LineString)
            {
                AddLineString(geometry);
            }
            else if (geometry is Polygon)
            {
                AddPolygon(geometry);
            }
            else if (geometry is MultiPoint)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiPoint);
            }
            else if (geometry is MultiLineString)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiLineString);
            }
            else if (geometry is MultiPolygon)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.MultiPolygon);
            }
            else if (geometry is GeometryCollection)
            {
                AddGeometryCollection(geometry, OpenGisGeometryType.GeometryCollection);
            }
        }

        private void AddGeometryCollection(Geometry geometry, OpenGisGeometryType type)
        {
            builder.BeginGeometry(type);
            GeometryCollection coll = geometry as GeometryCollection;
            Array.ForEach<Geometry>(coll.Geometries, delegate(Geometry g)
            {
                AddGeometry(g);
            });
            builder.EndGeometry();
        }

        private void AddPolygon(Geometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Polygon);
            Polygon polygon = geometry as Polygon;
            AddCoordinates(polygon.ExteriorRing.Coordinates);
            Array.ForEach<LineString>(polygon.InteriorRings, delegate(LineString ring)
            {
                AddCoordinates(ring.Coordinates);
            });
            builder.EndGeometry();
        }

        private void AddLineString(Geometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.LineString);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeometry();
        }

        private void AddPoint(Geometry geometry)
        {
            builder.BeginGeometry(OpenGisGeometryType.Point);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeometry();
        }

        private void AddCoordinates(Coordinate[] coordinates)
        {
            int points = 0;
            Array.ForEach<Coordinate>(coordinates, delegate(Coordinate coordinate)
            {
                double? z = null;
                if (!double.IsNaN(coordinate.Z) && !double.IsInfinity(coordinate.Z))
                {
                    z = coordinate.Z;
                }
                if (points == 0)
                {
                    builder.BeginFigure(coordinate.X, coordinate.Y, z, null);
                }
                else
                {
                    builder.AddLine(coordinate.X, coordinate.Y, z, null);
                }
                points++;
            });
            if (points != 0)
            {
                builder.EndFigure();
            }
        }

        public SqlGeometry ConstructedGeometry
        {
            get { return builder.ConstructedGeometry; }
        }
    }
}