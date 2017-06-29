using System.IO;

namespace SeekFiles
{
    class SeekFile
    {
        private FileInfo filInfo = null;
        private DirectoryInfo dirInfo = null;

        public SeekFile() { }

        public FileInfo RecursiveFileSearch(string fileName, string currentDirectory) {

            DirectoryInfo rootDirectory = new DirectoryInfo(currentDirectory);

            foreach (FileInfo fileInfo in rootDirectory.GetFiles()) {
                try {
                    if (fileInfo.Name.Equals(fileName))
                        filInfo = fileInfo;
                    RecursiveFileSearch(fileName, fileInfo.Name);
                } catch (FileNotFoundException exception) {
                    continue;
                }
            }
            return filInfo;
        }

        public DirectoryInfo RecursiveDirectorySearch(string fileName, string currentDirectory) {

            DirectoryInfo rootDirectory = new DirectoryInfo(currentDirectory);
            
            foreach (DirectoryInfo directoryInfo in rootDirectory.GetDirectories()) {
                try {
                    if (directoryInfo.Name.Equals(fileName))
                        dirInfo = directoryInfo;
                    RecursiveDirectorySearch(fileName, directoryInfo.Name);
                } catch (DirectoryNotFoundException exception) {
                    continue;
                }  
            }
            return dirInfo;
        }
    }
}
