using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Infrastructure;

public class DialogService
{
    public string txtFilter = "Txt files|*.txt;*.docx";

    public enum FileType { TEXT, EXCEL, CTF, NAN };
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public FileType TypeFile { get; private set; } = FileType.NAN;
    public void SetFileType(FileType type)
    {
        this.TypeFile = type;
    }
    public bool OpenFileDialog(string filter = "")
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (filter == string.Empty)
        {
            openFileDialog.Filter = txtFilter;
        }
        else
        {
            openFileDialog.Filter = filter;
        }
        if (openFileDialog.ShowDialog() == true)
        {
            string ext = Path.GetExtension(openFileDialog.FileName);
            if (txtFilter.Contains(ext))
            {
                TypeFile = FileType.TEXT;
            }
            this.FilePath = openFileDialog.FileName;
            this.FileName = Path.GetFileName(this.FilePath);
            return true;
        }
        return false;
    }
    
    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }
}
