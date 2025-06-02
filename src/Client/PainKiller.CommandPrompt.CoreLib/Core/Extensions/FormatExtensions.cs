using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using static System.DateTime;

namespace PainKiller.CommandPrompt.CoreLib.Core.Extensions;
public static class FormatExtensions
{
    public static string FormatFileTimestamp(this string prefix) => $"{prefix}{Now.Year}{Now.Month}{Now.Day}{Now.Hour}{Now.Minute}";
    public static string PrefixFileTimestamp(this string fileName) => $"{Now.Year}{Now.Month}{Now.Day}{Now.Hour}{Now.Minute}{fileName}";
    public static string GetDisplayFormattedFileSize(this long length)
    {
        double size;
        string unit;

        switch (length)
        {
            case < 1024: // Less than 1 KB
                size = length;
                unit = "bytes";
                return $"{size.ToString("N2", CultureInfo.CurrentCulture)} {unit}"; // No parentheses for bytes
            case < 1048576: // 1024 * 1024, less than 1 MB
                size = length / 1024.0;
                unit = "KB";
                break;
            case < 1073741824: // 1024 * 1024 * 1024, less than 1 GB
                size = length / 1048576.0; // 1024 * 1024
                unit = "MB";
                break;
            case < 1099511627776: // 1024 * 1024 * 1024 * 1024, less than 1 TB
                size = length / 1073741824.0; // 1024 * 1024 * 1024
                unit = "GB";
                break;
            default: // 1 TB or more
                size = length / 1099511627776.0; // 1024 * 1024 * 1024 * 1024
                unit = "TB";
                break;
        }
        // Round to two decimal points and format the result using the current culture
        var formattedSize = $"{Math.Round(size, 2).ToString("N2", CultureInfo.CurrentCulture)} {unit}";
        var formattedBytes = $"{length.ToString("N0", CultureInfo.CurrentCulture)} bytes";

        return $"{formattedSize} ({formattedBytes})";
    }
    public static string GetDisplayFormattedFileSize(this string fileName)
    {
        if (!File.Exists(fileName)) return $"{fileName} does not exist.";

        var fileInfo = new FileInfo(fileName);
        var length = fileInfo.Length;
        return length.GetDisplayFormattedFileSize();
    }

