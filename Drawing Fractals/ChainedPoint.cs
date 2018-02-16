using System;
using System.Drawing;

namespace Drawing_Fractals
{
    internal class ChainedPoint : IEquatable<ChainedPoint>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int FromDirection { get; set; }

        public enum Directions { North, Northeast, East, Southeast, South, Southwest, West, Northwest, Center }

        public bool Alive { get; set; }
        public int LayerIndexAded { get; set; }

        public bool Equals(ChainedPoint other)
        {
            return other != null && other.X == X && other.Y == Y;
        }

        public ChainedPoint(int x, int y, int fromDirection)
        {
            X = x;
            Y = y;
            FromDirection = fromDirection;
            Alive = true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ChainedPoint);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        public bool IsDiagonal()
        {
            return FromDirection == (int)Directions.Northwest
                || FromDirection == (int)Directions.Southwest
                || FromDirection == (int)Directions.Southeast
                || FromDirection == (int)Directions.Northeast;
        }

        public bool IsStraight()
        {
            return !IsDiagonal();
        }
    }
}