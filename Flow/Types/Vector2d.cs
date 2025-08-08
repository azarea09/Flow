namespace Flow
{
    public struct Vector2d
    {
        public double X, Y;

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2d(double i)
        {
            X = i;
            Y = i;
        }

        public static Vector2d Zero => new Vector2d(0, 0);

        public static Vector2d One => new Vector2d(1, 1);

        public static Vector2d UnitX => new Vector2d(1, 0);

        public static Vector2d UnitY => new Vector2d(0, 1);

        public static implicit operator Vector2d((double x, double y) tuple) => new Vector2d(tuple.x, tuple.y);
        public static implicit operator (double x, double y)(Vector2d v) => (v.X, v.Y);

        public static bool operator ==(Vector2d left, Vector2d right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2d left, Vector2d right)
        {
            return !(left == right);
        }

        public static Vector2d operator -(Vector2d v)
        {
            return new Vector2d(-v.X, -v.Y);
        }

        public static Vector2d operator +(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2d operator -(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2d operator *(Vector2d left, Vector2d right)
        {
            return new Vector2d(left.X * right.X, left.Y * right.Y);
        }

        public static Vector2d operator /(Vector2d left, Vector2d right)
        {
            if (right.X == 0 || right.Y == 0)
                throw new DivideByZeroException("Division by zero in component-wise Vector2d division.");

            return new Vector2d(left.X / right.X, left.Y / right.Y);
        }

        public static Vector2d operator *(Vector2d left, double right)
        {
            return new Vector2d(left.X * right, left.Y * right);
        }

        public static Vector2d operator /(Vector2d left, double right)
        {
            if (right == 0)
                throw new DivideByZeroException("Division by zero in Vector2d division.");
            return new Vector2d(left.X / right, left.Y / right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2d other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static Vector2d Min(Vector2d a, Vector2d b)
        {
            return new Vector2d(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2d Max(Vector2d a, Vector2d b)
        {
            return new Vector2d(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2d Clamp(Vector2d value, Vector2d min, Vector2d max)
        {
            return new Vector2d(
                Math.Clamp(value.X, min.X, max.X),
                Math.Clamp(value.Y, min.Y, max.Y)
            );
        }

        public static Vector2d Lerp(Vector2d a, Vector2d b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2d(
                (double)(a.X + (b.X - a.X) * t),
                (double)(a.Y + (b.Y - a.Y) * t)
            );
        }

        // ----------------------------
        // 変換
        // ----------------------------
        public static implicit operator Vector2d(Vector2 v)
        {
            return new Vector2d((double)v.X, (double)v.Y);
        }

        public static implicit operator Vector2(Vector2d v)
        {
            return new Vector2((float)v.X, (float)v.Y);
        }

        public static implicit operator Vector2d(Vector2i v)
        {
            return new Vector2d((double)v.X, (double)v.Y);
        }
    }
}
