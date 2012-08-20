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
        string[] s = new string[10];
        Console.WriteLine(s.Length);
    }

    private class labeltree
    {
        labelitem root;

        public bool exists(string label)
        {
            labelitem current = root;
            int branch;

            if (root == null)
            {
                return false;
            }
            else
            {
                do
                {
                    branch = String.Compare(label, current.label);
                    if (branch > 0)
                    {
                        current = current.r;
                    }
                    else if (branch < 0)
                    {
                        current = current.l;
                    }
                    else
                    {
                        return true;
                    }

                } while (current != null && branch != 0);
            }

            return false;
        }

        public bool added(string l, ushort n)
        {
            labelitem current = root;
            bool successful = false;
            int branch;

            if (root == null)
            {
                root = new labelitem(l, n);

                return true;
            }
            else if (exists(l))
            {
                return false;
            }
            else
            {
                do
                {
                    branch = String.Compare(l, current.label);

                    if (branch > 0)
                    {
                        if (current.r == null)
                        {
                            current.r = new labelitem(l, n);
                            successful = true;
                        }
                        else
                        {
                            current = current.r;
                        }
                    }
                    else if (branch < 0)
                    {
                        if (current.l == null)
                        {
                            current.l = new labelitem(l, n);
                            successful = true;
                        }
                        else
                        {
                            current = current.l;
                        }
                    }

                } while (!successful);
                return true;
            }
        }

        //public ushort recall(string l);

        class labelitem
        {
            public string label;
            public ushort line;
            public labelitem l;
            public labelitem r;

            public labelitem(string s, ushort u)
            {
                label = s;
                line = u;
            }
        }
    }

    labeltree tree;
    ushort[] img;
    string srcfile;
    string destfile;
    ConsoleKeyInfo key;
    StreamReader sr;
    BinaryWriter bw;

    public asmblr()
    {
        tree = new labeltree();
        img = new ushort[65536];
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

        if (labelscanner())
        {
            assembler();
            filemaker();
        }
    }

    private bool labelscanner()
    {
        int currentaddress = 0;
        int index;
        uint fileline = 0;
        string line;
        string[] lineitems;

        sr = new StreamReader(srcfile);

        do
        {
            line = sr.ReadLine();
            fileline++;
            lineitems = line.Split();
            index = lineitems.Length;

                while(index >= 0)
                {

                }
        }
        while (line != null && currentaddress < 65536);

        sr.Close();

        if (currentaddress >= 65536)
        {
            Console.WriteLine("Exceeded Img capacity on line {0}", fileline);
            return false;
        }

        return true;
    }

    private void assembler()
    {

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
