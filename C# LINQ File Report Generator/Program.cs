// Program.cs
//
// CECS 342 Assignment 3
// File Type Report
// Solution Template

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace FileTypeReport
{
    internal static class Program
    {
        // 1. Enumerate all files in a folder recursively
        private static IEnumerable<string> EnumerateFilesRecursively(string path)
        {
            // Use Directory.EnumerateFiles with SearchOption.AllDirectories to get all files recursively
            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
        }

        // Human readable byte size
        private static string FormatByteSize(long byteSize)
        {
            // Define units and their corresponding multipliers
            string[] units = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB" };
            double size = byteSize;
            int unitIndex = 0;

            // Convert to appropriate unit
            while (size >= 1000 && unitIndex < units.Length - 1)
            {
                size /= 1000;
                unitIndex++;
            }

            // Round to 2 decimal places and format
            return $"{size:F2}{units[unitIndex]}";
        }

        // Create an HTML report file
        private static XDocument CreateReport(IEnumerable<string> files)
        {
            // 2. Process data
            var query =
              from file in files
              let extension = Path.GetExtension(file).ToLowerInvariant().TrimStart('.')
              group file by string.IsNullOrEmpty(extension) ? "(no extension)" : extension into fileGroup
              let totalSize = fileGroup.Sum(f => new FileInfo(f).Length)
              orderby totalSize descending
              select new
              {
                  Type = fileGroup.Key, // file extension
                  Count = fileGroup.Count(),
                  TotalSize = totalSize
              };

            // 3. Functionally construct XML
            var alignment = new XAttribute("align", "right");
            var style = "table, th, td { border: 1px solid black; border-collapse: collapse; padding: 5px; }";

            var tableRows = query.Select(item =>
              new XElement("tr",
                new XElement("td", item.Type),
                new XElement("td", new XAttribute("align", "right"), item.Count.ToString()),
                new XElement("td", new XAttribute("align", "right"), FormatByteSize(item.TotalSize))
              )
            );

            var table = new XElement("table",
              new XElement("thead",
                new XElement("tr",
                  new XElement("th", "Type"),
                  new XElement("th", "Count"),
                  new XElement("th", "Total Size"))),
              new XElement("tbody", tableRows));

            return new XDocument(
              new XDocumentType("html", null, null, null),
                new XElement("html",
                  new XElement("head",
                    new XElement("title", "File Report"),
                    new XElement("style", style)),
                  new XElement("body", table)));
        }

        // Console application with two arguments
        public static void Main(string[] args)
        {
            try
            {
                // Ensure two arguments are provided
                if (args.Length != 2)
                {
                    throw new ArgumentException("Incorrect number of arguments");
                }

                string inputFolder = args[0];
                string reportFile = args[1];

                // Validate input folder exists
                if (!Directory.Exists(inputFolder))
                {
                    throw new DirectoryNotFoundException($"Input folder not found: {inputFolder}");
                }

                // Ensure output directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(reportFile));

                // Create and save report
                CreateReport(EnumerateFilesRecursively(inputFolder)).Save(reportFile);

                Console.WriteLine($"Report generated successfully at {reportFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Usage: FileTypeReport <folder> <report file>");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}