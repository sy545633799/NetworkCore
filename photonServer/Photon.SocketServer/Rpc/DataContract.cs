using System;
using System.Collections.Generic;
using System.Text;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer.Rpc
{
    public abstract class DataContract
    {
        // Fields
        private readonly string errorMessage;
        private readonly List<ObjectMemberInfo<DataMemberAttribute>> invalidParams;
        private readonly bool isValid;
        private readonly List<ObjectMemberInfo<DataMemberAttribute>> missingParams;

        // Methods
        protected DataContract()
        {
            this.isValid = true;
        }

        protected DataContract(IRpcProtocol protocol, IDictionary<byte, object> dataMembers)
        {
            if (dataMembers != null)
            {
                this.isValid = ObjectDataMemberMapper.TrySetValues<DataMemberAttribute>(this, dataMembers, new ObjectDataMemberMapper.TryConvertDelegate<DataMemberAttribute>(protocol.TryConvertParameter), out this.missingParams, out this.invalidParams);
            }
            else
            {
                this.isValid = true;
            }
            this.errorMessage = this.BuildErrorMessage(dataMembers);
        }

        private string BuildErrorMessage(IDictionary<byte, object> @params)
        {
            if (this.isValid)
            {
                return "Ok";
            }
            StringBuilder builder = new StringBuilder();
            Type type = base.GetType();
            if (this.missingParams != null)
            {
                foreach (ObjectMemberInfo<DataMemberAttribute> info in this.missingParams)
                {
                    builder.AppendFormat("Missing value {0} ({2}.{1})", info.MemberAttribute.Code, info.MemberInfo.Name, type.Name);
                    builder.AppendLine();
                }
            }
            if (this.invalidParams != null)
            {
                foreach (ObjectMemberInfo<DataMemberAttribute> info2 in this.invalidParams)
                {
                    object obj2;
                    if (@params.TryGetValue(info2.MemberAttribute.Code, out obj2) && (obj2 != null))
                    {
                        builder.AppendFormat("Wrong parameter type {0} ({2}.{1}): should be {3} but received {4}. ", new object[] { info2.MemberAttribute.Code, info2.MemberInfo.Name, type.Name, info2.ValueType.Name, obj2.GetType().Name });
                    }
                    else
                    {
                        builder.AppendFormat("Wrong parameter type {0} ({2}.{1}): should be {3} but received null. ", new object[] { info2.MemberAttribute.Code, info2.MemberInfo.Name, type.Name, info2.ValueType.Name });
                    }
                }
            }
            return builder.ToString();
        }

        public string GetErrorMessage()
        {
            return this.errorMessage;
        }

        public Dictionary<byte, object> ToDictionary()
        {
            return ObjectDataMemberMapper.GetValues<DataMemberAttribute>(this);
        }

        // Properties
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }
    }
}
