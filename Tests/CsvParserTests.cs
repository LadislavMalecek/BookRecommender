using Xunit;
using BookRecommender.DataManipulation;
using Microsoft.DotNet.InternalAbstractions;


namespace BookRecommender.Tests
{

    public class CsvParserTests
    {

        [Fact]
        public void Empty4()
        {
            var data = ",,,\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            int count = 0;
            foreach (var item in result[0])
            {
                count++;
                Assert.Equal(item, string.Empty);
            }
            Assert.True(count == 4);
        }


        [Fact]
        public void Empty()
        {
            var data = "\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
        }

        [Fact]
        public void Empty2()
        {
            var data = "\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
        }
        [Fact]
        public void Empty3()
        {
            var data = "";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 0);
        }


        [Fact]
        public void MixedSimple()
        {
            var data = "asdf,,qwf,1\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            Assert.True(result[0].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[0][2], "qwf");
            Assert.Equal(result[0][3], "1");
        }
        [Fact]
        public void DoubleQuotes()
        {
            var data = "asdf,,\"asdfwwf\"\"\",1\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            Assert.True(result[0].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[0][2], "asdfwwf\"");
            Assert.Equal(result[0][3], "1");
        }
        [Fact]
        public void EolInValue()
        {
            var data = "asdf,,\"\r\n\",1\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            Assert.True(result[0].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[0][2], "\r\n");
            Assert.Equal(result[0][3], "1");
        }
        [Fact]
        public void CommaInValue()
        {
            var data = "asdf,,\",,,\",1\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            Assert.True(result[0].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[0][2], ",,,");
            Assert.Equal(result[0][3], "1");
        }

        [Fact]
        public void EmptyOnEndOfLineValue()
        {
            var data = "asdf,,\"\",\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 1);
            Assert.True(result[0].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[0][2], "");
            Assert.Equal(result[0][3], "");
        }

        [Fact]
        public void SimpleTwoLines()
        {
            var data = "asdf,asdf\r\nasd,asd,qqq,asd\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 2);
            Assert.True(result[0].Count == 2);
            Assert.True(result[1].Count == 4);
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "asdf");

            Assert.Equal(result[1][0], "asd");
            Assert.Equal(result[1][1], "asd");
            Assert.Equal(result[1][2], "qqq");
            Assert.Equal(result[1][3], "asd");
        }
        [Fact]
        public void EmptyThreeLines()
        {
            var data = "\r\n\r\n\r\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 3);
            Assert.True(result[0].Count == 1);
            Assert.True(result[1].Count == 1);
            Assert.True(result[2].Count == 1);
            
            Assert.Equal(result[0][0], "");
            Assert.Equal(result[1][0], "");
            Assert.Equal(result[2][0], "");
        }
        [Fact]
        public void EmptyBeforeSecond()
        {
            var data = "asdf,\r\naaa\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 2);
            Assert.True(result[0].Count == 2);
            Assert.True(result[1].Count == 1);
            
            Assert.Equal(result[0][0], "asdf");
            Assert.Equal(result[0][1], "");
            Assert.Equal(result[1][0], "aaa");
        }
        [Fact]
        public void Quotes()
        {           //   """","\n\r"\n\r"aa,a"
            var data = "\"\"\"\",\"\n\r\"\r\n\"aa,a\"\n";
            var result = new CsvParser(data).Parse();
            Assert.True(result.Count == 2);
            Assert.True(result[0].Count == 2);
            Assert.True(result[1].Count == 1);
            
            Assert.Equal(result[0][0], "\"");
            Assert.Equal(result[0][1], "\n\r");
            Assert.Equal(result[1][0], "aa,a");
        }
    }
}