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
    internal class MsSql2008GeographyWriter
    {
        private readonly SqlGeographyBuilder builder = new SqlGeographyBuilder();

        public SqlGeography Write(Geometry geometry)
        {
            builder.SetSrid(geometry.SRID);
            AddGeometry(geometry);
            return builder.ConstructedGeography;
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
                AddGeometryCollection(geometry, OpenGisGeographyType.MultiPoint);
            }
            else if (geometry is MultiLineString)
            {
                AddGeometryCollection(geometry, OpenGisGeographyType.MultiLineString);
            }
            else if (geometry is MultiPolygon)
            {
                AddGeometryCollection(geometry, OpenGisGeographyType.MultiPolygon);
            }
            else if (geometry is GeometryCollection)
            {
                AddGeometryCollection(geometry, OpenGisGeographyType.GeometryCollection);
            }
        }

        private void AddGeometryCollection(Geometry geometry, OpenGisGeographyType type)
        {
            builder.BeginGeography(type);
            GeometryCollection coll = geometry as GeometryCollection;
            Array.ForEach<Geometry>(coll.Geometries, delegate(Geometry g)
            {
                AddGeometry(g);
            });
            builder.EndGeography();
        }

        private void AddPolygon(Geometry geometry)
        {
            builder.BeginGeography(OpenGisGeographyType.Polygon);
            Polygon polygon = geometry as Polygon;
            AddCoordinates(polygon.ExteriorRing.Coordinates);
            Array.ForEach<LineString>(polygon.InteriorRings, delegate(LineString ring)
            {
                AddCoordinates(ring.Coordinates);
            });
            builder.EndGeography();
        }

        private void AddLineString(Geometry geometry)
        {
            builder.BeginGeography(OpenGisGeographyType.LineString);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeography();
        }

        private void AddPoint(Geometry geometry)
        {
            builder.BeginGeography(OpenGisGeographyType.Point);
            AddCoordinates(geometry.Coordinates);
            builder.EndGeography();
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
                    builder.BeginFigure(coordinate.Y, coordinate.X, z, null);
                }
                else
                {
                    builder.AddLine(coordinate.Y, coordinate.X, z, null);
                }
                points++;
            });
            if (points != 0)
            {
                builder.EndFigure();
            }
        }

        public SqlGeography ConstructedGeography
        {
            get { return builder.ConstructedGeography; }
        }
    }
}