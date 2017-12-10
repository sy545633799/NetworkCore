using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace test2.Models
{
    public class Blog
    {
        public int Id { set; get; }
        public string Title { set; get; }
        public string Content { set; get; }

        public int UserId { set; get; }
        public virtual User User { set; get; }
    }
}
