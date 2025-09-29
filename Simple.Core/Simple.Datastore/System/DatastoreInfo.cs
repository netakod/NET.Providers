using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Modeling;

namespace Simple.Datastore
{
    public class DatastoreInfo
    {
        public DatastoreInfo()
        {
        }

        public DatastoreInfo(DatastoreProviderType providerType, string name)
        {
            this.ProviderType = providerType;
            this.Name = name;
        }

        public string Name { get; set; }
        public DatastoreProviderType ProviderType { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    //public class DatastoreInfoModel : ModelBase<DatastoreInfoModel>
    //{
    //}
}
