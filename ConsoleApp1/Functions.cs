using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nine.Commands
{
    public class Functions
    {
        public static string GetUrl(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();

            return text;
        }
    }
}
