using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace MerxProject.Models
{
    public class PaypalLogger
    {
        public static string LogDirectory = Environment.CurrentDirectory;
        public static void Log(String message)
        {
            try
            {
                StreamWriter strw = new StreamWriter(LogDirectory + "\\PaypalError.log", true);
                strw.WriteLine("[0]--->[1]", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + message);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}