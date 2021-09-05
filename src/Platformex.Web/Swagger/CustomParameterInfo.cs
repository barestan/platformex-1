
using System;
using System.Reflection;

namespace Platformex.Web.Swagger
{
    public class CustomParameterInfo : ParameterInfo
    {
        private readonly string _name;
        private readonly Type _type;

        public CustomParameterInfo(string name, Type type)
        {
            _name = name;
            _type = type;
        }

        public override string Name => _name;

        public override Type ParameterType => _type;
    }
}
