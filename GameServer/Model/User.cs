using GameServer.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GameServer.Model
{
    //[Table("user")]
    public class User : Model<User>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public int Ran { get; set; }
        public int AccountId { get; set; }
        public string HeroList { get; set; }

        public override void Add(){ Add(context => { context.Users.Add(this); }); }
    }
}
