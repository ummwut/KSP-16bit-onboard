using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class loader
{
    public static void Main(string[] args)
    {
        string fn;
        int lines = 0;
        BinaryReader br;
        ushort[] memory;
        MACHINE computer;

        do
        {
            Console.WriteLine();
            Console.WriteLine("Enter the source file name: ");
            fn = Console.ReadLine();
            if (!File.Exists(fn))
            {
                Console.WriteLine("File does not exist. Enter a valid file name.");
            }
        } while (!File.Exists(fn));

        br = new BinaryReader(new FileStream(fn, FileMode.Open, FileAccess.Read));

        lines = 0;
        memory = new ushort[65536];

        while (lines < 65536)
        {
            memory[lines] = br.ReadUInt16();
            ++lines;
        }

        br.Close();

        computer = new MACHINE(memory);

        while (true) { computer.NextOp(); }
    }
}
