using System;
using System.Collections.Generic;
using System.Reflection;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    ///     This cache enables high performance mapping of operation codes to operations.
    ///      It collaborates with the <see cref="T:Photon.SocketServer.Rpc.Reflection.OperationDispatcher"/>. 
    ///      Instances of this class should be reused if possible since the method registration is slower than the mapping.
    ///      Registered methods must have the followig structure:
    ///      <code>
    ///         [Operation(OperationCode = 1)]
    ///         OperationResponse MyMethod(PeerBase peer, OperationRequest request); 
    ///       </code>
    /// </summary>
    public sealed class OperationMethodInfoCache
    {
        /// <summary>
        ///  The delegate type.
        /// </summary>
        internal static readonly Type OperationDelegateType = typeof(Func<PeerBase, OperationRequest, SendParameters, OperationResponse>);

        /// <summary>
        ///  The parameter 1.
        /// </summary>
        private static readonly Type parameter1 = typeof(PeerBase);

        /// <summary>
        ///  The parameter 2.
        /// </summary>
        private static readonly Type parameter2 = typeof(OperationRequest);

        /// <summary>
        /// The parameter 3.
        /// </summary>
        private static readonly Type parameter3 = typeof(SendParameters);

        /// <summary>
        ///  The return type.
        /// </summary>
        private static readonly Type returnType = typeof(OperationResponse);

        /// <summary>
        ///  The search type.
        /// </summary>
        private static readonly Type searchType = typeof(OperationAttribute);

        /// <summary>
        /// The functions.
        /// </summary>
        private readonly Dictionary<byte, Func<PeerBase, OperationRequest, SendParameters, OperationResponse>> functions = new Dictionary<byte, Func<PeerBase, OperationRequest, SendParameters, OperationResponse>>();

        /// <summary>
        ///  The dictionary.
        /// </summary>
        private readonly Dictionary<byte, MethodInfo> methodInfos = new Dictionary<byte, MethodInfo>();

        /// <summary>
        ///  The operation codes.
        /// </summary>
        private readonly HashSet<byte> operationCodes = new HashSet<byte>();

        /// <summary>
        /// Registers a method to map by operation code.
        /// </summary>
        /// <param name="method">The method to register.</param>
        /// <returns>true if successfully registered.</returns>
        /// <exception cref="T:System.ArgumentException">
        ///        method already registered
        ///  </exception>
        public bool RegisterOperation(Func<PeerBase, OperationRequest, SendParameters, OperationResponse> method)
        {
            return this.RegisterOperation(method.Method);
        }

        /// <summary>
        ///  Registers a method tp map by operation code.
        ///      The method must have the following structure:
        ///      <code>
        ///        [Operation(OperationCode = 1)]
        ///        OperationResponse MyMethod(Peer peer, OperationRequest request); 
        ///      </code>
        /// </summary>
        /// <param name="method">The method to register.</param>
        /// <returns>true if successfully registered.</returns>
        /// <exception cref="T:System.ArgumentException">
        ///       method already registered
        ///   </exception>
        public bool RegisterOperation(MethodInfo method)
        {
            if (!method.IsDefined(searchType, true))
            {
                return false;
            }
            foreach (OperationAttribute attribute in method.GetCustomAttributes(searchType, true))
            {
                if (!this.operationCodes.Add(attribute.OperationCode))
                {
                    MethodInfo info;
                    if (!this.methodInfos.TryGetValue(attribute.OperationCode, out info))
                    {
                        Func<PeerBase, OperationRequest, SendParameters, OperationResponse> func = this.functions[attribute.OperationCode];
                        info = func.Method;
                    }
                    throw new ArgumentException(string.Format("An operation with the same code has already been added. Code {0}; Methods {1}.{2} vs {3}.{4}", new object[] { attribute.OperationCode, method.ReflectedType.Name, method.Name, info.ReflectedType.Name, info.Name }));
                }
                if (method.ReturnType != returnType)
                {
                    throw new ArgumentException(string.Format("Operation {0}.{1} has wrong return type; {2} required.", method.ReflectedType.Name, method.Name, returnType.Name));
                }
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 3)
                {
                    throw new ArgumentException(string.Format("Operation {0}.{1} has wrong parameter count; 3 required.", method.ReflectedType.Name, method.Name));
                }
                if (parameters[0].ParameterType != parameter1)
                {
                    throw new ArgumentException(string.Format("Operation {0}.{1} has wrong parameter 1; {2} required.", method.ReflectedType.Name, method.Name, parameter1.Name));
                }
                if (parameters[1].ParameterType != parameter2)
                {
                    throw new ArgumentException(string.Format("Operation {0}.{1} has wrong parameter 2; {2} required.", method.ReflectedType.Name, method.Name, parameter2.Name));
                }
                if (parameters[2].ParameterType != parameter3)
                {
                    throw new ArgumentException(string.Format("Operation {0}.{1} has wrong parameter 3; {2} required.", method.ReflectedType.Name, method.Name, parameter3.Name));
                }
                if (method.IsStatic)
                {
                    Func<PeerBase, OperationRequest, SendParameters, OperationResponse> func2 = (Func<PeerBase, OperationRequest, SendParameters, OperationResponse>)Delegate.CreateDelegate(OperationDelegateType, method);
                    this.functions.Add(attribute.OperationCode, func2);
                }
                else
                {
                    this.methodInfos.Add(attribute.OperationCode, method);
                }
            }
            return true;
        }

        /// <summary>
        /// Registers all methods of the <paramref name="targetType"/> that are flagged with the <see cref="T:Photon.SocketServer.Rpc.Reflection.OperationAttribute"/>.
        ///         Theses methods must have the following structure:
        ///         <code>
        ///          [Operation(OperationCode = 1)]
        ///          OperationResponse MyMethod(Peer peer, OperationRequest request); 
        ///        </code>
        /// </summary>
        /// <param name="targetType">The target Type.</param>
        public void RegisterOperations(Type targetType)
        {
            foreach (MethodInfo info in targetType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                this.RegisterOperation(info);
            }
        }

        /// <summary>
        /// Gets OperationFunctions.
        /// </summary>
        internal Dictionary<byte, Func<PeerBase, OperationRequest, SendParameters, OperationResponse>> OperationFunctions
        {
            get
            {
                return this.functions;
            }
        }

        /// <summary>
        /// Gets OperationMethodInfos.
        /// </summary>
        internal Dictionary<byte, MethodInfo> OperationMethodInfos
        {
            get
            {
                return this.methodInfos;
            }
        }
    }
}
