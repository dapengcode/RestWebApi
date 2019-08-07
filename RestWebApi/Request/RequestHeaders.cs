using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestWebApi.Request
{
    internal class RequestHeaders : NameValueCollection
    {
        private readonly System.Net.Http.Headers.HttpHeaders _headers;
        public RequestHeaders(System.Net.Http.Headers.HttpHeaders headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }
            _headers = headers;
            IsReadOnly = true;
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return AllKeys.GetEnumerator();
        }

        private string[] _keys;
        public override string[] AllKeys
        {
            get { return _keys ?? (_keys = _headers.Select(it => it.Key).ToArray()); }
        }

        public override string GetKey(int index)
        {
            if (index < 0)
            {
                return null;
            }
            var keys = AllKeys;
            return index >= keys.Length ? null : keys[index];
        }

        public override string Get(int index)
        {
            return Get(GetKey(index));
        }

        public override string Get(string name)
        {
            if (name == null) return null;
            var values = _headers.GetValues(name);
            return values == null ? null : string.Join(",", values);
        }

        public override string[] GetValues(int index)
        {
            return GetValues(GetKey(index));
        }

        public override string[] GetValues(string name)
        {
            if (name == null) return null;
            var values = _headers.GetValues(name);
            return values?.ToArray();
        }
    }
}
