using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// Provides methods to get and set objects fields and properties per reflection.
    /// </summary>
    public sealed class ObjectDataMemberMapper
    {
        /// <summary>
        /// The get values.
        /// </summary>
        /// <typeparam name="TAttribute">The source.</typeparam>
        /// <param name="source">Search type for fields of <paramref name="source"/>.</param>
        /// <returns>A dictionary with the key/value pairs of error code 1 if a value is null.</returns>
        /// <exception cref="T:System.ArgumentException">
        /// Mandatory member of <paramref name="source"/> is null.
        ///</exception>
        public static Dictionary<byte, object> GetValues<TAttribute>(object source) where TAttribute : DataMemberAttribute
        {
            Type targetType = source.GetType();
            List<ObjectMemberInfo<TAttribute>> members = ObjectMemberInfoCache<TAttribute>.GetMembers(targetType);
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(members.Count);
            foreach (ObjectMemberInfo<TAttribute> info in members)
            {
                object obj2 = info.GetValue(source);
                if (obj2 == null)
                {
                    if (!info.MemberAttribute.IsOptional)
                    {
                        throw new ArgumentException(string.Format("Null value: {0} ({2}.{1})", info.MemberAttribute.Code, info.MemberInfo.Name, targetType.Name));
                    }
                }
                else
                {
                    dictionary.Add(info.MemberAttribute.Code, obj2);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Gets a data objects member values as a name value dictionary.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="source">The data object source.</param>
        /// <returns>returns a dictionary containing the data objects member values.</returns>
        public static Dictionary<string, object> GetValuesByName<TAttribute>(object source) where TAttribute : DataMemberAttribute
        {
            Type targetType = source.GetType();
            List<ObjectMemberInfo<TAttribute>> members = ObjectMemberInfoCache<TAttribute>.GetMembers(targetType);
            Dictionary<string, object> dictionary = new Dictionary<string, object>(members.Count);
            foreach (ObjectMemberInfo<TAttribute> info in members)
            {
                object obj2 = info.GetValue(source);
                if (obj2 == null)
                {
                    if (!info.MemberAttribute.IsOptional)
                    {
                        throw new ArgumentException(string.Format("Null value: {0} ({2}.{1})", info.MemberAttribute.Code, info.MemberInfo.Name, targetType.Name));
                    }
                }
                else
                {
                    dictionary.Add(info.MemberAttribute.Name, obj2);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// The try set values.
        /// </summary>
        /// <typeparam name="TAttribute"> the attribute type</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="values">The values.</param>
        /// <param name="convertMethod"> The convert method.</param>
        /// <param name="missingParams">The missing params.</param>
        /// <param name="invalidParams">The invalid params.</param>
        /// <returns>true if successful.</returns>
        public static bool TrySetValues<TAttribute>(object target, IDictionary<byte, object> values, TryConvertDelegate<TAttribute> convertMethod, out List<ObjectMemberInfo<TAttribute>> missingParams, out List<ObjectMemberInfo<TAttribute>> invalidParams) where TAttribute : DataMemberAttribute
        {
            missingParams = null;
            invalidParams = null;
            foreach (ObjectMemberInfo<TAttribute> info in ObjectMemberInfoCache<TAttribute>.GetMembers(target.GetType()))
            {
                object obj2;
                if (!values.TryGetValue(info.MemberAttribute.Code, out obj2) || (obj2 == null))
                {
                    if (!info.MemberAttribute.IsOptional)
                    {
                        if (missingParams == null)
                        {
                            missingParams = new List<ObjectMemberInfo<TAttribute>>();
                        }
                        missingParams.Add(info);
                    }
                }
                else if (info.ValueType.IsInstanceOfType(obj2))
                {
                    info.SetValue(target, obj2);
                }
                else if (convertMethod(info, ref obj2) && info.ValueType.IsInstanceOfType(obj2))
                {
                    info.SetValue(target, obj2);
                }
                else
                {
                    if (invalidParams == null)
                    {
                        invalidParams = new List<ObjectMemberInfo<TAttribute>>();
                    }
                    invalidParams.Add(info);
                }
            }
            return (missingParams == null && invalidParams == null);
        }

        /// <summary>
        ///   The try set values.
        /// </summary>
        /// <typeparam name="TAttribute"> the attribute type</typeparam>
        /// <param name="target">The target.</param>
        /// <param name="values">The values.</param>
        /// <param name="convertMethod"> The convert method.</param>
        /// <param name="missingParams">The missing params.</param>
        /// <param name="invalidParams">The invalid params.</param>
        /// <returns>true if successful.</returns>
        public static bool TrySetValues<TAttribute>(object target, IDictionary<string, object> values, TryConvertDelegate<TAttribute> convertMethod, out List<ObjectMemberInfo<TAttribute>> missingParams, out List<ObjectMemberInfo<TAttribute>> invalidParams) where TAttribute : DataMemberAttribute
        {
            missingParams = null;
            invalidParams = null;
            foreach (ObjectMemberInfo<TAttribute> info in ObjectMemberInfoCache<TAttribute>.GetMembers(target.GetType()))
            {
                object obj2;
                if (!values.TryGetValue(info.MemberAttribute.Name, out obj2))
                {
                    if (!info.MemberAttribute.IsOptional)
                    {
                        if (missingParams == null)
                        {
                            missingParams = new List<ObjectMemberInfo<TAttribute>>();
                        }
                        missingParams.Add(info);
                    }
                }
                else if (convertMethod(info, ref obj2) && info.ValueType.IsInstanceOfType(obj2))
                {
                    info.SetValue(target, obj2);
                }
                else
                {
                    if (invalidParams == null)
                    {
                        invalidParams = new List<ObjectMemberInfo<TAttribute>>();
                    }
                    invalidParams.Add(info);
                }
            }
            return (missingParams == null && invalidParams == null);
        }

        /// <summary>
        /// The try convert delegate.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute type</typeparam>
        /// <param name="targetMember">The target member.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if value is legal.</returns>
        public delegate bool TryConvertDelegate<TAttribute>(ObjectMemberInfo<TAttribute> targetMember, ref object value) where TAttribute : DataMemberAttribute;
    }
}
