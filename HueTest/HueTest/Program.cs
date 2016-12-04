using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

namespace HueTest
{

    public static class Extender
    {
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsEnum;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HueWrapper hueWrapper;
            if (File.Exists("username.txt") == false)
            {
                Console.WriteLine("CREATING USERNAME");
                var temp = HueWrapper.CreateHueWrapper("192.168.1.179");
                hueWrapper = temp.Item1;
                File.WriteAllText("username.txt", temp.Item2);
            }
            else
            {
                Console.WriteLine("LOADING USERNAME");
                hueWrapper = HueWrapper.CreateHueWrapper("192.168.1.179", File.ReadAllText("username.txt")).Item1;
            }
            /*hueWrapper[10].state.Brightness = 254;
            while (true)
            {
                Console.ReadLine();
                hueWrapper[10].state.Brightness -= 10;
                hueWrapper[10].state.Alert = HueWrapper.HueLamp.StateStruct.AlertState.LSELECT.Some();
            }*/
            double avarageHue = (from lamp in hueWrapper
                where lamp.state.Hue.HasValue
                select lamp.state.Hue).Average(n => n.ValueOr(0));
            Console.WriteLine("avarage hue = " + avarageHue);
            Console.ReadLine();
        }
    }
}
