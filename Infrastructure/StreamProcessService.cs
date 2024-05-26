using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Syncfusion.CompoundFile.XlsIO.Native;
using System.IO;
using System.IO.Compression;
using System.Text;
using Xceed.Words.NET;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Infrastructure;

public static class StreamProcessService
{
    public static IEnumerable<byte> OpenFile_TXT_DOCX(this FileStream stream)
    {
        if (stream == null)
        {
            throw new Exception("Поток данных не существует.");
        }
        byte[] data = new byte[stream.Length];
        stream.Read(data);
        switch (Path.GetExtension(stream.Name))
        {
            case ".txt":
                return data;
            case ".docx":
                var paragraphs = DocX.Load(stream).Paragraphs;
                string result = string.Empty;
                foreach (var el in paragraphs)
                {
                    result += el.Text + "\n";
                }
                return result.ToTypeByte();
        }
        throw new Exception("Не известный формат файла");
    }
    public static void SaveFile_TXT_DOCX(this FileStream stream, IEnumerable<int>? data)
    {
        if (stream == null)
        {
            throw new Exception("Поток данных не существует.");
        }
        if (data == null)
        {
            throw new Exception("Массив данных пуст");
        }
        IEnumerable<byte> bytes = data.ToTypeByte();
        if (Path.GetExtension(stream.Name) == ".txt")
        {
            byte[] array = bytes.ToArray();
            stream.Write(array, 0, array.Length);
        }
        if (Path.GetExtension(stream.Name) == ".docx")
        {
            var docX = DocX.Create(stream);
            string text = bytes.ToTypeString();
            var paragraphs = text.Split(new char[] { '\n' });
            foreach (var p in paragraphs)
            {
                docX.InsertParagraph(p);
            }
            docX.SaveAs(stream);
        }
    }


    public static T? ReadJson<T>(this string path)
    {
        string data = File.ReadAllText($"{path}.json");
        return JsonConvert.DeserializeObject<T>(data);
    }
    public static bool SaveJson<T>(this string path, T data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText($"{path}.json", json);
            return true;
        }
        catch
        {
            return false;
        }
    }


    public static T? ReadGz<T>(this string path)
    {
        // поток для чтения из сжатого файла
        using FileStream sourceStream = new FileStream($"{path}.gz", FileMode.Open);

        // поток для записи восстановленного файла
        using FileStream targetStream = File.Create($"{path}.json");

        using GZipStream decompressionStream = new(sourceStream, CompressionMode.Decompress);

        decompressionStream.CopyTo(targetStream);

        targetStream.Close();
        sourceStream.Close();

        T? result =  ReadJson<T>(path);

        DeleteFile($"{path}.json");

        return result;
    }
    public static bool SaveGz<T>(this string path, T data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            using GZipStream compressionStream = new(new FileStream($"{path}.gz", FileMode.OpenOrCreate), CompressionMode.Compress);

            byte[] bytes = json.ToTypeByte().ToArray();

            compressionStream.Write(bytes);

            compressionStream.Close();

            return true;
        }
        catch
        {
            return false;
        }
    }


    public static void CopyFile(string path, string newPath)
    {
        File.Copy(path, newPath, true);
    }
    public static void DeleteFile(string path)
    {
        File.Delete(path);
    }


    public static KeyValuePair<string, IEnumerable<string>> GetListFilesFromFolder(this string folderPath)
    {
        return new($"{folderPath.Trim()}", Directory.GetFiles(folderPath));
    }
}