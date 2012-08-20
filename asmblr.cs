using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

class asmblr
{
    public static void Main(string[] args)
    {
        asmblr a = new asmblr();
        a.startup();
        Console.WriteLine("Program ended.");
        Console.ReadLine();
    }

    labeltree tree;
    ushort[] img;
    string srcfile;
    string destfile;
    StreamReader sr;
    BinaryWriter bw;

    public asmblr()
    {
        tree = new labeltree();
        img = new ushort[65536];
    }

    ~asmblr()
    {
        Console.WriteLine("Program ended.");
        Console.ReadLine();
    }

    public void startup()
    {
        do
        {
            Console.WriteLine("Enter the source file name: ");
            srcfile = Console.ReadLine();

            if (!File.Exists(srcfile))
            {
                Console.WriteLine("File does not exist. Enter a valid file name.");
            }

            Console.WriteLine();
        }
        while (!File.Exists(srcfile));

        Console.WriteLine();
        Console.WriteLine("Enter the destination binary file name: ");
        destfile = Console.ReadLine();

        if (this.labelscanner())
        {
            if (this.assembler())
            {
                this.filemaker();
                Console.WriteLine("Finished successfully.");
            }
            else
            {
                Console.WriteLine("Assembly failed.");
            }
        }
        else
        {
            Console.WriteLine("Label Scanner failed.");
        }
    }

