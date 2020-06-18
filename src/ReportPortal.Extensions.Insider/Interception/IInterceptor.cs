using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace ReportPortal.Extensions.Insider.Interception
{
    public interface IInterceptor
    {
        void OnBefore(string name, ParamInfo[] parameters);

        void OnException(Exception exp);

        void OnAfter(object result);

        void OnAfter();
    }
}
