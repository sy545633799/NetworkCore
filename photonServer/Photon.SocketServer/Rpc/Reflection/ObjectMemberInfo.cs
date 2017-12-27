using System;
using System.Reflection;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// This class contains reflection data about a property or a field.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the mapped attribute.</typeparam>
    public sealed class ObjectMemberInfo<TAttribute> where TAttribute : DataMemberAttribute
    {
        /// <summary>
        /// Delegate used to invoke the field or property get method. 
        /// </summary>
        private readonly Func<object, object> getterDelegate;

        /// <summary>
        /// Delegate used to invoke the field or property set method. 
        /// </summary>
        private readonly Action<object, object> setterDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Reflection.ObjectMemberInfo`1"/> class.
        /// </summary>
        /// <param name="fieldInfo">The field info.</param>
        /// <param name="attributeType">The attribute type.</param>
        public ObjectMemberInfo(FieldInfo fieldInfo, Type attributeType)
            : this(fieldInfo, attributeType, fieldInfo.FieldType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Reflection.ObjectMemberInfo`1"/> class.
        /// </summary>
        /// <param name="propertyInfo"> The property info.</param>
        /// <param name="attributeType">The attribute type.</param>
        public ObjectMemberInfo(PropertyInfo propertyInfo, Type attributeType)
            : this(propertyInfo, attributeType, propertyInfo.PropertyType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Reflection.ObjectMemberInfo`1"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="attributeType">The attribute type.</param>
        /// <param name="valueType">The value type.</param>
        /// <exception cref="T:System.ArgumentException">
        ///<paramref name="memberInfo"/> does not define the <paramref name="attributeType"/>.
        ///</exception>
        private ObjectMemberInfo(MemberInfo memberInfo, Type attributeType, Type valueType)
        {
            if (!memberInfo.IsDefined(attributeType, true))
            {
                throw new ArgumentException(string.Format("Member does not define expected attribute {0}.", attributeType), "memberInfo");
            }
            MemberInfo = memberInfo;
            ValueType = valueType;
            TypeCode = Type.GetTypeCode(ValueType);
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(TAttribute), true);
            MemberAttribute = (TAttribute)customAttributes[0];
            if (string.IsNullOrEmpty(MemberAttribute.Name))
            {
                MemberAttribute.Name = memberInfo.Name;
            }
            getterDelegate = DynamicMethodCreator.CreateGetter(memberInfo);
            setterDelegate = DynamicMethodCreator.CreateSetter(memberInfo);
        }

        /// <summary>
        /// The get value.
        /// </summary>
        /// <param name="target"> The target.</param>
        /// <returns>The value.</returns>
        internal object GetValue(object target)
        {
            return getterDelegate(target);
        }

        /// <summary>
        /// The set value.
        /// </summary>
        /// <param name="target"> The target.</param>
        /// <param name="value">The value.</param>
        internal void SetValue(object target, object value)
        {
            setterDelegate(target, value);
        }

        /// <summary>
        ///  Gets the members attribute.
        /// </summary>
        public TAttribute MemberAttribute { get; private set; }

        /// <summary>
        /// Gets the member info.
        /// </summary>
        public MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// Gets the members <see cref="P:Photon.SocketServer.Rpc.Reflection.ObjectMemberInfo`1.TypeCode"/>.
        /// </summary>
        public TypeCode TypeCode { get; private set; }

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        public Type ValueType { get; private set; }
    }
}
