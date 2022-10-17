using System;

// Disable warning that code is not being used.
#pragma warning disable CS0169, CS0067, CS0649

enum _Color { Red, Blue, Green }

namespace _TestProject
{
    interface _IProcess {}

    struct _ValueWidget {
        // Fields
        private int _total;

        // Methods
        public void _M(int i) {}
    }

    unsafe class _Widget : _IProcess
    {
        // Nested types
        public class _NestedClass {
            private int _value;
            public void _M(int i) {}
        }
        public interface _IMenuItem {}
        public delegate void _Del(int i);
        public enum _Direction { North, South, East, West }

        // Fields
        private string _message;
        private static _Color _defaultColor;
        private const double _PI = 3.14159;
        protected readonly double _monthlyAverage;
        private long[] _array1;
        private _Widget[,] _array2;
        private unsafe int* _pCount;
        private unsafe float** _ppValues;

        // Constructors
        static _Widget() {}
        public _Widget() {}
        public _Widget(string s) {}

        // Destructors
        ~_Widget() {}

        // Methods
        public static void _M0() {}

        public void _M1(char c, out float f, ref _ValueWidget v) {
            f = 0f;
        }
        public void _M2(short[] x1, int[,] x2, long[][] x3) {}
        public void _M3(long[][] x3, _Widget[][,,] x4) {}
        public unsafe void _M4(char* pc, _Color** pf) {}
        public unsafe void _M5(void* pv, double*[][,] pd) {}
        public void _M6(int i, params object[] args) {}

        public void _M7((int max, int min) _tuple) {}

        // Properties
        public int _Width
        {
            get { return 0;} set {} }
        public int this[int _i]
        {
            get { return 0;} set {} }
        public int this[string _s, int i]
        {
            get { return 0;} set {} }

        // Events
        public event _Del _AnEvent;

        // Operators
        public static _Widget operator +(_Widget _x) {
            return _x;
        }

        public static _Widget operator +(_Widget _x1, _Widget x2) {
            return _x1;
        }

        public static explicit operator int(_Widget _x) {
            return 0;
        }

        public static implicit operator long(_Widget x) {
            return 0;
        }
    }

    class _MyList<T>
    {
        class _Helper<U, V> {}

        public void _Test(T t) { }
    }

    class _UseList
    {
        public void _Process(_MyList<int> list) { }
        public _MyList<T> _GetValues<T>(T inputValue) { return null; }
    }
}
