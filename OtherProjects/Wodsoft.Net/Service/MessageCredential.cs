using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class MessageCredential : Credential
    {
        public MessageCredential(string username, string password)
        {
            _Username = username;
            Password = password;
        }

        private string _Username;
        public override string Username
        {
            get { throw new NotImplementedException(); }
        }

        public string Password { get; private set; }
    }
}
