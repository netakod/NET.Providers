using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
    public interface IDatastoreConnection
    {
        string ConnectionString { get; set; }
        void Connect();
        void Disconnect();
        bool Connected { get; }
    }
}
