using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{
    public class ACMIObjects
    {
        public static List<TacviewObject> UnitList = new List<TacviewObject>();

        public static void InitilizeUnitDict()
        {
            string[] units = File.ReadAllLines(Program.ObjectConverterPath);

            foreach(string unit in units)
            {

                string[] split = unit.Split(':');
                string tacviewName = split.Length > 1 ? split[1] : "none";

                UnitList.Add(new TacviewObject(split[0], tacviewName));
            }
        }

        public class TacviewObject
        {
            public TacviewObject(string vtolName, string tacviewObjName)
            {
                this.vtolName = vtolName;
                this.tacviewObjName = tacviewObjName;
            }

            public string vtolName;
            public string tacviewObjName;
        }

    }
}
