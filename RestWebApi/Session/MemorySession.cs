using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace RestWebApi.Session
{
    /// <summary>
    /// 基于内存的Session实现机制
    /// </summary>
    public class MemorySession:HttpSessionStateBase,IObserver<string>
    {
        #region 字段
        private static readonly MemoryCache Cache = new MemoryCache("WebApi$MemorySessionCache");
        private string _sessionId;
        private bool _isNewSession;
        private PropertyCollection _items;
        #endregion

        #region Session override

        public override int Timeout { get; set; }
        public override void Abandon()
        {
            Cache.Remove(_sessionId);
            _items.Clear();
            _sessionId = Guid.NewGuid().ToString("N");
        }

        public override void Add(string name, object value)
        {
            _items[name] = value;
        }

        public override HttpSessionStateBase Contents => this;
        public override HttpCookieMode CookieMode => HttpCookieMode.UseCookies;
        public override void CopyTo(Array array, int index)
        {
            if (array != null) ((ICollection)_items).CopyTo(array,index);
        }

        public override bool IsCookieless => false;
        public override bool IsNewSession => _isNewSession;
        public override bool IsSynchronized => false;
        public override NameObjectCollectionBase.KeysCollection Keys => _items.Keys;
        public override bool IsReadOnly => false;
        public override SessionStateMode Mode =>SessionStateMode.Off;
        public override void Remove(string name)
        {
            _items.Remove(name);
        }

        public override void RemoveAll()
        {
            _items.Clear();
        }

        public override void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public override string SessionID => _sessionId;
        public override object SyncRoot => _items;

        public override object this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public override object this[string name]
        {
            get { return _items[name]; }
            set { _items[name] = value; }
        }

        public override IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override int Count => _items.Count;

        #endregion

        #region IObserver
        public void OnNext(string sessionId)
        {
            _sessionId = sessionId;
            if (sessionId != null)
            {
                _items = Cache[sessionId] as PropertyCollection;
                if (_items != null) return;
            }
            _sessionId = Guid.NewGuid().ToString("N");
            _items=new PropertyCollection();
            _isNewSession = true;
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnCompleted()
        {
            var timeout = this.Timeout;
            if (timeout == 0) timeout = 3600;
            Cache.Set(_sessionId,_items,new CacheItemPolicy() {SlidingExpiration=new TimeSpan(0,0,timeout)});
        }
        #endregion  
    }
}
