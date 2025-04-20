using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VTReplayConverter
{
    public class FileDecoder
    {

		public static Bitmap DecodeImage(string filepath)
		{
			if (!filepath.EndsWith("b"))
			{
				Console.WriteLine("Tried to wsDecode a file that did not have an extension ending with 'b'");
				return null;
			}
			byte[] array = File.ReadAllBytes(filepath);
			VTSteamWorkshopUtils.WSDecode(array);

			Bitmap bmp;
			using (var ms = new MemoryStream(array))
			{
				bmp = new Bitmap(ms);
			}

			return bmp;
		}

		public static ConfigNode LoadFromFile(string filePath, bool logErrors = true)
		{
			filePath = filePath.Replace('\\', '/');
			ConfigNode result;
			try
			{
				result = ConfigNode.ParseNode(File.ReadAllText(filePath));
			}
			catch (Exception arg)
			{
				if (logErrors)
				{
					Console.WriteLine(string.Format("Error loading config node {0}\n{1}", filePath, arg));
				}
				result = null;
			}
			return result;
		}
	}
}
