using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitrixSessionDumper
{
    public class GPOGroupResult
    {
        public List<string> ActiveGPOS {  get; set; }
        public List<string> GhostedGOPS { get; set; }
    }
}
