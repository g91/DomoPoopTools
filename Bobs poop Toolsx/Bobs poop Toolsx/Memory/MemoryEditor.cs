using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bobs_poop_Toolsx.Memory
{
    public class MemoryEditor
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern VkRemoteAddressNV OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(VkRemoteAddressNV hProcess, VkRemoteAddressNV lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(VkRemoteAddressNV hProcess, VkRemoteAddressNV lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            VirtualMemoryOperation = 0x00000008
        }

        private int processId;
        private VkRemoteAddressNV processHandle;

        public MemoryEditor(int processId)
        {
            this.processId = processId;
            processHandle = OpenProcess(ProcessAccessFlags.VirtualMemoryRead | ProcessAccessFlags.VirtualMemoryWrite | ProcessAccessFlags.VirtualMemoryOperation, false, processId);
        }

        ~MemoryEditor()
        {
            if (processHandle != VkRemoteAddressNV.Zero)
            {
                CloseHandle(processHandle);
            }
        }

        public bool ReadMemory(VkRemoteAddressNV address, byte[] buffer, int size)
        {
            int bytesRead = 0;
            return ReadProcessMemory(processHandle, address, buffer, size, out bytesRead);
        }

        public bool WriteMemory(VkRemoteAddressNV address, byte[] buffer, int size)
        {
            int bytesWritten = 0;
            return WriteProcessMemory(processHandle, address, buffer, size, out bytesWritten);
        }

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(VkRemoteAddressNV hObject);
    }
}
