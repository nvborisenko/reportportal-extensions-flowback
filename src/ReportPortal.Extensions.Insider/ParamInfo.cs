using System;
using System.Collections.Generic;
using System.Text;

namespace ReportPortal.Extensions.Insider
{
    public class ParamInfo
    {
        public ParamInfo(string name, object value)
        {
            Name = name;

            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}
