using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ReportPortal.Extensions.Insider.Test.Internal
{
    public class SimpleClass
    {
        private string _p1;
        public SimpleClass(string p1)
        {
            try
            {
                _p1 = p1;
            }
            catch (Exception)
            {

            }
        }

        public SimpleClass() { }

        [Insider]
        public void SimpleEmptyMethod()
        {

        }

        [Insider(Name = null)]
        public void SimpleMethod()
        {
            var a = DateTime.Now;
            var b = a.AddDays(5);
        }

        public void SimpleMethodWithParams(string a, int b, object c)
        {
            var array = new ParamInfo[5];
            array[0] = new ParamInfo("a", a);
            array[2] = new ParamInfo("b", b);
        }

        [Insider("a")]
        public int SimpleReturnValueMethod()
        {
            var a = 2;
            var b = 3;
            return a + b;
        }

        [Insider("q")]
        public int Sum(int a, int b)
        {
            var isNull = (object)string.IsNullOrEmpty(null);

            var res = a + b;
            var res2 = a + b + 1;
            if (res < res2)
            {
                return res;
            }

            return res2;
        }

        [Insider("a")]
        public Type SimpleReturnRefMethod()
        {
            return typeof(SimpleClass);
        }

        [Insider("asd")]
        public void ThrowsExceptionMethod()
        {
            for (int i = 0; i < 5; i++)
            {

            }

            ThrowInner();
        }

        private void ThrowInner()
        {
            throw new Exception("asd");
        }

        [Insider("asd")]
        public int WithTryCatchBlockMethod()
        {
            try
            {
                try
                {
                    throw new Exception();
                    return 0;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        [Insider(null)]
        public async Task SimpleAsyncMethod()
        {
            await Task.Delay(0);
        }

        public async Task<int> SimpleSumAsyncMethod(int a, int b, Type type)
        {
            using (var scope = Shared.Log.BeginScope("qwe"))
            {

            }
            return a + b;
        }

        [Insider("a")]
        public async Task<int> SimpleAsyncRetValueMethod()
        {
            await Task.Delay(1);
            return 6;
        }

        [Insider("a")]
        public async Task SimpleAsyncThrowsMethod()
        {
            await Task.Delay(0);

            throw new Exception("asd");
        }

        public async Task SimpleEmptyAsyncMethod()
        {

        }

        [Insider("a")]
        public async Task SimpleAsyncMethodWithTryCatch()
        {
            try
            {
                await Task.Delay(0);
            }
            catch (Exception)
            { }
        }

        [Insider(Ignore = true)]
        public void IggnoredMethof()
        {

        }

        public T SomeGenericMethod<T>()
        {
            return default(T);
        }

        readonly IList<DecompressionMethods> _allowedDecompressionMethods = new List<DecompressionMethods>();

        public IList<DecompressionMethods> AllowedDecompressionMethods => _allowedDecompressionMethods.Any()
            ? _allowedDecompressionMethods
            : new[] { DecompressionMethods.None, DecompressionMethods.Deflate, DecompressionMethods.GZip };

        public IList<DateTime> SomeList()
        {
            return new List<DateTime>();
        }

        private object _obj = new object();
        public object ReturnSomeField()
        {
            if (_obj != null)
            {
                return _obj;
            }
            else
            {
                return null;
            }
        }
    }
}
