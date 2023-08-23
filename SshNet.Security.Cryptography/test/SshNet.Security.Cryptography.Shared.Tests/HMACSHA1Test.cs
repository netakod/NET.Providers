﻿using System.Text;
using SshNet.Security.Cryptography.Common.Tests;
using Xunit;

namespace SshNet.Security.Cryptography.Tests
{
    /// <summary>
    /// Test cases are from https://tools.ietf.org/html/rfc2202.
    /// </summary>
    public class HMACSHA1Test
    {
        [Fact]
        public void Rfc2202_1()
        {
            var key = ByteExtensions.Repeat(0x0b, 20);
            var data = ByteExtensions.HexToByteArray("4869205468657265"); // "Hi There"
            var expectedHash = ByteExtensions.HexToByteArray("b617318655057264e28bc0b6fb378c8ef146be00");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void Rfc2202_2()
        {
            var key = ByteExtensions.HexToByteArray("4a656665"); // "Jefe";
            var data = ByteExtensions.HexToByteArray("7768617420646f2079612077616e7420666f72206e6f7468696e673f"); // "what do ya want for nothing?"
            var expectedHash = ByteExtensions.HexToByteArray("effcdf6ae5eb2fa2d27416d5f184df9c259a7c79");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void Rfc2202_3()
        {
            var key = ByteExtensions.Repeat(0xaa, 20);
            var data = ByteExtensions.Repeat(0xdd, 50);
            var expectedHash = ByteExtensions.HexToByteArray("125d7342b9ac11cd91a39af48aa17b4f63f175d3");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void Rfc2202_4()
        {
            var key = ByteExtensions.HexToByteArray("0102030405060708090a0b0c0d0e0f10111213141516171819");
            var data = ByteExtensions.Repeat(0xcd, 50);
            var expectedHash = ByteExtensions.HexToByteArray("4c9007f4026250c6bc8414f9bf50c86c2d7235da");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void Rfc2202_5()
        {
            var key = ByteExtensions.Repeat(0x0c, 20);
            var data = ByteExtensions.HexToByteArray("546573742057697468205472756e636174696f6e"); // "Test With Truncation"

            var expectedHash = ByteExtensions.HexToByteArray("4c1a03424b55e07fe7f27be1d58bb9324a9a5a04");
            var hmac = new HMACSHA1(key);
            var actualHash = hmac.ComputeHash(data);
            Assert.Equal(expectedHash, actualHash);

            var expectedHash96 = ByteExtensions.HexToByteArray("4c1a03424b55e07fe7f27be1");
            var hmac96 = new HMACSHA1(key, 96);
            var actualHash96 = hmac96.ComputeHash(data);
            Assert.Equal(expectedHash96, actualHash96);
        }

        [Fact]
        public void Rfc2202_6()
        {
            var key = ByteExtensions.Repeat(0xaa, 80);
            var data = ByteExtensions.HexToByteArray("54657374205573696e67204c6172676572205468616e20426c6f636b2d53697a65204b6579202d2048617368204b6579204669727374"); // "Test Using Larger Than Block-Size Key - Hash Key First"
            var expectedHash = ByteExtensions.HexToByteArray("aa4ae5e15272d00e95705637ce8a3b55ed402112");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void Rfc2202_7()
        {
            var key = ByteExtensions.Repeat(0xaa, 80);
            var data = ByteExtensions.HexToByteArray("54657374205573696e67204c6172676572205468616e20426c6f636b2d53697a65204b657920616e64204c6172676572205468616e204f6e6520426c6f636b2d53697a652044617461"); // "Test Using Larger Than Block-Size Key and Larger Than One Block-Size Data"
            var expectedHash = ByteExtensions.HexToByteArray("e8e99d0f45237d786d6bbaa7965c7808bbff1a91");
            var hmac = new HMACSHA1(key);

            var actualHash = hmac.ComputeHash(data);

            Assert.Equal(expectedHash, actualHash);
        }
    }
}
