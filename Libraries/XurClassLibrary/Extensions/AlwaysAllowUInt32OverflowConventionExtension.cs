using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace XurClassLibrary.Extensions
{
    public class AlwaysAllowUInt32OverflowConventionExtension : IMemberMapConvention
    {
        public string Name
        {
            get { return "AlwaysAllowUInt32Overflow"; }
        }

        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == typeof(UInt32))
            {
                var uint32Serializer = new UInt32Serializer(BsonType.String, new RepresentationConverter(true, true));
                memberMap.SetSerializer(uint32Serializer);
            }
        }
    }
}
