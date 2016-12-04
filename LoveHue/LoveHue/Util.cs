using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LoveHue
{
    public static class Util
    {
        public static bool Contains(this SerializationInfo info, string name, Type type)
        {
            try
            {
                info.GetValue(name, type);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
