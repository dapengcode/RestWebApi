using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestWebApi.Sign;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var s = Signer.ToMD5("/api/productmonth/toplistwithmom20201019135822555");
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
