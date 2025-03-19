using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Email.Main.LogFileAddress
{
    public class TextLogFileAddress
    {
        private string filePath = @"C:\EmailApiLogFile\EmailApiJob.txt";
        public string FilePath
        {
            get { return filePath; }
        }
    }
}