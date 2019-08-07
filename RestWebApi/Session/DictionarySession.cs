using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace RestWebApi.Session
{
    internal class DictionarySession : HttpSessionStateBase
    {
        private readonly SessionStateItemCollection _items = new SessionStateItemCollection();

        public override void Abandon()
        {
            _items.Clear();
        }

        public override void Add(string key, object value)
        {
            _items[key] = value;
        }

        public override void Clear()
        {
            _items.Clear();
        }

        public override int Count => _items.Count;

        public override IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override bool IsNewSession => true;

        public override NameObjectCollectionBase.KeysCollection Keys => _items.Keys;

        public override void Remove(string key)
        {
            _items.Remove(key);
        }

        public override string SessionID => "";

        public override object this[string key]
        {
            get
            {
                return _items[key];
            }
            set
            {
                _items[key] = value;
            }
        }


        public override HttpSessionStateBase Contents => this;

        public override HttpCookieMode CookieMode => HttpCookieMode.UseCookies;

        public override void CopyTo(Array array, int index)
        {
            if (array != null) ((ICollection)_items).CopyTo(array, index);
        }

        public override bool IsCookieless => false;

        public override bool IsReadOnly => false;

        public override bool IsSynchronized => false;

        public override System.Web.SessionState.SessionStateMode Mode => System.Web.SessionState.SessionStateMode.Off;

        public override void RemoveAll()
        {
            _items.Clear();
        }

        public override void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public override HttpStaticObjectsCollectionBase StaticObjects => null;

        public override object SyncRoot => _items;

        public override object this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }
    }
}
