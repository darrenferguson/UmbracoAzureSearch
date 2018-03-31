using System;
using System.IO;
using System.Reflection;
using Moriyama.AzureSearch.Tests.Helper;
using NUnit.Framework;

namespace Moriyama.AzureSearch.Tests.Unit.Random
{
    [TestFixture]
    class RandomTextTests
    {
        [Test]
        public void TestRandomSentence()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string wordsFile = Path.Combine(path, "words.txt");
            var lines = File.ReadAllLines(wordsFile);

            RandomText random = new RandomText(lines);

            random.AddContentParagraphs(2, 2, 4, 5, 12);
            string content = random.Content;

            Console.WriteLine(content);
        }
    }
}
