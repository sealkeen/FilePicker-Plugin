using Plugin.FilePicker.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Plugin.FilePicker
{
    /// <summary>
    /// Implementation for file picking on WPF platform
    /// </summary>
    public class FilePickerImplementation : IFilePicker
    {
        /// <summary>
        /// File picker implementation for WPF; uses the OpenFileDialog from
        /// System.Windows.Forms reference assembly.
        /// </summary>
        /// <param name="allowedTypes">
        /// Specifies one or multiple allowed types. When null, all file types
        /// can be selected while picking.
        /// On WPF, specify strings like this: "Data type (*.ext)|*.ext", which
        /// corresponds how the Windows file open dialog specifies file types.
        /// </param>
        /// <returns>file data of picked file, or null when picking was cancelled</returns>
        public Task<FileData> PickFile(string[] allowedTypes = null)
        {
            System.Windows.Forms.OpenFileDialog picker = new System.Windows.Forms.OpenFileDialog();
            picker.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            picker.Multiselect = true;

            if (allowedTypes != null)
            {
                picker.Filter = string.Join("|", allowedTypes);
            }

            var result = picker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileData fd = null;
                return Task.Factory.StartNew(() => fd);
            }

            var fileName = Path.GetFileName(picker.FileName);

            var data = new FileData(picker.FileName, fileName, () => File.OpenRead(picker.FileName), (x) => { });

            if (picker.FileNames.Length > 1)
            {
                data.FileNames = new List<string>();
                foreach (var filename in picker.FileNames)
                {
                    data.FileNames.Add(filename);
                }
            }
            return Task.Factory.StartNew(() => data);
        }

        /// <summary>
        /// WPF implementation of saving a picked file to a local folder.
        /// </summary>
        /// <param name="fileToSave">picked file data for file to save</param>
        /// <returns>true when file was saved successfully, false when not</returns>
        public Task<bool> SaveFile(FileData fileToSave)
        {
            try
            {
                Task result;
                using (FileStream sourceStream =
                    new FileStream(fileToSave.FilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    result = Task.Factory.StartNew(() => sourceStream.Write(fileToSave.DataArray, 0, fileToSave.DataArray.Length));
                    result.Wait();
                }

                return Task.Factory.StartNew(() => true);
            }
            catch (Exception)
            {
                // ignore exception
                return Task.Factory.StartNew(() => false);
            }
        }

        /// <summary>
        /// WPF implementation of OpenFile(), opening a file already stored in a local folder.
        /// </summary>
        /// <param name="fileToOpen">relative filename of file to open</param>
        public void OpenFile(string fileToOpen)
        {
            try
            {
                if (File.Exists(fileToOpen))
                {
                    Process.Start(fileToOpen);
                }
            }
            catch (Exception)
            {
                // ignore exception
            }
        }

        /// <summary>
        /// WPF implementation of OpenFile(), opening a picked file in an external viewer. The
        /// picked file is saved to a local folder before opening.
        /// </summary>
        /// <param name="fileToOpen">picked file data</param>
        public void OpenFile(FileData fileToOpen)
        {
            try
            {
                if (!File.Exists(fileToOpen.FileName))
                {
                    SaveFile(fileToOpen);
                }

                Process.Start(fileToOpen.FileName);
            }
            catch (Exception)
            {
                // ignore exception
            }
        }
    }
}
