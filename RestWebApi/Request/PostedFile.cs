using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RestWebApi.Request
{
    internal class PostedFile : HttpPostedFileBase
    {
        public int Length { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public Stream Stream { get; set; }

        public override int ContentLength => Length;
        public override string ContentType => Type;
        public override string FileName => Name;
        public override Stream InputStream => Stream;
    }
}
