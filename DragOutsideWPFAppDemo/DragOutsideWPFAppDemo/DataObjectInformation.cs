using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragOutsideWPFAppDemo
{
    [Serializable]
    public class DataObjectInformation
    {
        public string InfoValue { get; }

        public DataObjectInformation(string infoValue)
        {
            this.InfoValue = infoValue;
        }
    }

}
