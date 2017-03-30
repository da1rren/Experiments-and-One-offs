using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;

namespace Database_Login_Logger
{
    public static class PreformContextExtensions
    {
        public static void Write(this PerformContext context, string text, ConsoleTextColor color)
        {
            context.SetTextColor(color);
            context.WriteLine(text);
            context.ResetTextColor();
        }
    }
}
