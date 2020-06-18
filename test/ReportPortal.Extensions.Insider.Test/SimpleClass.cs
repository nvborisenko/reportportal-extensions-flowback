using NUnit.Framework;
using ReportPortal.Extensions.Insider.Test.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportPortal.Extensions.Insider.Test
{
    public class SimpleClass
    {
        private Internal.SimpleClass _simpleClass = new Internal.SimpleClass("q");

        [Test]
        public void SimpleEmptyMethod()
        {
            _simpleClass.SimpleEmptyMethod();
        }

        [Test]
        public void SimpleMethod()
        {
            _simpleClass.SimpleMethod();
        }

        [Test]
        public void SimpleMethodWithParams()
        {
            _simpleClass.SimpleMethodWithParams("qwe", 2, null);
        }

        [Test]
        public void SimpleExtensionMethod()
        {
            var str = _simpleClass.ToAnyStr();
            Assert.AreEqual("AnyString", str);
        }

        [Test]
        public void SimpleReturnValueMethod()
        {
            var res = _simpleClass.SimpleReturnValueMethod();
            Assert.AreEqual(5, res);
        }

        [Test]
        [TestCase(1, 4, 5)]
        [TestCase(1, 5, 6)]
        [TestCase(33, 22, 55)]
        public void SimpleReturnSum(int a, int b, int c)
        {
            var res = _simpleClass.Sum(a, b);
            Assert.AreEqual(c, res);
        }

        [Test]
        [TestCase(1, 4, 5)]
        [TestCase(1, 5, 6)]
        [TestCase(33, 22, 55)]
        public async System.Threading.Tasks.Task SimpleAsyncReturnSum(int a, int b, int c)
        {
            var res = await _simpleClass.SimpleSumAsyncMethod(a, b, null);
            Assert.AreEqual(c, res);
        }

        [Test]
        public void SimpleReturnRefMethod()
        {
            var res = _simpleClass.SimpleReturnRefMethod();
            Assert.AreEqual(res, typeof(Internal.SimpleClass));
        }

        [Test]
        public void SimpleThrowsExceptionMethod()
        {
            var exp = Assert.Throws<Exception>(() => _simpleClass.ThrowsExceptionMethod());
            Assert.AreEqual("asd", exp.Message);
        }

        [Test]
        public void SimpleThrowsExceptionWithTryCatchBlock()
        {
            var a = _simpleClass.WithTryCatchBlockMethod();
            Assert.AreEqual(1, a);
        }

        [Test]
        public async System.Threading.Tasks.Task SimpleAsyncMethod()
        {
            await _simpleClass.SimpleAsyncMethod();
        }

        [Test]
        public async System.Threading.Tasks.Task SimpleAsyncRetValueTask()
        {
            var res = await _simpleClass.SimpleAsyncRetValueMethod();
            Assert.AreEqual(6, res);
        }

        [Test]
        public void SimpleAsyncThrowsTask()
        {
            Assert.ThrowsAsync<Exception>(() => _simpleClass.SimpleAsyncThrowsMethod());
        }

        [Test]
        public void SomeGenericMethod()
        {
            var a = _simpleClass.SomeGenericMethod<DateTime>();
            Assert.AreEqual(default(DateTime), a);
        }

        [Test]
        public void SomeInterfaceMethod()
        {
            var l = _simpleClass.SomeList();
        }

        [Test]
        public void ReturnSomeField()
        {
            var o = _simpleClass.ReturnSomeField();
        }

        [Test]
        public void ReturnSomeProperty()
        {
            var o = _simpleClass.AllowedDecompressionMethods;
        }
    }
}