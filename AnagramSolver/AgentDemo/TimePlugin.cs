using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AgentDemo
{
    public class TimePlugin
    {
        [KernelFunction]
        [Description("Gets the current date and time in the format 'yyyy-MM-dd HH:mm:ss'.")]
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
