using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
namespace ConsoleApplication1
{
    // test용 vo객체
    class LoginPacket
    {
        [PacketAttribute(2)]
        public short PacketSize { get; set; }
        [PacketAttribute(4)]
        public int ServiceId { get; set; }
        [PacketAttribute(15)]
        public String Id{ get; set;}
        [PacketAttribute(12)]
        public String Password { get; set; }
        [PacketAttribute(4)]
        public int IntField { get; set; }
        [PacketAttribute(8)]
        public double DoubleField { get; set; }
        [PacketAttribute(4)]
        public float FloatField { get; set; }
    }
    class LoginResponsePacket
    {
        [PacketAttribute(2)]
        public short PacketSize { get; set; }
        [PacketAttribute(4)]
        public int ServiceId { get; set; }
        [PacketAttribute(15)]
        public Boolean Success { get; set; }
    }
    // vo 객체를 packet serialize & deserialize
    class PacketUtil
    {
        public byte[] packetSerialize<T>(T t)
        {
            Type type = typeof(T);
            byte[] packet = null;
            
            PropertyInfo[] proInfo = type.GetProperties();

            int offset = 0;
            // property 검색
            foreach (PropertyInfo info in proInfo)
            {
                // 해당 property의 attributes
                object[] attributes = info.GetCustomAttributes(true);
                int propSize = 0;
                foreach (object attr in attributes)
                {
                    PacketAttribute p = attr as PacketAttribute;
                    if (p != null)
                    {
                        propSize = p.Length;
                        if (packet == null)
                        {
                            packet = new byte[propSize];
                        }
                        else
                        {
                            Array.Resize(ref packet, offset + propSize);
                        }

                        byte[] result = getBytes(info.GetValue(t, null), propSize);
                        Console.WriteLine("propSize : {0} | 데이터 : {1} | 바이트코드 : {2}", p.Length, info.GetValue(t, null), BitConverter.ToString(result));
                        System.Buffer.BlockCopy(result, 0, packet, offset, result.Length);
                        offset += propSize;
                    }
                }
               
            }
            // header length 정보
            System.Buffer.BlockCopy(getBytes(packet.Length, 2), 0, packet, 0, 2);
            return packet;
        }

        public Object packetDeserialize(Type type, byte[] packet)
        {
            PropertyInfo[] proInfo = type.GetProperties();
            Object o = Activator.CreateInstance(type);
            int offset = 0;
            // property 검색
            foreach (PropertyInfo info in proInfo)
            {
                // 해당 property의 attributes
                object[] attributes = info.GetCustomAttributes(true);
                MethodInfo methodInfo = type.GetMethod("set_" + info.Name);
                
                foreach (object attr in attributes)
                {
                    PacketAttribute p = attr as PacketAttribute;
                    if (p != null)
                    {
                        int propSize = p.Length;
                        byte[] data = new byte[propSize];
                        for (int i = offset; i < propSize; i++)
                        {
                            data[i - offset] = packet[i];
                        }
                        offset += propSize;
                        object[] parametersArray = new object[1];
                        parametersArray[0] = getValue(info, data);
                        methodInfo.Invoke(o, parametersArray);
                    }
                }
            }
            return o;
        }

        // object to byte
        private byte[] getBytes<T>(T value, int propSize)
        {
            Type type = value.GetType();
            byte[] result = new byte[propSize];

            if (Type.Equals(type, typeof(string)))
            {
                Console.WriteLine("convert String type to byte array");
                result = Encoding.UTF8.GetBytes(Convert.ToString(value).ToCharArray());
            }
            else if (Type.Equals(type, typeof(short)))
            {
                Console.WriteLine("convert short type to byte array");
                result = BitConverter.GetBytes(Convert.ToInt16(value));
            }
            else if (Type.Equals(type, typeof(int)))
            {
                Console.WriteLine("convert int type to byte array");
                result = BitConverter.GetBytes(Convert.ToInt32(value));
            }
            else if (Type.Equals(type, typeof(double)))
            {
                Console.WriteLine("convert double type to byte array");
                result = BitConverter.GetBytes(Convert.ToDouble(value));
            }
            else if (Type.Equals(type, typeof(float)))
            {
                Console.WriteLine("convert float type to byte array");
                result = BitConverter.GetBytes(Convert.ToSingle(value));
            }
            else if (Type.Equals(type, typeof(Boolean)))
            {
                Console.WriteLine("convert boolean type to byte array");
                result = BitConverter.GetBytes(Convert.ToBoolean(value));
            }
            return result;
        }

        // byte to object
        private object getValue(PropertyInfo info, byte[] data)
        {
            if (Type.Equals(info.PropertyType, typeof(short)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToInt16(data, 0));
                return BitConverter.ToInt16(data, 0);
            }
            else if (Type.Equals(info.PropertyType, typeof(int)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToInt32(data, 0));
                return BitConverter.ToInt32(data, 0);
            }
            else if (Type.Equals(info.PropertyType, typeof(float)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToSingle(data, 0));
                return BitConverter.ToSingle(data, 0);
            }
            else if (Type.Equals(info.PropertyType, typeof(double)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToDouble(data, 0));
                return BitConverter.ToDouble(data, 0);
            }
            else if (Type.Equals(info.PropertyType, typeof(string)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToString(data, 0));
                return BitConverter.ToString(data, 0);
            }
            else if (Type.Equals(info.PropertyType, typeof(Boolean)))
            {
                Console.WriteLine("deserialized data : {0} ==> {1}", BitConverter.ToString(data), BitConverter.ToBoolean(data, 0));
                return BitConverter.ToBoolean(data, 0);
            }
            return null;
        }
    }

    // custom attribute 클래스
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PacketAttribute : System.Attribute
    {
        public readonly int Length;

        public PacketAttribute(int length)
        {
            this.Length = length;
        }
    }

    class Program
    {
        enum ServiceType { Login = 1 };

        static void Main(string[] args)
        {
            PacketUtil packetUtil = new PacketUtil();
            LoginPacket login = new LoginPacket();
            login.ServiceId = (int)ServiceType.Login;
            login.Id = "ekohss0514";
            login.Password = "test1111";
            login.IntField = 1234;
            login.DoubleField = 1.7E+3;
            login.FloatField = 4.5f;
            Console.WriteLine("========================== login packet serialize start =================================");
            byte[] loginPacket = packetUtil.packetSerialize(login);
            Console.WriteLine("최종 packet : {0}", BitConverter.ToString(loginPacket));
            Console.WriteLine("========================== login packet serialize end =================================");

            LoginResponsePacket loginResponse = new LoginResponsePacket();
            Console.WriteLine((int)ServiceType.Login);
            loginResponse.PacketSize = 21;
            loginResponse.ServiceId = (int)ServiceType.Login;
            loginResponse.Success = true;
            Console.WriteLine("========================== loginResponse packet serialize start =================================");
            byte[] loginResponsePacket = packetUtil.packetSerialize(loginResponse);
            Console.WriteLine("최종 loginResponsePacket : {0}", BitConverter.ToString(loginResponsePacket));
            Console.WriteLine("========================== loginResponse packet serialize end =================================");
            Console.WriteLine("========================== loginResponse packet deserialize start =================================");
            LoginResponsePacket response = packetUtil.packetDeserialize(Type.GetType("ConsoleApplication1.LoginResponsePacket"), loginResponsePacket) as LoginResponsePacket;
            Console.WriteLine("최종 response success : {0}", response.Success);
            Console.WriteLine("========================== loginResponse packet deserialize end =================================");
            Console.ReadKey();
        }
    }
}
