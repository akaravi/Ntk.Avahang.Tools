using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;


namespace Ntk.ToolsProject.Windows.AvahangHelper.Poco
{
    internal  class CommandHelper
    {
        public string Error {  get; set; }
        public int ExitCode {  get; set; }
        public  string ExecuteCommand(string command,string WorkingDirectory="", int timeOutMin = 0)
        {
            Error = "";
            string retOut = "";
            var lastIndex = command.LastIndexOf("\\");
            if (lastIndex < command.LastIndexOf("/"))
                lastIndex = command.LastIndexOf("/");
            if (string.IsNullOrEmpty(WorkingDirectory))
            {
                WorkingDirectory = command;
                if (lastIndex > 0)
                    WorkingDirectory = command.Substring(0, lastIndex);
            }

            try
            {
                //#help# سر جدت دست به ین تظیمات نزن خیلی با دردسر تنظیم شده
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WorkingDirectory = WorkingDirectory,
                    FileName = "cmd.exe",
                    Arguments = "/c " + "\"" + command + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,//false
                    RedirectStandardError = true,//true
                    UseShellExecute = false,//false
                    CreateNoWindow = true,//true
                };
                var process = System.Diagnostics.Process.Start(processInfo);

                if (timeOutMin == 0)
                    process.WaitForExit();
                else
                    process.WaitForExit(timeOutMin * 60 * 1000);

                ExitCode = process.ExitCode;

                //string output = process.StandardOutput.ReadToEnd();
                retOut = process.StandardError.ReadToEnd();
                
             
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
            return retOut;
        }
    }
}
