using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DLS.SaveSystem.Serializable.SerializationHelper
{
    public class ChipSerializerContract : DefaultContractResolver
    {
        public new static readonly ChipSerializerContract Instance = new ChipSerializerContract();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(SavedComponentChip) && property.PropertyName == "isInGroup")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        SavedComponentChip e = (SavedComponentChip)instance;
                        return !e.isInGroup;
                    };
            }
            
            
            if (property.DeclaringType == typeof(SavedComponentChip) && property.PropertyName == "signalGroupId")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        SavedComponentChip e = (SavedComponentChip)instance;
                        return !e.isInGroup;
                    };
            }
            

            return property;
        }
    }
}





