using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GooglePhotoExifFix
{
    public class CreationTime    {
        public string timestamp { get; set; } 
        public string formatted { get; set; } 
    }

    public class PhotoTakenTime    {
        public string timestamp { get; set; } 
        public string formatted { get; set; } 
        
        public DateTime DateTime {
            get =>
            DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(timestamp)).DateTime;
        }
    }

    public class GeoData    {
        public double latitude { get; set; } 
        public double longitude { get; set; } 
        public double altitude { get; set; } 
        public double latitudeSpan { get; set; } 
        public double longitudeSpan { get; set; } 
    }

    public class GeoDataExif    {
        public double latitude { get; set; } 
        public double longitude { get; set; } 
        public double altitude { get; set; } 
        public double latitudeSpan { get; set; } 
        public double longitudeSpan { get; set; } 
    }

    public class DriveDesktopUploader    {
    }

    public class GooglePhotosOrigin    {
        public DriveDesktopUploader driveDesktopUploader { get; set; } 
    }

    public class JsonMetadata    {
        static Dictionary<string,Dictionary<string,string>> Dir_JsonFiles_FileName_Dict = new Dictionary<string, Dictionary<string,string>>();

        public string title { get; set; } 
        public string description { get; set; } 
        public string imageViews { get; set; } 
        public CreationTime creationTime { get; set; } 
        public PhotoTakenTime photoTakenTime { get; set; } 
        public GeoData geoData { get; set; } 
        public GeoDataExif geoDataExif { get; set; } 
        public GooglePhotosOrigin googlePhotosOrigin { get; set; } 


        public static void ReadJsonsFromDir(string dir)
        {
            var JsonFilePaths = Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories);
            Dictionary<string,string> JsonFullPath_FileName_dict = new Dictionary<string, string>();

            foreach (var JsonFilePath in JsonFilePaths)
                JsonFullPath_FileName_dict.Add(JsonFilePath,Path.GetFileNameWithoutExtension(JsonFilePath));
            
            Dir_JsonFiles_FileName_Dict.Add(dir,JsonFullPath_FileName_dict);
        }

        public static JsonMetadata Search(string filePath)
        {
            JsonMetadata metadata =null;
            string jsonstr = null;

            var dir = Path.GetDirectoryName(filePath);
            if (Dir_JsonFiles_FileName_Dict.Keys.Contains(dir)==false)
                ReadJsonsFromDir(dir);

            var JsonFullPath_FileName_dict = Dir_JsonFiles_FileName_Dict[dir];
            var FileName = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                //most case
                //IMG_0005.JPG => MG_0005.JPG.json
                var json_metadata_path = filePath+".json";
                if (JsonFullPath_FileName_dict.Keys.Contains(json_metadata_path))
                    jsonstr = File.ReadAllText(json_metadata_path);
                else if(FileName.EndsWith(')'))
                {
                    // IMG_0005(1).JPG => IMG_0005.JPG(1).json
                    json_metadata_path=Regex.Replace(filePath, @"(.+)(\(.+\))(.+$)", @"$1$3$2")+".json";
                    if (JsonFullPath_FileName_dict.Keys.Contains(json_metadata_path))
                        jsonstr = File.ReadAllText(json_metadata_path);
                }

                
                if(String.IsNullOrEmpty(jsonstr))
                {
                    //search by filename
                    var found_json_kvp = JsonFullPath_FileName_dict
                        .FirstOrDefault(kvp=>FileName.Contains(kvp.Value) || kvp.Value.Contains(FileName));

                    if(found_json_kvp.Equals(default(KeyValuePair<string,string>))==false)
                        jsonstr = File.ReadAllText(found_json_kvp.Key);
                }

                if(String.IsNullOrEmpty(jsonstr)==false)
                    metadata = JsonSerializer.Deserialize<JsonMetadata>(jsonstr);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"SearchJsonMetadata,Ex:{ex.Message}");
            }

            return metadata;
        }
    }
}