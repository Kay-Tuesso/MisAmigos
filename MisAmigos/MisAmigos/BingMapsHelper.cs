using System;
using System.Collections.Generic;
using System.Text;

namespace MisAmigos.Helpers
{
    public class BingMapsHelper
    {
        delegate void Operation(Uri url);
        public bool Run(Uri url)
        {
            bool success = false;
            Operation op = GetAsync;
            ExecOperation(url, op);
            return success;
        }
               
        public static void GetAsync(Uri url)
        {
            Console.WriteLine($"Url: {url.AbsoluteUri}");
            Console.WriteLine($"Url: {url.Fragment}");
        }

        static void ExecOperation(Uri url, Operation op)
        {
            op(url);
        }
    }
}
