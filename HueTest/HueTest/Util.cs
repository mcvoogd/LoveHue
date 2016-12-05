using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Optional;

namespace HueTest
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

        public static Option<T> GetValueOption<T>(this JObject obj, string name)
        {
            JToken valueToken;
            return obj.TryGetValue(name, out valueToken) ? valueToken.ToObject<T>().Some() : Option.None<T>();
        }

        public static bool Contains(this JObject obj, string name)
        {
            JToken valueToken;
            return obj.TryGetValue(name, out valueToken);
        }
    }
}
