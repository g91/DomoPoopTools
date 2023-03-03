using System;

namespace DisassemblerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the byte array containing the hex code to disassemble
            byte[] code = new byte[] { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x08, 0x8D, 0x45, 0xF8, 0x50, 0x8B, 0x45, 0x08, 0xC7, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE8, 0xFC, 0xFF, 0xFF, 0xFF, 0x83, 0xC4, 0x04, 0x8B, 0xE5, 0x5D, 0xC3 };

            // Disassemble the code and display the assembly language output
            Disassemble(code);

            // Wait for the user to press a key before exiting
            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void Disassemble(byte[] code)
        {
            int offset = 0;

            while (offset < code.Length)
            {
                // Read the next opcode and any associated operands
                byte opcode = code[offset++];
                string operand = "";

                if (offset < code.Length)
                {
                    operand = $"0x{BitConverter.ToString(code, offset, Math.Min(4, code.Length - offset)).Replace("-", "")}";
                    offset += 4;
                }

                // Decode the opcode and operands into assembly language
                switch (opcode)
                {
                    case 0x55:
                        Console.WriteLine("PUSH EBP");
                        break;

                    case 0x8B:
                        if ((code[offset] & 0xC0) == 0xC0)
                        {
                            Console.WriteLine($"MOV {RegName(code[offset] & 0x07)}, {RegName((code[offset] & 0x38) >> 3)}");
                            offset++;
                        }
                        else
                        {
                            Console.WriteLine($"MOV {RegName((code[offset] & 0x38) >> 3)}, [EBP-{code[offset] & 0x07}-4]");
                            offset++;
                        }
                        break;

                    case 0x83:
                        if ((code[offset] & 0xF8) == 0xE0)
                        {
                            Console.WriteLine($"SUB {RegName(code[offset] & 0x07)}, {code[offset + 1]}");
                            offset += 2;
                        }
                        else if ((code[offset] & 0xF8) == 0xE8)
                        {
                            Console.WriteLine($"SUB {RegName(code[offset] & 0x07)}, {operand}");
                            offset += 4;
                        }
                        break;

                    case 0x8D:
                        if ((code[offset] & 0xC0) == 0x40)
                        {
                            Console.WriteLine($"LEA {RegName((code[offset] & 0x38) >> 3)}, [EBP-{code[offset] & 0x07}-4]");
                            offset++;
                        }
                        else if ((code[offset] & 0xC0) == 0x80)
                        {
                            Console.WriteLine($"LEA {RegName((code[offset] & 0x38) >> 3)}, [{operand}]");
                            offset += 4;
                        }
                        break;

                    case 0x50:
                        Console.WriteLine($"PUSH {RegName(code[offset] & 0x07)}");
                        offset++;
                        break;
                    case 0xC7:
                        if ((code[offset] & 0xC0) == 0x00)
                        {
                            Console.WriteLine($"MOV DWORD PTR [{RegName((code[offset] & 0x38) >> 3)}], {operand}");
                            offset += 4;
                        }
                        break;

                    case 0xE8:
                        Console.WriteLine($"CALL {operand}");
                        offset += 4;
                        break;
                    case 0xC3:
                        Console.WriteLine("RET");
                        break;
                    default:
                        Console.WriteLine($"UNKNOWN OPCODE {opcode:X2}");
                        break;
                }
            }
        }

        static string RegName(int regNum)
        {
            switch (regNum)
            {
                case 0:
                    return "EAX";

                case 1:
                        return "ECX";

                    case 2:
                        return "EDX";

                    case 3:
                        return "EBX";

                    case 4:
                        return "ESP";

                    case 5:
                        return "EBP";

                    case 6:
                        return "ESI";

                    case 7:
                        return "EDI";

                    default:
                        return "UNKNOWN";
                }
            }
        }
    }
