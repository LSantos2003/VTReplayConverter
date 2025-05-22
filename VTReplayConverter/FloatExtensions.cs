using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
    public static class FloatExtensions
    {
        public static string Invariant(this float value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public static string Invariant(this double value) =>
            value.ToString(CultureInfo.InvariantCulture);
    }
}
