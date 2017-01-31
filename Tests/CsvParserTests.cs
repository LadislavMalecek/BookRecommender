using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookRecommender.DataManipulation;

namespace BookRecommender.Tests
{

    [TestClass]
    public class TestClass
    {

        [TestMethod]
        public void Empty4()
        {
            var data = ",,,\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            int count = 0;
            foreach(var item in result[0]){
                count++;
                Assert.AreEqual(item, string.Empty);
            }
            Assert.IsTrue(count == 4);
        }


        [TestMethod]
        public void Empty()
        {
            var data = "\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 0);
            int count = 0;
        }

        [TestMethod]
        public void MixedSimple()
        {
            var data = "asdf,,qwf,1\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],"qwf");
            Assert.AreEqual(result[0][3],"1");
        }
        [TestMethod]
        public void DoubleQuotes()
        {
            var data = "asdf,,\"asdfwwf\"\"\",1\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],"asdfwwf\"");
            Assert.AreEqual(result[0][3],"1");
        }
        [TestMethod]
        public void EolInValue()
        {
            var data = "asdf,,\"\r\n\",1\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],"\r\n");
            Assert.AreEqual(result[0][3],"1");
        }
        [TestMethod]
        public void CommaInValue()
        {
            var data = "asdf,,\",,,\",1\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],",,,");
            Assert.AreEqual(result[0][3],"1");
        }

        [TestMethod]
        public void EmptyOnEndOfLineValue()
        {
            var data = "asdf,,\"\",\n\r";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],"");
            Assert.AreEqual(result[0][3],"");
        }

        
        public void SimpleTwoLines(){
            var data = "asdf,asdf\n\rasd,asd,qqq,asd";
            var result = new CsvParser(data).Parse();
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].Count == 4);
            Assert.IsTrue(result[1].Count == 4);
            Assert.AreEqual(result[0][0],"asdf");
            Assert.AreEqual(result[0][1],"");
            Assert.AreEqual(result[0][2],",,,");
            Assert.AreEqual(result[0][3],"1");

            Assert.AreEqual(result[1][0],"rasd");
            Assert.AreEqual(result[1][1],"asd");
            Assert.AreEqual(result[1][2],"qqq");
            Assert.AreEqual(result[1][3],"asd");
        }
    }
}