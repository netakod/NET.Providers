using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Datastore
{
	public struct TableInfo
	{
		//public TableInfo(bool isSystem)
		//{
		//	this.TableId = 0;
		//	this.TableName = null;
		//	this.IsSystem = isSystem;
		//}

		public TableInfo(int tableId, bool isSystemTable = false)
		{
			//this.BaseTableInfo = default(TableInfo);
			this.TableId = tableId;
			this.TableName = String.Empty;
			this.IsSystemTable = isSystemTable;

			if (tableId <= 0 && !isSystemTable)
				throw new ArgumentException("tableId must be greater than zero");
		}

		public TableInfo(int tableId, string tableName)
			: this(tableId)
		{
			this.TableName = tableName;
			this.IsSystemTable = false;
		}

		public bool IsSystemTable { get; private set; }

		//public TableInfo(int tableId, TableInfo baseTableInfo)
		//{
		//	this.BaseTableInfo = baseTableInfo;
		//	baseTableInfo.TableId = tableId;
		//	this.TableId = this.BaseTableInfo.TableId;
		//	this.TableName = this.BaseTableInfo.TableName;
		//}

		//private TableInfo BaseTableInfo { get; set; }

		public int TableId { get; set; }
		public string TableName { get; set; }
	}
}
