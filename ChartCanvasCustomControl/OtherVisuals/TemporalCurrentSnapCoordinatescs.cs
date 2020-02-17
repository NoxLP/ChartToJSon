using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartCanvasNamespace.OtherVisuals
{
    public class TemporalCurrentSnapCoordinates : IEquatable<TemporalCurrentSnapCoordinates>
    {
        public double CenterX;
        public double CenterY;
        public double Left;
        public double Right;
        public double Top;
        public double Bottom;
        public double ThLeft;
        public double ThRight;
        public double ThTop;
        public double ThBottom;

        public override bool Equals(object obj)
        {
            return Equals(obj as TemporalCurrentSnapCoordinates);
        }
        public bool Equals(TemporalCurrentSnapCoordinates other)
        {
            return other != null &&
                   CenterX == other.CenterX &&
                   CenterY == other.CenterY &&
                   Left == other.Left &&
                   Right == other.Right &&
                   Top == other.Top &&
                   Bottom == other.Bottom;
        }
        public bool EqualsTruncated(TemporalCurrentSnapCoordinates other)
        {
            return other != null &&
                   (int)CenterX == (int)other.CenterX &&
                   (int)CenterY == (int)other.CenterY &&
                   (int)Left == (int)other.Left &&
                   (int)Right == (int)other.Right &&
                   (int)Top == (int)other.Top &&
                   (int)Bottom == (int)other.Bottom;
        }
        public override string ToString()
        {
            return $"Center: {CenterX},{CenterY} ; Horiz: {Left},{Right} ; Vert: {Top},{Bottom}";
        }
    }
}
