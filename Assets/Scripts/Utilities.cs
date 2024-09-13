using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Utilities
    {
        private static System.Random rng = new System.Random();

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to choose from.</param>
        /// <returns>A random element from the list, or default(T) if the list is empty.</returns>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T IndexByPercentage<T>(this IList<T> list, float percentage)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List cannot be null or empty.");
            }

            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 1.");
            }

            int index = (int)Math.Floor(percentage * list.Count);

            // Ensure the index is within bounds (important when percentage is exactly 1)
            index = Math.Min(index, list.Count - 1);

            return list[index];
        }


        /// <summary>
        /// Returns and removes a random element from the list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to choose from.</param>
        /// <returns>A random element from the list, or default(T) if the list is empty.</returns>
        public static T PopRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            int index = UnityEngine.Random.Range(0, list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Shuffles the list in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }


        public static Connection GetOppositeConnection(Connection con)
        {
            if (con == Connection.Down) return Connection.Up;
            if (con == Connection.Up) return Connection.Down;
            if (con == Connection.Left) return Connection.Right;
            if (con == Connection.Right) return Connection.Left;
            return Connection.None;
        }

        public static Direction GetOppositeDirection(Direction dir)
        {
            if (dir == Direction.Down) return Direction.Up;
            if (dir == Direction.Up) return Direction.Down;
            if (dir == Direction.Left) return Direction.Right;
            if (dir == Direction.Right) return Direction.Left;
            return Direction.None;
        }

        public static Vector2 ConnectionToVector(Connection con)
        {
            if (con == Connection.Down) return Vector2.down;
            if (con == Connection.Up) return Vector2.up;
            if (con == Connection.Left) return Vector2.left;
            if (con == Connection.Right) return Vector2.right;
            return Vector2.zero;
        }


        [System.Flags]
        public enum Connection
        {
            None = 0,
            Up = 1,
            Right = 2,
            Down = 4,
            Left = 8
        }

        public enum Direction
        {
            None = -1,
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        }
    }
}
