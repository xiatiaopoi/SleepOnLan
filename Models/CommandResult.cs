namespace SleepOnLan.Models
{
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Response { get; set; } = "";
        public string LogMessage { get; set; } = "";

        public static CommandResult Ok(string response, string logMessage = "")
        {
            return new CommandResult { Success = true, Response = response, LogMessage = logMessage };
        }

        public static CommandResult Error(string response, string logMessage = "")
        {
            return new CommandResult { Success = false, Response = response, LogMessage = logMessage };
        }
    }
}
