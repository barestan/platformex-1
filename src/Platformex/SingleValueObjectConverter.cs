using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Platformex
{
    public class SingleValueObjectConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, Type> ConstructorArgumentTypes = new ConcurrentDictionary<Type, Type>();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is ISingleValueObject singleValueObject))
            {
                return;
            }

            serializer.Serialize(writer, singleValueObject.GetValue());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return null;
            }

            var parameterType = ConstructorArgumentTypes.GetOrAdd(
                objectType,
                _ =>
                {
                    var constructorInfo = objectType.GetTypeInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
                    var parameterInfo = constructorInfo.GetParameters().Single();
                    return parameterInfo.ParameterType;
                });

            var value = serializer.Deserialize(reader, parameterType);

            return Activator.CreateInstance(objectType, value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ISingleValueObject).GetTypeInfo().IsAssignableFrom(objectType);
        }
    }
}