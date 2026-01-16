namespace MarikinAlert.Web.Services
{
    using MarikinAlert.Web.Models;

    /// <summary>
    /// Interface for scanning text and extracting disaster-related information
    /// Follows SOLID - Single Responsibility: Only handles text analysis
    /// </summary>
    public interface ITextScanner
    {
        /// <summary>
        /// Scans Taglish text for disaster-related keywords
        /// </summary>
        /// <param name="rawMessage">The citizen's message in Taglish</param>
        /// <returns>Detected category based on keywords</returns>
        ReportCategory ScanForCategory(string rawMessage);

        /// <summary>
        /// Determines priority level based on keyword urgency
        /// </summary>
        /// <param name="rawMessage">The citizen's message in Taglish</param>
        /// <param name="category">The detected category</param>
        /// <returns>Priority level (Critical, High, Medium, Low)</returns>
        ReportPriority DeterminePriority(string rawMessage, ReportCategory category);
    }
}