using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuMaster.Tools
{
    class Field
    {
        public char Value { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? Step { get; set; }

        public Field()
        {
            Value = ' ';
            X = 0;
            Y = 0;
        }

        public Field(int _x, int _y)
        {
            Value = ' ';
            X = _x;
            Y = _y;
        }
    }
}
