using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal interface IModel
    {
        int Id { set; get; }
        string Name { set; get; }
        string EGRPOU { set; get; }
    }
}
