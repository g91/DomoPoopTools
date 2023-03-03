// See https://aka.msusing System;

class Program
{
    static void Main(string[] args)
    {
        ushort[] program = new ushort[] {
            0x1001, // LDA 1
            0x3002, // ADD 2
            0x2003, // STA 3
            0xF000  // RET
        };

        Emulator8808 emulator = new Emulator8808();
        emulator.LoadProgram(program, 0);
        emulator.Run();
    }
}


class Emulator8808
{
    private ushort[] memory;  // Memory of 64KB
    private ushort ip;        // Instruction Pointer
    private ushort sp;        // Stack Pointer
    private ushort flags;     // Flags Register
    private ushort acc;       // Accumulator Register

    public Emulator8808()
    {
        memory = new ushort[65536];
        ip = 0;
        sp = 0xFFFF;
        flags = 0;
        acc = 0;
    }

    public void Run()
    {
        while (true)
        {
            ushort opcode = Fetch();     // Fetch next instruction
            DecodeAndExecute(opcode);    // Decode and execute instruction
        }
    }

    private ushort Fetch()
    {
        ushort opcode = memory[ip];
        ip++;
        return opcode;
    }

    private void DecodeAndExecute(ushort opcode)
    {
        ushort op = (ushort)(opcode >> 12);     // Extract opcode from instruction
        ushort data = (ushort)(opcode & 0xFFF); // Extract data from instruction

        switch (op)
        {
            case 0x0:   // NOP
                break;
            case 0x1:   // LDA
                acc = memory[data];
                break;
            case 0x2:   // STA
                memory[data] = acc;
                break;
            case 0x3:   // ADD
                acc += memory[data];
                UpdateFlags();
                break;
            case 0x4:   // SUB
                acc -= memory[data];
                UpdateFlags();
                break;
            case 0x5:   // MUL
                acc *= memory[data];
                UpdateFlags();
                break;
            case 0x6:   // DIV
                if (memory[data] == 0)
                {
                    flags |= 0x1;  // Set divide-by-zero flag
                }
                else
                {
                    acc /= memory[data];
                    UpdateFlags();
                }
                break;
            case 0x7:   // AND
                acc &= memory[data];
                UpdateFlags();
                break;
            case 0x8:   // OR
                acc |= memory[data];
                UpdateFlags();
                break;
            case 0x9:   // XOR
                acc ^= memory[data];
                UpdateFlags();
                break;
            case 0xA:   // NOT
                acc = (ushort)~acc;
                UpdateFlags();
                break;
            case 0xB:   // SHL
                acc <<= 1;
                UpdateFlags();
                break;
            case 0xC:   // SHR
                acc >>= 1;
                UpdateFlags();
                break;
            case 0xD:   // PUSH
                memory[sp] = acc;
                sp--;
                break;
            case 0xE:   // POP
                sp++;
                acc = memory[sp];
                break;
            case 0xF:   // RET
                sp++;
                ip = memory[sp];
                break;
            default:
                Console.WriteLine("Invalid opcode");
                break;
        }
    }

    private void UpdateFlags()
    {
        flags &= 0xFFFE;  // Clear overflow flag
        if ((short)acc < 0)
        {
            flags |= 0x2;  // Set negative flag
        }
        else if (acc == 0)
        {
            flags |= 0x4;  // Set zero flag
        }
        if (acc > ushort.MaxValue)
        {
            flags |= 0x1;  // Set overflow flag
        }
    }

    public void LoadProgram(ushort[] program, ushort startAddress)
    {
        Array.Copy(program, 0, memory, startAddress, program.Length);
        ip = startAddress;
    }
}