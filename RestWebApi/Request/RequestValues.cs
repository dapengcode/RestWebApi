using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestWebApi.Request
{
    public sealed class RequestValues:UnvalidatedRequestValuesBase,IRequestValues
    {
        private static readonly NameValueCollection Empty = new NameValueCollection();
        private NameValueCollection _form;
        private NameValueCollection _queryString;
        private HttpFileCollectionBase _files;
        private NameValueCollection _headers;
        private HttpCookieCollection _cookies;
        private NameValueCollection _routeDatas;
        private Uri _url;
        private string _path;
        private string _pathInfo;
        private string _rawUrl;
        private string _contentType;
        private byte[] _formBody;
        public override HttpCookieCollection Cookies => _cookies;
        HttpCookieCollection IRequestValues.Cookies
        {
            get { return _cookies; }
            set { _cookies = value; }
        } 
        public override HttpFileCollectionBase Files => _files;

        HttpFileCollectionBase IRequestValues.Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public override NameValueCollection Form => _form;

        NameValueCollection IRequestValues.Form
        {
            get { return _form; }
            set { _form = value; }
        }

        public override NameValueCollection Headers => _headers;

        NameValueCollection IRequestValues.Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        public override string Path => _path;
        string IRequestValues.Path {
            get { return _path; }
            set { _path = value; }
        }

        public override string PathInfo => _pathInfo;
        string IRequestValues.PathInfo {
            get { return _pathInfo;}
            set { _pathInfo = value; }
        }

        public override NameValueCollection QueryString => _queryString;

        NameValueCollection IRequestValues.QueryString{
            get { return _queryString;}
            set { _queryString = value; }
        }

        public override string RawUrl => _rawUrl;
        string IRequestValues.RawUrl {
            get { return _rawUrl; }
            set { _rawUrl = value; }
        }

        public override Uri Url => _url;
        Uri IRequestValues.Url {
            get { return _url; }
            set { _url = value; }
        }

        public string ContentType => _contentType;

        string IRequestValues.ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        public NameValueCollection RouteDatas => _routeDatas;
        NameValueCollection IRequestValues.RouteDatas {
            get { return _routeDatas; }
            set { _routeDatas = value; }
        }

        public byte[] FormBody => _formBody;
        byte[] IRequestValues.FormBody {
            get { return _formBody;}
            set { _formBody = value; }
        }
        public override string this[string field] => RouteDatas[field] ?? Form[field] ?? QueryString[field];
        public RequestValues()
        {
            _form = Empty;
            _queryString = Empty;
            _headers = Empty;
            _files = PostedFileCollection.Empty;
            _cookies = new HttpCookieCollection();
            _routeDatas = Empty;
        }
        public RequestValues(UnvalidatedRequestValuesBase requestvalues)
        {
            _form = requestvalues.Form;
            _queryString = requestvalues.QueryString;
            _headers = requestvalues.Headers;
            _files = requestvalues.Files;
            _cookies = requestvalues.Cookies;
            _url = requestvalues.Url;
            _rawUrl = requestvalues.RawUrl;
            _path = requestvalues.Path;
            _pathInfo = requestvalues.PathInfo;
            _routeDatas = Empty;
        }
    }
}
