using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Common
{
    public class FileToBase64String
    {
        public static string Convert(string filePath)
        {
            FileStream inFile;
            byte[] binaryData;
            string base64String = string.Empty;

            try
            {
                inFile = new FileStream(filePath,
                                          System.IO.FileMode.Open,
                                          System.IO.FileAccess.Read);
                binaryData = new Byte[inFile.Length];
                long bytesRead = inFile.Read(binaryData, 0,
                                     (int)inFile.Length);
                inFile.Close();
            }
            catch
            {            
                return String.Empty;
            }
           
            try
            {
                base64String =
                  System.Convert.ToBase64String(binaryData,
                                         0,
                                         binaryData.Length);
            }
            catch (ArgumentNullException)
            {               
            }

            return base64String;
        }
    }
}
