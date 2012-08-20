using System;

public class labeltree
{
    labelitem root;

    public bool exists(string label)
    {
        labelitem current;
        int branch;

        current = root;

        while (current != null)
        {
            branch = String.Compare(label, current.label);
            if (branch == 0)
            {
                return true;
            }
            else if (branch < 0)
            {
                current = current.l;
            }
            else
            {
                current = current.r;
            }
        }

        return false;
    }

    public void add(string label, ushort address)
    {
        labelitem current;

        if (root == null)
        {
            root = new labelitem(label, address);
            return;
        }

        current = root;

        while (true)
        {
            if (String.Compare(label, current.label) < 0)
            {
                if (current.l == null)
                {
                    current.l = new labelitem(label, address);
                    return;
                }

                current = current.l;
            }
            else
            {
                if(current.r == null)
                {
                    current.r = new labelitem(label, address);
                    return;
                }

                current = current.r;
            }
        }
    }

    public ushort recall(string label)
    {
        labelitem current;
        int branch;

        current = root;

        do
        {
            branch = String.Compare(label, current.label);

            if (branch < 0)
            {
                current = current.l;
            }
            else if (branch > 0)
            {
                current = current.r;
            }
        }
        while (branch != 0);

        return current.address;
    }

    public string printlabels()
    {
        return printlabelitem(root) + ".";
    }

    private string printlabelitem(labelitem start)
    {
        if (start == null) { return ""; }

        return printlabelitem(start.l) + " " + start.label + " " + printlabelitem(start.r) + "";
    }
}

public class labelitem
{
    public labelitem l;
    public labelitem r;
    public string label;
    public ushort address;

    public labelitem(string s, ushort u)
    {
        label = s;
        address = u;
    }
}