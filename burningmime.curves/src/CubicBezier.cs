using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

#if SYSTEM_WINDOWS_VECTOR
using VECTOR = System.Windows.Vector;
using FLOAT = System.Double;
#elif SYSTEM_NUMERICS_VECTOR
using VECTOR = System.Numerics.Vector2;
using FLOAT = System.Single;
#elif UNITY
using VECTOR = UnityEngine.Vector2;
using FLOAT = System.Single;
#else
#error Unknown vector type -- must define one of SYSTEM_WINDOWS_VECTOR, SYSTEM_NUMERICS_VECTOR or UNITY
#endif

namespace burningmime.curves
{
    /// <summary>
    /// Cubic Bezier curve in 2D consisting of 4 control points.
    /// </summary>
    public struct CubicBezier : IEquatable<CubicBezier>
    {
        // Control points
        public readonly VECTOR p0;
        public readonly VECTOR p1;
        public readonly VECTOR p2;
        public readonly VECTOR p3;

        /// <summary>
        /// Creates a new cubic bezier using the given control points.
        /// </summary>
        public CubicBezier(VECTOR p0, VECTOR p1, VECTOR p2, VECTOR p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        /// <summary>
        /// Samples the bezier curve at the given t value.
        /// </summary>
        /// <param name="t">Time value at which to sample (should be between 0 and 1, though it won't fail if outside that range).</param>
        /// <returns>Sampled point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VECTOR Sample(FLOAT t)
        {
            FLOAT ti = 1 - t;
            FLOAT t0 = ti * ti * ti;
            FLOAT t1 = 3 * ti * ti * t;
            FLOAT t2 = 3 * ti * t * t;
            FLOAT t3 = t * t * t;
            return (t0 * p0) + (t1 * p1) + (t2 * p2) + (t3 * p3);
        }

        /// <summary>
        /// Gets the first derivative of the curve at the given T value.
        /// </summary>
        /// <param name="t">Time value at which to sample (should be between 0 and 1, though it won't fail if outside that range).</param>
        /// <returns>First derivative of curve at sampled point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VECTOR Derivative(FLOAT t)
        {
            FLOAT ti = 1 - t;
            FLOAT tp0 = 3 * ti * ti;
            FLOAT tp1 = 6 * t * ti;
            FLOAT tp2 = 3 * t * t;
            return (tp0 * (p1 - p0)) + (tp1 * (p2 - p1)) + (tp2 * (p3 - p2));
        }

        /// <summary>
        /// Gets the tangent (normalized derivative) of the curve at a given T value.
        /// </summary>
        /// <param name="t">Time value at which to sample (should be between 0 and 1, though it won't fail if outside that range).</param>
        /// <returns>Direction the curve is going at that point.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VECTOR Tangent(FLOAT t)
        {
            return VectorHelper.Normalize(Derivative(t));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CubicBezier: (<");
            sb.Append(VectorHelper.GetX(p0).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(VectorHelper.GetY(p0).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append("> <");
            sb.Append(VectorHelper.GetX(p1).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(VectorHelper.GetY(p1).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append("> <");
            sb.Append(VectorHelper.GetX(p2).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(VectorHelper.GetY(p2).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append("> <");
            sb.Append(VectorHelper.GetX(p3).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append(", ");
            sb.Append(VectorHelper.GetY(p3).ToString("N3", CultureInfo.InvariantCulture));
            sb.Append(">)");
            return sb.ToString();
        }

        // Equality members -- pretty straightforeward
        public static bool operator ==(CubicBezier left, CubicBezier right) { return left.Equals(right); }
        public static bool operator !=(CubicBezier left, CubicBezier right) { return !left.Equals(right); }
        public bool Equals(CubicBezier other) { return p0.Equals(other.p0) && p1.Equals(other.p1) && p2.Equals(other.p2) && p3.Equals(other.p3); }
        public override bool Equals(object obj) { return obj is CubicBezier && Equals((CubicBezier) obj); }
        public override int GetHashCode()
        {
            JenkinsHash hash = new JenkinsHash();
            hash.Mixin(VectorHelper.GetX(p0).GetHashCode());
            hash.Mixin(VectorHelper.GetY(p0).GetHashCode());
            hash.Mixin(VectorHelper.GetX(p1).GetHashCode());
            hash.Mixin(VectorHelper.GetY(p1).GetHashCode());
            hash.Mixin(VectorHelper.GetX(p2).GetHashCode());
            hash.Mixin(VectorHelper.GetY(p2).GetHashCode());
            hash.Mixin(VectorHelper.GetX(p3).GetHashCode());
            hash.Mixin(VectorHelper.GetY(p3).GetHashCode());
            return hash.GetValue();
        }

        /// <summary>
        /// Simple implementation of Jenkin's hashing algorithm.
        /// http://en.wikipedia.org/wiki/Jenkins_hash_function
        /// I forget where I got these magic numbers from; supposedly they're good.
        /// 
        /// Copied from the utils because the curve code should be usable without them.
        /// </summary>
        private struct JenkinsHash
        {
            private int _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Mixin(int hash)
            {
                unchecked
                {
                    int num = _current;
                    if(num == 0)
                        num = 0x7e53a269;
                    else
                        num *= -0x5aaaaad7;
                    num += hash;
                    num += (num << 10);
                    num ^= (num >> 6);
                    _current = num;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int GetValue()
            {
                unchecked
                {
                    int num = _current;
                    num += (num << 3);
                    num ^= (num >> 11);
                    num += (num << 15);
                    return num;
                }
            }
        }
    }
}