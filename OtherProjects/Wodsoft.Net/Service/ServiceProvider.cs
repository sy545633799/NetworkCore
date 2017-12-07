using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    public class ServiceProvider
    {
        public ServiceProvider(Type instanceType, Type contractType)
        {
            if (contractType.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length == 0)
                throw new ArgumentException("contractType不是契约接口。");

            _ServerOperations = new List<MethodInfo>();
            _ClientOperations = new List<MethodInfo>();

            foreach (var member in contractType.GetMembers())
            {
                var operation = member.GetCustomAttributes(typeof(OperationContractAttribute), false).FirstOrDefault() as OperationContractAttribute;
                if (operation == null)
                    throw new ArgumentException("contractType包含非操作契约方法。");
                else
                {
                    if (operation.Mode == OperationMode.Server)
                        _ServerOperations.Add((MethodInfo)member);
                    else
                        _ClientOperations.Add((MethodInfo)member);
                }
            }

            if (!instanceType.GetInterfaces().Contains(contractType))
                throw new ArgumentException("serviceType没有实现contractType契约接口。");

            Instance = instanceType;
            Contract = contractType;

            ServiceModeAttribute serviceMode = Instance.GetCustomAttributes(typeof(ServiceModeAttribute), false).FirstOrDefault() as ServiceModeAttribute;
            if (serviceMode == null)
                Mode = ServiceMode.Single;
            else
                Mode = serviceMode.Mode;
        }

        public Type Instance { get; private set; }

        public Type Contract { get; private set; }

        public ServiceMode Mode { get; private set; }

        private List<MethodInfo> _ServerOperations;
        public List<MethodInfo> ServerOperations { get { return _ServerOperations.ToList(); } }

        private List<MethodInfo> _ClientOperations;
        public List<MethodInfo> ClientOperations { get { return _ClientOperations.ToList(); } }
    }
}
