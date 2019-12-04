using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using emf_viewer;
using emf_viewer.Models;
using System.IO;

namespace TESTER
{
    class Program
    {
        static void Main(string[] args)
        {
            Exception e = null;
            List<EMF_Data> dataList = null;
            EMF_Manager manager = new EMF_Manager();

            manager.SetTargetFile("00002.SPL");
            manager.filterList.Add(new EMF_Filter
            {
                type = new byte[] { 84, 0, 0, 0 },
            });
            dataList = manager.Anal(out e);

            using (TextWriter tw = new StreamWriter("report.csv"))
            {
                foreach (EMF_Data data in dataList)
                {
                    tw.WriteLine($"{data.sp},{data.areaType},{data.size},{data.bounds.x1},{data.bounds.y1},{data.bounds.x2},{data.bounds.y2},{data.text}");
                }
            }

            return;
        }
    }
}
