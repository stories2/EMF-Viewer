using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emf_viewer.Models
{
    class EMF_Data
    {
        public UInt32 areaType { get; set; }
        public int sp { get; set; }
        public UInt32 size { get; set; }
        public Rect bounds { get; set; }
        public string text { get; set; }
    }
}
