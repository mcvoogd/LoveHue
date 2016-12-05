using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;

namespace HueTest
{
    class HueWrapper : IEnumerable<HueWrapper.HueLamp>
    {
        private static void SetValue(HttpClient httpClient, string Header, string name, object value)
        {
            string message = value is string
                ? $"{{\"{name}\":\"{value}\"}}"
                : $"{{\"{name}\":{(value is bool ? value.ToString().ToLower() : value)}}}";

            HttpResponseMessage temp = httpClient.PutAsync(
                new Uri(Header),
                new StringContent(message)
            ).Result;
            string content = ((StreamContent) temp.Content).ReadAsStringAsync().Result;
            content = content.Substring(1, content.Length - 2);
            JObject obj = (JObject)JsonConvert.DeserializeObject(content);
            if (obj["error"] != null)
                Console.WriteLine($"ERROR: {obj["error"]["description"]}\n\r\tREQUEST: {message}");
        }

        public class HueLamp
        {
            public struct StateStruct
            {
                public HueLamp root;
                private string Header => $"{root.Header}/state";
                private HttpClient httpClient => root.httpClient;
                private void Set(string name, object value) { SetValue(httpClient, Header, name, value); }

                public enum AlertState
                {
                    /// <summary>
                    /// The light is not performing an alert effect
                    /// </summary>
                    NONE,
                    /// <summary>
                    /// The light is performing one breathe cycle
                    /// </summary>
                    SELECT,
                    /// <summary>
                    /// The light is performing breathe cycles for 15 seconds or until an ["alert": "none"] command is received
                    /// </summary>
                    LSELECT
                }
                private static AlertState AlertStateDesirialize(string alert)
                {
                    return (AlertState)Enum.Parse(typeof(AlertState), alert.ToUpper());
                }
                private static string AlertStateSerialize(Option<AlertState> alert)
                {
                    return alert.ValueOr(AlertState.NONE).ToString().ToLower();
                }

                public enum ColorModeState
                {
                    /// <summary>
                    /// Can only be used when both Hue and Saturation are present
                    /// </summary>
                    HUE_AND_SATURATION,
                    /// <summary>
                    /// Can only be used when X&Y are present
                    /// </summary>
                    XY,
                    /// <summary>
                    /// Can only be used when ColorTemprature is present
                    /// </summary>
                    COLOR_TEMPRATURE
                }
                private static ColorModeState ColorModeStateDesirialize(string colorMode)
                {
                    if (colorMode == "hs") return ColorModeState.HUE_AND_SATURATION;
                    if (colorMode == "xy") return ColorModeState.XY;
                    if (colorMode == "ct") return ColorModeState.COLOR_TEMPRATURE;
                    throw new ArgumentException($"invalid colorMode({colorMode})");
                }
                private static string ColorModeStateSerialize(Option<ColorModeState> colorMode)
                {
                    ColorModeState state = colorMode.ValueOr(ColorModeState.HUE_AND_SATURATION);

                    if (state == ColorModeState.HUE_AND_SATURATION) return "hs";
                    if (state == ColorModeState.XY) return "xy";
                    if (state == ColorModeState.COLOR_TEMPRATURE) return "ct";
                    throw new ArgumentException($"invalid colorMode({colorMode})");
                }

                private bool on;
                public bool On
                {
                    get { return on; }
                    set { Set("on", on = value); }
                }

                private byte bri;
                /// <summary>
                /// Range(1,254)
                /// </summary>
                public byte Brightness
                {
                    get { return bri; }
                    set { Assert.Range(value, 1, 254); Set("bri", bri = value); }
                }

                private Option<ushort> hue;
                /// <summary>
                /// Optional, Range(0,65535)
                /// </summary>
                public Option<ushort> Hue
                {
                    get { return hue; }
                    set { Assert.True(hue.HasValue && value.HasValue); Assert.Range(value, 0, 65535); Set("hue", (hue = value).ValueOr(0)); }
                }

                private Option<byte> sat;
                /// <summary>
                /// Optional, Range(0,254)
                /// </summary>
                public Option<byte> Saturation
                {
                    get { return sat; }
                    set { Assert.True(sat.HasValue && value.HasValue); Assert.Range(value, 0, 254); Set("sat", (sat = value).ValueOr(0)); }
                }

                private Option<string> effect;
                /// <summary>
                /// Optional, sets 
                /// </summary>
                public Option<bool> Effect
                {
                    get { return effect.HasValue ? (effect.ValueOr("none") == "colorloop").Some() : Option.None<bool>(); }
                    set { Assert.True(effect.HasValue && value.HasValue);Set("effect", (effect = (value.ValueOr(false) ? "colorloop" : "none").Some()).ValueOr("")); }
                }

