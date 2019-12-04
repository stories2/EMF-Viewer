using emf_viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emf_viewer
{
    class EMF_Manager
    {
        public string targetFile { get; set; }

        public List<EMF_Data> emfDataList { get; }

        EMF_Manager()
        {
            this.targetFile = null;
            this.emfDataList = new List<EMF_Data>();
        }
    }
}
