using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFrameworkDemo
{
    public class AnagramTools
    {
        private readonly string[] _wordRepository;

        public AnagramTools()
        {
            _wordRepository = File.Exists("zodziai.txt") ? File.ReadAllLines("zodziai.txt") : throw new Exception("Failed to read file.");
        }

        [Description("Randa anagramas nurodytam žodžiui.")]
        public string FindAnagrams([Description("Žodis, kurio anagramas ieškoti.")]string word)
        {
            var sortedWord = string.Concat(word.OrderBy(c => char.ToLowerInvariant(c)));
            var anagrams = _wordRepository.Where(w => w.Length == word.Length &&
                                             !string.Equals(w, word, StringComparison.OrdinalIgnoreCase) &&
                                             string.Concat(w.OrderBy(c => char.ToLowerInvariant(c))) == sortedWord)
                                 .ToList();

            return anagrams.Count > 0 ? string.Join(", ", anagrams) : "Anagramų nerasta.";
        }

        [Description("Grąžina bendrą žodžių skaičių duomenų bazėje.")]
        public int GetWordCount()
        {
            return _wordRepository.Length;
        }

        [Description("Filtruoja ir grąžina žodžius pagal nurodytą ilgį (iki 20 pavyzdžių).")]
        public string FilterByLength(int length)
        {
            var filtered = _wordRepository.Where(w => w.Length == length).Take(20);
            return filtered.Any() ? string.Join(", ", filtered) : "Tokio ilgio žodžių nerasta.";
        }
    }
}
