using System;

namespace Aicup2020.Model
{
    public struct Vec2Int
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vec2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public static Vec2Int ReadFrom(System.IO.BinaryReader reader)
        {
            var result = new Vec2Int();
            result.X = reader.ReadInt32();
            result.Y = reader.ReadInt32();
            return result;
        }
        public void WriteTo(System.IO.BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
        }

        public double RangeTo(Vec2Int p2)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(p2.X - this.X, 2) + Math.Pow(p2.Y - this.Y, 2)));
        }

        public static bool operator ==(Vec2Int v1, Vec2Int v2) => v1.X == v2.X && v1.Y == v2.Y;

        public static bool operator !=(Vec2Int v1, Vec2Int v2) => v1.X != v2.X || v1.Y != v2.Y;
    }
}
