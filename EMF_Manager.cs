using emf_viewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emf_viewer
{
    public class EMF_Manager
    {
        private string targetFile;

        public List<EMF_Filter> filterList { get; set; }

        private byte[] splFile;

        private int emfSP;

        public EMF_Manager()
        {
            this.targetFile = null;
            this.filterList = new List<EMF_Filter>();
        }

        public void SetTargetFile(string path)
        {
            targetFile = path;
        }

        public List<EMF_Data> Anal(out Exception e)
        {
            List<EMF_Data> emfDataList = null;
            EMF_Header emfHeader;
            e = null;
            try
            {
                splFile = File.ReadAllBytes(targetFile);
                if (this.splFile.Length <= 0)
                    throw new Exception("Zero length file.");

                emfHeader = GetHeader(splFile);

                emfDataList = GetData(splFile.Skip(emfHeader.emfSP + 8).Take(emfHeader.emfSize).ToArray());
            }
            catch(Exception err)
            {
                e = err;
            }
            return emfDataList;
        }

        private List<EMF_Data> GetData(byte[] emf)
        {
            List<EMF_Data> dataList = new List<EMF_Data>();

            int emfCnt;
            for(emfCnt = GetFastestFilteredIndexPoint(emf, 0); emfCnt < emf.Length; emfCnt ++)
            {
                int currentType = BitConverter.ToInt32(emf.Skip(emfCnt).Take(4).ToArray(), 0);

                if (BitConverter.ToInt32(emf.Skip(emfCnt + 24).Take(4).ToArray(), 0) == 1)
                {
                    foreach (EMF_Filter filter in filterList)
                    {
                        if (BitConverter.ToInt32(filter.type, 0) == currentType)
                        {
                            UInt32 emfTextSize = BitConverter.ToUInt32(emf.Skip(emfCnt + 4).Take(4).ToArray(), 0);
                            int textLen = 0;
                            string text = null;

                            if (filter.parseLen > 0)
                                textLen = filter.parseLen;
                            else
                                textLen = BitConverter.ToInt32(emf.Skip(emfCnt + 44).Take(4).ToArray(), 0) * 2;

                            if (filter.parsePoint > 0)
                                text = System.Text.Encoding.Unicode.GetString(emf.Skip(emfCnt + filter.parsePoint).Take((int)textLen).ToArray());
                            else
                                text = System.Text.Encoding.Unicode.GetString(emf.Skip(emfCnt + 76).Take((int)textLen).ToArray());

                            Rect bounds = new Rect()
                            {
                                x1 = BitConverter.ToUInt32(emf.Skip(emfCnt + 8).Take(4).ToArray(), 0),
                                y1 = BitConverter.ToUInt32(emf.Skip(emfCnt + 12).Take(4).ToArray(), 0),
                                x2 = BitConverter.ToUInt32(emf.Skip(emfCnt + 16).Take(4).ToArray(), 0),
                                y2 = BitConverter.ToUInt32(emf.Skip(emfCnt + 20).Take(4).ToArray(), 0),
                            };

                            if (text.Length > 0)
                            {
                                dataList.Add(new EMF_Data
                                {
                                    areaType = currentType,
                                    bounds = bounds,
                                    size = emfTextSize,
                                    sp = emfCnt,
                                    text = text
                                });
                            }
                        }
                    }
                }
                emfCnt = GetFastestFilteredIndexPoint(emf, emfCnt + 1) - 1;
                if (emfCnt == 0x7fffffff)
                {
                    break;
                }
            }

            return dataList;
        }

        private int GetFastestFilteredIndexPoint(byte[] emf, int searchPoint)
        {
            int min = 0x7fffffff, point;
            foreach(EMF_Filter filter in filterList)
            {
                point = ByteSearch(emf, filter.type, searchPoint);
                if (point != -1 && point < min)
                    min = point;
            }
            return min;
        }

        private EMF_Header GetHeader(byte[] spl)
        {
            this.emfSP = BitConverter.ToInt32(spl.Skip(4).Take(4).ToArray(), 0);
            return new EMF_Header
            {
                version = BitConverter.ToInt32(spl.Take(4).ToArray(), 0),
                emfSP = this.emfSP,
                emfSize = BitConverter.ToInt32(spl.Skip(this.emfSP + 4).Take(4).ToArray(), 0)
            };
        }

        private int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up
                            if (matched)
                            {
                                found = i;
                                break;
                            }
                        }
                        else
                        {
                            //search byte is only one bit nothing else to do
                            found = i;
                            break; //stop the loop
                        }
                    }
                }
            }
            return found;
        }
    }
}
