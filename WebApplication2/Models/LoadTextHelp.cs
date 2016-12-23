using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class LoadTextHelp
    {
        
        public LoadTextHelp() { }
        
        public string Load(string path)
        {
            string text;
            using (StreamReader readtext = new StreamReader(path))
            {
                text = readtext.ReadToEnd();
            }
            return text;
        }
    }
}