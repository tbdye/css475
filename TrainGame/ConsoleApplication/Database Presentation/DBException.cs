using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Presentation
{
    public class DBException : Exception
    {
        private string Message { get; set; }

        public DBException()
        {
        }

        public DBException(string msg)
            : base(msg)
        {
            this.Message = msg;
        }

        public DBException(string msg, Exception inner)
            : base(msg, inner)
        {
            this.Message = msg;
        }
    }
}