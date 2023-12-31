﻿/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2010/12/5
 * Time: 15:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Text;
using Lextm.SharpSnmpLib.Security;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete
namespace Lextm.SharpSnmpLib.Unit.Security
{
    public class MD5AuthenticationProviderTestFixture
    {
        [Fact]
        public void TestException()
        {
            var provider = new MD5AuthenticationProvider(new OctetString("longlongago"));
            Assert.Equal("MD5 authentication provider", provider.ToString());
            Assert.Throws<ArgumentNullException>(() => new MD5AuthenticationProvider(null));
            Assert.Throws<ArgumentNullException>(() => provider.PasswordToKey(null, null));
            Assert.Throws<ArgumentNullException>(() => provider.PasswordToKey(new byte[0], null));
            Assert.Throws<ArgumentException>(() => provider.PasswordToKey(new byte[0], new byte[0]));
        }

        [Fact]
        public void TestPasswordToKey()
        {
            byte[] password = Encoding.ASCII.GetBytes("testpass");
            byte[] engineId = new byte[] { 0x80, 0x00, 0x1F, 0x88, 0x80, 0xE9, 0x63, 0x00, 0x00, 0xD6, 0x1F, 0xF4, 0x49 };

            byte[] key = new MD5AuthenticationProvider(new OctetString("")).PasswordToKey(password, engineId);
            Assert.Equal(new byte[] { 226, 221, 44, 186, 149, 93, 73, 79, 237, 69, 120, 155, 145, 7, 44, 255 }, key);
        }

        [Fact]
        public void TestPasswordToKey2()
        {
            byte[] password = Encoding.ASCII.GetBytes("testpasstestpass");
            byte[] engineId = new byte[] { 0x80, 0x00, 0x1F, 0x88, 0x80, 0xE9, 0x63, 0x00, 0x00, 0xD6, 0x1F, 0xF4, 0x49 };

            byte[] key = new MD5AuthenticationProvider(new OctetString("")).PasswordToKey(password, engineId);
            Assert.Equal(new byte[] { 226, 221, 44, 186, 149, 93, 73, 79, 237, 69, 120, 155, 145, 7, 44, 255 }, key);
        }

        [Fact]
        public void TestPasswordToKey3()
        {
            byte[] password = Encoding.ASCII.GetBytes("testpasstestpasstestpas5");
            byte[] engineId = new byte[] { 0x80, 0x00, 0x1F, 0x88, 0x80, 0xE9, 0x63, 0x00, 0x00, 0xD6, 0x1F, 0xF4, 0x49 };

            byte[] key = new MD5AuthenticationProvider(new OctetString("")).PasswordToKey(password, engineId);
            Assert.NotEqual(new byte[] { 226, 221, 44, 186, 149, 93, 73, 79, 237, 69, 120, 155, 145, 7, 44, 255 }, key);
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
