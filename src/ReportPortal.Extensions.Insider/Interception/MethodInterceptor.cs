using ReportPortal.Shared;
using ReportPortal.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportPortal.Extensions.Insider.Interception
{
    public class MethodInterceptor : IInterceptor
    {
        private readonly ILogScope _parentScope;
        private ILogScope _scope;
        public MethodInterceptor()
        {
            _parentScope = Log.ActiveScope;
            //Console.WriteLine($"Hi from .ctor: {_parentScope.Id}");
        }

        public void OnBefore(string name, ParamInfo[] parameters)
        {
            _scope = _parentScope.BeginScope(name);

            if (parameters != null)
            {
                foreach (var paramInfo in parameters)
                {
                    _scope.Trace($"{paramInfo.Name} `{paramInfo.Value}`");
                }
            }

            //Console.WriteLine($"Hi from .OnBefore: {_scope.Id} " + name);
        }

        public void OnException(Exception exp)
        {
            _scope.Warn(exp.ToString());

            //Console.WriteLine($"Hi from .OnException: {_scope.Id} " + exp.Message);
        }

        public void OnAfter(object result)
        {
            string ret = result == null ? "null" : result.ToString();
            var message = $"Returning `{ret}`";
            _scope.Trace(message);

            //Console.WriteLine($"Hi from .OnAfter {_scope.Id}: Message: {message}");

            _scope.Dispose();
        }

        public void OnAfter()
        {
            _scope.Dispose();
        }
    }
}
