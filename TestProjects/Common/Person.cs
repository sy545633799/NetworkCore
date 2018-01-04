using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

[Serializable]
public class Person
{
    public int Age;
    public string Job;
    public string Name;

    public override string ToString()
    {
        return string.Format("姓名：{0}\t年龄：{1}\t职业：{2}", Name, Age, Job);
    }
}
