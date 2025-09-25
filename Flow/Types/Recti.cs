namespace Flow
{
    public struct Recti
    {
        public int X, Y, Width, Height;

        public Recti(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        public static implicit operator Recti((int x, int y, int width, int height) tuple) => new Recti(tuple.x, tuple.y, tuple.width, tuple.height);
        public static implicit operator (int x, int y, int width, int height)(Recti r) => (r.X, r.Y, r.Width, r.Height);
        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }
    }
}
