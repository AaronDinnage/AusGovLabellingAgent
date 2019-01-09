using AusGovLabellingAgent.Data;
using System;
using System.IO;
using System.Reflection;

namespace AusGovLabellingAgent
{
    public class Logging
    {
        public const int MaxLogSizeInMb = 100;

        public static readonly string LogDelimiter = new string('=', 78);

        public const string LogFilename = "AusGovLabellingAgent.log";

        public const string LogTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff";
        public const int LogTimeFormatPad = 30;

        public const double BytesPerMegabyte = 1024 * 1024;


        public static Logging Log { get; private set; } = new Logging();

        public LogLevel Level { get; set; } = LogLevel.All;

        public string LogFile { get; set; }

        public Logging()
        {
            try
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string logDir = Path.GetDirectoryName(assemblyLocation);

                LogFile = Path.Combine(logDir, LogFilename);
                System.Diagnostics.Debug.WriteLine("Logging.ctor(): LogFile=" + LogFile);

                var logInfo = new FileInfo(LogFile);

                if (logInfo.Exists && logInfo.Length / BytesPerMegabyte > MaxLogSizeInMb)
                {
                    logInfo.Delete();
                }

                using (var writer = File.AppendText(LogFile))
                {
                    writer.WriteLine();
                    writer.WriteLine(LogDelimiter);
                    writer.WriteLine("New instance: " + DateTime.Now.ToString(LogTimeFormat));
                    writer.WriteLine(LogDelimiter);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logging.ctor() - Error: " + ex.ToString());
            }
        }

        private void WriteLine(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);

            string time = DateTime.Now.ToString(LogTimeFormat).PadRight(LogTimeFormatPad);

            try
            {
                using (var writer = File.AppendText(LogFile))
                {
                    writer.Write(time);
                    writer.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error writing to log file: " + ex.ToString());
            }
        }

        public void Error(string message, params object[] args)
        {
            if (Level > LogLevel.Error)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            string text = "Error: " + string.Format(message, args);
            WriteLine(text);
        }
        public void Warning(string message, params object[] args)
        {
            if (Level > LogLevel.Warning)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            string text = "Warning: " + string.Format(message, args);
            WriteLine(text);
        }
        public void Information(string message, params object[] args)
        {
            if (Level > LogLevel.Information)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            string text = "Information: " + string.Format(message, args);
            WriteLine(text);
        }

        public void Debug(string message, params object[] args)
        {
            if (Level > LogLevel.Debug)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            string text = "Debug: " + string.Format(message, args);
            WriteLine(text);
        }
    }
}
