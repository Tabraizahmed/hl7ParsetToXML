using System;
using System.Threading.Tasks;
using HL7Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HL7ParserUnitTest
{
    [TestClass]
    public class TestParse
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            Parser parser=new Parser();
            var result= await parser.GetXmlFileContentToWrite(@"D:\MyApps\HL7Parset\HL7 Files");

        }
    }
}
