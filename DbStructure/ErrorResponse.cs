using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs
{

    public class ErrorResponse<T>
    {
        public T OffendingObject { get; set; }
        public string error_description { get; set; }

        public ErrorResponse(string error, T cause)
        {
            error_description = error;
            OffendingObject = cause; 
        }

    }
}

