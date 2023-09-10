using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSoftware
{
    public static class Commands
    {
        public static void RunCommand(string command, string input = "")
        {
            switch (command)
            {
                case "exit":
                    break;
                default:
                    Program.ServerClass.UpdateWindow();
                    break;
            }
        }
    }
}
