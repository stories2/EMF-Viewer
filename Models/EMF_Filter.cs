using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emf_viewer.Models
{
    public class EMF_Filter
    {
        public byte[] type { get; set; }
        public int parseLen { get; set; }
        public int parsePoint { get; set; }
    }
}
