using System.Text;
using System.Linq;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using ExifLibrary;
using CommandLine;

namespace GooglePhotoExifFix
{
    public class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path to root folder")]
        public string Path { get; set; }

        [Option('f', "force", Required = false, HelpText = "Force overwrite exif date")]
        public bool Force { get; set; }

        [Option('d', "filedate", Required = false, HelpText = "Modify file last write and creation time")]
        public bool WriteFileDate { get; set; }

        [Option('e', "extensions",Separator = ',', Required = false, HelpText = "Image file extensions you want to edit such as jpg, use',' to seperate multiple extensions")]
        public IEnumerable<string> Extensions { get; set; }

        [Option('r', "remove", Required = false, HelpText = "Set which file extensions to remove when found duplicate file name in same folder")]
        public string RemoveExtension { get; set; }

        [Option('v', "verbose", Required = false, HelpText="Show verbose information")]
        public bool Verbose { get; set; }
    }

    class Program
    {
        public static Options Option {get;set;}

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o=>Option=o)
                .WithNotParsed(o=>Environment.Exit(-1));    

            List<string> ImageExtensions = Option.Extensions.ToList();

            var RemoveList = new List<string>();
            var UpdatedList = new List<string>();
            var IgnoredList = new List<string>();
            var ErrorList = new List<string>();
            var MetadataNotFoundList = new List<string>();

            //Remove duplicate
            if (string.IsNullOrEmpty(Option.RemoveExtension)==false)
            {
                RemoveList = DuplicateRemover.Remove(Option.Path,Option.RemoveExtension);
            }

            //Write exif
            var FilePaths = Directory.GetFiles(Option.Path, "*", SearchOption.AllDirectories)
                .Where(x=>x.EndsWith(".json")==false);

            foreach (var filePath in FilePaths)
            {
                JsonMetadata metadata = JsonMetadata.Search(filePath);

                if (metadata!=null)
                {
                    try
                    {
                        bool Updated = false;                        

                        if(Option.WriteFileDate)
                        {
                            File.SetCreationTime(filePath,metadata.photoTakenTime.DateTime);
                            File.SetLastWriteTime(filePath,metadata.photoTakenTime.DateTime);
                            Updated = true;
                        }                        
                        
                        if (IsImage(filePath,ImageExtensions))
                        {
                            var imgFile = ImageFile.FromFile(filePath);
                            var original_dt = imgFile.Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);

                            if (Option.Force || original_dt == null || original_dt.Value==DateTime.MinValue)
                            {
                                imgFile.Properties.Set(
                                    ExifTag.DateTimeOriginal,
                                    new ExifDateTime(ExifTag.DateTimeOriginal,metadata.photoTakenTime.DateTime)                
                                );

                                imgFile.Save(filePath);

                                var new_dt = ImageFile.FromFile(filePath).Properties.Get<ExifDateTime>(ExifTag.DateTimeOriginal);
                                if (new_dt== null || new_dt.Value==DateTime.MinValue)
                                    throw new Exception("exif write fail");

                                Updated = true;
                            }
                        }

                        if(Updated)
                        {
                            if(Option.Verbose)
                                Console.WriteLine($"Updated:{filePath}");
                            UpdatedList.Add(filePath);
                        }
                        else
                        {
                            if(Option.Verbose)
                                Console.WriteLine($"Ignored:{filePath}");
                            IgnoredList.Add(filePath);
                        }                        
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"File:{filePath},Ex:{ex.Message}");
                        ErrorList.Add(filePath);
                    }
                }
                else
                {
                    if(Option.Verbose)
                        Console.WriteLine($"File:{filePath},medatada not found");
                    MetadataNotFoundList.Add(filePath);                    
                }
                
                WriteOnBottomLine(
                    $@"Removed:{RemoveList.Count.ToString()} "+
                    $@"Updated:{UpdatedList.Count.ToString()} "+
                    $@"Ignored:{IgnoredList.Count.ToString()} "+
                    $@"NotFound:{MetadataNotFoundList.Count.ToString()} "+
                    $@"Error:{ErrorList.Count.ToString()} "
                    );
            }

            Console.WriteLine();
            Console.WriteLine($"All done.");
            Console.WriteLine();            
            Console.WriteLine($"===========================");
            Console.WriteLine($"Error files:{ErrorList.Count}");
            ErrorList.ForEach(x=>Console.WriteLine(x));                
            Console.WriteLine();
            Console.WriteLine($"===========================");
            Console.WriteLine($"Metadata not found:{MetadataNotFoundList.Count}");
            MetadataNotFoundList.ForEach(x=>Console.WriteLine(x));
            Console.WriteLine();
            Console.WriteLine($"===========================");
            Console.WriteLine($"Removed files:{RemoveList.Count}");
            RemoveList.ForEach(x=>Console.WriteLine(x));                
            Console.WriteLine();
        }

        static bool IsImage(string path,List<string> ImageExtensions)
        {
            var file_extension = Path.GetExtension(path);
            return ImageExtensions.Any(ext=>file_extension.Contains(ext,StringComparison.CurrentCultureIgnoreCase));
        }

        static void WriteOnBottomLine(string text)
        {
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.Write(text);
            Console.SetCursorPosition(x, y);
        }

    }
}
