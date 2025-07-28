using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
    public class DatastoreConnectionStringBuilderBase
    {
        public DatastoreConnectionStringBuilderBase()
        {
            this.ConnectionTimeout = 20;
        }
        
        public string Username { get; set; }
        public string Password { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}