    public static long GetDirectorySize(this DirectoryInfo dir)
    {
        if (!dir.Exists) return 0;
        var files = dir.GetFiles("*", SearchOption.AllDirectories);
        return files.Sum(file => file.Length);
    }
    public static string GetDisplayTimeSinceLastUpdate(this DateTime lastUpdated)
    {
        var timeDifference = Now - lastUpdated;

        if (timeDifference.TotalSeconds < 60) return $"{(int)timeDifference.TotalSeconds} seconds ago";
        if (timeDifference.TotalMinutes < 60) return $"{(int)timeDifference.TotalMinutes} minutes ago";
        if (timeDifference.TotalHours < 24) return $"{(int)timeDifference.TotalHours} hours ago";
        if (timeDifference.TotalDays < 30) return $"{(int)timeDifference.TotalDays} days ago";
        if (timeDifference.TotalDays < 365)
        {
            var months = (int)(timeDifference.TotalDays / 30);
            return $"{months} months ago";
        }
        var years = (int)(timeDifference.TotalDays / 365);
        return $"{years} years ago";
    }
    public static bool IsValidFileName(this string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.Any(ch => invalidChars.Contains(ch))) return false;

        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return !reservedNames.Contains(nameWithoutExtension, StringComparer.OrdinalIgnoreCase);
    }

    private static readonly Dictionary<string, string> FileTypeDescriptions = new()
        {
            // Text files
            { ".txt", "Text file" },
            { ".log", "Log file" },
            { ".md", "Markdown file" },
            { ".rtf", "Rich Text Format file" },

            // Document files
            { ".doc", "Microsoft Word document" },
            { ".docx", "Microsoft Word document" },
            { ".pdf", "PDF document" },
            { ".odt", "OpenDocument text document" },
            { ".xls", "Microsoft Excel spreadsheet" },
            { ".xlsx", "Microsoft Excel spreadsheet" },
            { ".csv", "Comma-separated values file" },
            { ".ppt", "Microsoft PowerPoint presentation" },
            { ".pptx", "Microsoft PowerPoint presentation" },

            // Image files
            { ".jpg", "JPEG image" },
            { ".jpeg", "JPEG image" },
            { ".png", "PNG image" },
            { ".gif", "GIF image" },
            { ".bmp", "Bitmap image" },
            { ".tiff", "TIFF image" },
            { ".svg", "Scalable Vector Graphics" },
            { ".webp", "WebP image" },

            // Audio files
            { ".mp3", "Compressed sound file" },
            { ".wav", "Uncompressed sound file" },
            { ".flac", "Lossless audio file" },
            { ".aac", "Advanced Audio Coding file" },
            { ".ogg", "Ogg Vorbis audio file" },
            { ".m4a", "MPEG-4 audio file" },

            // Video files
            { ".mp4", "MPEG-4 video file" },
            { ".mkv", "Matroska video file" },
            { ".avi", "AVI video file" },
            { ".mov", "QuickTime video file" },
            { ".wmv", "Windows Media Video file" },
            { ".flv", "Flash video file" },
            { ".webm", "WebM video file" },

            // Archive files
            { ".zip", "Compressed archive file" },
            { ".rar", "Compressed archive file" },
            { ".7z", "7-Zip compressed file" },
            { ".tar", "Tape Archive file" },
            { ".gz", "Gzip compressed file" },
            { ".bz2", "Bzip2 compressed file" },

            // System files
            { ".exe", "Executable file" },
            { ".dll", "Dynamic link library" },
            { ".sys", "Windows system file" },
            { ".bat", "Batch file" },
            { ".cmd", "Command file" },
            { ".msi", "Windows installer package" },

            // Code and script files
            { ".cs", "C# source code file" },
            { ".java", "Java source code file" },
            { ".py", "Python script" },
            { ".js", "JavaScript file" },
            { ".html", "HTML file" },
            { ".css", "Cascading Style Sheets file" },
            { ".php", "PHP script" },
            { ".cpp", "C++ source code file" },
            { ".h", "C/C++ header file" },
            { ".sh", "Shell script" },
            { ".rb", "Ruby script" },
            { ".ts", "TypeScript file" },
            { ".pdb", "Program Database file" },

            // Database files
            { ".db", "Database file" },
            { ".sql", "SQL database file" },
            { ".data", "Object storage file" },
            { ".sqlite", "SQLite database file" },
            { ".mdb", "Microsoft Access database" },

            // Configuration files
            { ".json", "JSON configuration file" },
            { ".xml", "XML file" },
            { ".ini", "Initialization file" },
            { ".yml", "YAML configuration file" },
            { ".yaml", "YAML configuration file" },

            // Miscellaneous files
            { ".iso", "Disk image file" },
            { ".bin", "Binary file" },
            { ".cue", "Cue sheet file" },
            { ".torrent", "BitTorrent file" },
        };
    public static string GetFileTypeDescription(this FileInfo fileInfo)
    {
        if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
        var extension = fileInfo.Extension.ToLowerInvariant();
        return FileTypeDescriptions.GetValueOrDefault(extension, "Unknown file type");
    }
    private static readonly Dictionary<string, string> PlainTextFileContent = new()
        {
            // Text files
            { ".txt", "Text file" },
            { ".log", "Log file" },
            { ".md", "Markdown file" },
            // Code and script files
            { ".cs", "C# source code file" },
            { ".java", "Java source code file" },
            { ".py", "Python script" },
            { ".js", "JavaScript file" },
            { ".html", "HTML file" },
            { ".css", "Cascading Style Sheets file" },
            { ".php", "PHP script" },
            { ".cpp", "C++ source code file" },
            { ".h", "C/C++ header file" },
            { ".sh", "Shell script" },
            { ".rb", "Ruby script" },
            { ".ts", "TypeScript file" },
            // Configuration files
            { ".json", "JSON configuration file" },
            { ".xml", "XML file" },
            { ".ini", "Initialization file" },
            { ".yml", "YAML configuration file" },
            { ".yaml", "YAML configuration file" }
        };
    public static bool IsPlainTextFileContent(this FileInfo fileInfo)
    {
        if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
        var extension = fileInfo.Extension.ToLowerInvariant();
        return PlainTextFileContent.ContainsKey(extension);
    }
    public static string GetCompressedPath(this string path, int maxLength)
    {
        if (path.Length <= maxLength)
            return path;

        var ellipsis = "...";
        var keepLength = (maxLength - ellipsis.Length) / 2;
        var start = path.Substring(0, keepLength);
        var end = path.Substring(path.Length - keepLength);

        return $"{start}{ellipsis}{end}";
    }
    public static string Truncate(this string input, int maxLength, string ellipse = "...")
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength - ellipse.Length) + ellipse;
    }
    public static Guid GenerateGuidFromString(this string input)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    public const string UserNamePlaceholder = "$USERNAME$";
    public const string RoamingDirectoryPlaceholder = "$ROAMING$";
    public static string GetReplacedPlaceHolderPath(this string uriWithPlaceHolder) => uriWithPlaceHolder.Replace(RoamingDirectoryPlaceholder, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{nameof(CommandPrompt)}").Replace(UserNamePlaceholder, Environment.UserName, StringComparison.CurrentCultureIgnoreCase);
}