//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Simple
//{

//	//[Serializable]
//	/// <summary>
//	/// SimpleObject Key is identified by unigue Guid and composed by TableKey (2 bytes), ClientId (48 bytes) and ObjectId (64 bytes).
//	/// </summary>
//	public struct SimpleObjectKey //: ISerializable
//	{
//		private int tableId;
//		private long clientId, objectId;

//		public static SimpleObjectKey Empty = new SimpleObjectKey(0, 0, 0);

//		public const string StringTableId = "TableId";
//		public const string StringClientId = "ClientId";
//		public const string StringObjectId = "ObjectId";
//		public const string StringGuid = "Guid";

//		public SimpleObjectKey(Guid guid)
//			: this()
//		{
//			this.Guid = guid;

//			this.tableId = SimpleObjectKey.GetTableId(guid);
//			this.clientId = SimpleObjectKey.GetClientId(guid);
//			this.objectId = SimpleObjectKey.GetObjectId(guid);
//		}

//		/// <summary>
//		/// Create SimpleObjectKey struct that uniqely describes an object as its key.
//		/// </summary>
//		/// <param name="tableId">Unique object type identifier. It is truncated and stored as Int16 value.</param>
//		/// <param name="clientId">The client identifier.</param>
//		/// <param name="objectId">The object identifier.</param>
//		public SimpleObjectKey(int tableId, long clientId, long objectId)
//			: this()
//		{
//			//TableId = tableId;
//			this.tableId = tableId;
//			this.clientId = clientId; //(clientId << 16) >> 16; // clientId is only 48 bytes in lenght.
//			this.objectId = objectId;

//			byte[] guidBytes = new byte[16];
//			byte[] tableIdBytes = BitConverter.GetBytes(tableId);
//			byte[] clientIdBytes = BitConverter.GetBytes(clientId);
//			byte[] objectIdBytes = BitConverter.GetBytes(objectId);


//			//if (BitConverter.IsLittleEndian)
//			//{
//			//	Array.Reverse(clientIdBytes);
//			//	Array.Reverse(objectIdBytes);
//			//}

//			//randomBytes = new byte[8];
//			//SequentialGuid.RandomGenerator.GetBytes(randomBytes);

//			Buffer.BlockCopy(tableIdBytes, 0, guidBytes, 0, 2);
//			Buffer.BlockCopy(clientIdBytes, 0, guidBytes, 2, 6);
//			Buffer.BlockCopy(objectIdBytes, 0, guidBytes, 8, 8);

//			this.Guid = new Guid(guidBytes);
//		}

//		//public SimpleObjectKey(SerializationInfo info, StreamingContext context)
//		//{
//		//    this.TableId = (int)info.GetValue(strTableId, typeof(int));
//		//    this.ObjectId = (long)info.GetValue(strObjectId, typeof(long));
//		//    this.CreatorServerId = (int)info.GetValue(strCreatorServerId, typeof(int));
//		//}

//		//public int TableId { get; private set; }
//		public Guid Guid { get; private set; }

//		//public static SimpleObjectKey GetEmptyKey(int objectTypeId)
//		//{
//		//	return new SimpleObjectKey(objectTypeId, 0);
//		//}

//		public static int GetTableId(Guid guid)
//		{
//			//byte[] byteArray = guid.ToByteArray();
//			//Array.Reverse(byteArray);

//			return BitConverter.ToUInt16(guid.ToByteArray(), 0);
//		}

//		public static long GetClientId(Guid guid)
//		{
//			//byte[] byteArray = guid.ToByteArray();
//			//Array.Reverse(byteArray);

//			return BitConverter.ToInt64(guid.ToByteArray(), 0) >> 16;
//		}

//		public static long GetObjectId(Guid guid)
//		{
//			//byte[] byteArray = guid.ToByteArray();
//			//Array.Reverse(byteArray);

//			return BitConverter.ToInt64(guid.ToByteArray(), 8);

//			//			return BitConverter.ToInt64(guid.ToByteArray(), 7);
//		}

//		public int GetTableId()
//		{
//			return this.tableId;
//		}

//		public long GetClientId()
//		{
//			return this.clientId;
//		}

//		public long GetObjectId()
//		{
//			return this.objectId;
//		}

//		public override bool Equals(object obj)
//		{
//			// If parameter is null return false.
//			if (obj == null)
//				return false;

//			// If parameter cannot be cast to Point return false.
//			SimpleObjectKey objectKeyToCompare = (SimpleObjectKey)obj;

//			if ((object)objectKeyToCompare == null)
//				return false;

//			// If both are the same instance, return true.
//			if (System.Object.ReferenceEquals(this, objectKeyToCompare))
//				return true;

//			// Return true if the key fields match.
//			return this.Guid == objectKeyToCompare.Guid;
//		}

//		public bool Equals(SimpleObjectKey objectKey)
//		{
//			// If parameter is null return false:
//			if ((object)objectKey == null)
//				return false;

//			// If both are null, or both are same instance, return true.
//			if (System.Object.ReferenceEquals(this, objectKey))
//				return true;

//			// Return true if the key fields match.
//			return this.Guid == objectKey.Guid;
//		}

//		public override int GetHashCode()
//		{
//			return this.Guid.GetHashCode(); //^ this.TableId.GetHashCode();
//		}

//		public static bool operator ==(SimpleObjectKey a, SimpleObjectKey b)
//		{
//			// If both are null, or both are same instance, return true.
//			if (System.Object.ReferenceEquals(a, b))
//				return true;

//			// If one is null, but not both, return false.
//			if ((object)a == null ^ (object)b == null)
//				return false;

//			// Return true if the key fields match.
//			return a.Guid == b.Guid;
//		}

//		public static bool operator !=(SimpleObjectKey a, SimpleObjectKey b)
//		{
//			return !(a == b);
//		}

//		public static string GetKeyGuidString(Guid simpleObjectKeyGuid)
//		{
//			return GetTableId(simpleObjectKeyGuid) + "." + GetClientId(simpleObjectKeyGuid) + "." + GetObjectId(simpleObjectKeyGuid);
//		}

//		public override string ToString()
//		{
//			return this.GetTableId().ToString() + "." + this.GetClientId().ToString() + "." + this.GetObjectId().ToString();
//		}

//		//void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
//		//{
//		//    info.AddValue(strTableId, this.TableId);
//		//    info.AddValue(strObjectId, this.ObjectId);
//		//    info.AddValue(strCreatorServerId, this.CreatorServerId);
//		//}
//	}
//}
