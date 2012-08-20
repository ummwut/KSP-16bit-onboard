abstract class hdwr
{
    public ushort[] reg;
    public ushort[] mem;
    public ushort intchannel;

    abstract public void connect(ushort[] r, ushort[] m);

    abstract public ushort update();

    abstract public void signal(ushort s);
}
