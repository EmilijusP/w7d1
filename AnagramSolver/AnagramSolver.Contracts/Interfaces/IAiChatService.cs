using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnagramSolver.Contracts.Interfaces
{
    public interface IAiChatService
    {
        Task<string> GetResponseAsync(string sessionId, string prompt);
    }
}
