using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emf_viewer.Models
{
    class EMF_Header
    {
        public int version { get; set; }
        public int emfSP { get; set; }
        public int emfSize { get; set; }
    }
}
