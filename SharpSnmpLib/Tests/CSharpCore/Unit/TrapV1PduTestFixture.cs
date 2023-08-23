/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/5/1
 * Time: 11:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Xunit;

namespace Lextm.SharpSnmpLib.Unit
{
    public class TrapV1PduTestFixture
    {
        [Fact]
        public void TestException()
        {
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new Tuple<int, byte[]>(0, new byte[] { 0 }),null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu((ObjectIdentifier)null, null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}), null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                                                     new IP("127.0.0.1"), null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          null, null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          new Integer32(12),
                                          null, null));
            Assert.Throws<ArgumentNullException>(() => new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          new Integer32(12),
                                          new TimeTicks(16352),
                                          null));
            Variable v = new Variable(new ObjectIdentifier(new uint[] {1,3,6,1,4,1,2162,1001,21,0}), 
                                      new OctetString("TrapTest"));
            List<Variable> vList = new List<Variable> {v};

            TrapV1Pdu pdu = new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          new Integer32(12),
                                          new TimeTicks(16352),
                                          vList);
            Assert.Throws<ArgumentNullException>(() => pdu.AppendBytesTo(null));
            
            Assert.Throws<NotSupportedException>(() => { var test = pdu.RequestId; });
            Assert.Throws<NotSupportedException>(() => { var test = pdu.ErrorIndex; });
            Assert.Throws<NotSupportedException>(() => { var test = pdu.ErrorStatus; });
        }
        
        [Fact]
        public void TestToTrapMessage()
        {
            Variable v = new Variable(new ObjectIdentifier(new uint[] {1,3,6,1,4,1,2162,1001,21,0}), 
                                      new OctetString("TrapTest"));
            List<Variable> vList = new List<Variable> {v};

            TrapV1Pdu pdu = new TrapV1Pdu(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2},
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          new Integer32(12),
                                          new TimeTicks(16352),
                                          vList);
            byte[] bytes = TrapV1Message.PackMessage(VersionCode.V1, new OctetString("public"), pdu).ToBytes();
            TrapV1Message message = (TrapV1Message)MessageFactory.ParseMessages(bytes, new UserRegistry())[0];
            Assert.Equal("127.0.0.1", message.AgentAddress.ToString());
            Assert.Equal(GenericCode.EnterpriseSpecific, message.Generic);
            Assert.Equal(12, message.Specific);
            Assert.Equal(16352U, message.TimeStamp);
            Assert.Equal(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}, message.Enterprise.ToNumerical());
            Assert.Equal(1, message.Variables().Count);
            Assert.Equal(new uint[] {1,3,6,1,4,1,2162,1001,21,0}, message.Variables()[0].Id.ToNumerical());
            Assert.Equal("TrapTest", message.Variables()[0].Data.ToString());
            Assert.Equal("SNMPv1 TRAP PDU: agent address: 127.0.0.1; time stamp: 00:02:43.5200000; enterprise: 1.3.6.1.4.1.2162.1000.2; generic: EnterpriseSpecific; specific: 12; varbind count: 1", pdu.ToString());
        }
        
        [Fact]
        public void TestToTrapMessageChinese()
        {
            Variable v = new Variable(new ObjectIdentifier(new uint[] {1,3,6,1,4,1,2162,1001,21,0}), 
                                      new OctetString("嶄忽", Encoding.Unicode));
            List<Variable> vList = new List<Variable> {v};

            TrapV1Pdu pdu = new TrapV1Pdu(new ObjectIdentifier(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}),
                                          new IP("127.0.0.1"),
                                          new Integer32((int)GenericCode.EnterpriseSpecific),
                                          new Integer32(12),
                                          new TimeTicks(16352),
                                          vList);
            byte[] bytes = TrapV1Message.PackMessage(VersionCode.V1, new OctetString("public"), pdu).ToBytes();
            TrapV1Message message = (TrapV1Message)MessageFactory.ParseMessages(bytes, new UserRegistry())[0];
            Assert.Equal("127.0.0.1", message.AgentAddress.ToString());
            Assert.Equal(GenericCode.EnterpriseSpecific, message.Generic);
            Assert.Equal(12, message.Specific);
            Assert.Equal(16352U, message.TimeStamp);
            Assert.Equal(new uint[] {1, 3, 6, 1, 4, 1, 2162, 1000, 2}, message.Enterprise.ToNumerical());
            Assert.Equal(1, message.Variables().Count);
            Assert.Equal(new uint[] {1,3,6,1,4,1,2162,1001,21,0}, message.Variables()[0].Id.ToNumerical());
            Assert.Equal("嶄忽", ((OctetString)message.Variables()[0].Data).ToString(Encoding.Unicode));
        }
    }
}