                private Option<float[]> xy;
                public Option<float> X
                {
                    get { return xy.HasValue ? Option.None<float>() : Option.Some<float>(xy.ValueOr(new float[2])[0]); }
                    set { Assert.True(xy.HasValue && value.HasValue); Assert.Range(value, 0.0f, 1.0f); xy.ValueOr(new float[2])[0] = value.ValueOr(0); Set("xy", xy.ValueOr(new float[2])); }
                }
                public Option<float> Y
                {
                    get { return xy.HasValue ? Option.None<float>() : Option.Some<float>(xy.ValueOr(new float[2])[1]); }
                    set { Assert.True(xy.HasValue && value.HasValue); Assert.Range(value, 0.0f, 1.0f); xy.ValueOr(new float[2])[1] = value.ValueOr(0); Set("xy", xy.ValueOr(new float[2])); }
                }

                private Option<ushort> ct;
                /// <summary>
                /// Optional, Range(153, 500)
                /// </summary>
                public Option<ushort> ColorTemperature
                {
                    get { return ct; }
                    set { Assert.True(ct.HasValue && value.HasValue); Assert.Range(value, 153, 500); Set("ct", ct = value); }
                }

                private Option<AlertState> alert;
                public Option<AlertState> Alert
                {
                    get { return alert; }
                    set { Set("alert", AlertStateSerialize(alert = value)); }
                }

                private Option<ColorModeState> colormode;
                public Option<ColorModeState> ColorMode
                {
                    get { return colormode; }
                    set { Assert.True(colormode.HasValue && value.HasValue); CheckColorMode(value.ValueOr(ColorModeState.COLOR_TEMPRATURE)); Set("colormode", ColorModeStateSerialize(colormode = value)); }
                }

                private bool reachable;
                public bool Reachable
                {
                    get { return reachable; }
                    set { Set("reachable", reachable = value); }
                }

                private void CheckColorMode(ColorModeState colorMode)
                {
                    switch (colorMode)
                    {
                            case ColorModeState.HUE_AND_SATURATION:
                            if (!Hue.HasValue || !Saturation.HasValue)
                                throw new InvalidOperationException("both Hue and Saturation need to be present for this colormode");
                            break;
                            case ColorModeState.XY:
                            if (!xy.HasValue)
                                throw new InvalidOperationException("X&Y need to be present for this colormode");
                            break;
                            case ColorModeState.COLOR_TEMPRATURE:
                            if (!colormode.HasValue)
                                throw new InvalidOperationException("ColorTemprature needs to be present for this colormode");
                            break;
                    }
                }

                public StateStruct(JObject state, HueLamp root)
                {
                    Assert.Debug.True(root != null);
                    Assert.Debug.True(state != null);
                    Assert.Debug.True(state.Contains("on"));
                    Assert.Debug.True(state.Contains("bri"));
                    Assert.Debug.True(state.Contains("reachable"));

                    this.root = root;

                    on = (bool) state.GetValue("on");
                    bri = (byte) state.GetValue("bri");
                    hue = state.GetValueOption<ushort>("hue");
                    sat = state.GetValueOption<byte>("sat");
                    effect = state.GetValueOption<string>("effect");
                    xy = state.GetValueOption<float[]>("xy");
                    ct = state.GetValueOption<ushort>("ct");
                    alert = state.Contains("alert")
                        ? AlertStateDesirialize((string) state.GetValue("alert")).Some()
                        : Option.None<AlertState>();
                    colormode = state.Contains("colormode")
                        ? ColorModeStateDesirialize((string) state.GetValue("colormode")).Some()
                        : Option.None<ColorModeState>();
                    reachable = (bool) state.GetValue("reachable");
                }

                public JObject Serialize()
                {
                    JObject state = new JObject {{"on", on}, {"bri", bri}};

                    if (hue.HasValue) state.Add("hue", hue.ValueOr(0));
                    if (sat.HasValue) state.Add("sat", sat.ValueOr(0));
                    if (effect.HasValue) state.Add("effect", effect.ValueOr(""));
                    if (xy.HasValue) state.Add("xy", new JArray(xy.ValueOr(new float[0])));
                    if (ct.HasValue) state.Add("ct", ct.ValueOr(0));
                    if (alert.HasValue) state.Add("alert", AlertStateSerialize(alert));
                    if (colormode.HasValue) state.Add("colormode", ColorModeStateSerialize(colormode));
                    
                    state.Add("reachable", reachable);

                    return state;
                }

