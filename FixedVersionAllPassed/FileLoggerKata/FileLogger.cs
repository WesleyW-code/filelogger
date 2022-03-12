using System;

namespace FileLoggerKata
{
    public class FileLogger
    {
        private const string LogExtension = "txt";
        private const string LogFileName = "log";
        private const string WeekendLogFileName = "weekend";

        private IDateProvider DateProvider { get; }
        private IFileSystem FileSystem { get; }

        public FileLogger(IFileSystem fileSystem, IDateProvider dateProvider = null)
        {
            DateProvider = dateProvider ?? DefaultDataProvider.Instance;
            FileSystem = fileSystem;
        }

        public void Log(string message)
        {
            var logFileName = GetLogFileName();

            if (ShouldRotateWeekendLogs())
            {
                ArchiveWeekendLog();
            }

            if (!FileSystem.Exists(logFileName))
            {
                FileSystem.Create(logFileName);
            }

            FileSystem.Append(logFileName, message);

            bool ShouldRotateWeekendLogs()
            {
                return IsWeekend(DateProvider.Today) &&
                       FileSystem.Exists(logFileName) &&
                       (DateProvider.Today - FileSystem.GetLastWriteTime(logFileName)).Days > 2;
            }
        }

        private string GetLogFileName()
        {
            var today = DateProvider.Today;

            return IsWeekend(today)
                ? $"{WeekendLogFileName}.{LogExtension}"
                : $"{LogFileName}{ToFileDateFormat(today)}.{LogExtension}";            
        }

        private void ArchiveWeekendLog()
        {
            var WeekendDate = FileSystem.GetLastWriteTime($"{WeekendLogFileName}.{LogExtension}");
            
            // WESLEY WEBER FIX TO FileLogger ----------------------------------------------------------------------------------------------------------------------
            // I discovered that when there is an old weekend.txt file with the last write date being on a Sunday. The code didnt append the Saturdays Date of that weekend as the Weekend.TXT File name.
            // I added this if statement to make sure that the correct filename will be made for old Weekend.txt files.
            if (WeekendDate.DayOfWeek == DayOfWeek.Sunday)
            {
                WeekendDate = WeekendDate.AddDays(-1);
            }
            //-------------------------------------------------------------------------------------------------------------------------------------------------------
            var archivedFileName = $"{WeekendLogFileName}-{ToFileDateFormat(WeekendDate)}.{LogExtension}";
            FileSystem.Rename($"{WeekendLogFileName}.{LogExtension}", archivedFileName);
        }

        private static string ToFileDateFormat(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        private static bool IsWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
}
