using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GooglePhotoExifFix
{
    public class DuplicateRemover
    {
        public static List<string> Remove(string path,string del_extension)
        {
            var RemovedList=new List<string>();
            
            var dirs = Directory.GetDirectories(path,"*",SearchOption.AllDirectories).ToList();
                dirs.Add(path);
                dirs.ForEach(dir=>{
                    Directory.GetFiles(dir).Where(x=>x.EndsWith(".json")==false)
                        .GroupBy(x=>Path.GetFileNameWithoutExtension(x))
                        .Where(g=>g.Count()>1)//Remove only duplicate files
                        .SelectMany(g=>g)
                        .Where(s=>s.EndsWith(del_extension,StringComparison.CurrentCultureIgnoreCase))
                        .ToList()
                        .ForEach(s=>
                        {
                            File.Delete(s);
                            RemovedList.Add(s);
                            Console.WriteLine($"Remove duplicated image:{s}");
                        });
                });

            return RemovedList;
        }
    }
}