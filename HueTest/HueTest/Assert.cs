using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueTest
{
    public static class Assert
    {
        public class AssertFailed : Exception
        {
            public AssertFailed(string desciption) : base(desciption)
            {
            }
        }

        public static void True(bool value)
        {
            if (value == false)
                throw new AssertFailed("value is false");
        }

        public static void False(bool value)
        {
            if (value == true)
                throw new AssertFailed("value is true");
        }

        public static void Range(dynamic value, dynamic min, dynamic max)
        {
            if (value < min || value > max)




                throw new AssertFailed($"value({value}) is outside range<{min},{max}>");
        }
    }
}
