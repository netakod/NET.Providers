using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Simple;
using NET.Tools.Web;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikSwOS)]
    public class WebClientMikroTikSwOS : NET.Tools.Web.WebClient
    {
		#region |   Private Values  |

		private Dictionary<string, string> snmpDictionary = null;
		private string snmpConfigLine = null;
		private Dictionary<string, string> linkDictionary = null;
		private string linkConfigLine = null;
		private Dictionary<string, string>? forwardingDictionary = null;
		private string forwardingConfigLine = null;
		private bool? isForwardingConfigSupported = null;
		private List<Dictionary<string, string>> vlanConfigSegmentDictionary = null;
		private string vlanConfigLine = null;
		private Dictionary<string, string> systemDictionary = null;
		private string systemConfigLine = null;

		#endregion |   Private Values  |

		#region |   Constructors and Initialization   |

		public WebClientMikroTikSwOS()
        {
            this.connectAction = "index.html";
            this.logOffAction = "logout.html";
			//this.logOffRefererAction = "left2.htm";
			this.getContentType = "text/plain"; // "application/x-javascript";
			this.postContentType = "text/plain";
			this.defaultRefererAction = "index.html";
		}

		#endregion |   Constructors and Initialization   |

		#region |   Public Methods   |

		public async ValueTask<Dictionary<string, string>> GetSnmpDictionary()
		{
			if (this.snmpDictionary == null)
			{
				HttpWebResponse response = await this.SendGetRequestAsync("snmp.b");
				string responseText = await response.GetResponseTextAsync();

				this.snmpDictionary = this.GetConfigDictionary(responseText);
				this.snmpConfigLine = this.CreateConfigTextFromConfigDictionary(this.snmpDictionary);
			}

			return this.snmpDictionary;
		}

		public async ValueTask<Dictionary<string, string>> GetLinkDictionary()
		{
			if (this.linkDictionary == null)
			{
				HttpWebResponse response = await this.SendGetRequestAsync("link.b");
				string responseText = await response.GetResponseTextAsync();

				this.linkDictionary = this.GetConfigDictionary(responseText);
				this.linkConfigLine = this.CreateConfigTextFromConfigDictionary(this.linkDictionary);
			}

			return this.linkDictionary;
		}

		public async ValueTask<Dictionary<string, string>?> GetForwardingDictionary()
		{
			if (this.forwardingDictionary == null)
			{
				HttpWebResponse response = await this.SendGetRequestAsync("fwd.b");

				if (response.StatusCode != HttpStatusCode.OK)
					return null; // Get fwd.b is not supported for older versions of the Mikrotik SwOS software (fwd.b features is implemented in link.b)

				string responseText = await response.GetResponseTextAsync();

				this.forwardingDictionary = this.GetConfigDictionary(responseText);
				this.forwardingConfigLine = this.CreateConfigTextFromConfigDictionary(this.forwardingDictionary);
			}

			return this.forwardingDictionary;
		}

		public async ValueTask<bool> IsForwardingConfigSupported()
		{
			if (this.isForwardingConfigSupported is null)
				this.isForwardingConfigSupported = (await this.GetForwardingDictionary() != null);

			return (bool)this.isForwardingConfigSupported;
		}

		public async ValueTask<Dictionary<string, string>> GetDvidDictionary() => (await this.IsForwardingConfigSupported()) ? await this.GetForwardingDictionary()
																															 : await this.GetLinkDictionary();
		public async ValueTask<List<Dictionary<string, string>>> GetVlanConfigSegments()
		{
			if (this.vlanConfigSegmentDictionary == null)
			{
				HttpWebResponse response = await this.SendGetRequestAsync("vlan.b");
				string responseText = await response.GetResponseTextAsync();
				List<string> vlanConfigSegments = this.GetConfigSegments(responseText);
				
				this.vlanConfigSegmentDictionary = this.GetConfigSegmentDictionary(vlanConfigSegments);
				this.vlanConfigLine = this.CreateConfigTextFromConfigSegmentDictionary(this.vlanConfigSegmentDictionary);
			}

			return this.vlanConfigSegmentDictionary;
		}

		public async ValueTask<Dictionary<string, string>> GetSystemDictionary()
		{
			if (this.systemDictionary == null)
			{
				HttpWebResponse response = await this.SendGetRequestAsync("sys.b");
				string responseText = await response.GetResponseTextAsync();

				this.systemDictionary = this.GetConfigDictionary(responseText);
				this.systemConfigLine = this.CreateConfigTextFromConfigDictionary(this.systemDictionary);
			}

			return this.systemDictionary;
		}

		public override async ValueTask FinishUpdateAsync()
		{
			await base.FinishUpdateAsync();

			if (this.snmpDictionary != null)
			{
				string postData = this.CreateConfigTextFromConfigDictionary(this.snmpDictionary);

				if (postData != this.snmpConfigLine)
				{
					HttpWebResponse response = await this.SendPostRequestAsync("snmp.b", "{" + postData + "}");

					if (response.StatusCode == HttpStatusCode.OK)
						this.snmpConfigLine = postData;
				}
			}

			if (this.linkDictionary != null)
			{
				string postData = this.CreateConfigTextFromConfigDictionary(this.linkDictionary);

				if (postData != this.linkConfigLine)
				{
					HttpWebResponse response = await this.SendPostRequestAsync("link.b", "{" + postData + "}");

					if (response.StatusCode == HttpStatusCode.OK)
						this.linkConfigLine = postData;
				}
			}

			if (this.forwardingDictionary != null)
			{
				string postData = this.CreateConfigTextFromConfigDictionary(this.forwardingDictionary);

				if (postData != this.forwardingConfigLine)
				{
					HttpWebResponse response = await this.SendPostRequestAsync("fwd.b", "{" + postData + "}");

					if (response.StatusCode == HttpStatusCode.OK)
						this.forwardingConfigLine = postData;
				}
			}

			if (this.vlanConfigSegmentDictionary != null)
			{
				//string[] vlanConfigLines = new string[this.vlanConfigSegmentDictionary.Count];

				//for (int i = 0; i < this.vlanConfigSegmentDictionary.Count; i++)
				//	vlanConfigLines[i] = this.vlanConfigSegmentDictionary[i].ToConfigLine();

				string postData = this.CreateConfigTextFromConfigSegmentDictionary(this.vlanConfigSegmentDictionary);

				if (postData != this.vlanConfigLine)
				{
					HttpWebResponse response = await this.SendPostRequestAsync("vlan.b", postData);

					if (response.StatusCode == HttpStatusCode.OK)
						this.vlanConfigLine = postData;
				}
			}

			if (this.systemDictionary != null)
			{
				string postData = this.CreateConfigTextFromConfigDictionary(this.systemDictionary);

				if (postData != this.systemConfigLine)
				{
					HttpWebResponse response = await this.SendPostRequestAsync("sys.b", "{" + postData + "}");

					if (response.StatusCode == HttpStatusCode.OK)
						this.systemConfigLine = postData;
				}
			}
		}

		#endregion |   Public Methods   |

		#region |   Public MikroTik Helper Methods  |


		/// <summary>
		/// Parse the multiple value string. String example: "[0x02,0x01,0x01,0x01,0x01,0x01]"
		/// </summary>
		/// <param name="value">The value to parse.</param>
		/// <returns>List of single values.</returns>
		public string[] ParseMultipleValues(string value)
		{
			value = this.RemoveParenthesesEnclosure(value);

			return value.Split(',');
		}

		/// <summary>
		/// Convert MikroTik style array filed with sequencial data to string separated with the separator. 
		/// Example: "0x00,0x00,0x00,0x00,0x00,0x00"
		/// </summary>
		/// <param name="array">String array with data values</param>
		/// <param name="separator">The value separator.</param>
		/// <returns>String value.</returns>
		public string CreateMultipleValueText(string[] array, string separator = ",")
		{
			string result = String.Empty;

			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
					result += separator;

				result += array[i];
			}

			return result;
		}

		/// <summary>
		/// Reads the string writen in ASCII format, e.g. "'343136353031413245393731'", where the numbers represents string ASCII values of string "MikroTik".
		/// </summary>
		/// <param name="value">The input string in ASCII format enclosed with apostrophes</param>
		/// <returns>String original value.</returns>
		public string ConvertAsciiToString(string value)
		{
			if (value == null)
				value = String.Empty;

			// remove ', if exists
			value = value.Replace("'", "");

			//int start = value.IndexOf("'"); // '\'');
			//int end = value.IndexOf("'", start + 1); // '\'', start);

			//if (end > start)
			//	value = value.Substring(start, end - start);

			byte[] bytes = new byte[value.Length / 2];

			for (int i = 0; i < value.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);

			return ASCIIEncoding.ASCII.GetString(bytes);
		}

		/// <summary>
		/// Writes the input string info ASCII representation, e.q. "MikroTik" input string is transalated to "'343136353031413245393731'"
		/// </summary>
		/// <param name="value">The input string value.</param>
		/// <returns>ASCII string valuese enclosed with apostrophes.</returns>
		public string ConvertStringToAscii(string value)
		{
			if (value == null)
				value = String.Empty;

			string result = "'";

			foreach (char item in value)
				result += String.Format("{0:x2}", Convert.ToInt32(item)); // write ascii char value in hex format

			result += "'";

			return result;
		}

		public int ConvertHexStringToInt32(string value) => Convert.ToInt32(value, 16);

		public ulong ConvertHexStringToUInt64(string value)
		{
			return Convert.ToUInt64(value, 16);

			//// strip the leading 0x
			//if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			//	value = value.Substring(2);

			//return Int32.Parse(value, NumberStyles.HexNumber);
		}

		public string ConvertInt32ToHexString(int value, int numOfDecimalPlaces = 2)
		{
			string format = "x" + numOfDecimalPlaces.ToString();
			var result = String.Format("0x{0:X8}", value.ToString(format)); // "0x{0}"

			return result;
			//return String.Format("0x{0:X2}", value);
		}

		public string ConvertInt32ToHexString(int value, string oldHexValueText) => this.ConvertInt32ToHexString(value, numOfDecimalPlaces: oldHexValueText.Length - 2);

		public string ConvertUInt64ToHexString(ulong value) => "0x" + value.ToString("x");
				
		public string ConvertUInt64ToHexString(ulong value, int numOfDecimalPlaces) // = 8)
		{
			string format = "x" + numOfDecimalPlaces.ToString();
			string result = String.Format("0x{0}", value.ToString(format)); // "0x{0:X8}"

			return result;
		}

		public string ConvertUInt64ToHexString(ulong value, string oldHexValueText) => this.ConvertUInt64ToHexString(value, numOfDecimalPlaces: oldHexValueText.Length - 2);

		#endregion |   Private MikroTik Helper Methods  |

		#region |   Private MikroTik Helper Methods  |

		private string CreateConfigTextFromConfigSegments(IEnumerable<string> configSegments)
		{
			string result = "[";

			for (int i = 0; i < configSegments.Count(); i++)
			{
				if (i > 0)
					result += ",";

				result += "{" + configSegments.ElementAt(i) + "}";
			}

			result += "]";

			return result;
		}

		/// <summary>
		/// Convert MikroTik style key value pair dictionary to string separated with separator. 
		/// Example: "vid:0x0001,prt:[0x00,0x00,0x00,0x00,0x00,0x00],ivl:0x00"
		/// </summary>
		/// <param name="keyValueDictionary">MikroTik style key value pair dictionary.</param>
		/// <param name="separator">The value separator.</param>
		/// <returns>Config line string.</returns>
		private string CreateConfigTextFromConfigDictionary(Dictionary<string, string> keyValueDictionary, string separator = ",")
		{
			string result = String.Empty;

			for (int i = 0; i < keyValueDictionary.Count; i++)
			{
				var item = keyValueDictionary.ElementAt(i);

				if (i > 0)
					result += separator;

				result += String.Format("{0}:{1}", item.Key, item.Value);
			}

			return result;
		}

		private string CreateConfigTextFromConfigSegmentDictionary(List<Dictionary<string, string>> configSegmentDictionary)
		{
			string result = String.Empty;
			List<string> configSegments = new List<string>();

			foreach (var item in configSegmentDictionary)
				configSegments.Add(this.CreateConfigTextFromConfigDictionary(item));

			result = this.CreateConfigTextFromConfigSegments(configSegments);

			return result;
		}

		private List<Dictionary<string, string>> GetConfigSegmentDictionary(List<string> configSegments)
		{
			List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

			foreach (var text in configSegments)
				result.Add(this.GetConfigDictionary(text));

			return result;
		}

		/// <summary>
		/// Creates key value pairs of GET request result enclosed with large parentheses. Input text format looks like this: {ip:0x0258a8c0,mac:'000c427fb1c6',sid:'343136353031413245393731',upt:0x0000cdb4,id:'4d696b726f54696b',
		/// ver:'312e3137',brd:'52423236304753',wdt:0x01,dsc:0x01,alla:0x00000000,allm:0x00,allp:0x3f,avln:0x0000,volt:0x0000,temp:0x0000,lcbl:0x00}
		/// </summary>
		/// <param name="configTextLine">The response text to parse.</param>
		/// <returns></returns>
		private Dictionary<string, string> GetConfigDictionary(string configTextLine)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			List<string> segments = new List<string>();
			int pos = 0;

			if (String.IsNullOrEmpty(configTextLine))
				return result;

			segments = this.GetConfigSegments(configTextLine);

			// Get the key value pairs spliteted by the first occurence of ":"
			foreach (string segment in segments)
			{
				pos = segment.IndexOf(':');

				if (pos > 0)
				{
					string key = segment.Substring(0, pos);
					string value = segment.Substring(pos + 1);

					result.Add(key, value);
				}
			}

			return result;
		}


		///// <summary>
		///// Gets the response text segments, if enclosed with '[' ']' parentness pairs. Input example: [{vid:0x0001,prt:[0x00,0x00,0x00,0x00,0x00,0x00],ivl:0x00},{vid:0x0002,prt:[0x00,0x00,0x00,0x00,0x00,0x00],ivl:0x00}]
		///// </summary>
		///// <param name="configText">The response text to parse.</param>
		///// <returns>The result text segments.</returns>
		//public List<string> CreateConfigSegments(string configText)
		//{
		//	List<string> result = new List<string>();

		//	if (configText.IsNullOrEmpty())
		//		return result;

		//	// remove middle parentheses enclosure ("[.....]"), if exists
		//	int start = configText.IndexOf('[');
		//	int end = configText.LastIndexOf(']');

		//	if (end > start)
		//	{
		//		configText = configText.Substring(start + 1, end - start - 1); // [] Parentheses exists, remove them and split data into segments

		//		start = configText.IndexOf('{');
		//		end = configText.IndexOf('}');

		//		while (end > start)
		//		{
		//			string line = configText.Substring(start + 1, end - start - 1); // {} Parentheses exists, remove them and split data into segments

		//			result.Add(line);

		//			start = configText.IndexOf('{', end + 1);
		//			end = configText.IndexOf('}', end + 1);

		//		}
		//	}
		//	else
		//	{
		//		result.Add(configText); // result is input text as a single line, e.g. {ip:0x0258a8c0,mac:'000c427fb1c6',sid:'343136353031413245393731'}
		//	}

		//	return result;
		//}


		///// <summary>
		///// Creates key value pairs of GET request result enclosed with large parentheses. Input text format looks like this: {ip:0x0258a8c0,mac:'000c427fb1c6',sid:'343136353031413245393731',upt:0x0000cdb4,id:'4d696b726f54696b',
		///// ver:'312e3137',brd:'52423236304753',wdt:0x01,dsc:0x01,alla:0x00000000,allm:0x00,allp:0x3f,avln:0x0000,volt:0x0000,temp:0x0000,lcbl:0x00}
		///// </summary>
		///// <param name="configTextLine">The response text to parse.</param>
		///// <returns></returns>
		//public Dictionary<string, string> ParseConfigLine(string configTextLine)
		//{
		//	Dictionary<string, string> result = new Dictionary<string, string>();
		//	List<string> segments = new List<string>();

		//	if (configTextLine.IsNullOrEmpty())
		//		return result;

		//	int pos = 0;
		//	//int end = 0;

		//	configTextLine = this.RemoveParenthesesEnclosure(configTextLine);

		//	// Avoid splitting by the "," inside [...] or {...} parentheses
		//	int largeParentnessCount = 0;
		//	int middleParentnessCount = 0;

		//	for (int i = 0; i < configTextLine.Length; i++)
		//	{
		//		char element = configTextLine[i];

		//		if (element == '[')
		//		{
		//			largeParentnessCount++;
		//		}
		//		else if (element == ']')
		//		{
		//			largeParentnessCount--;
		//		}
		//		else if (element == '{')
		//		{
		//			middleParentnessCount++;
		//		}
		//		else if (element == '}')
		//		{
		//			middleParentnessCount--;
		//		}

		//		if ((element == ',' || i == configTextLine.Length - 1) && largeParentnessCount == 0 && middleParentnessCount == 0 && i > pos)
		//		{
		//			int len = i - pos;

		//			if (i == configTextLine.Length - 1) // the last char
		//				len++;

		//			string segment = configTextLine.Substring(pos, len);

		//			segments.Add(segment);
		//			pos = i + 1;
		//		}
		//	}

		//	// Get the key value pairs spliteted by the first occurence of ":"
		//	foreach (string segment in segments)
		//	{
		//		pos = segment.IndexOf(':');

		//		if (pos > 0)
		//		{
		//			string key = segment.Substring(0, pos);
		//			string value = segment.Substring(pos + 1);

		//			result.Add(key, value);
		//		}
		//	}

		//	return result;
		//}

		private List<string> GetConfigSegments(string configTextLine)
		{
			List<string> segments = new List<string>();

			if (String.IsNullOrEmpty(configTextLine))
				return segments;

			configTextLine = this.RemoveParenthesesEnclosure(configTextLine);

			// Avoid splitting by the "," inside [...] or {...} parentheses
			int largeParentnessCount = 0;
			int middleParentnessCount = 0;
			int pos = 0;

			for (int i = 0; i < configTextLine.Length; i++)
			{
				char element = configTextLine[i];

				if (element == '[')
				{
					largeParentnessCount++;
				}
				else if (element == ']')
				{
					largeParentnessCount--;
				}
				else if (element == '{')
				{
					middleParentnessCount++;
				}
				else if (element == '}')
				{
					middleParentnessCount--;
				}

				if ((element == ',' || i == configTextLine.Length - 1) && largeParentnessCount == 0 && middleParentnessCount == 0 && i > pos)
				{
					int len = i - pos;

					if (i == configTextLine.Length - 1) // the last char
						len++;

					string segment = configTextLine.Substring(pos, len);

					segments.Add(segment);
					pos = i + 1;
				}
			}

			return segments;
		}

		private string RemoveParenthesesEnclosure(string value)
		{
			int start = 0;
			int end = 0;

			if (value.TrimStart().StartsWith("{"))
			{
				start = value.IndexOf('{');
				end = value.LastIndexOf('}');

				if (end > start)
					value = value.Substring(start + 1, end - start - 1);
			}
			else if (value.TrimStart().StartsWith("["))
			{
				start = value.IndexOf('[');
				end = value.LastIndexOf(']');

				if (end > start)
					value = value.Substring(start + 1, end - start - 1);
			}

			return value;
		}

		// Parsing Test:
		// link.b {comb:0x00000000,qsfp:0x00000000,en:0x00003fff,blkp:0x00000000,an:0x0001ffff,dpxc:0x0001ffff,fctc:0x0001ffff,fctr:0x00000000,lnk:0x00000004,dpx:0x00000004,tfct:0x00000004,rfct:0x00000000,paus:0x00000000,spd:[0x07,0x07,0x02,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07,0x07],spdc:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],cm:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],qtyp:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],prt:0x11,sfp:0x10,sfpo:0x00,nm:['53465031','53465032','53465033','53465034','53465035','53465036','53465037','53465038','53465039','5346503130','5346503131','5346503132','5346503133','5346503134','5346503135','5346503136','4d474d54'],hop:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],hops:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],len:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],flt:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],pair:[0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff,0xffff]}
		// vlan.b [{nm:'496e7465726e657420416363657373',mbr:0x00000000,vid:0x0001,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00},{nm:'536572766572204661726d',mbr:0x00000000,vid:0x0002,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00},{nm:'4d656e6167656d656e74204465706172',mbr:0x00000000,vid:0x0003,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00},{nm:'4e6574776f726b696e67204465706172',mbr:0x00000000,vid:0x0004,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00},{nm:'566c616e333333',mbr:0x00000000,vid:0x014d,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00},{nm:'4e6574776f726b204d616e6167656d65',mbr:0x00000000,vid:0x03e8,piso:0x00,lrn:0x00,mrr:0x00,igmp:0x00}]
		// fwd.b {fp1:0x0001fffe,fp2:0x0001fffd,fp3:0x0001fffb,fp4:0x0001fff7,fp5:0x0001ffef,fp6:0x0001ffdf,fp7:0x0001ffbf,fp8:0x0001ff7f,fp9:0x0001feff,fp10:0x0001fdff,fp11:0x0001fbff,fp12:0x0001f7ff,fp13:0x0001efff,fp14:0x0001dfff,fp15:0x0001bfff,fp16:0x00017fff,fp17:0x0000ffff,lck:0x00000000,lckf:0x00000000,imr:0x00000000,omr:0x00000000,mrto:0x00000001,vlan:[0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x03,0x02,0x02,0x02,0x02,0x03],vlni:[0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],dvid:[0x0003,0x0002,0x0001,0x0003,0x0003,0x0003,0x0003,0x0003,0x0003,0x0003,0x0003,0x014d,0x0001,0x0001,0x0001,0x0001,0x0001],fvid:0x00000000,srt:[0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64,0x64],suni:0x00000000,fmc:0x0001ffff,ir:[0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000]}

		#endregion |   Private MikroTik Helper Methods  |

		#region |   Protected Methods  |

		protected override async ValueTask<TaskInfo<bool>> GetConnectionTestResultAsync()
		{
			HttpWebResponse response = await this.SendGetRequestAsync("sys.b");
			//HttpWebResponse response = this.SendGetRequestAsync("sys.b");
			//string responseText = response.GetResponseText();
			TaskResultInfo resultInfo = (response.StatusCode == HttpStatusCode.OK) ? TaskResultInfo.Succeeded : TaskResultInfo.Error;
			string message = (resultInfo == TaskResultInfo.Succeeded) ? "Success" : response.StatusDescription;

			return new TaskInfo<bool>(resultInfo == TaskResultInfo.Succeeded, resultInfo, message);
		}

		#endregion |   Protected Methods  |
	}
}
