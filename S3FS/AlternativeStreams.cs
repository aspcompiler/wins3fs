// Define this const if you want to use the NTFS alternative streams
#define NTFS_ALTERNATIVE_STREAMS

// Define this const if you want to use a dummy implementation of streams. Is added to show how stream support can be implemented
//#define DUMMY_ALTERNATIVE_STREAMS


// In any case you have to rename the System.IO.File in FileEx and FileInfo in FileInfoEx
using System;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Suchwerk.Filesystem
{

#if (NTFS_ALTERNATIVE_STREAMS)
    /// <summary>
    /// Superset of class FileInfo that can handle alternative streams
    /// </summary>
    internal class FileInfoEx : FileSystemInfo
    {
        bool _IsStream = false;
        FileInfo _FI;

        public FileInfoEx(string fileName)
            : base()
        {
            _IsStream = AlternativeStreams.IsStream(fileName);

            if (!_IsStream)
                _FI = new FileInfo(fileName);

            base.OriginalPath = fileName;
            base.FullPath = fileName;
        }

        public override string Name
        {
            get
            {
                if (_IsStream)
                    return base.FullPath.Substring(base.FullPath.LastIndexOf('\\') + 1);
                else
                    return _FI.Name;
            }
        }

        public override bool Exists
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override void Delete()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long Length
        {
            get
            {
                if (_IsStream)
                    return AlternativeStreams.GetStreamLength2(base.FullPath);
                else
                    return _FI.Length;
            }
        }
    }
       
    /// <summary>
    /// Superset of class File that can hanlde alternative streams
    /// </summary>
    internal static class FileEx
    {
        public static bool Exists(string path)
        {
            if (!AlternativeStreams.IsStream(path))
                return File.Exists(path);

            return AlternativeStreams.GetStreamLength2(path) != -1;
        }

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!AlternativeStreams.IsStream(path))
                return File.Open(path, mode, access, share);

            SafeFileHandle hFile = AlternativeStreams.CreateFile(path, AlternativeStreams.Access2API(access), share, 0, mode, 0, 0);

            if (hFile.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
            }

            return new FileStream(hFile, access);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public static void Delete(string path)
        {
            File.Delete(path);
        }

        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            File.SetLastWriteTime(path, lastWriteTime);
        }

        public static DateTime GetCreationTimeUtc(string path)
        {
            return File.GetCreationTimeUtc(path);
        }

        public static string[] StreamList(string path)
        {
            System.Diagnostics.Debugger.Break();
            return null;
        }

    }

    /// <summary>
    /// Wraps the API functions, structures and constants.
    /// </summary>
    internal static class AlternativeStreams
    {
        internal const char STREAM_SEP = ':';
        internal const int INVALID_HANDLE_VALUE = -1;
        internal const int MAX_PATH = 256;

        internal static bool IsStream(string path)
        {
            return path.LastIndexOf(':') != 1;
        }

        internal enum FILE_INFORMATION_CLASS
        {
		FileDirectoryInformation = 1,
		FileFullDirectoryInformation,   // 2
		FileBothDirectoryInformation,   // 3
		FileBasicInformation,           // 4  wdm
		FileStandardInformation,        // 5  wdm
		FileInternalInformation,        // 6
		FileEaInformation,              // 7
		FileAccessInformation,          // 8
		FileNameInformation,            // 9
		FileRenameInformation,          // 10
		FileLinkInformation,            // 11
		FileNamesInformation,           // 12
		FileDispositionInformation,     // 13
		FilePositionInformation,        // 14 wdm
		FileFullEaInformation,          // 15
		FileModeInformation,            // 16
		FileAlignmentInformation,       // 17
		FileAllInformation,             // 18
		FileAllocationInformation,      // 19
		FileEndOfFileInformation,       // 20 wdm
		FileAlternateNameInformation,   // 21
		FileStreamInformation,          // 22
		FilePipeInformation,            // 23
		FilePipeLocalInformation,       // 24
		FilePipeRemoteInformation,      // 25
		FileMailslotQueryInformation,   // 26
		FileMailslotSetInformation,     // 27
		FileCompressionInformation,     // 28
		fileContextIdInformation,        // 29
		FileCompletionInformation,      // 30
		FileMoveClusterInformation,     // 31
		FileQuotaInformation,           // 32
		FileReparsePointInformation,    // 33
		FileNetworkOpenInformation,     // 34
		FileAttributeTagInformation,    // 35
		FileTrackingInformation,        // 36
		FileMaximumInformation
        }
	

        /// <summary>
        /// Get the lengths (and therefore existens) of a alternative stream by the undocumented function NtQueryInformationFile
        /// 
        /// This cod eis not finished !!!
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static unsafe long GetStreamLength2(string path)
        {
            //typedef struct _FILE_STREAM_INFORMATION 
            //{    
            //0,4   ULONG NextEntryOffset;    
            //4,4   ULONG StreamNameLength;
            //8,8   LARGE_INTEGER StreamSize;
            //16,8  LARGE_INTEGER StreamAllocationSize;
            //24,2  WCHAR StreamName[1];
            //26} FILE_STREAM_INFORMATION, *PFILE_STREAM_INFORMATION;

            //const int FileStreamInformation = 0x16;     // FILE_STREAM_INFORMATION 

            string StreamName = Path.GetFileName(path);
            StreamName = StreamName.Substring(StreamName.IndexOf(':') + 1);

            //Open the file with backup semantics
            SafeFileHandle hFile = AlternativeStreams.CreateFile(path.Substring(0, path.Length - StreamName.Length - 1), AlternativeStreams.FileAccessAPI.GENERIC_READ, FileShare.Read, 0, FileMode.Open, AlternativeStreams.FileFlags.None, 0);
            if (hFile.IsInvalid)
                return -1;

            IO_STATUS_BLOCK ioStatus = new IO_STATUS_BLOCK();

            byte[] InfoBlock = new byte[64 * 1024];

            fixed (byte* pIB = &InfoBlock[0])
            {
                int error = NtQueryInformationFile(hFile, &ioStatus, pIB, InfoBlock.Length, FILE_INFORMATION_CLASS.FileStreamInformation);
                if (error != 0)
                    return -1;
            }

            CloseHandle(hFile);

            int Pos = 0;
            int NameLength;
            long Size = -1;
            int Next = -1;
            StringBuilder sbName;
            string Name;

            while (true)
            {
                Next = BitConverter.ToInt32(InfoBlock, Pos);
                NameLength = BitConverter.ToInt32(InfoBlock, Pos + 4);

                if (NameLength == 0)
                    return -1;

                Size = BitConverter.ToInt64(InfoBlock, Pos + 8);
                sbName = new StringBuilder(NameLength);

                Name = UnicodeEncoding.Unicode.GetString(InfoBlock, 24, NameLength / 2);

                for (int i=0; i<NameLength/2; i++)
                    sbName.Append((char)BitConverter.ToInt16(InfoBlock, Pos + 24 + i*2));

                Name = sbName.ToString();

                if (Name.EndsWith("::$DATA"))
                    Name = Name.Substring(0, Name.Length - 7);

                if (Name == StreamName)
                    return Size;

                if (Next == 0)
                    return -1;

                Pos += Next;
            }
        }

        /// <summary>
        /// Get the lengths (and therefore existens) of a alternative stream by seeking the file,
        /// what can be very slow !!
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static long GetStreamLength(string path)
        {
            string StreamName = Path.GetFileName(path);
            StreamName = StreamName.Substring(StreamName.IndexOf(':') + 1);

            //Open the file with backup semantics
            SafeFileHandle hFile = AlternativeStreams.CreateFile(path.Substring(0, path.Length - StreamName.Length - 1), AlternativeStreams.FileAccessAPI.GENERIC_READ, FileShare.Read, 0, FileMode.Open, AlternativeStreams.FileFlags.BackupSemantics, 0);
            if (hFile.IsInvalid)
                return -1;

            AlternativeStreams.WIN32_STREAM_ID sid = new AlternativeStreams.WIN32_STREAM_ID();
            int dwStreamHeaderSize = Marshal.SizeOf(sid);
            int Context = 0;
            bool Continue = true;
            long Size = -1;

            int lRead = 0;
            IntPtr pName;
            char[] bName;
            string sName;
            int i;
            int low; int high;

            try
            {
                while (Continue && Size == -1)
                {
                    //Read the next stream header
                    lRead = 0;
                    Continue = AlternativeStreams.BackupRead(hFile, ref sid, dwStreamHeaderSize, ref lRead, false, false, ref Context);
                    if (Continue && lRead == dwStreamHeaderSize)
                    {
                        if (sid.dwStreamNameSize > 0)
                        {
                            //Read the stream name
                            lRead = 0;
                            pName = Marshal.AllocHGlobal(sid.dwStreamNameSize);
                            try
                            {
                                Continue = AlternativeStreams.BackupRead(hFile, pName, sid.dwStreamNameSize, ref lRead, false, false, ref Context);
                                bName = new char[sid.dwStreamNameSize];
                                Marshal.Copy(pName, bName, 0, sid.dwStreamNameSize);

                                //Name is of the format ":NAME:$DATA\0"
                                sName = new string(bName);
                                i = sName.IndexOf(AlternativeStreams.STREAM_SEP, 1);
                                if (i > -1)
                                    sName = sName.Substring(1, i - 1);
                                else
                                {
                                    //This should never happen. 
                                    //Truncate the name at the first null char.
                                    i = sName.IndexOf('\0');
                                    if (i > -1)
                                        sName = sName.Substring(1, i - 1);
                                }

    #region "Stream type"
                                /*                               switch( streamId.streamId )
 110:                                  {
 111:                                      case ( int ) StreamType.BACKUP_ALTERNATE_DATA:
 112:                                          streamInfo.StreamType = "Alternative Data Stream";
 113:                                          break;
 114:   
 115:                                      case ( int ) StreamType.BACKUP_DATA:
 116:                                          streamInfo.StreamType = "Standard Data";
 117:                                          break;
 118:   
 119:                                      case ( int ) StreamType.BACKUP_EA_DATA:
 120:                                          streamInfo.StreamType = "Extended attribute Data";
 121:                                          break;
 122:   
 123:                                      case ( int ) StreamType.BACKUP_LINK:
 124:                                          streamInfo.StreamType = "Hard link information";
 125:                                          break;
 126:   
 127:                                      case ( int ) StreamType.BACKUP_OBJECT_ID:
 128:                                          streamInfo.StreamType = "Object identifiers";
 129:                                          break;
 130:   
 131:                                      case ( int ) StreamType.BACKUP_PROPERTY_DATA:
 132:                                          streamInfo.StreamType = "Property data";
 133:                                          break;
 134:   
 135:                                      case ( int ) StreamType.BACKUP_REPARSE_DATA:
 136:                                          streamInfo.StreamType = "Reparse points";
 137:                                          break;
 138:   
 139:                                      case ( int ) StreamType.BACKUP_SECURITY_DATA:
 140:                                          streamInfo.StreamType = "Security descriptor data";
 141:                                          break;
 142:   
 143:                                      case ( int ) StreamType.BACKUP_SPARSE_BLOCK:
 144:                                          streamInfo.StreamType = "Sparse file";
 145:                                          break;
 146:                                  }  
 */
                                #endregion

                                if (StreamName == sName)
                                    Size = sid.Size.ToInt64();
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(pName);
                            }
                        }

                        //Skip the stream contents
                        low = 0; high = 0;
                        Continue = AlternativeStreams.BackupSeek(hFile, sid.Size.Low, sid.Size.High, ref low, ref high, ref Context);
                    }
                    else
                        break;
                }
            }
            finally
            {
                if (Context != 0 && Context != -1)
                    AlternativeStreams.BackupRead(hFile, ref sid, 0, ref lRead, true, false, ref Context);

                AlternativeStreams.CloseHandle(hFile);
            }

            return Size;
        }

        [Flags()]
        internal enum FileFlags : uint
        {
            None            = 0,
            WriteThrough    = 0x80000000,
            Overlapped      = 0x40000000,
            NoBuffering     = 0x20000000,
            RandomAccess    = 0x10000000,
            SequentialScan  = 0x08000000,
            DeleteOnClose   = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics  = 0x01000000,
            OpenReparsePoint= 0x00200000,
            OpenNoRecall    = 0x00100000
        }

        [Flags()]
        internal enum FileAccessAPI : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }
        /// <summary>
        /// Provides a mapping between a System.IO.FileAccess value and a FileAccessAPI value.
        /// </summary>
        /// <param name="Access">The <see cref="System.IO.FileAccess"/> value to map.</param>
        /// <returns>The <see cref="FileAccessAPI"/> value.</returns>
        internal static FileAccessAPI Access2API(FileAccess Access)
        {
            FileAccessAPI lRet = 0;
            if ((Access & FileAccess.Read) == FileAccess.Read) lRet |= FileAccessAPI.GENERIC_READ;
            if ((Access & FileAccess.Write) == FileAccess.Write) lRet |= FileAccessAPI.GENERIC_WRITE;
            return lRet;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LARGE_INTEGER
        {
            public int Low;
            public int High;

            public long ToInt64()
            {
                return (long)High * 4294967296 + (long)Low;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_STREAM_ID
        {
            public int dwStreamID;
            public int dwStreamAttributes;
            public LARGE_INTEGER Size;
            public int dwStreamNameSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct IO_STATUS_BLOCK
        {   //typedef struct _ {
            //union {
            //0,4   NTSTATUS Status;
            //0,4   PVOID Pointer; };
            //4,4   ULONG_PTR Information;
            //8} IO_STATUS_BLOCK, *PIO_STATUS_BLOCK;
            [FieldOffset(0)]
            public int Status;
            [FieldOffset(0)]
            public int Pointer;
            [FieldOffset(4)]
            public FILE_INFORMATION_CLASS Information;
        }

        [DllImport("kernel32", SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(string Name, FileAccessAPI Access, FileShare Share, int Security, FileMode Creation, FileFlags Flags, int Template);

        [DllImport("kernel32", SetLastError = true, EntryPoint = "CreateFile")]
        internal static extern IntPtr CreateFile2(string Name, FileAccessAPI Access, FileShare Share, int Security, FileMode Creation, FileFlags Flags, int Template);
        [DllImport("kernel32")]
        internal static extern bool DeleteFile(string Name);
        [DllImport("kernel32")]
        internal static extern bool CloseHandle(SafeFileHandle hObject);

        [DllImport("kernel32")]
        internal static extern bool BackupRead(SafeFileHandle hFile, IntPtr pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref int Context);
        [DllImport("kernel32")]
        internal static extern bool BackupRead(SafeFileHandle hFile, ref WIN32_STREAM_ID pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref int Context);
        [DllImport("kernel32")]
        internal static extern bool BackupSeek(SafeFileHandle hFile, int dwLowBytesToSeek, int dwHighBytesToSeek, ref int dwLow, ref int dwHigh, ref int Context);

        [DllImport("ntdll.dll")]
        public static unsafe extern int NtQueryInformationFile(
            SafeFileHandle hFile,
            IO_STATUS_BLOCK* io,
            byte* InfoBlock,
            int len,
            FILE_INFORMATION_CLASS FILE_INFORMATION_CLASS
        );
    }
#endif

#if (DUMMY_ALTERNATIVE_STREAMS)
    /// <summary>
    /// Superset of class FileInfo that can handle alternative streams
    /// </summary>
    internal class FileInfoEx : FileSystemInfo
    {
        bool _IsStream = false;
        FileInfo _FI;

        /// <summary>
        /// Creates a new FileInfo obejct for the given file or stream of the file
        /// </summary>
        /// <param name="fileName"></param>
        public FileInfoEx(string fileName)
            : base()
        {
            _IsStream = AlternativeStreams.IsStream(fileName);

            //if (!_IsStream)
                _FI = new FileInfo(AlternativeStreams.GetFileName(fileName));

            base.OriginalPath = fileName;
            base.FullPath = AlternativeStreams.GetFileName(fileName);
        }

        /// <summary>
        /// Returns the name of the file or the name of the stream
        /// </summary>
        public override string Name
        {
            get
            {
                if (_IsStream)
                    return base.FullPath.Substring(base.FullPath.LastIndexOf('\\') + 1);
                else
                    return _FI.Name;
            }
        }

        public override bool Exists
        {
            get
            {
                return File.Exists(FullPath);
            }
        }

        public override void Delete()
        {
            //System.Diagnostics.Debugger.Break();
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Returns the length of the file or length of the stream in this file
        /// </summary>
        public long Length
        {
            get
            {
               // if (_IsStream)
               //     return AlternativeStreams.GetStreamLength(base.FullPath);
               // else
                    return _FI.Length;
            }
        }
    } 
       
    /// <summary>
    /// Superset of class File that can handle alternative streams
    /// </summary>
    internal static class FileEx 
    {
        /// <summary>
        /// Checks if this file or the stream of this file exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Exists(string path)
        {
            return File.Exists(AlternativeStreams.GetFileName(path));
        }

        /// <summary>
        /// Opens the file or a stream of this file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <returns></returns>
        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (!AlternativeStreams.IsStream(path))
                return File.Open(path, mode, access, share);

            string newPath = AlternativeStreams.GetFileName(path);
            
            // Check if the stream directory and the base file is already existing
            if (mode == FileMode.CreateNew || mode == FileMode.OpenOrCreate)
            {
                string helpString = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(helpString))
                    Directory.CreateDirectory(helpString);

                helpString = helpString.Substring(0, helpString.Length - AlternativeStreams.SUBDIRNAME.Length);
                if (!File.Exists(helpString))
                    File.WriteAllText(helpString,"");
            }

            return File.Open(newPath, mode, access, share);            
        }
      
        /// <summary>
        /// Returns a list of all existing streams for this file
        /// </summary>
        /// <param name="path">Name of the file</param>
        /// <returns>List of streams that are in the file</returns>
        public static string[] StreamList(string path)
        {
            if (!File.Exists(AlternativeStreams.GetFileName(path)))
                 throw new System.IO.FileNotFoundException();

            if (!Directory.Exists(path + AlternativeStreams.SUBDIRNAME))
                return new string[1]  { "" } ;      // Report at least the main stream, is the one without a name

            // We make a shourtcut here. Streams can be organized in directories and even directories can have streams !
            string[] temp = Directory.GetFiles(path + AlternativeStreams.SUBDIRNAME);

            string[] result = new string[temp.Length + 1];

            result[0] = "";
            for (int i = 0; i < temp.Length; i++)
                result[i + 1] = Path.GetFileName(temp[i]);

            return result;
        }

        /// <summary>
        /// Delete a file and all streams of this file
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            // Check if we want to delete a stream only
            if (AlternativeStreams.IsStream(path))
                File.Delete(AlternativeStreams.GetFileName(path));
            else
            {
                if (Directory.Exists(path + AlternativeStreams.SUBDIRNAME))
                    Directory.Delete(path + AlternativeStreams.SUBDIRNAME, true);

                File.Delete(path);
            }
        }

        /// <summary>
        /// Moves a file and all streams of this file
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public static void Move(string sourceFileName, string destFileName)
        {
            if (AlternativeStreams.IsStream(sourceFileName) || AlternativeStreams.IsStream(destFileName))
                throw new System.ArgumentException();

            try
            {
                File.Move(sourceFileName,  destFileName);

                if (Directory.Exists(sourceFileName +  AlternativeStreams.SUBDIRNAME))
                    Directory.Move(sourceFileName +  AlternativeStreams.SUBDIRNAME, destFileName +  AlternativeStreams.SUBDIRNAME);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Returns the CreationTime of a file or returns the CreationTime of a stream of this file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetCreationTimeUtc(string path)
        {
            return File.GetCreationTimeUtc(AlternativeStreams.GetFileName(path));
        }

        /// <summary>
        /// Sets the LastWrite Time of this file or of a stream of this file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lastWriteTime"></param>
        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            File.SetLastWriteTime(AlternativeStreams.GetFileName(path), lastWriteTime);
        }
    }

    /// <summary>
    /// Class to implemented alternative streams in subdirectories
    /// If a file has a alternative stream a direcotory with the name "FILENAME" + "$DATA" is created and
    /// for each stream a seperate file is created
    /// </summary>
    internal static class AlternativeStreams
    {
        internal const char STREAM_SEP = ':';
        internal const int MAX_PATH = 256;
        internal const string SUBDIRNAME = "$DATA";

        /// <summary>
        /// Checks if the filename contains stream information
        /// </summary>
        /// <param name="path">File name with optional alternative stream information</param>
        /// <returns>TRUE is file name is about an alternative stream</returns>
        internal static bool IsStream(string path)
        {
            int StreamCharPos = path.LastIndexOf(STREAM_SEP);

            if (StreamCharPos == -1)
                return false;

            if (StreamCharPos == path.Length)
                return false;       // We are in the main stream

            if (StreamCharPos == 1 && path.Length > 2 && path[2] == '\\')
                return false;

            return true;
        }

        /// <summary>
        /// Creates out of a FileName:StreamName a new simple file name that stores the stream content
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetFileName(string path)
        {
            if (!IsStream(path))
                return path;

            int pos = path.LastIndexOf(STREAM_SEP);
            
            path = path.Substring(0, pos) + SUBDIRNAME + "\\" + path.Substring(pos + 1);
            
            return path;
        }
   }
#endif
}
