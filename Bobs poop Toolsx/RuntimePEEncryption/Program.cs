using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace RuntimePEEncryption
{
    class RuntimePEEncryptor
    {
        private const int offsetToEncryptedCode = 0x1000;

        public static void EncryptPE(string inputFileName, string outputFileName)
        {
            // Load the PE file into memory as a byte array
            byte[] peFile = File.ReadAllBytes(inputFileName);

            // Encrypt the code and data sections of the PE file
            byte[] encryptedPE = EncryptPE(peFile);

            // Allocate memory for the encrypted sections
            IntPtr memoryPtr = Marshal.AllocHGlobal(encryptedPE.Length);

            // Copy the encrypted sections to the allocated memory
            Marshal.Copy(encryptedPE, 0, memoryPtr, encryptedPE.Length);

            // Use dynamic code generation to create a new executable code segment that calls the encrypted code in memory
            byte[] newCode = CreateNewCodeSegment(memoryPtr);

            // Write the new code segment to a new PE file or overwrite the existing file in memory
            WriteNewPEFile(inputFileName, outputFileName, newCode);
        }

        private static byte[] EncryptPE(byte[] peFile)
        {
            // Use AES encryption to encrypt the code and data sections of the PE file
            byte[] key = new byte[32];
            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(peFile, 0, peFile.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private static byte[] CreateNewCodeSegment(IntPtr memoryPtr)
        {
            // Define the new code segment as a dynamic method
            DynamicMethod newCode = new DynamicMethod("NewCode", typeof(void), null);

            // Get a pointer to the encrypted code in memory
            IntPtr encryptedCodePtr = new IntPtr(memoryPtr.ToInt64() + offsetToEncryptedCode);

            // Define the signature of the encrypted code as a delegate type
            delegate void EncryptedCode();

            // Create a delegate to the encrypted code in memory
            EncryptedCode encryptedCode = (EncryptedCode)Marshal.GetDelegateForFunctionPointer(encryptedCodePtr, typeof(EncryptedCode));

            // Call the encrypted code from the new code segment
            ILGenerator il = newCode.GetILGenerator();
            il.Emit(OpCodes.Calli, typeof(void));
            il.Emit(OpCodes.Ret);

            // Convert the new code segment to a byte array
            byte[] newCodeBytes = GetDynamicMethodBytes(newCode);

            return newCodeBytes;
        }

        private static byte[] GetDynamicMethodBytes(DynamicMethod method)
        {
            // Get the IL byte array of the dynamic method
            byte[] ilBytes = method.GetILGenerator().GetILAsByteArray();

            // Create a new byte array that includes the method header and the IL byte array
            byte[] methodBytes = new byte[sizeof(MethodHeader) + ilBytes.Length];
            BitConverter.GetBytes((int)CorILMethod.TinyFormat).CopyTo(methodBytes, 0);
            BitConverter.GetBytes((byte)ilBytes.Length).CopyTo(methodBytes, sizeof(MethodHeader));
            ilBytes.CopyTo(methodBytes, sizeof(MethodHeader));

            return methodBytes;
        }

        private static void WriteNewPEFile(string inputFileName, string outputFileName, byte[] newCode)
        {
            // Load the original PE file into memory as a byte array
            byte[] originalPE = File.ReadAllBytes(inputFileName);

            // Get the offset and size of the code segment in the original PE file
            int codeOffset = BitConverter.ToInt32(originalPE, 0x3C + 4 + 2 + 2 + 2 + 2 + 2 + 2);
            int codeSize = BitConverter.ToInt32(originalPE, 0x3C + 4 + 2 + 2 + 2 + 2 + 2 + 2 + 4 + 2 + 2);

            // Create a new PE file buffer with the same size as the original PE file buffer
            byte[] newPE = new byte[originalPE.Length];

            // Copy the DOS header and PE header from the original PE file buffer to the new PE file buffer
            Buffer.BlockCopy(originalPE, 0, newPE, 0, 0x3C + 4);

            // Copy the encrypted sections from the memory buffer to the new PE file buffer
            Buffer.BlockCopy(newCode, 0, newPE, codeOffset, codeSize);

            // Write the new PE file buffer to disk
            File.WriteAllBytes(outputFileName, newPE);
        }
    }
}
