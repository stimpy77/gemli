using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gemli.Reflection
{
    public class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string message) : base(message) {}

        public InvalidAttributeException(string message, string associatedWith_classEnumOrMember, Attribute attribute)
            : this(message)
        {
            AttributeInstance = attribute;
            AssociatedWith = associatedWith_classEnumOrMember;
        }

        public Attribute AttributeInstance { get; set; }
        public string AssociatedWith { get; set; }
    }
}
