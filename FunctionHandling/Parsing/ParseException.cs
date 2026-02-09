using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Science_NEA.FunctionHandling.Parsing
{
    public sealed class ParseException : Exception
    {
        public int Position { get; }
        public ParseException(string message, int position) : base($"{message} (at index {position})")
        {
            Position = position;
        }
    }
}
