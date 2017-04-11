using System;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Suchwerk.Filesystem
{
    
    internal static class DeviceIO_Unmanaged
    {
        [Flags]
        public enum EMethod : uint
        {
            Buffered = 0,
            InDirect = 1,
            OutDirect = 2,
            Neither = 3
        }

        [Flags]
        public enum EFileDevice : uint
        {
            Beep = 0x00000001,
            CDRom = 0x00000002,
            CDRomFileSytem = 0x00000003,
            Controller = 0x00000004,
            Datalink = 0x00000005,
            Dfs = 0x00000006,
            Disk = 0x00000007,
            DiskFileSystem = 0x00000008,
            FileSystem = 0x00000009,
            InPortPort = 0x0000000a,
            Keyboard = 0x0000000b,
            Mailslot = 0x0000000c,
            MidiIn = 0x0000000d,
            MidiOut = 0x0000000e,
            Mouse = 0x0000000f,
            MultiUncProvider = 0x00000010,
            NamedPipe = 0x00000011,
            Network = 0x00000012,
            NetworkBrowser = 0x00000013,
            NetworkFileSystem = 0x00000014,
            Null = 0x00000015,
            ParellelPort = 0x00000016,
            PhysicalNetcard = 0x00000017,
            Printer = 0x00000018,
            Scanner = 0x00000019,
            SerialMousePort = 0x0000001a,
            SerialPort = 0x0000001b,
            Screen = 0x0000001c,
            Sound = 0x0000001d,
            Streams = 0x0000001e,
            Tape = 0x0000001f,
            TapeFileSystem = 0x00000020,
            Transport = 0x00000021,
            Unknown = 0x00000022,
            Video = 0x00000023,
            VirtualDisk = 0x00000024,
            WaveIn = 0x00000025,
            WaveOut = 0x00000026,
            Port8042 = 0x00000027,
            NetworkRedirector = 0x00000028,
            Battery = 0x00000029,
            BusExtender = 0x0000002a,
            Modem = 0x0000002b,
            Vdm = 0x0000002c,
            MassStorage = 0x0000002d,
            Smb = 0x0000002e,
            Ks = 0x0000002f,
            Changer = 0x00000030,
            Smartcard = 0x00000031,
            Acpi = 0x00000032,
            Dvd = 0x00000033,
            FullscreenVideo = 0x00000034,
            DfsFileSystem = 0x00000035,
            DfsVolume = 0x00000036,
            Serenum = 0x00000037,
            Termsrv = 0x00000038,
            Ksec = 0x00000039
        }

        [Flags]
        public enum EIOControlCode : uint
        {
            // STORAGE
            StorageBase = EFileDevice.MassStorage,
            StorageCheckVerify = (StorageBase << 16) | (0x0200 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageCheckVerify2 = (StorageBase << 16) | (0x0200 << 2) | EMethod.Buffered | (0 << 14), // FileAccess.Any
            StorageMediaRemoval = (StorageBase << 16) | (0x0201 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageEjectMedia = (StorageBase << 16) | (0x0202 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageLoadMedia = (StorageBase << 16) | (0x0203 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageLoadMedia2 = (StorageBase << 16) | (0x0203 << 2) | EMethod.Buffered | (0 << 14),
            StorageReserve = (StorageBase << 16) | (0x0204 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageRelease = (StorageBase << 16) | (0x0205 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageFindNewDevices = (StorageBase << 16) | (0x0206 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageEjectionControl = (StorageBase << 16) | (0x0250 << 2) | EMethod.Buffered | (0 << 14),
            StorageMcnControl = (StorageBase << 16) | (0x0251 << 2) | EMethod.Buffered | (0 << 14),
            StorageGetMediaTypes = (StorageBase << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
            StorageGetMediaTypesEx = (StorageBase << 16) | (0x0301 << 2) | EMethod.Buffered | (0 << 14),
            StorageResetBus = (StorageBase << 16) | (0x0400 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageResetDevice = (StorageBase << 16) | (0x0401 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            StorageGetDeviceNumber = (StorageBase << 16) | (0x0420 << 2) | EMethod.Buffered | (0 << 14),
            StoragePredictFailure = (StorageBase << 16) | (0x0440 << 2) | EMethod.Buffered | (0 << 14),
            StorageObsoleteResetBus = (StorageBase << 16) | (0x0400 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            StorageObsoleteResetDevice = (StorageBase << 16) | (0x0401 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            // DISK
            DiskBase = EFileDevice.Disk,
            DiskGetDriveGeometry = (DiskBase << 16) | (0x0000 << 2) | EMethod.Buffered | (0 << 14),
            DiskGetPartitionInfo = (DiskBase << 16) | (0x0001 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskSetPartitionInfo = (DiskBase << 16) | (0x0002 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskGetDriveLayout = (DiskBase << 16) | (0x0003 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskSetDriveLayout = (DiskBase << 16) | (0x0004 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskVerify = (DiskBase << 16) | (0x0005 << 2) | EMethod.Buffered | (0 << 14),
            DiskFormatTracks = (DiskBase << 16) | (0x0006 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskReassignBlocks = (DiskBase << 16) | (0x0007 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskPerformance = (DiskBase << 16) | (0x0008 << 2) | EMethod.Buffered | (0 << 14),
            DiskIsWritable = (DiskBase << 16) | (0x0009 << 2) | EMethod.Buffered | (0 << 14),
            DiskLogging = (DiskBase << 16) | (0x000a << 2) | EMethod.Buffered | (0 << 14),
            DiskFormatTracksEx = (DiskBase << 16) | (0x000b << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskHistogramStructure = (DiskBase << 16) | (0x000c << 2) | EMethod.Buffered | (0 << 14),
            DiskHistogramData = (DiskBase << 16) | (0x000d << 2) | EMethod.Buffered | (0 << 14),
            DiskHistogramReset = (DiskBase << 16) | (0x000e << 2) | EMethod.Buffered | (0 << 14),
            DiskRequestStructure = (DiskBase << 16) | (0x000f << 2) | EMethod.Buffered | (0 << 14),
            DiskRequestData = (DiskBase << 16) | (0x0010 << 2) | EMethod.Buffered | (0 << 14),
            DiskControllerNumber = (DiskBase << 16) | (0x0011 << 2) | EMethod.Buffered | (0 << 14),
            DiskSmartGetVersion = (DiskBase << 16) | (0x0020 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskSmartSendDriveCommand = (DiskBase << 16) | (0x0021 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskSmartRcvDriveData = (DiskBase << 16) | (0x0022 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskUpdateDriveSize = (DiskBase << 16) | (0x0032 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskGrowPartition = (DiskBase << 16) | (0x0034 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskGetCacheInformation = (DiskBase << 16) | (0x0035 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskSetCacheInformation = (DiskBase << 16) | (0x0036 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskDeleteDriveLayout = (DiskBase << 16) | (0x0040 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskFormatDrive = (DiskBase << 16) | (0x00f3 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            DiskSenseDevice = (DiskBase << 16) | (0x00f8 << 2) | EMethod.Buffered | (0 << 14),
            DiskCheckVerify = (DiskBase << 16) | (0x0200 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskMediaRemoval = (DiskBase << 16) | (0x0201 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskEjectMedia = (DiskBase << 16) | (0x0202 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskLoadMedia = (DiskBase << 16) | (0x0203 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskReserve = (DiskBase << 16) | (0x0204 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskRelease = (DiskBase << 16) | (0x0205 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskFindNewDevices = (DiskBase << 16) | (0x0206 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            DiskGetMediaTypes = (DiskBase << 16) | (0x0300 << 2) | EMethod.Buffered | (0 << 14),
            //Changer
            ChangerBase = EFileDevice.Changer,
            ChangerGetParameters = (ChangerBase << 16) | (0x0000 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerGetStatus = (ChangerBase << 16) | (0x0001 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerGetProductData = (ChangerBase << 16) | (0x0002 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerSetAccess = (ChangerBase << 16) | (0x0004 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            ChangerGetElementStatus = (ChangerBase << 16) | (0x0005 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            ChangerInitializeElementStatus = (ChangerBase << 16) | (0x0006 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerSetPosition = (ChangerBase << 16) | (0x0007 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerExchangeMedium = (ChangerBase << 16) | (0x0008 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerMoveMedium = (ChangerBase << 16) | (0x0009 << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerReinitializeTarget = (ChangerBase << 16) | (0x000A << 2) | EMethod.Buffered | (FileAccess.Read << 14),
            ChangerQueryVolumeTags = (ChangerBase << 16) | (0x000B << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            FsctlRequestOplockLevel1 = (EFileDevice.FileSystem << 16) | (0 << 2) | EMethod.Buffered | (0 << 14),
            FsctlRequestOplockLevel2 = (EFileDevice.FileSystem << 16) | (1 << 2) | EMethod.Buffered | (0 << 14),
            FsctlRequestBatchOplock = (EFileDevice.FileSystem << 16) | (2 << 2) | EMethod.Buffered | (0 << 14),
            FsctlOplockBreakAcknowledge = (EFileDevice.FileSystem << 16) | (3 << 2) | EMethod.Buffered | (0 << 14),
            FsctlOpBatchAckClosePending = (EFileDevice.FileSystem << 16) | (4 << 2) | EMethod.Buffered | (0 << 14),
            FsctlOplockBreakNotify = (EFileDevice.FileSystem << 16) | (5 << 2) | EMethod.Buffered | (0 << 14),
            FsctlLockVolume = (EFileDevice.FileSystem << 16) | (6 << 2) | EMethod.Buffered | (0 << 14),
            FsctlUnlockVolume = (EFileDevice.FileSystem << 16) | (7 << 2) | EMethod.Buffered | (0 << 14),
            FsctlDismountVolume = (EFileDevice.FileSystem << 16) | (8 << 2) | EMethod.Buffered | (0 << 14),
            FsctlIsVolumeMounted = (EFileDevice.FileSystem << 16) | (10 << 2) | EMethod.Buffered | (0 << 14),
            FsctlIsPathnameValid = (EFileDevice.FileSystem << 16) | (11 << 2) | EMethod.Buffered | (0 << 14),
            FsctlMarkVolumeDirty = (EFileDevice.FileSystem << 16) | (12 << 2) | EMethod.Buffered | (0 << 14),
            FsctlQueryRetrievalPointers = (EFileDevice.FileSystem << 16) | (14 << 2) | EMethod.Neither | (0 << 14),
            FsctlGetCompression = (EFileDevice.FileSystem << 16) | (15 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSetCompression = (EFileDevice.FileSystem << 16) | (16 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            FsctlMarkAsSystemHive = (EFileDevice.FileSystem << 16) | (19 << 2) | EMethod.Neither | (0 << 14),
            FsctlOplockBreakAckNo2 = (EFileDevice.FileSystem << 16) | (20 << 2) | EMethod.Buffered | (0 << 14),
            FsctlInvalidateVolumes = (EFileDevice.FileSystem << 16) | (21 << 2) | EMethod.Buffered | (0 << 14),
            FsctlQueryFatBpb = (EFileDevice.FileSystem << 16) | (22 << 2) | EMethod.Buffered | (0 << 14),
            FsctlRequestFilterOplock = (EFileDevice.FileSystem << 16) | (23 << 2) | EMethod.Buffered | (0 << 14),
            FsctlFileSystemGetStatistics = (EFileDevice.FileSystem << 16) | (24 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetNtfsVolumeData = (EFileDevice.FileSystem << 16) | (25 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetNtfsFileRecord = (EFileDevice.FileSystem << 16) | (26 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetVolumeBitmap = (EFileDevice.FileSystem << 16) | (27 << 2) | EMethod.Neither | (0 << 14),
            FsctlGetRetrievalPointers = (EFileDevice.FileSystem << 16) | (28 << 2) | EMethod.Neither | (0 << 14),
            FsctlMoveFile = (EFileDevice.FileSystem << 16) | (29 << 2) | EMethod.Buffered | (0 << 14),
            FsctlIsVolumeDirty = (EFileDevice.FileSystem << 16) | (30 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetHfsInformation = (EFileDevice.FileSystem << 16) | (31 << 2) | EMethod.Buffered | (0 << 14),
            FsctlAllowExtendedDasdIo = (EFileDevice.FileSystem << 16) | (32 << 2) | EMethod.Neither | (0 << 14),
            FsctlReadPropertyData = (EFileDevice.FileSystem << 16) | (33 << 2) | EMethod.Neither | (0 << 14),
            FsctlWritePropertyData = (EFileDevice.FileSystem << 16) | (34 << 2) | EMethod.Neither | (0 << 14),
            FsctlFindFilesBySid = (EFileDevice.FileSystem << 16) | (35 << 2) | EMethod.Neither | (0 << 14),
            FsctlDumpPropertyData = (EFileDevice.FileSystem << 16) | (37 << 2) | EMethod.Neither | (0 << 14),
            FsctlSetObjectId = (EFileDevice.FileSystem << 16) | (38 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetObjectId = (EFileDevice.FileSystem << 16) | (39 << 2) | EMethod.Buffered | (0 << 14),
            FsctlDeleteObjectId = (EFileDevice.FileSystem << 16) | (40 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSetReparsePoint = (EFileDevice.FileSystem << 16) | (41 << 2) | EMethod.Buffered | (0 << 14),
            FsctlGetReparsePoint = (EFileDevice.FileSystem << 16) | (42 << 2) | EMethod.Buffered | (0 << 14),
            FsctlDeleteReparsePoint = (EFileDevice.FileSystem << 16) | (43 << 2) | EMethod.Buffered | (0 << 14),
            FsctlEnumUsnData = (EFileDevice.FileSystem << 16) | (44 << 2) | EMethod.Neither | (0 << 14),
            FsctlSecurityIdCheck = (EFileDevice.FileSystem << 16) | (45 << 2) | EMethod.Neither | (FileAccess.Read << 14),
            FsctlReadUsnJournal = (EFileDevice.FileSystem << 16) | (46 << 2) | EMethod.Neither | (0 << 14),
            FsctlSetObjectIdExtended = (EFileDevice.FileSystem << 16) | (47 << 2) | EMethod.Buffered | (0 << 14),
            FsctlCreateOrGetObjectId = (EFileDevice.FileSystem << 16) | (48 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSetSparse = (EFileDevice.FileSystem << 16) | (49 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSetZeroData = (EFileDevice.FileSystem << 16) | (50 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
            FsctlQueryAllocatedRanges = (EFileDevice.FileSystem << 16) | (51 << 2) | EMethod.Neither | (FileAccess.Read << 14),
            FsctlEnableUpgrade = (EFileDevice.FileSystem << 16) | (52 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
            FsctlSetEncryption = (EFileDevice.FileSystem << 16) | (53 << 2) | EMethod.Neither | (0 << 14),
            FsctlEncryptionFsctlIo = (EFileDevice.FileSystem << 16) | (54 << 2) | EMethod.Neither | (0 << 14),
            FsctlWriteRawEncrypted = (EFileDevice.FileSystem << 16) | (55 << 2) | EMethod.Neither | (0 << 14),
            FsctlReadRawEncrypted = (EFileDevice.FileSystem << 16) | (56 << 2) | EMethod.Neither | (0 << 14),
            FsctlCreateUsnJournal = (EFileDevice.FileSystem << 16) | (57 << 2) | EMethod.Neither | (0 << 14),
            FsctlReadFileUsnData = (EFileDevice.FileSystem << 16) | (58 << 2) | EMethod.Neither | (0 << 14),
            FsctlWriteUsnCloseRecord = (EFileDevice.FileSystem << 16) | (59 << 2) | EMethod.Neither | (0 << 14),
            FsctlExtendVolume = (EFileDevice.FileSystem << 16) | (60 << 2) | EMethod.Buffered | (0 << 14),
            FsctlQueryUsnJournal = (EFileDevice.FileSystem << 16) | (61 << 2) | EMethod.Buffered | (0 << 14),
            FsctlDeleteUsnJournal = (EFileDevice.FileSystem << 16) | (62 << 2) | EMethod.Buffered | (0 << 14),
            FsctlMarkHandle = (EFileDevice.FileSystem << 16) | (63 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSisCopyFile = (EFileDevice.FileSystem << 16) | (64 << 2) | EMethod.Buffered | (0 << 14),
            FsctlSisLinkFiles = (EFileDevice.FileSystem << 16) | (65 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            FsctlHsmMsg = (EFileDevice.FileSystem << 16) | (66 << 2) | EMethod.Buffered | ((FileAccess.Read | FileAccess.Write) << 14),
            FsctlNssControl = (EFileDevice.FileSystem << 16) | (67 << 2) | EMethod.Buffered | (FileAccess.Write << 14),
            FsctlHsmData = (EFileDevice.FileSystem << 16) | (68 << 2) | EMethod.Neither | ((FileAccess.Read | FileAccess.Write) << 14),
            FsctlRecallFile = (EFileDevice.FileSystem << 16) | (69 << 2) | EMethod.Neither | (0 << 14),
            FsctlNssRcontrol = (EFileDevice.FileSystem << 16) | (70 << 2) | EMethod.Buffered | (FileAccess.Read << 14)
        }

        public static unsafe bool DeviceIoControl(
            SafeFileHandle hDevice,
            EIOControlCode dwIoControlCode,
            ref byte[] InBuffer,
            ref byte[] OutBuffer,
            ref int pBytesReturned)
        {
            bool ret;

            //We need this helper in case that the byte-Array has zero length
            byte[] _InBuffer, _OutBuffer;

            if (InBuffer.Length == 0)
                _InBuffer = new byte[1];
            else
                _InBuffer = InBuffer;
            if (OutBuffer.Length == 0)
                _OutBuffer = new byte[4];
            else
                _OutBuffer = OutBuffer;

            // Now we fix the arrays so that the DeviceIO function always have fixed
            fixed (byte* inp = &_InBuffer[0])
            {
                fixed (byte* outp = &_OutBuffer[0])
                {
                    ret = DeviceIO_Unmanaged.DeviceIoControl(hDevice, dwIoControlCode, (IntPtr)inp, InBuffer.Length, (IntPtr)outp, OutBuffer.Length, ref pBytesReturned, IntPtr.Zero);
                }
            }

            if (pBytesReturned > OutBuffer.Length)
                return false;

            return ret;
        }


        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            EIOControlCode dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            IntPtr lpOverlapped //[In] ref NativeOverlapped lpOverlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
            IntPtr template);

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            Write_Through = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(SafeFileHandle hObject);
    }
}