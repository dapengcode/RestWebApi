using System;
using System.Collections.Generic;
using System.Web;

namespace RestWebApi.Request
{
    internal class PostedFileCollection : HttpFileCollectionBase
    {
        public PostedFileCollection()
        {

        }
        public static readonly PostedFileCollection Empty = new PostedFileCollection() { IsReadOnly = true };

        internal void Add(string name, PostedFile file)
        {
            var obj = base.BaseGet(name);
            if (obj != null)
            {
                throw new ArgumentException("多个文件参数名重复");
            }
            base.BaseAdd(name, file);
            _count++;
        }

        internal void ReadOnly()
        {
            IsReadOnly = true;
        }


        private int _count;

        #region 重写
        public override int Count => _count;

        public override string[] AllKeys => base.BaseGetAllKeys();

        public override bool IsSynchronized => false;

        public override object SyncRoot => this;

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return base.BaseGetAllValues().GetEnumerator();
        }
        public override string GetKey(int index)
        {
            return base.GetKey(index);
        }
        public override IList<HttpPostedFileBase> GetMultiple(string name)
        {
            var file = this[name];
            return file == null ? new HttpPostedFileBase[0] : new HttpPostedFileBase[] { file };
        }
        public override HttpPostedFileBase this[int index] => (HttpPostedFileBase)base.BaseGet(index);

        public override HttpPostedFileBase this[string name] => (HttpPostedFileBase)base.BaseGet(name);

        public override HttpPostedFileBase Get(int index)
        {
            return (HttpPostedFileBase)base.BaseGet(index);
        }
        public override HttpPostedFileBase Get(string name)
        {
            return (HttpPostedFileBase)base.BaseGet(name);
        }
        #endregion
    }
}
