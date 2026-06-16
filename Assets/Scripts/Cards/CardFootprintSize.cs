using System;
using UnityEngine;

namespace Cards
{
    [Serializable]
    public struct CardFootprintSize : IEquatable<CardFootprintSize>
    {
        [SerializeField] private int _columns;
        [SerializeField] private int _rows;

        public int Columns => Mathf.Max(1, _columns);
        public int Rows => Mathf.Max(1, _rows);

        public CardFootprintSize(int columns, int rows)
        {
            _columns = columns;
            _rows = rows;
        }

        public static CardFootprintSize OneByOne => new(1, 1);
        public static CardFootprintSize TwoByOne => new(2, 1);
        public static CardFootprintSize OneByTwo => new(1, 2);
        public static CardFootprintSize ThreeByOne => new(3, 1);

        public bool Equals(CardFootprintSize other)
        {
            return Columns == other.Columns && Rows == other.Rows;
        }

        public override bool Equals(object obj)
        {
            return obj is CardFootprintSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Columns, Rows);
        }

        public override string ToString() => $"{Columns}x{Rows}";
    }
}
