using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NQMP.Object.Disciple
{
   public class GetTabDisciple
    {
        public static GetTabDisciple gI;
        public bool setTab;
        static GetTabDisciple()
        {
            gI = new GetTabDisciple();
        }
    }
}
