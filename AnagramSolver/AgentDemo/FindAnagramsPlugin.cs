using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SemanticKernel;

namespace AgentDemo
{
    public class FindAnagramsPlugin
    {
        [KernelFunction("FindAnagrams")]
        [Description("Finds anagrams for a given word.")]
        public string FindAnagrams(string word)
        {
            var filePath = "zodziai.txt";
            if (!File.Exists(filePath))
            {
                return "No anagrams found. (Failed to read data file)";
            }

            var words = File.ReadAllLines(filePath);
            var sortedWord = string.Concat(word.OrderBy(c => char.ToLowerInvariant(c)));
            var anagrams = new List<string>();

            foreach (var w in words)
            {
                var cleanWord = w.Trim();
                if (cleanWord.Length == word.Length && !string.Equals(cleanWord, word, StringComparison.OrdinalIgnoreCase))
                {
                    var sortedW = string.Concat(cleanWord.OrderBy(c => char.ToLowerInvariant(c)));
                    if (sortedWord == sortedW)
                    {
                        anagrams.Add(cleanWord);
                    }
                }
            }   

            return anagrams.Count > 0 ? string.Join(", ", anagrams) : "No anagrams found.";
        }

        [KernelFunction]
        [Description("Find how many anagrams there are for a given word")]
        public int CountMatches(string word)
        {
            int count = 0;
            var anagrams = FindAnagrams(word);
            if (anagrams != "No anagrams found.")
            {
                count = anagrams.Split(',').Length;
            }

            return count;
        }
    }
}
