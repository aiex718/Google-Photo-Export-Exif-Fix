# Google-Photo-Export-Exif-Fix
A tool to read json exported from Google photo or takeout, then write to image exif metadata.

Currently only modify DateTimeOriginal in exif, lastwrite and creation time of the file.

# Requirements
* [.Net 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)

# Arguments
| Argument Name | Short Name | HelpText                                                                                    |
|---------------|------------|---------------------------------------------------------------------------------------------|
| path          | p          | Path to root folder.                                                                        |
| force         | f          | Force overwrite exif date, otherwise only write exif date when empty.                       |
| filedate      | d          | Modify file last write and creation time.                                                   |
| extensions    | e          | Image file extensions* you want to edit such as jpg, use',' to seperate multiple extensions. |
| remove        | r          | Set which file extensions to remove when found duplicate file name in same folder.          |
| verbose       | v          | Show verbose information.                                                                   |

**PNG does not embed EXIF info*

# Example Usage
    GooglePhotoExifFix -p "C:\Download\Google Takeout" -f -d -e jpeg,jpg,heic,heif 

# Package List
* [CommandLineParser](https://github.com/commandlineparser/commandline)
* [ExifLibNet](https://github.com/oozcitak/exiflibrary)
