using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace RestWebApi.Request
{
    internal class JsonFormBody : NameValueCollection
    {
        private readonly object _data;
        private readonly string _json;
        public JsonFormBody(string json)
        {
            _data = JsonConvert.DeserializeObject(json,null, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            _json = json;
        }

        private static object Get(object obj, string name, int? index)
        {
            if (obj.GetType() == typeof(JArray))
            {
                var list = obj as IList;
                if (list != null)
                {
                    return name != null ? Get(list, name) : Get(list, index.Value);
                }

            }
            else if(obj.GetType() == typeof(JObject))
            {
                var map = obj as JToken;
                if (map != null)
                {
                    return name != null ? Get(map, name) : Get(map, index.Value);
                }
            }

            

            
            
            return null;
        }

        private static object Get(IList list, int index)
        {
            return index < list.Count ? list[index] : null;
        }

        private static object Get(IList list, string name)
        {
            int index;
            return int.TryParse(name, out index) ? Get(list, index) : null;
        }

        private static object Get(JToken map, int index)
        {
            return map.Children().ElementAt(index);
        }

        private static object Get(JToken map, string name)
        {
            return map.SelectToken(name);
        }

        private static IEnumerable<string> ParseNames(string name)
        {
            if (name == null)
            {
                yield return null;
                yield break;
            }
            var chars = name.ToCharArray();
            var start = 0;
            for (int i = 0, length = chars.Length; i < length; i++)
            {
                var c = chars[i];
                switch (c)
                {
                    case '[':
                        yield return new string(chars, start, i - start);
                        i++;
                        start = i;
                        while (i < length)
                        {
                            if (chars[i] == ']')
                            {
                                break;
                            }
                            i++;
                        }
                        if (i == length)
                        {
                            yield return null;
                            yield break;
                        }
                        int index;
                        var str = new string(chars, start, i - start);
                        if (int.TryParse(str, out index))
                        {
                            yield return str;
                        }
                        else
                        {
                            yield return null;
                            yield break;
                        }
                        start = i + 1;
                        break;
                    case '.':
                        yield return new string(chars, start, i - start);
                        start = i + 1;
                        break;
                    default:
                        break;
                }
            }
            if (start != 0 && start != chars.Length)
            {
                yield return new string(chars, start, chars.Length - start);
            }
        }

        public override string Get(string name)
        {
            var obj = _data;
            if (_data == null) return null;
            var names = ParseNames(name);
            foreach (var n in names)
            {
                if (n == null)
                {
                    return Get(_data, name, null)?.ToString();
                }
                obj = Get(obj, n, null);
                if (obj == null)
                {
                    return Get(_data, name, null)?.ToString();
                }
            }
            return ReferenceEquals(obj, _data) ? Get(_data, name, null)?.ToString() : obj?.ToString();
        }

        public override string ToString()
        {
            return _json ?? "";
        }
    }
}
