using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Lextm.SharpSnmpLib.Messaging;

namespace NET.Tools.Snmp
{
	public interface ISnmpClient
	{
		string RemoteHost { get; set; }
		int RemotePort { get; set; }
		SnmpVersion SnmpVersion { get; set; }
		string Community { get; set; }

		/// <summary>
		/// Timeout in seconds
		/// </summary>
		int Timeout { get; set; }
		int NumberOfRetries { get; set; }

		SnmpAuthenticationProtocol AuthenticationProtocol { get; set; }
		string AuthenticationPassword { get; set; }
		string Username { get; set; }
		string Password { get; set; }
		SnmpEncryptionProtocol EncryptionProtocol { get; set; }
		string EncryptionPassword { get; set; }
		int RemoteEngineBoots { get; set; }
		int RemoteEngineTime { get; set; }

		SnmpData Get(string oid);

#if NET40
		bool GetAsync(string oid, object userToken);
#else
		ValueTask<SnmpData> GetAsync(string oid);
#endif
		//bool GetAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback);
		SnmpData GetNext();
		ValueTask<SnmpData> GetNextAsync();
		SnmpData GetNext(string oid);
		//bool GetNextAsync(object userToken);
		ValueTask<SnmpData> GetNextAsync(string oid);
		//bool GetNextAsync(string oid, object userToken);
		//bool GetNextAsync(object userToken, SnmpResponseEventHandler responseCallback);
		//bool GetNextAsync(string oid, object userToken, SnmpResponseEventHandler responseCallback);
		SnmpData[] GetBulk(int nonRepeaters, int maxRepetitions, params string[] oids);
		ValueTask<SnmpData[]> GetBulkAsync(int nonRepeaters, int maxRepetitions, params string[] oids);
		//bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, params string[] oids);
		//bool GetBulkAsync(int nonRepeaters, int maxRepetitions, object userToken, SnmpResponseEventHandler responseCallback, params string[] oids);
		bool Set(string oid, object value);
		ValueTask<bool> SetAsync(string oid, object value);
		//bool SetAsync(string oid, object value, object token, SnmpResponseEventHandler responseCallback);

		/// <summary>
		/// Perform V3 Discovery
		/// </summary>
		void Discover();

		event SnmpResponseEventHandler OnResponse;
	}
}
