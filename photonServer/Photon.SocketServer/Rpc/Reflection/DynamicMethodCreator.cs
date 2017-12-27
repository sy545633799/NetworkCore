using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// Provides methods to create delegates for fast dynamic access to properties and fields of a specified target class. 
    /// </summary>
    internal static class DynamicMethodCreator
    {
        /// <summary>
        /// Creates a <see cref="T:System.Func`2">getter</see> for a specified <see cref="T:System.Reflection.MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">
        /// The <see cref="T:System.Reflection.MemberInfo"/> to obtain the <see cref="T:System.Func`2">getter</see> for.
        /// Must be of type <see cref="T:System.Reflection.FieldInfo"/> or <see cref="T:System.Reflection.PropertyInfo"/>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Func`2">getter</see> instance or null if the specified <see cref="T:System.Reflection.MemberInfo"/> does not contain a get method.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// memberInfo is a null reference (Nothing in Visual Basic).
        ///</exception>
        ///<exception cref="T:System.ArgumentException">
        /// memberInfo is not of type <see cref="T:System.Reflection.FieldInfo"/> or <see cref="T:System.Reflection.PropertyInfo"/>.
        ///</exception>
        public static Func<object, object> CreateGetter(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                return CreateGetter((FieldInfo)memberInfo);
            }
            if (memberInfo.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Invalid MemberInfo type.", "memberInfo");
            }
            return CreateGetter((PropertyInfo)memberInfo);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Func`2">getter</see> for a specified property.
        /// </summary>
        /// <param name="propertyInfo">
        /// The <see cref="T:System.Reflection.PropertyInfo"/> of the property for which to create a <see cref="T:System.Func`2">getter</see>.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Func`2">getter</see> instance or null if the specified property does 
        /// not contain a get method.
        ///</returns>
        ///<exception cref="T:System.ArgumentNullException">
        ///propertyInfo is a null reference (Nothing in Visual Basic). 
        ///</exception>
        public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            MethodInfo getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null)
            {
                return null;
            }
            DynamicMethod method = CreateGetDynamicMethod(propertyInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, getMethod);
            BoxIfNeeded(getMethod.ReturnType, iLGenerator);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates a <see cref="T:System.Func`2">getter</see> for a specified field.
        /// </summary>
        /// <param name="fieldInfo">
        /// The <see cref="T:System.Reflection.FieldInfo"/> of the field for which to create a <see cref="T:System.Func`2">getter</see>.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Func`2">getter</see> instance.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///fieldInfo is a null reference (Nothing in Visual Basic). 
        ///</exception>
        public static Func<object, object> CreateGetter(FieldInfo fieldInfo)
        {
            DynamicMethod method = CreateGetDynamicMethod(fieldInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
            BoxIfNeeded(fieldInfo.FieldType, iLGenerator);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        ///  Creates a <see cref="T:System.Func`1">instantiator</see> for a specified type.
        /// </summary>
        /// <param name="type">The type for which to create the delegate.</param>
        /// <returns>An <see cref="T:System.Func`1">instantiator</see> delegate for acessing the types contructor method.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///type paramter is a null reference (Nothing in Visual Basic). 
        ///</exception>
        ///<exception cref="T:System.ArgumentException">
        ///</exception>
        public static Func<object> CreateInstantiateObjectHandler(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            ConstructorInfo con = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            if (con == null)
            {
                throw new ArgumentException(string.Format("The type {0} must declare an empty constructor", type), "type");
            }
            DynamicMethod method = new DynamicMethod("InstantiateObject", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(object), null, type, true);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Newobj, con);
            iLGenerator.Emit(OpCodes.Ret);
            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }

        /// <summary>
        /// Creates a <see cref="T:System.Action`2">setter</see> for a specified <see cref="T:System.Reflection.MemberInfo"/>.
        /// </summary>
        /// <param name="memberInfo">
        /// The <see cref="T:System.Reflection.MemberInfo"/> to obtain the <see cref="T:System.Action`2">setter</see> for.
        ///  Must be of type <see cref="T:System.Reflection.FieldInfo"/> or <see cref="T:System.Reflection.PropertyInfo"/>.
        ///  </param>
        /// <returns>
        /// A <see cref="T:System.Action`2">setter</see> instance or null if the specified <see cref="T:System.Reflection.MemberInfo"/> 
        ///  does not contain a set method.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///  memberInfo is a null reference (Nothing in Visual Basic).
        ///</exception>
        ///<exception cref="T:System.ArgumentException">
        ///  memberInfo is not of type <see cref="T:System.Reflection.FieldInfo"/> or <see cref="T:System.Reflection.PropertyInfo"/>.
        ///</exception>
        public static Action<object, object> CreateSetter(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                return CreateSetter((FieldInfo)memberInfo);
            }
            if (memberInfo.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Invalid MemberInfo type.", "memberInfo");
            }
            return CreateSetter((PropertyInfo)memberInfo);
        }

        /// <summary>
        ///  Creates a <see cref="T:System.Action`2">setter</see> for a specified field.
        /// </summary>
        /// <param name="propertyInfo">
        /// The <see cref="T:System.Reflection.PropertyInfo"/> of the property for which to create a <see cref="T:System.Action`2">setter</see>.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Action`2">setter</see> instance.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///  propertyInfo is a null reference (Nothing in Visual Basic). 
        ///  </exception>
        public static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            MethodInfo setMethod = propertyInfo.GetSetMethod(true);
            if (setMethod == null)
            {
                return null;
            }
            DynamicMethod method = CreateSetDynamicMethod(propertyInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            UnboxOrCast(propertyInfo.PropertyType, iLGenerator);
            iLGenerator.Emit(OpCodes.Call, setMethod);
            iLGenerator.Emit(OpCodes.Ret);
            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// Creates a <see cref="T:System.Action`2">setter</see> for a specified field.
        /// </summary>
        /// <param name="fieldInfo">
        /// The <see cref="T:System.Reflection.FieldInfo"/> of the field for which to create a <see cref="T:System.Action`2">setter</see>.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Action`2">setter</see> instance.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///fieldInfo is a null reference (Nothing in Visual Basic). 
        ///</exception>
        public static Action<object, object> CreateSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException("fieldInfo");
            }
            DynamicMethod method = CreateSetDynamicMethod(fieldInfo.DeclaringType);
            ILGenerator iLGenerator = method.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldarg_1);
            UnboxOrCast(fieldInfo.FieldType, iLGenerator);
            iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
            iLGenerator.Emit(OpCodes.Ret);
            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        ///  Helper method to generate "Boxing" code for value types.
        /// </summary>
        /// <param name="type">The type to genrate the code for.</param>
        /// <param name="generator">An <see cref="T:System.Reflection.Emit.ILGenerator"/> instance.</param>
        private static void BoxIfNeeded(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Box, type);
            }
        }


        /// <summary>
        ///  Creates a dynamic get method for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A <see cref="T:System.Reflection.Emit.DynamicMethod"/> instace.</returns>
        private static DynamicMethod CreateGetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicGet", typeof(object), new Type[] { typeof(object) }, type, true);
        }

        /// <summary>
        ///  Creates a dynamic set method for a specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A <see cref="T:System.Reflection.Emit.DynamicMethod"/> instace.</returns>
        private static DynamicMethod CreateSetDynamicMethod(Type type)
        {
            return new DynamicMethod("DynamicSet", typeof(void), new Type[] { typeof(object), typeof(object) }, type, true);
        }

        /// <summary>
        ///  Generates unboxing code for value types or casting code for reference types.
        /// </summary>
        /// <param name="type">The type to generate code for</param>
        /// <param name="generator">An <see cref="T:System.Reflection.Emit.ILGenerator"/> instance.</param>
        private static void UnboxOrCast(Type type, ILGenerator generator)
        {
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, type);
            }
        }
    }
}
