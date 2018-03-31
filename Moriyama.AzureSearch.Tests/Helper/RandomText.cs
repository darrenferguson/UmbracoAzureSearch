using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moriyama.AzureSearch.Tests.Helper
{
    public class RandomText
    {
        static Random _random = new Random();
        StringBuilder _builder;
        string[] _words;



        public RandomText(string[] words)
        {
            _builder = new StringBuilder();
            _words = words;
        }

        public DateTime RandomDateTime()
        {
            DateTime d = DateTime.Now.Subtract(TimeSpan.FromDays(RandomInt(0,1000)));
            return d;

        }

        public string RandomUrl()
        {
            List <string> items = new List<string>();

            for (int i = 0; i <= RandomInt(1, 6); i++)
            {
                string segment = _words[RandomInt(0, _words.Length)];
                items.Add(segment);
            }

            return "/" + string.Join("/", items);
        }

        public int[] RangeList(int from, int to)
        {

            IList<int> items = new List<int>();

            for(int i = from; i<=to; i++)
            {
                items.Add(i);
            }

            return items.ToArray();
        }

        public string RandomFrom(string[] items)
        {
            int index = RandomInt(0, items.Length);
            return items[index];
        }

        public string RandomintList(int y, int min, int max)
        {
            List<string> items = new List<string>();

            for (int a = 0; a <= RandomInt(1, y); a++)
            {
                items.Add(RandomInt(min, max).ToString());
            }

            return string.Join(",", items);
        }
        
        public int RandomInt(int x, int y)
        {
            Random random = new Random();
            return random.Next(x, y);
        }

        public void Reset()
        {
            _builder = new StringBuilder();
        }

        public void AddContentParagraphs(int numberParagraphs, int minSentences,

            int maxSentences, int minWords, int maxWords)
        {

            for (int i = 0; i < numberParagraphs; i++)
            {
                AddParagraph(_random.Next(minSentences, maxSentences + 1),
                    minWords, maxWords);
                _builder.Append("\n\n");
            }
        }

        void AddParagraph(int numberSentences, int minWords, int maxWords)
        {
            for (int i = 0; i < numberSentences; i++)
            {
                int count = _random.Next(minWords, maxWords + 1);
                AddSentence(count);
            }
        }

        void AddSentence(int numberWords)
        {
            StringBuilder b = new StringBuilder();
            // Add n words together.
            for (int i = 0; i < numberWords; i++) // Number of words
            {
                b.Append(_words[_random.Next(_words.Length)]).Append(" ");
            }
            string sentence = b.ToString().Trim() + ". ";
            // Uppercase sentence
            sentence = char.ToUpper(sentence[0]) + sentence.Substring(1);
            // Add this sentence to the class
            _builder.Append(sentence);
        }

        public string Content
        {
            get
            {
                return _builder.ToString();
            }
        }
    }
}
