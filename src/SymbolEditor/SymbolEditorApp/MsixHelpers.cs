using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SymbolEditorApp
{
    internal static class MsixHelpers
    {
        public static PackageId GetPackageId()
        {
            byte[] buffer = new byte[1024];
            int size = 1024;
            var result = GetCurrentPackageId(ref size, buffer);
            if (result == 0)
            {
                PackageId id = new PackageId();
                using (var br = new BinaryReader(new MemoryStream(buffer)))
                {
                    br.BaseStream.Seek(4, SeekOrigin.Begin); // Skip reserved
                    id.Architecture = (PackageArchitecture)br.ReadUInt32();
                    var revision = br.ReadUInt16();
                    var build = br.ReadUInt16();
                    var minor = br.ReadUInt16();
                    var major = br.ReadUInt16();
                    id.Version = new Version(major, minor, build, revision);
                    id.Name = Marshal.PtrToStringUni(new IntPtr(br.ReadInt64()));
                    id.PublisherId = Marshal.PtrToStringUni(new IntPtr(br.ReadInt64()));
                    id.ResourceId = Marshal.PtrToStringUni(new IntPtr(br.ReadInt64()));
                    id.PublisherId = Marshal.PtrToStringUni(new IntPtr(br.ReadInt64()));
                }
                return id;
            }
            return null;
        }
        
        public class PackageId
        {
            public UInt32 Reserved { get; set; }
            public PackageArchitecture Architecture { get; set; }
            public Version Version { get; set; }
            public string Name { get; set; }
            public string Publisher { get; set; }
            public string ResourceId { get; set; }
            public string PublisherId { get; set; }
        }
        public enum PackageArchitecture : UInt32
        {
            x86 = 0,
            ARM = 5, 
            x64 = 9, 
            Neutral = 11,
            ARM64 = 12
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetCurrentPackageId(ref Int32 pBufferLength, byte[] pBuffer);
    }
}
