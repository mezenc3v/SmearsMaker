using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmearTracer.AL.Tests
{
    [TestClass]
    public class SmearsTests
    {
        [TestMethod]
        public void ShouldCreateSmear()
        {
            //arrange
            ISmears smears = new SmearsGrayScale();
            //act
            smears.Compute();
            var testListSmears = smears.Smears();
            //asserts
            Assert.IsTrue(testListSmears.Count > 0);
            foreach (var testSmear in testListSmears)
            {
                Assert.IsNotNull(testSmear);
            }
        }
    }
}
