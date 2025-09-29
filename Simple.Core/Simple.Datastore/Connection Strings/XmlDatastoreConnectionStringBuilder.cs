using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;
using Simple.Serialization;

namespace Simple.Datastore
{
    public class XmlDatastoreConnectionStringBuilder : ConnectionStringBuilder
    {
        public const string StrDataSourceFolder = "DataSourceFolder";

        public string DataSourceFolder
        {
            get { return this.GetPropertyValue<string>(StrDataSourceFolder); }
            set { this.SetPropertyValue(StrDataSourceFolder, value); }
        }
    }
}
