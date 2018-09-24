using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    internal class Model : IModel
    {
        public int Id { set; get; }
        public string Name { set; get; }
        public string EGRPOU { set; get; }
    }
}
