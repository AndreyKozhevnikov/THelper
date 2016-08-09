using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Language.Flow;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

namespace THelper {
    [TestFixture]
    public class Test_TEst {
        [Test]
        public void TestClass() {
            //arrange
            TestClass t = new TestClass();
            var moq = new Mock<ITestInterFace>();
            t.MyProcessor = moq.Object;

            int callBackCounter = 0;
            int orderCounter = 0;
            Dictionary<string, int> callOrderDictionary = new Dictionary<string, int>();
            moq.Setup(x => x.Test1()).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["Test1"])));
            moq.Setup(x => x.Test2()).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["Test2"])));
            moq.Setup(x => x.Test3()).Do((x3) => { callOrderDictionary[ReturnNameDelete(x3)] = orderCounter++; }).Callback(() => Assert.That(callBackCounter++, Is.EqualTo(callOrderDictionary["Test3"])));


            t.MyMethod();

            Assert.AreEqual(orderCounter, callBackCounter);
        }

        private void MyCallBack(object o,[CallerMemberName]string st2="") {
            var st = o as ITestInterFace;
       
            throw new NotImplementedException();
        }
        private void MyCallBack2(object o, [CallerMemberName]string st2 = "") {
            var st = o as ITestInterFace;

            throw new NotImplementedException();
        }

        private string ReturnNameDelete(object x3) {
            var st = x3.GetType();
            var st2 = x3.ToString();
            var ind = st2.IndexOf("=> x.") + 5;
            var st3 = st2.Substring(ind, st2.Length - ind);
            var ind2 = st3.IndexOf("(");
            var st4 = st3.Substring(0, ind2);
            // var st5 = (x3 as Moq.IProxyCall);
            return st4;
            return "null";
        }
    }
    public static class MyExtensions {
        public static TI Do<TI>(this TI input, Action<TI> action) where TI : class {
            if (input == null)
                return null;
            action(input);
            return input;
        }
    }
    public interface ITestInterFace {
        void Test1();
        void Test3();
        void Test2();
    }

    public class TestClass {
        public ITestInterFace MyProcessor;
        public void MyMethod() {

            MyProcessor.Test1();
            MyProcessor.Test2();
        
            MyProcessor.Test3();


        }
    }

}
