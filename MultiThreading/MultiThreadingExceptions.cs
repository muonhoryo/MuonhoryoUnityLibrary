using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuonhoryoLibrary.Unity.Threading
{
    public class InvalidIdReservatorException : Exception
    {
        public InvalidIdReservatorException(string objectName)
        {
            this.objectName = objectName;
        }
        private string objectName;
        public override string Message => "Reservator with name \"" + objectName + "\" has been used.";
    }
}
