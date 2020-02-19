using CodingCat.Serializers.Interfaces;
using System;

namespace CodingCat.RabbitMq.Tests.Impls
{
    public class DeserializeNotImplementedSerializer : ISerializer<string>
    {
        public string FromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes(string input)
        {
            return null;
        }
    }

    public class DeserializeNotImplementedSerializer<T> : ISerializer<T>
    {
        public T FromBytes(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes(T input)
        {
            return null;
        }
    }
}