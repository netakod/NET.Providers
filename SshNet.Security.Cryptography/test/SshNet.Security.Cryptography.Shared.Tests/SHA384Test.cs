﻿using System.Text;
using SshNet.Security.Cryptography.Common.Tests;
using Xunit;

namespace SshNet.Security.Cryptography.Tests
{
    /// <summary>
    /// Test cases are from http://csrc.nist.gov/groups/ST/toolkit/documents/Examples/SHA_All.pdf
    /// </summary>
    public class SHA384Test
    {
        private readonly SHA384 _hashAlgorithm;

        public SHA384Test()
        {
            _hashAlgorithm = new SHA384();
        }

        [Fact]
        public void NistShaAll_1()
        {
            var data = ByteExtensions.HexToByteArray("616263"); // "abc"
            var expectedHash = ByteExtensions.HexToByteArray("CB00753F45A35E8BB5A03D699AC65007272C32AB0EDED1631A8B605A43FF5BED8086072BA1E7CC2358BAECA134C825A7");

            var actualHash = _hashAlgorithm.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void NistShaAll_2()
        {
            var data = ByteExtensions.HexToByteArray("61626364656667686263646566676869636465666768696a6465666768696a6b65666768696a6b6c666768696a6b6c6d6768696a6b6c6d6e68696a6b6c6d6e6f696a6b6c6d6e6f706a6b6c6d6e6f70716b6c6d6e6f7071726c6d6e6f707172736d6e6f70717273746e6f707172737475"); // "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
            var expectedHash = ByteExtensions.HexToByteArray("09330C33F71147E83D192FC782CD1B4753111B173B3B05D22FA08086E3B0F712FCC7C71A557E2DB966C3E9FA91746039");

            var actualHash = _hashAlgorithm.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }
    }
}
