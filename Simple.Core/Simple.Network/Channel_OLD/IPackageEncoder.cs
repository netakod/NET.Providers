using System.Buffers;

namespace Simple.Network
{
    public interface IPackageEncoder<in TPackageInfo>
    {
        int Encode(IBufferWriter<byte> writer, TPackageInfo pack);
    }
}