    private bool labelscanner()
    {
        int currentaddress = 0;
        int index;
        uint fileline = 0;
        ushort returns;
        string line;
        string[] lineitems;

        sr = new StreamReader(srcfile);

        while (sr.Peek() > -1 && currentaddress < 65536)
        {
            line = sr.ReadLine().ToUpper();
            lineitems = line.Split(new Char[] {' '});

            index = lineitems.Length;

            if (index > 0)
            {
                if (opcode(lineitems[0]) < 64)
                {
                    if (regcode(lineitems[1]) < 16)
                    {
                        if (regcode(lineitems[2]) < 16)
                        {
                            currentaddress++;
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                }
                else if(lineitems[0].Equals("DATA"))
                {
                    if (UInt16.TryParse(lineitems[1], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns))
                    {
                    }
                    else if(lineitems[1].StartsWith("&"))
                    {
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                    currentaddress++;
                }
                else if (lineitems[0].Equals("BASE"))
                {
                    if (UInt16.TryParse(lineitems[1], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns) && returns >= currentaddress)
                    {
                        currentaddress = returns;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect Base address on line {0}.", fileline);
                        return false;
                    }
                }
                else if (lineitems[0].EndsWith(":"))
                {
                    if (tree.exists(lineitems[0].TrimEnd(new char[] { ':' })))
                    {
                        Console.WriteLine("Duplicate Label on line {0}.", fileline);
                        return false;
                    }
                    else
                    {
                        tree.add(lineitems[0].TrimEnd(new char[] { ':' }), (ushort)currentaddress);
                    }

                    if (opcode(lineitems[1]) < 64)
                    {
                        if (regcode(lineitems[2]) < 16)
                        {
                            if (regcode(lineitems[3]) < 16)
                            {
                                currentaddress++;
                            }
                            else
                            {
                                Console.WriteLine("Incorrect entry on line {0}.", fileline);
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                    }
                    else if (lineitems[1].Equals("DATA"))
                    {
                        if (UInt16.TryParse(lineitems[2], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns))
                        {
                        }
                        else if (lineitems[2].StartsWith("&"))
                        {
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                        currentaddress++;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                }
            }
            fileline++;
        }
        sr.Close();

        return true;
    }

    private bool assembler()
    {
        int currentaddress = 0;
        int index;
        uint fileline = 0;
        ushort returns;
        string line;
        string[] lineitems;

        sr = new StreamReader(srcfile);

        while (sr.Peek() > -1 && currentaddress < 65536)
        {
            line = sr.ReadLine().ToUpper();
            lineitems = line.Split(new Char[] { ' ' });

            index = lineitems.Length;

            if (index > 0)
            {
                if (opcode(lineitems[0]) < 64)
                {
                    returns = opcode(lineitems[0]);

                    if (regcode(lineitems[1]) < 16)
                    {
                        returns = (ushort)(returns << 5 | regcode(lineitems[1]));

                        if (regcode(lineitems[2]) < 16)
                        {
                            returns = (ushort)(returns << 5 | regcode(lineitems[2]));
                            img[currentaddress] = returns;
                            currentaddress++;
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                }
                else if(lineitems[0].Equals("DATA"))
                {
                    if (UInt16.TryParse(lineitems[1], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns))
                    {
                        img[currentaddress] = returns;
                    }
                    else if(lineitems[1].StartsWith("&"))
                    {
                        if(tree.exists(lineitems[2].TrimStart(new char[] { '&' })))
                        {
                            img[currentaddress] = tree.recall(lineitems[2].TrimStart(new char[] { '&' }));
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                    currentaddress++;
                }
                else if (lineitems[0].Equals("BASE"))
                {
                    if (UInt16.TryParse(lineitems[1], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns) && returns >= currentaddress)
                    {
                        currentaddress = returns;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect Base address on line {0}.", fileline);
                        return false;
                    }
                }
                else if(lineitems[0].EndsWith(":"))
                {
                    if (opcode(lineitems[1]) < 64)
                    {
                        returns = opcode(lineitems[1]);

                        if (regcode(lineitems[2]) < 16)
                        {
                            returns = (ushort)(returns << 5 | regcode(lineitems[2]));

                            if (regcode(lineitems[3]) < 16)
                            {
                                returns = (ushort)(returns << 5 | regcode(lineitems[3]));
                                img[currentaddress] = returns;
                                currentaddress++;
                            }
                            else
                            {
                                Console.WriteLine("Incorrect entry on line {0}.", fileline);
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                    }
                    else if (lineitems[1].Equals("DATA"))
                    {
                        if (UInt16.TryParse(lineitems[2], NumberStyles.HexNumber, CultureInfo.CurrentCulture, out returns))
                        {
                            img[currentaddress] = returns;
                        }
                        else if (lineitems[2].StartsWith("&"))
                        {
                            if(tree.exists(lineitems[2].TrimStart(new char[] { '&' })))
                            {
                                img[currentaddress] = tree.recall(lineitems[2].TrimStart(new char[] { '&' }));
                            }
                            else
                            {
                                Console.WriteLine("Incorrect entry on line {0}.", fileline);
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incorrect entry on line {0}.", fileline);
                            return false;
                        }
                        currentaddress++;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect entry on line {0}.", fileline);
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect entry on line {0}.", fileline);
                    return false;
                }
            }

            fileline++;
        }
        sr.Close();

        return true;
    }

    private byte opcode(string s)
    {
        switch(s)
        {
            case "SET":
                return 0;
            case "EXCH":
                return 1;
            case "TWOC":
                return 2;
            case "ADD":
                return 3;
            case "ADDC":
                return 4;
            case "SUB":
                return 5;
            case "SUBC":
                return 6;
            case "MUL":
                return 7;
            case "DIV":
                return 8;
            case "XOR":
                return 9;
            case "AND":
                return 10;
            case "OR":
                return 11;
            case "INC":
                return 12;
            case "DEC":
                return 13;
            case "STI":
                return 14;
            case "STD":
                return 15;
            case "ROL":
                return 16;
            case "ROR":
                return 17;
            case "ROLC":
                return 18;
            case "RORC":
                return 19;
            case "SHL":
                return 20;
            case "SHR":
                return 21;
            case "SHRA":
                return 22;
            case "IFSET":
                return 23;
            case "IFADD":
                return 24;
            case "CALL":
                return 25;
            case "INT":
                return 26;
            case "IRET":
                return 27;
            case "GSPEC":
                return 28;
            case "SSPEC":
                return 29;
            case "HDWR":
                return 30;
            default:
                return 255;
        }
    }

    private byte regcode(string s)
    {
        switch(s)
        {
            case "R0":
                return 0;
            case "R1":
                return 1;
            case "R2":
                return 2;
            case "R3":
                return 3;
            case "R4":
                return 4;
            case "R5":
                return 5;
            case "R6":
                return 6;
            case "R7":
                return 7;
            case "R8":
                return 8;
            case "R9":
                return 9;
            case "IB":
                return 10;
            case "PC":
                return 11;
            case "FL":
                return 12;
            case "SP":
                return 13;
            case "ST":
                return 14;
            case "NW":
                return 15;
            default:
                return 255;
        }
    }

    private void filemaker()
    {
        bw = new BinaryWriter(new FileStream(destfile, FileMode.Create));
        int index = 0;

        while (index < 65536)
        {
            bw.Write(img[index]);
            ++index;
        }

        bw.Close();
    }
}
