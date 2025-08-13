namespace Flow
{
    public struct Rectd
    {
        public double X, Y, Width, Height;

        public Rectd(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static implicit operator Rectd((double x, double y, double width, double height) tuple) => new Rectd(tuple.x, tuple.y, tuple.width, tuple.height);
        public static implicit operator (double x, double y, double width, double height)(Rectd r) => (r.X, r.Y, r.Width, r.Height);
        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }
    }
}
