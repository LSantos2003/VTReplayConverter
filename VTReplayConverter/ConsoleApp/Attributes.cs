using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTReplayConverter
{

    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string prefix, string description)
        {
            this.Prefix = prefix;
            this.Description = description;
        }

        public CommandAttribute(string prefix, string description, string args)
        {
            this.Prefix = prefix;
            this.Description = description;
            this.Args = args;
        }

        public CommandAttribute(string prefix, string description, bool logAfterCall)
        {
            this.Prefix = prefix;
            this.Description = description;
            this.LogAfterCall = logAfterCall;
        }


        public string Prefix;
        public string Description;
        public string Args;
        public bool LogAfterCall = false;
    }
}
