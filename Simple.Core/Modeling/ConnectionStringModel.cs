using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Collections;

//[assembly: CLSCompliant(true)]
namespace Simple.Modeling
{
	public class ConnectionStringModel<TObjectModel> : ModelElement, IConnectionStringModel 
        where TObjectModel : ConnectionStringModel<TObjectModel>, new()
    {
		private static TObjectModel instance = null;
		private static object lockObject = new object();

		public ConnectionStringModel()
        {
            this.CreatePropertyModelDictionary(new ConnectionStringPropertyModel());
        }

        public ModelDictionary<string, ConnectionStringPropertyModel> Properties
        {
            get { return this.GetModelDictionary<string, ConnectionStringPropertyModel>(); }
            set { this.SetModelDictionary<string, ConnectionStringPropertyModel>(value); }
        }

        public void CreatePropertyModelDictionary(object objectModelFieldHolder)
        {
            this.Properties = this.CreateModelDictionary<string, ConnectionStringPropertyModel>(objectModelFieldHolder, pm => pm.Name);
        }

		public static TObjectModel Instance
		{
			get
			{
				lock (lockObject)
				{
					if (instance == null)
						instance = new TObjectModel();
				}

				return instance;
			}
		}

		IDictionary<string, IConnectionStringPropertyModel> IConnectionStringModel.Properties
        {
            get { return this.Properties.AsCustom<IConnectionStringPropertyModel>().AsReadOnly(); }

        }
    }

    public class ConnectionStringPropertyModelBase
    {
        public ConnectionStringPropertyModel Protocol        = new ConnectionStringPropertyModel();
        public ConnectionStringPropertyModel ProtocolVersion = new ConnectionStringPropertyModel();
        public ConnectionStringPropertyModel IsEncrypted     = new ConnectionStringPropertyModel();
    }

    public interface IConnectionStringModel : IModelElement
    {
        IDictionary<string, IConnectionStringPropertyModel> Properties { get; }
    }
}
