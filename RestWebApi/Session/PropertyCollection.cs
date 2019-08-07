using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RestWebApi.Session
{
    /// <summary>
    /// 属性集合
    /// </summary>
    public class PropertyCollection:NameObjectCollectionBase
    {
        public bool ContainsKey(string name) {
            var val = BaseGet(name);
            return val != null;
        }
        public IEnumerable<KeyValuePair<string, T>> GetEnumerable<T>() {
            for (var i = 0; i < this.Count; i++)
            {
                var name = BaseGetKey(i);
                var val = BaseGet(i);
                if (val is T) {
                    yield return new KeyValuePair<string, T>(name, (T)val);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, T>> GetEnumerable<T>(string prefix) {
            for (int i = 0; i < this.Count; i++)
            {
                var name = BaseGetKey(i);
                var val = BaseGet(i);
                if (val is T && name.StartsWith(prefix)) {
                    yield return new KeyValuePair<string, T>(name, (T)val);
                }
            }
        }
        

        public IEnumerable<KeyValuePair<string, string>> GetEnumerable(string prefix)
        {
            var length = this.Count;
            for (int i = 0; i < length; i++)
            {
                var name = BaseGetKey(i);
                var val = BaseGet(name) as string;
                if (val != null && name.StartsWith(prefix))
                {
                    yield return new KeyValuePair<string, string>(name, val);
                }
            }
        }

        public object this[int index] {
            get { return BaseGet(index); }
            set { BaseSet(index,value); }
        }
        public object this[string name] {
            get { return BaseGet(name); }
            set { BaseSet(name, value); }
        }
        public void Remove(string name) {
            BaseRemove(name);
        }
        public void Clear()
        {
            BaseClear();
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}