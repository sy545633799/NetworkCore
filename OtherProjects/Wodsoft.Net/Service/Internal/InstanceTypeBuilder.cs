using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    internal class InstanceTypeBuilder
    {
        private ModuleBuilder _module;
        private AssemblyBuilder _assembly;
        private Dictionary<Type, Type> cache;
        private OperationMode Mode;

        public InstanceTypeBuilder(OperationMode mode)
        {
            _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("InstanceType"), AssemblyBuilderAccess.RunAndCollect);
            _module = _assembly.DefineDynamicModule("InstanceTypeModule", "InstanceType.dll");
            cache = new Dictionary<Type, Type>();
            Mode = mode;
        }

        private void InitConstructor(TypeBuilder builder, FieldBuilder proxy, ConstructorInfo constructorInfo)
        {
            var parameters = new List<Type>();
            parameters.Add(typeof(InstanceProxy));
            if (constructorInfo != null)
                foreach (var parameter in constructorInfo.GetParameters())
                    parameters.Add(parameter.ParameterType);
            var _constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters.ToArray());

            var il = _constructor.GetILGenerator();
            if (constructorInfo != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                for (int i = 1; i < parameters.Count; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Call, constructorInfo);
            }
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, proxy);
            il.Emit(OpCodes.Ret);
        }

        private void InitMethod(TypeBuilder contract, FieldBuilder proxy, MethodInfo method)
        {
            FieldBuilder methodInfo = contract.DefineField("_" + method.Name, typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static);

            ParameterInfo[] parameters = method.GetParameters();

            MethodBuilder builder;
            if (method.ReturnType != typeof(void))
                builder = contract.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, method.ReturnType, parameters.Select(p => p.ParameterType).ToArray());
            else
                builder = contract.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual, null, parameters.Select(p => p.ParameterType).ToArray());

            ILGenerator il = builder.GetILGenerator();
            if (method.ReturnType != typeof(void))
                il.DeclareLocal(method.ReturnType);
            if (parameters.Length > 0)
                il.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, proxy);
            il.Emit(OpCodes.Ldsfld, methodInfo);
            il.Emit(OpCodes.Ldc_I4, method.GetParameters().Length);
            il.Emit(OpCodes.Newarr, typeof(object));

            if (parameters.Length != 0)
            {
                if (method.ReturnType == typeof(void))
                {
                    il.Emit(OpCodes.Stloc_0);
                    il.Emit(OpCodes.Ldloc_0);
                }
                else
                {
                    il.Emit(OpCodes.Stloc_1);
                    il.Emit(OpCodes.Ldloc_1);
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    if (parameters[i].ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, parameters[i].ParameterType);
                    il.Emit(OpCodes.Stelem_Ref);

                    if (method.ReturnType == typeof(void))
                        il.Emit(OpCodes.Ldloc_0);
                    else
                        il.Emit(OpCodes.Ldloc_1);
                }
            }

            il.EmitCall(OpCodes.Callvirt, typeof(InstanceProxy).GetMethod("Invoke"), new Type[] { typeof(MethodInfo), typeof(object[]) });

            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Pop);
            else
            {
                if (!method.ReturnType.IsValueType)
                    il.Emit(OpCodes.Castclass, method.ReturnType);
                else
                    il.Emit(OpCodes.Unbox_Any, method.ReturnType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
            }

            il.Emit(OpCodes.Ret);
        }

        private void Initialize(Type parentType, Type contractType)
        {
            if (cache.ContainsKey(parentType))
                return;

            var contract = _module.DefineType(parentType.Name + "_" + Guid.NewGuid().ToString().Replace("-", ""),
                System.Reflection.TypeAttributes.Public, parentType);

            var proxy = contract.DefineField("Proxy", typeof(InvokeMethodDelegate), FieldAttributes.Private);

            var constructors = parentType.GetConstructors();
            if (constructors.Length > 0)
                foreach (var constructor in parentType.GetConstructors())
                    InitConstructor(contract, proxy, constructor);
            else
                InitConstructor(contract, proxy, null);

            var methods = contractType.GetMethods().Where(t =>
                ((OperationContractAttribute)t.GetCustomAttributes(typeof(OperationContractAttribute), true).FirstOrDefault()).Mode != Mode).ToArray();
            
            foreach (var methodInfo in methods)
                InitMethod(contract,proxy, methodInfo);

            cache.Add(parentType, contract.CreateType());

            foreach (var methodInfo in methods)
                cache[parentType].GetField("_" + methodInfo.Name, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, methodInfo);

            //_assembly.Save(@"channelproxy.dll");
        }

        public Type GetInstanceType(Type parentType, Type contractType)
        {
            Initialize(parentType, contractType);
            return cache[parentType];
        }
    }
}
