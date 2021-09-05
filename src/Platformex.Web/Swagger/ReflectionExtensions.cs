using System;
using System.Linq;

namespace Platformex.Web.Swagger
{
    public static class ReflectionExtensions
    {
        public static Type GetSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            for (; toCheck != null && toCheck != typeof(object); toCheck = toCheck.BaseType)
            {
                var type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == type)
                    return toCheck;
            }
            return null;
        }

        public static Type GetSubclassOfRawGenericInterface(Type generic, Type toCheck)
        {
            return toCheck.GetInterfaces().FirstOrDefault(i =>
            {
                if (i.IsGenericType)
                    return i.GetGenericTypeDefinition() == generic;
                return false;
            });
        }
    }
}
