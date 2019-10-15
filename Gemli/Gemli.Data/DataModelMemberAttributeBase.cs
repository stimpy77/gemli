using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Gemli.Data
{
    /// <summary>
    /// Attribute class that describes a <see cref="DataModel"/> data mapping
    /// for an class member (property/field) and its associated database counterpart
    /// (column/field). This class is abstract.
    /// </summary>
    [Serializable]
    public abstract class DataModelMemberAttributeBase : DataModelMappingAttributeBase
    {
        /// <summary>
        /// 
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [XmlIgnore]
        public Type DeclaringType
        {
            get
            {
                return _DeclaringType;
            }
            set
            {
                _DeclaringType = value;
                if (value != null && _TargetMember == null && _TargetMemberName != null)
                {
                    var targetMembers = value.GetMember(_TargetMemberName);
                    TargetMember = targetMembers[0];

                }
            }
        }

        [NonSerialized] private Type _DeclaringType;

        /// <summary>
        /// Used for deserializing the <see cref="DeclaringType"/>
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [XmlElement("ownerType")]
        public string DeclaringTypeName
        {
            get { return DeclaringType.FullName; }
            set { DeclaringType = Type.GetType(value); }
        }

        /// <summary>
        /// Gets or sets the name of the property/field that this mapping 
        /// object is associated with.
        /// </summary>
        [XmlIgnore]
        public MemberInfo TargetMember
        {
            get { return _TargetMember; }
            set
            {
                _TargetMember = value;
                DeclaringType = value.DeclaringType;
                _TargetMemberName = value.Name;
            }
        }

        private MemberInfo _TargetMember;

        /// <summary>
        /// Gets the type of the property/field that this mapping
        /// object is associated with.
        /// </summary>
        [XmlIgnore]
        public Type TargetMemberType { get; protected internal set; }

        /// <summary>
        /// Allows the XML serializer to get or set the target member.
        /// </summary>
        /// <remarks>This property is not exposed to Intellisense if the binary assembly is referenced.</remarks>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [XmlElement("type")]
        public string TargetMemberTypeName
        {
            get { return TargetMemberType.FullName; }
            set
            {
                if (TargetMemberType != null)
                    throw new InvalidOperationException("TargetMemberType has already been set and cannot be changed.");
                TargetMemberType = Type.GetType(value);
            }
        }

        /// <summary>
        /// Allows the XML serializer to get or set the target member.
        /// </summary>
        /// <remarks>This property is not exposed to Intellisense if the binary assembly is referenced.</remarks>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [XmlElement("memberName")]
        public string TargetMemberName
        {
            get
            {
                return _TargetMemberName ?? TargetMember.Name;
            }
            set
            {
                if (TargetMember != null)
                {
                    throw new InvalidOperationException("TargetMember is already set and its name is read-only.");
                }
                _TargetMemberName = value;
                if (value != null && DeclaringType != null)
                {
                    var targetMembers = this.DeclaringType.GetMember(value);
                    TargetMember = targetMembers[0];
                }
            }
        }

        private string _TargetMemberName;
    }
}
