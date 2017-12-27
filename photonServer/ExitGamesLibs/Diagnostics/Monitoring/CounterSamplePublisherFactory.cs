using System;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Diagnostics.Counter;
using ExitGames.Logging;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// A factory for <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplePublisher"/>s.
    /// </summary>
    public class CounterSamplePublisherFactory
    {
        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a counter publisher.
        /// </summary>
        /// <param name="counterSamplePublisher">The counter publisher.</param>
        /// <param name="counterContainer">An object providing counter instance members.</param>
        public static void InitializeCounterPublisher(CounterSamplePublisher counterSamplePublisher, object counterContainer)
        {
            InitializeCounterPublisher(counterSamplePublisher, counterContainer, string.Empty);
        }

        /// <summary>
        /// Initializes a counter publisher.
        /// </summary>
        /// <param name="counterSamplePublisher">The counter publisher.</param>
        /// <param name="counterContainerType">ype of the counter container.</param>
        public static void InitializeCounterPublisher(CounterSamplePublisher counterSamplePublisher, Type counterContainerType)
        {
            MemberInfo[] infoArray = counterContainerType.GetMembers(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            InitializeCounterPublisher(counterSamplePublisher, null, infoArray, string.Empty);
        }

        /// <summary>
        ///   Initializes a counter publisher.
        /// </summary>
        /// <param name="counterSamplePublisher">The counter publisher.</param>
        /// <param name="counterSet">An object providing counter instance members.</param>
        /// <param name="counterSetName">The name of the counter set.</param>
        public static void InitializeCounterPublisher(CounterSamplePublisher counterSamplePublisher, object counterSet, string counterSetName)
        {
            Type type = counterSet.GetType();
            MemberInfo[] infoArray = type.GetMembers(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            InitializeCounterPublisher(counterSamplePublisher, counterSet, infoArray, counterSetName);
        }

        /// <summary>
        ///   Initializes a counter publisher.
        /// </summary>
        /// <param name="counterSamplePublisher">The counter publisher.</param>
        /// <param name="counterContainerType">Type of the counter container.</param>
        /// <param name="counterSetName">Name of the counter set.</param>
        public static void InitializeCounterPublisher(CounterSamplePublisher counterSamplePublisher, Type counterContainerType, string counterSetName)
        {
            MemberInfo[] infoArray = counterContainerType.GetMembers(BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            InitializeCounterPublisher(counterSamplePublisher, null, infoArray, counterSetName);
        }

        /// <summary>
        /// Initializes a counter publisher.
        /// </summary>
        /// <param name="counterSamplePublisher">The counter publisher.</param>
        /// <param name="counterSet">An object providing counter instance members.</param>
        /// <param name="memberList">The member list.</param>
        /// <param name="counterSetName">Name of the counter set.</param>
        private static void InitializeCounterPublisher(CounterSamplePublisher counterSamplePublisher, object counterSet, IEnumerable<MemberInfo> memberList, string counterSetName)
        {
            foreach (var member in memberList)
            {
                PublishCounterAttribute attribute = GetCounterAttribute(member);
                if (attribute != null)
                {
                    if (!IsCounterType(member))
                    {
                        if (log.IsWarnEnabled)
                        {
                            log.WarnFormat("PublishCounterAttribute applied to member '{0}' which is not a ICounter type.", member.Name);
                        }
                    }
                    else
                    {
                        ICounter counter = (ICounter)GetMemberValue(counterSet, member);
                        if (counter == null)
                        {
                            if (log.IsWarnEnabled)
                            {
                                log.WarnFormat("Counter for member {0} has not been initialized.", member.Name);
                            }
                        }
                        else if (counter.CounterType != CounterType.Undefined)
                        {
                            string name = GetCounterName(member, attribute, counterSetName);
                            counterSamplePublisher.AddCounter(counter, name);
                            if (log.IsDebugEnabled)
                            {
                                log.DebugFormat("Added counter '{0}' to counter publisher.", name);
                            }
                        }
                    }
                }
                else
                {
                    CounterSetAttribute attribute2 = GetCounterSetAttribute(member);
                    if (attribute2 != null)
                    {
                        if (GetMemberValue(counterSet, member) == null)
                        {
                            if (log.IsWarnEnabled)
                            {
                                log.WarnFormat("CounterSet for member {0} has not been initialized.", member.Name);
                            }
                        }
                        else
                        {
                            InitializeCounterPublisher(counterSamplePublisher, counterSet, GetCounterSetName(member, attribute2, counterSetName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="T:ExitGames.Diagnostics.Monitoring.PublishCounterAttribute"/> 
        /// from a <see cref="T:System.Reflection.MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>A <see cref="T:ExitGames.Diagnostics.Monitoring.PublishCounterAttribute"/> or null.</returns>
        private static PublishCounterAttribute GetCounterAttribute(MemberInfo memberInfo)
        {
            object[] objArray = memberInfo.GetCustomAttributes(typeof(PublishCounterAttribute), false);
            if (objArray.Length > 0)
            {
                return (PublishCounterAttribute)objArray[0];
            }
            return null;
        }

        /// <summary>
        /// Returns the name of a <see 
        /// cref="T:ExitGames.Diagnostics.Monitoring.PublishCounterAttribute"/> or the <see
        /// cref="T:System.Reflection.MemberInfo"/> name combined with a counter set name.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="publishCounterAttribute">The publish Counter Attribute.</param>
        /// <param name="counterSetName">The counter Set Name.</param>
        /// <returns>The name of the <paramref name="publishCounterAttribute"/> or the <paramref name="memberInfo"/> combined with <paramref name="counterSetName"/>.</returns>
        private static string GetCounterName(MemberInfo memberInfo, PublishCounterAttribute publishCounterAttribute, string counterSetName)
        {
            if (string.IsNullOrEmpty(publishCounterAttribute.Name))
            {
                return GetName(memberInfo.Name, counterSetName);
            }
            return GetName(publishCounterAttribute.Name, counterSetName);
        }

        /// <summary>
        /// Gets a <see 
        /// cref="M:ExitGames.Diagnostics.Monitoring.CounterSamplePublisherFactory.GetCounterSetAttribute(System.Reflection.MemberInfo)"/> from a <see 
        /// cref="T:System.Reflection.MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>A <see 
        /// cref="M:ExitGames.Diagnostics.Monitoring.CounterSamplePublisherFactory.GetCounterSetAttribute(System.Reflection.MemberInfo)"/> or null.</returns>

        private static CounterSetAttribute GetCounterSetAttribute(MemberInfo memberInfo)
        {
            object[] objArray = memberInfo.GetCustomAttributes(typeof(CounterSetAttribute), false);
            if (objArray.Length > 0)
            {
                return (CounterSetAttribute)objArray[0];
            }
            return null;
        }

        /// <summary>
        ///  Returns the name of a <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSetAttribute"/> or the <see cref="T:System.Reflection.MemberInfo"/> name combined with a parent counter set name.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        /// <param name="counterSetAttribute">The counter set attribute.</param>
        /// <param name="parentCounterSetName">The parent counter set name.</param>
        /// <returns>The name of the <paramref name="counterSetAttribute"/> or the <paramref name="memberInfo"/> combined with <paramref name="parentCounterSetName"/>.</returns>

        private static string GetCounterSetName(MemberInfo memberInfo, CounterSetAttribute counterSetAttribute, string parentCounterSetName)
        {
            if (string.IsNullOrEmpty(counterSetAttribute.Name))
            {
                return GetName(memberInfo.Name, parentCounterSetName);
            }
            return GetName(counterSetAttribute.Name, parentCounterSetName);
        }

        /// <summary>
        ///  Gets the value of a field or property.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="memberInfo">The member info.</param>
        /// <returns>The value or false.</returns>
        private static object GetMemberValue(object obj, MemberInfo memberInfo)
        {
            MemberTypes types = memberInfo.MemberType;
            if (types != MemberTypes.Field)
            {
                if (types == MemberTypes.Property)
                {
                    return ((PropertyInfo)memberInfo).GetValue(obj, null);
                }
                return false;
            }
            return ((FieldInfo)memberInfo).GetValue(obj);
        }

        /// <summary>
        ///  Helper method to build counter names.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentName">Name of the parent.</param>
        /// <returns><paramref name = "name" /> if <paramref name = "parentName" /> is not set, otherwise "<paramref name = "name" />.<paramref name = "parentName" />".</returns>
        private static string GetName(string name, string parentName)
        {
            if (string.IsNullOrEmpty(parentName))
            {
                return name;
            }
            return string.Format("{0}.{1}", parentName, name);
        }

        /// <summary>
        ///  Checks whether field or property is a counter.
        /// </summary>
        /// <param name="memberInfo"> The member info.</param>
        /// <returns>True if field or property is a counter.</returns>
        private static bool IsCounterType(MemberInfo memberInfo)
        {
            MemberTypes types = memberInfo.MemberType;
            if (types != MemberTypes.Field)
            {
                return ((types == MemberTypes.Property) && IsCounterType(((PropertyInfo)memberInfo).PropertyType));
            }
            return IsCounterType(((FieldInfo)memberInfo).FieldType);
        }

        /// <summary>
        /// Checks whether field or property is of type <see cref="T:ExitGames.Diagnostics.Counter.ICounter"/>.
        /// </summary>
        /// <param name="type1">The member type.</param>
        /// <returns>True on success.</returns>
        private static bool IsCounterType(Type memberType)
        {
            return typeof(ICounter).Equals(memberType);
        }
    }
}
