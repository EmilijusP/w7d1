using AnagramSolver.Contracts.Interfaces;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnagramSolver.BusinessLogic.Plugins
{
    public class AnagramPlugin
    {
        private readonly IAnagramSolver _anagramSolverService;

        public AnagramPlugin(IAnagramSolver anagramSolverService)
        {
            _anagramSolverService = anagramSolverService;
        }

        [KernelFunction]
        [Description("Finds anagrams for a given word or words combinations.")]
        public async Task<string> FindAnagrams([Description("Word or word combination for which to find anagrams for.")] string words, CancellationToken ct)
        {
            var anagrams = await _anagramSolverService.GetAnagramsAsync(words, ct);

            return anagrams != null && anagrams.Any() ? string.Join(", ", anagrams) : "No anagrams found.";
        }
    }
}
