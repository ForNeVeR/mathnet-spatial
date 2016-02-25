using System;
using System.Collections.Generic;

namespace MathNet.Spatial.Euclidean
{
    public class Box3D
    {
        public Point3D P1 { get; }
        public Point3D P2 { get; }

        public IEnumerable<Rectangle3D> Faces { get; }
        public IEnumerable<Line3D> Edges { get; }
        public IEnumerable<Point3D> Nodes { get; }

        public IEnumerable<Rectangle2D> Project() // TODO: Pass a projection direction
        {
            throw new NotImplementedException();
        }
    }
}
