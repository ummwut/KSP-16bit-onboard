class counter : hdwr
{
    ushort updates;
    ushort count;
    ushort multiplier;

    public override void connect(ushort[] r, ushort[] m)
    {
        reg = r;
        mem = m;
        intchannel = 0;
        updates = 0;
        count = 0;
        multiplier = 1;
    }

    public override ushort update()
    {
        updates = (ushort)(updates + 1);
        if (updates == multiplier && multiplier != 0 && count != 0)
        {
            count = (ushort)(count - 1);
            updates = 0;
        }
        if (count == 0)
        {
            return intchannel;
        }
        return 0;
    }

    public override void signal(ushort s)
    {
        switch (s)
        {
            case 0: //reset
                intchannel = 0;
                updates = 0;
                count = 0;
                multiplier = 1;
                break;
            case 1: //set interrupt channel
                intchannel = reg[0];
                break;
            case 2: //set scaler
                updates = 0;
                multiplier = reg[0];
                break;
            case 3: //set counter
                count = reg[0];
                break;
            case 4: //set all
                updates = 0;
                count = reg[0];
                multiplier = reg[1];
                intchannel = reg[2];
                break;
            case 5: //read counter
                reg[0] = count;
                break;
            case 6: //read interrupt channel
                reg[0] = intchannel;
                break;
            default:
                break;
        }
    }
}
