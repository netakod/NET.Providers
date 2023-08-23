using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NET.Tools.Terminal;
using Simple;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.MikroTikRouterOS)]
	public class TerminalClientlMikroTikRouterOS : TerminalClient
	{
		protected override async ValueTask<string> ProcessTerminalData(string data)
		{
			string mikroTikUnwantedSequence = "\u001b[H??H"; // Convert.ToChar(0x1B) + "[H" + Convert.ToChar(0xC4) + Convert.ToChar(0x9B) + "H";

			if (data.Contains(mikroTikUnwantedSequence))
				data = data.Replace(mikroTikUnwantedSequence, "");

			return await base.ProcessTerminalData(data);
		}

		protected override bool RemoveCommandFromReceivedDataIfExists(string receivedData, string sentCommand, out string result)
		{
			bool isRemoved = false;
			result = receivedData;

			if (receivedData.IsNullOrEmpty() || receivedData.Trim().Length == 0 || sentCommand.IsNullOrEmpty() || sentCommand.Trim().Length == 0)
				return isRemoved;

			string sentCommandWithoutSplitters = sentCommand.Replace("\r\n", "").Replace("\n\r", "").Replace("\r", "").Replace("\b", "").Replace("\n", "").Trim();
			char[] receivedChars = receivedData.ToCharArray();
			int[] positionIndexer = new int[receivedChars.Length]; //
			int pos = 0;
			StringBuilder receivedDataWithoutSplitters = new StringBuilder();

			// Remove splitters first, while saving original position information
			for (int index = 0; index < receivedChars.Length; index++)
			{
				char element = receivedChars[index];

				if (element == "\r"[0] || element == "\n"[0] || element == "\b"[0])
					continue;

				receivedDataWithoutSplitters.Append(element);
				positionIndexer[pos++] = index;
			}

			int lastCommandStartIndex = receivedDataWithoutSplitters.ToString().LastIndexOf(sentCommandWithoutSplitters);

			if (lastCommandStartIndex >= 0)
			{
				int originEndIndex = positionIndexer[lastCommandStartIndex + sentCommandWithoutSplitters.Length - 1] + (sentCommand.Length - sentCommandWithoutSplitters.Length); // sentCommand "\r\n" is also removed if exists
				result = this.processedReceivedData.Substring(originEndIndex + 1);
				isRemoved = true;
			}

			return isRemoved;
		}

		//protected override string[] SplitToLines(string processedReceivedData)
		//{
		//	// "\r" is not included since MikroTik sometimes respond: sent command + "\r[ admin@MikroTik ] > ...
		//	return processedReceivedData.Split(new string[] { "\r\n", "\n\r", "\b" }, StringSplitOptions.RemoveEmptyEntries); // 
		//}
	}
}
