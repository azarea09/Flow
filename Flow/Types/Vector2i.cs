namespace Flow
{
    public struct Vector2i
    {
        public int X, Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i(int i)
        {
            X = i;
            Y = i;
        }

        public static Vector2i Zero => new Vector2i(0, 0);

        public static Vector2i One => new Vector2i(1, 1);

        public static Vector2i UnitX => new Vector2i(1, 0);

        public static Vector2i UnitY => new Vector2i(0, 1);

        public static implicit operator Vector2i((int x, int y) tuple) => new Vector2i(tuple.x, tuple.y);
        public static implicit operator (int x, int y)(Vector2i v) => (v.X, v.Y);

        public static bool operator ==(Vector2i left, Vector2i right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2i left, Vector2i right)
        {
            return !(left == right);
        }

        public static Vector2i operator +(Vector2i left, Vector2i right)
        {
            return new Vector2i(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2i operator -(Vector2i left, Vector2i right)
        {
            return new Vector2i(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2i operator *(Vector2i left, int right)
        {
            return new Vector2i(left.X * right, left.Y * right);
        }

        public static Vector2i operator /(Vector2i left, int right)
        {
            if (right == 0)
                throw new DivideByZeroException("Division by zero in Vector2i division.");
            return new Vector2i(left.X / right, left.Y / right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2i other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Vector2i Min(Vector2i a, Vector2i b)
        {
            return new Vector2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2i Max(Vector2i a, Vector2i b)
        {
            return new Vector2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2i Clamp(Vector2i value, Vector2i min, Vector2i max)
        {
            return new Vector2i(
                Math.Clamp(value.X, min.X, max.X),
                Math.Clamp(value.Y, min.Y, max.Y)
            );
        }

        public static Vector2i Lerp(Vector2i a, Vector2i b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2i(
                (int)(a.X + (b.X - a.X) * t),
                (int)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Vector2i(Vector2 v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }

        public static implicit operator Vector2i(Vector2d v)
        {
            return new Vector2i((int)v.X, (int)v.Y);
        }
    }
}
