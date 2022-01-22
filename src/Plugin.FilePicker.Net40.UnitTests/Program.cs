using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin.FilePicker.Net40.UnitTests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var filedata = Plugin.FilePicker.CrossFilePicker.Current.PickFile();
            filedata.Wait();
            var fN = filedata.Result.FileName;
            Console.ReadKey();
        }
    }
}
