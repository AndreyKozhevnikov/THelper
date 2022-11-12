using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THelper.Tests {
    [TestFixture]
    public class DataBaseCreatorTests {
        [Test]
        public void GetPrefix_0() {
            //assert
            var c = 1;
            //act
            var res = DataBaseCreatorLib.DataBaseCreator.GetPrefix(c);
            //arrange
            Assert.AreEqual( "001",res);
        }
        [Test]
        public void GetPrefix_1() {
            //assert
            var c = 22;
            //act
            var res = DataBaseCreatorLib.DataBaseCreator.GetPrefix(c);
            //arrange
            Assert.AreEqual("022", res);
        }
        [Test]
        public void GetPrefix_2() {
            //assert
            var c = 321;
            //act
            var res = DataBaseCreatorLib.DataBaseCreator.GetPrefix(c);
            //arrange
            Assert.AreEqual("321", res);
        }
    }
}