                public override string ToString()
                {
                    return JsonConvert.SerializeObject(Serialize());
                }
            }

            public HueWrapper root = null;

            private string Header => $"{root.Header}/lights/{sequentialID}";
            private HttpClient httpClient => root.httpClient;
            private void Set(string name, object value) { SetValue(httpClient, Header, name, value);}

            public int sequentialID = -1;

            public StateStruct state;
            public StateStruct State
            {
                get { return state; }
                set { Set("state", state = value);}
            }

            private readonly string type;

            private string name;
            /// <summary>
            /// Range(0,32)
            /// </summary>
            public string Name
            {
                get { return name; }
                set { Assert.True(value!=null); Assert.Range(value.Length, 0, 32); Set("name", name=value);}
            }

            private readonly string modelid;
            public string ModelID => modelid;

            private readonly Option<string> uniqueid;
            public Option<string> UniqueID => uniqueid;

            private readonly Option<string> manufacturername;
            public Option<string> ManufacturerName => manufacturername;

            private readonly Option<string> luminaireuniqueid;
            public Option<string> LuminairUniqueID => luminaireuniqueid;

            private readonly string swversion;
            public string SoftwareVersion => swversion;

            public HueLamp(JObject lamp, HueWrapper root, int sequentialID)
            {
                this.root = root;
                this.sequentialID = sequentialID;

                state = new StateStruct((JObject)lamp["state"], this);
                type = (string) lamp["type"];
                name = (string) lamp["name"];
                modelid = (string) lamp["modelid"];
                uniqueid = lamp.GetValueOption<string>("uniqueid");
                manufacturername = lamp.GetValueOption<string>("manufacturername");
                luminaireuniqueid = lamp.GetValueOption<string>("luminaireuniqueid");
                swversion = (string) lamp["swversion"];
            }
        }

        private HttpClient httpClient = new HttpClient();
        private Dictionary<int, HueLamp> lamps;

        private string baseAddress = null;
        private string username = null;
        private string Header => baseAddress == null ? "" : username == null ? baseAddress : baseAddress + "/" + username;

        public int LampCount => lamps.Count;
        public HueLamp this[int i] => lamps[i];

        private HueWrapper(string baseAddress)
        {
            if (baseAddress == null)
                throw new ArgumentNullException("baseAddress may not be null");
            this.baseAddress = baseAddress;
            httpClient.BaseAddress = new Uri(baseAddress);
        }

        private void SetLamps(string lampsJson)
        {
            if (lamps != null)
                throw new InvalidOperationException("lamps may only be set once");
            lamps = new Dictionary<int, HueLamp>();
            JObject lampsDynamic = ((dynamic)JsonConvert.DeserializeObject(lampsJson)).lights;
            foreach (KeyValuePair<string, JToken> element in lampsDynamic)
            {
                int sequentialID = int.Parse(element.Key);

                lamps.Add(sequentialID, new HueLamp((JObject)element.Value, this, sequentialID));
            }
        }

        public static Tuple<HueWrapper,string> CreateHueWrapper(string baseAdress, string username = null)
        {
            try
            {
                baseAdress = $"http://{baseAdress}/api";
                HueWrapper hueWrapper = new HueWrapper(baseAdress);
                JObject idJObject = null;
                if (username == null)
                {
                    do
                    {
                        if (idJObject != null)
                        {
                            Console.WriteLine($"ERROR: {idJObject["error"]["description"]}");
                            Thread.Sleep(100);
                        }
                        HttpResponseMessage idRaw =

                            hueWrapper.httpClient.PostAsync("",
                                new StringContent("{\"devicetype\":\"LoveHue#Flobo\"}")).Result;
                        string idString = ((StreamContent) idRaw.Content).ReadAsStringAsync().Result;
                        idString = idString.Substring(1, idString.Length - 2);
                        idJObject = (JObject) JsonConvert.DeserializeObject(idString);
                    } while (idJObject["success"] == null);

                    hueWrapper.username = (string) idJObject["success"]["username"];
                }
                else
                    hueWrapper.username = username;

                Console.WriteLine($"CONNECTION ESTEBLISHED: {hueWrapper.username}");
                string lampsJson = hueWrapper.httpClient.GetStringAsync(hueWrapper.Header).Result;
                hueWrapper.SetLamps(lampsJson);
                foreach (KeyValuePair<int, HueLamp> hueWrapperLamp in hueWrapper.lamps)
                {
                    Console.WriteLine(hueWrapperLamp.ToString());
                }
                return new Tuple<HueWrapper, string>(hueWrapper, hueWrapper.username);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public IEnumerator<HueLamp> GetEnumerator()
        {
            return lamps.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
