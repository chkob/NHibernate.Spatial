﻿using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Prepared;
using Open.Topology.TestRunner.Result;
using System;

namespace Open.Topology.TestRunner.Operations
{
    /**
     * A {@link GeometryOperation} which uses {@link PreparedGeometry}s
     * for applicable operations.
     * This allows testing correctness of the <tt>PreparedGeometry</tt> implementation.
     * <p>
     * This class can be used via the <tt>-geomop</tt> command-line option
     * or by the <tt>&lt;geometryOperation&gt;</tt> XML test file setting.
     *
     * @author mbdavis
     *
     */

    public class PreparedGeometryOperation : IGeometryOperation
    {
        private readonly GeometryMethodOperation _chainOp;

        public PreparedGeometryOperation()
            : this(new GeometryMethodOperation())
        {
        }

        public Type GetReturnType(XmlTestType op)
        {
            return GetReturnType(op.ToString());
        }

        public Type GetReturnType(String opName)
        {
            if (IsPreparedOp(opName))
                return typeof(bool);
            return _chainOp.GetReturnType(opName);
        }

        /**
   * Creates a new operation which chains to the given {@link GeometryMethodOperation}
   * for non-intercepted methods.
   *
   * @param chainOp the operation to chain to
   */

        public PreparedGeometryOperation(GeometryMethodOperation chainOp)
        {
            _chainOp = chainOp;
        }

        private static bool IsPreparedOp(String opName)
        {
            if (opName.Equals("intersects", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (opName.Equals("contains", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (opName.Equals("containsProperly", StringComparison.InvariantCultureIgnoreCase)) return true;
            if (opName.Equals("covers", StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }

        /**
   * Invokes the named operation
   *
   * @param opName
   * @param geometry
   * @param args
   * @return
   * @throws Exception
   * @see GeometryOperation#invoke
   */

        public IResult Invoke(XmlTestType opName, IGeometry geometry, Object[] args)
        {
            return Invoke(opName.ToString(), geometry, args);
        }

        public IResult Invoke(String opName, IGeometry geometry, Object[] args)
        {
            if (!IsPreparedOp(opName))
            {
                return _chainOp.Invoke(opName, geometry, args);
            }
            return InvokePreparedOp(opName, geometry, args);
        }

        private static IResult InvokePreparedOp(String opName, IGeometry geometry, Object[] args)
        {
            var g2 = (IGeometry)args[0];
            if (opName.Equals("intersects", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BooleanResult(PreparedGeometryOp.Intersects(geometry, g2));
            }
            if (opName.Equals("contains", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BooleanResult(PreparedGeometryOp.Contains(geometry, g2));
            }
            if (opName.Equals("containsProperly", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BooleanResult(PreparedGeometryOp.ContainsProperly(geometry, g2));
            }
            if (opName.Equals("covers", StringComparison.InvariantCultureIgnoreCase))
            {
                return new BooleanResult(PreparedGeometryOp.Covers(geometry, g2));
            }
            return null;
        }

        internal static class PreparedGeometryOp
        {
            public static bool Intersects(IGeometry g1, IGeometry g2)
            {
                var prepGeom = PreparedGeometryFactory.Prepare(g1);
                return prepGeom.Intersects(g2);
            }

            public static bool Contains(IGeometry g1, IGeometry g2)
            {
                var prepGeom = PreparedGeometryFactory.Prepare(g1);
                return prepGeom.Contains(g2);
            }

            public static bool ContainsProperly(IGeometry g1, IGeometry g2)
            {
                var prepGeom = PreparedGeometryFactory.Prepare(g1);
                return prepGeom.ContainsProperly(g2);
            }

            public static bool Covers(IGeometry g1, IGeometry g2)
            {
                var prepGeom = PreparedGeometryFactory.Prepare(g1);
                return prepGeom.Covers(g2);
            }
        }
    }
}