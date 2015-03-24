using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public class UpdateProcessingFileChangedEventArgs : EventArgs
    {
        public UpdateProcessingFileChangedEventArgs(string fileName)
        {
            FileName = fileName;            
        }
       
        public string FileName {get;set;}             
    }
}