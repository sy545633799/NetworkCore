using System;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Threading;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// A cache for <see cref="T:Photon.SocketServer.Rpc.Reflection.ObjectMemberInfo`1">ObjectMemberInfo</see> instances.
    /// </summary>
    /// <typeparam name="TAttribute">The ObjectMemberInfo attribute type.</typeparam>
    internal static class ObjectMemberInfoCache<TAttribute> where TAttribute : DataMemberAttribute
    {
        /// <summary>
        /// The dictionary.
        /// </summary>
        private static readonly SynchronizedDictionary<Type, List<ObjectMemberInfo<TAttribute>>> dictionary = new SynchronizedDictionary<Type, List<ObjectMemberInfo<TAttribute>>>();

        /// <summary>
        /// The get member infos.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <returns>A list of mapped properties.</returns>
        private static List<ObjectMemberInfo<TAttribute>> GetMemberInfos(Type targetType)
        {
            List<ObjectMemberInfo<TAttribute>> list = new List<ObjectMemberInfo<TAttribute>>();
            foreach (FieldInfo info in targetType.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (info.IsDefined(typeof(TAttribute), true))
                {
                    ObjectMemberInfo<TAttribute> item = new ObjectMemberInfo<TAttribute>(info, typeof(TAttribute));
                    list.Add(item);
                }
            }
            foreach (PropertyInfo info3 in targetType.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (info3.IsDefined(typeof(TAttribute), true))
                {
                    ObjectMemberInfo<TAttribute> info4 = new ObjectMemberInfo<TAttribute>(info3, typeof(TAttribute));
                    list.Add(info4);
                }
            }
            return list;
        }

        /// <summary>
        /// The get members.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <returns>a list of member infos </returns>
        public static List<ObjectMemberInfo<TAttribute>> GetMembers(Type targetType)
        {
            List<ObjectMemberInfo<TAttribute>> memberInfos;
            if (!ObjectMemberInfoCache<TAttribute>.dictionary.TryGetValue(targetType, out memberInfos))
            {
                memberInfos = ObjectMemberInfoCache<TAttribute>.GetMemberInfos(targetType);
                ObjectMemberInfoCache<TAttribute>.dictionary.TryAdd(targetType, memberInfos);
            }
            return memberInfos;
        }
    }
}
