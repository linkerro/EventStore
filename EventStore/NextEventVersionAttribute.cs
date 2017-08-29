using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public class EventVersionTransformsAttribute : Attribute
    {
        private Type type;
        public Type Type
        {
            get
            {
                return type;
            }
        }

        public EventVersionTransformsAttribute(Type type)
        {
            this.type = type;
        }
    }
}
