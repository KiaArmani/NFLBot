namespace XurClassLibrary.Models
{
    public enum LogSeverity
    {
        /// <summary>
        ///     Logs that contain the most severe level of error. This type of error indicate that immediate attention
        ///     may be required.
        /// </summary>
        Critical,

        /// <summary>
        ///     Logs that highlight when the flow of execution is stopped due to a failure.
        /// </summary>
        Error,

        /// <summary>
        ///     Logs that highlight an abnormal activity in the flow of execution.
        /// </summary>
        Warning,

        /// <summary>
        ///     Logs that track the general flow of the application.
        /// </summary>
        Info,

        /// <summary>
        ///     Logs that are used for interactive investigation during development.
        /// </summary>
        Verbose,

        /// <summary>Logs that contain the most detailed messages.</summary>
        Debug
    }
}