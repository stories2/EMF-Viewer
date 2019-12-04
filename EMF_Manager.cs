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
        private const int EMR_SMALLTEXTOUT = 108, EMR_EXTTEXTOUTW = 84;
        public int EMR_SMALLTEXTOUT_TEXT_POINT { get; set; }
        public int EMR_EXTTEXTOUTW_TEXT_POINT { get; set; }
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
                int currentType = BitConverter.ToInt32(emf, emfCnt);

                if (BitConverter.ToInt32(emf, emfCnt + 24) == 1)
                {
                    UInt32 emfTextSize;
                    int textLen;
                    string text;
                    Rect bounds;
                    switch (currentType)
                    {
                        // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-emf/20eee81d-0bd4-42d1-a624-860adfe62358
                        case EMR_SMALLTEXTOUT:
                            emfTextSize = BitConverter.ToUInt32(emf, emfCnt + 4);
                            textLen = BitConverter.ToInt32(emf, emfCnt + 16) * 2;
                            text = System.Text.Encoding.Unicode.GetString(emf.Skip(emfCnt + (EMR_SMALLTEXTOUT_TEXT_POINT > 0 ? EMR_SMALLTEXTOUT_TEXT_POINT : 52) ).Take((int)textLen).ToArray());
                            bounds = new Rect()
                            {
                                x1 = BitConverter.ToUInt32(emf, emfCnt + 8),
                                y1 = BitConverter.ToUInt32(emf, emfCnt + 12),
                                x2 = BitConverter.ToUInt32(emf, emfCnt + 28),
                                y2 = BitConverter.ToUInt32(emf, emfCnt + 32),
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
                            break;
                        // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-emfspool/0af7426e-2767-4456-a4e2-6a57c8c640bd
                        case EMR_EXTTEXTOUTW:
                            emfTextSize = BitConverter.ToUInt32(emf, emfCnt + 4);
                            textLen = BitConverter.ToInt32(emf, emfCnt + 44) * 2;
                            text = System.Text.Encoding.Unicode.GetString(emf.Skip(emfCnt + (EMR_EXTTEXTOUTW_TEXT_POINT > 0 ? EMR_EXTTEXTOUTW_TEXT_POINT : 76)).Take((int)textLen).ToArray());
                            bounds = new Rect()
                            {
                                x1 = BitConverter.ToUInt32(emf, emfCnt + 8),
                                y1 = BitConverter.ToUInt32(emf, emfCnt + 12),
                                x2 = BitConverter.ToUInt32(emf, emfCnt + 16),
                                y2 = BitConverter.ToUInt32(emf, emfCnt + 20),
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
                            break;
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
            this.emfSP = BitConverter.ToInt32(spl, 4);
            return new EMF_Header
            {
                version = BitConverter.ToInt32(spl, 0),
                emfSP = this.emfSP,
                emfSize = BitConverter.ToInt32(spl, this.emfSP + 4)
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
