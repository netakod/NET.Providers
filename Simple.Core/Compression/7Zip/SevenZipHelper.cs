// NOTE: If you get any "out of memory" errors using this, try reducing the size of the dictionary in the SevenZipHelper class (e.g. <<21 instead of << 23).

using System;
using System.IO;
using Simple.Compression.SevenZip;
using Simple.Compression.SevenZip.LZMA;

namespace Simple.Compression
{
    public static class SevenZipHelper
    {

        static int defaultDictionarySize = 1 << 23;

        // static Int32 posStateBits = 2;
        // static  Int32 litContextBits = 3; // for normal files
        // UInt32 litContextBits = 0; // for 32-bit data
        // static  Int32 litPosBits = 0;
        // UInt32 litPosBits = 2; // for 32-bit data
        // static   Int32 algorithm = 2;
        // static    Int32 numFastBytes = 128;

        static   bool eos = false;





        static   CoderPropID[] propIDs = 
				{
					CoderPropID.DictionarySize,
					CoderPropID.PosStateBits,
					CoderPropID.LitContextBits,
					CoderPropID.LitPosBits,
					CoderPropID.Algorithm,
					CoderPropID.NumFastBytes,
					CoderPropID.MatchFinder,
					CoderPropID.EndMarker
				};

        // these are the default properties, keeping it simple for now:
        static   object[] GetProperties(int dictionarySize)
        { 
		    return new object[] 
            {
				dictionarySize,
				(Int32)(2),
				(Int32)(3),
				(Int32)(0),
				(Int32)(2),
				(Int32)(128),
				"bt4",
				eos
			};
        }

        public static byte[] Compress(byte[] inputBytes)
        {
            return Compress(inputBytes, defaultDictionarySize);
        }

        public static byte[] Compress(byte[] inputBytes, int dictionarySize)
        {

            MemoryStream inStream = new MemoryStream(inputBytes);
            MemoryStream outStream = new MemoryStream();
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, GetProperties(dictionarySize));
            encoder.WriteCoderProperties(outStream);
            long fileSize = inStream.Length;
            for (int i = 0; i < 8; i++)
                outStream.WriteByte((Byte)(fileSize >> (8 * i)));
            encoder.Code(inStream, outStream, -1, -1, null);
            return outStream.ToArray();
        }

        public static byte[] Decompress(byte[] inputBytes)
        {
            MemoryStream newInStream = new MemoryStream(inputBytes);

            Decoder decoder = new Decoder();

            newInStream.Seek(0, 0);
            MemoryStream newOutStream = new MemoryStream();

            byte[] properties2 = new byte[5];
            if (newInStream.Read(properties2, 0, 5) != 5)
                throw (new Exception("input .lzma is too short"));
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = newInStream.ReadByte();
                if (v < 0)
                    throw (new Exception("Can't Read 1"));
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties2);

            long compressedSize = newInStream.Length - newInStream.Position;
            decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

            byte[] b = newOutStream.ToArray();

            return b;
        }
    }
}
