class MACHINE
{
    public ushort[] mem;
    private ushort[] queue;
    private byte qp;
    private ushort[] reg;
    private ushort[] spec;
    private hdwr[] device;

    public MACHINE(ushort[] m)
    {
        mem = m;
        reg = new ushort[14];
        spec = new ushort[32];
        queue =  new ushort[256];
        qp = 0;
        device = new hdwr[65536];

        device[0] = new counter();

        for (int i = 0; i < 65536; i++)
        {
            if (device[i] != null)
            {
                device[i].connect(reg, mem);
            }
        }
    }

    public void NextOp()
    {
        if ((reg[12] & 0x0080) > 0)
        {
            if (((reg[12] & 0xffbf) > 0) && QA())
            {
                wr((byte)14, reg[11]);
                wr((byte)14, QN());
                reg[11] = spec[0];
            }
        }
        else
        {
            QC();
        }

        ushort iav;
        ushort ibv;
        uint dwv;
        ushort op = mem[reg[11]];
        reg[11] = (ushort)(reg[11] + 1);
        byte opcode = (byte) (op >> 10);
        byte ia = (byte) ((op >> 5) & 0x1f);
        byte ib = (byte) (op & 0x1f);

        switch (opcode)
        {
            case 0: //set
                wr(ib, rr(ia));
                break;
            case 1: //exch
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                wr(ib, iav);
                wr(ia, ibv);
                break;
            case 2: //twoc
                iav = rr(ia);
                iav = (ushort)(iav ^ 0xffff);
                iav = (ushort)(iav + 1);
                wr(ib, iav);
                break;
            case 3: //add
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                dwv = iav;
                dwv = (uint)(dwv + ibv);
                reg[12] = (ushort)(reg[12] & 0x1fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                iav = (ushort)(dwv & 0xffff);
                wr(ib, iav);
                break;
            case 4: //addc
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                dwv = iav;
                dwv = (uint)(dwv + ibv + ((reg[12] & 0x8000) > 0 ? 1 : 0));
                reg[12] = (ushort)(reg[12] & 0x1fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                iav = (ushort)(dwv & 0xffff);
                wr(ib, iav);
                break;
            case 5: //sub
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                dwv = iav;
                dwv = (uint)(dwv - ibv);
                reg[12] = (ushort)(reg[12] & 0x1fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                iav = (ushort)(dwv & 0xffff);
                wr(ib, iav);
                break;
            case 6: //subc
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                dwv = iav;
                dwv = (uint)(dwv - ibv - ((reg[12] & 0x8000) > 0 ? 1 : 0));
                reg[12] = (ushort)(reg[12] & 0x1fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                iav = (ushort)(dwv & 0xffff);
                wr(ib, iav);
                break;
            case 7: //mul
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                dwv = iav;
                dwv = dwv * ibv;
                reg[12] = (ushort)(reg[12] & 0x1fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                iav = (ushort)(dwv & 0x0000ffff);
                ibv = (ushort)(dwv >> 16 & 0x0000ffff);
                wr(ia, iav);
                wr(ib, ibv);
                break;
            case 8: //div
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                if(ibv != 0)
                {
                    dwv = (uint)(iav << 16);
                    dwv = (uint)(dwv / ibv);
                    reg[12] = (ushort)(reg[12] & 0x1fff);
                    reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                    reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                    ibv = (ushort)(dwv & 0x0000ffff);
                    iav = (ushort)(dwv >> 16 & 0x0000ffff);
                    wr(ia, iav);
                    wr(ib, ibv);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] | 0x2000);
                }
                break;
            case 9: //xor
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                ibv = (ushort)(iav ^ ibv);
                reg[12] = (ushort)(reg[12] & 0xbfff);
                reg[12] = (ushort)(reg[12] | ((ibv) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                wr(ib, ibv);
                break;
            case 10: //and
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                ibv = (ushort)(iav & ibv);
                reg[12] = (ushort)(reg[12] & 0xbfff);
                reg[12] = (ushort)(reg[12] | ((ibv) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                wr(ib, ibv);
                break;
            case 11: //or
                iav = rr(ia);
                ibv = rr(ib);
                cmp(iav, ibv);
                ibv = (ushort)(iav | ibv);
                reg[12] = (ushort)(reg[12] & 0xbfff);
                reg[12] = (ushort)(reg[12] | ((ibv) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                wr(ib, ibv);
                break;
            case 12: //inc
                dwv = rr(ia);
                dwv = dwv + 1;
                reg[12] = (ushort)(reg[12] & 0x3fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                ibv = (ushort)(dwv & 0xffff);
                wr(ib, ibv);
                break;
            case 13: //dec
                dwv = rr(ia);
                dwv = dwv + 1;
                reg[12] = (ushort)(reg[12] & 0x3fff);
                reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                reg[12] = (ushort)(reg[12] | ((dwv & 0x0000ffff) > 0 ? (ushort)0x0000 : (ushort)0x4000));
                ibv = (ushort)(dwv & 0xffff);
                wr(ib, ibv);
                break;
            case 14: //sti
                iav = rr(ia);
                ibv = rr(ib);
                mem[ibv] = mem[iav];
                iav = (ushort)(iav + 1);
                ibv = (ushort)(ibv + 1);
                wr(ia, iav);
                wr(ib, ibv);
                break;
            case 15: //std
                iav = rr(ia);
                ibv = rr(ib);
                mem[ibv] = mem[iav];
                iav = (ushort)(iav - 1);
                ibv = (ushort)(ibv - 1);
                wr(ia, iav);
                wr(ib, ibv);
                break;
            case 16: //rol
                dwv = rr(ia);
                dwv = dwv << 1;
                if ((dwv & 0xffff0000) > 0)
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                    reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                }
                if ((dwv & 0x0000ffff) == 0)
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                    reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                }
                ibv = (ushort)(dwv & 0xffff);
                wr(ib, ibv);
                break;
            case 17: //ror
                dwv = rr(ia);
                if ((dwv & 0x00000001) > 0)
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                    reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                }
                if ((dwv & 0x0001fffe) == 0)
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                    reg[12] = (ushort)(reg[12] | ((dwv & 0xffff0000) > 0 ? (ushort)0x8000 : (ushort)0x0000));
                }
                ibv = (ushort)((dwv>>1) & 0xffff);
                wr(ib, ibv);
                break;
            case 18: //rolc
                dwv = rr(ia);
                dwv = dwv << 1;
                if ((reg[12] & 0x8000) > 0)
                {
                    dwv = dwv | 0x00000001;
                }
                if ((dwv & 0xffff0000) > 0)
                {
                    reg[12] = (ushort)(reg[12] | 0x8000);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                }
                ibv = (ushort)(dwv & 0xffff);
                reg[12] = (ibv > 0) ? (ushort)(reg[12] | 0x4000) : (ushort)(reg[12] & 0xbfff);
                wr(ib, ibv);
                break;
            case 19: //rorc
                dwv = rr(ia);
                dwv = dwv << 1;
                if ((reg[12] & 0x8000) > 0)
                {
                    dwv = dwv | 0x00010000;
                }
                if ((dwv & 0x00000001) > 0)
                {
                    reg[12] = (ushort)(reg[12] | 0x8000);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                }
                ibv = (ushort)((dwv >> 1) & 0xffff);
                reg[12] = (ibv > 0) ? (ushort)(reg[12] | 0x4000) : (ushort)(reg[12] & 0xbfff);
                wr(ib, ibv);
                break;
            case 20: //shl
                dwv = rr(ia);
                dwv = dwv << 1;
                if ((dwv & 0x00010000) > 0)
                {
                    reg[12] = (ushort)(reg[12] | 0x8000);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                }
                ibv = (ushort)dwv;
                reg[12] = (ibv > 0) ? (ushort)(reg[12] | 0x4000) : (ushort)(reg[12] & 0xbfff);
                wr(ib, ibv);
                break;
            case 21: //shr
                dwv = rr(ia);
                dwv = dwv << 1;
                if ((dwv & 0x00000001) > 0)
                {
                    reg[12] = (ushort)(reg[12] | 0x8000);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                }
                ibv = (ushort)dwv;
                reg[12] = (ibv > 0) ? (ushort)(reg[12] | 0x4000) : (ushort)(reg[12] & 0xbfff);
                wr(ib, ibv);
                break;
            case 22: //shra
                dwv = rr(ia);
                if ((dwv & 0x00000001) > 0)
                {
                    reg[12] = (ushort)(reg[12] | 0x8000);
                }
                else
                {
                    reg[12] = (ushort)(reg[12] & 0x7fff);
                }
                ibv = (ushort)((dwv >> 1) & (dwv & 0x00008000));
                reg[12] = (ibv > 0) ? (ushort)(reg[12] | 0x4000) : (ushort)(reg[12] & 0xbfff);
                wr(ib, ibv);
                break;
            case 23: //IFSPC
                iav = rr(ia);
                ibv = rr(ib);
                if ((reg[12] & iav) > 0)
                {
                    reg[11] = ibv;
                }
                break;
            case 24: //IFAPC
                iav = rr(ia);
                ibv = rr(ib);
                if ((reg[12] & iav) > 0)
                {
                    reg[11] = (ushort)(reg[11] + ibv);
                }
                break;
            case 25: //call
                iav = rr(ia);
                wr((byte)14, reg[11]);
                reg[11] = iav;
                wr(ib, iav);
                break;
            case 26: //int
                iav = rr(ia);
                QI(iav);
                wr(ib, iav);
                break;
            case 27: //iret
                reg[12] = (ushort)(reg[12] | 0x0040);
                wr(ib, rr(ia));
                break;
            case 28: //gspec
                wr(ib, spec[ia]);
                break;
            case 29: //sspec
                spec[ib] = rr(ia);
                break;
            case 30: //hdwr
                iav = rr(ia);
                ibv = rr(ib);
                HDWR(iav, ibv);
                break;
            default:
                break;
        }
    }

    private ushort rr(byte b)
    {
        if ((b & 0x10) > 0)
        {
            return mem[(ushort)(rr((byte)(b & 0x0f)) + reg[10])];
        }
        else
        {
            switch (b)
            {
                case 14:
                    return mem[reg[13]++];
                case 15:
                    return mem[reg[11]++];
                default:
                    return reg[b];
            }
        }
    }

    private void wr(byte b, ushort i)
    {
        if ((b & 0x10) > 0)
        {
            mem[(ushort)(rr((byte)(b & 0x0f)) + reg[10])] = i;
        }
        else
        {
            switch (b)
            {
                case 14:
                    mem[--reg[13]] = i;
                    break;
                case 15:
                    break;
                default:
                    reg[b] = i;
                    break;
            }
        }
    }

    private void cmp(ushort a, ushort b)
    {
        reg[12] = (ushort)(reg[12] & 0xc1ff);
        if (a > b) { reg[12] = (ushort)(reg[12] | 0x1000); }
        if ((short)a > (short)b) { reg[12] = (ushort)(reg[12] | 0x0800); }
        if (a == b) { reg[12] = (ushort)(reg[12] | 0x0400); }
        if ((short)a < (short)b) { reg[12] = (ushort)(reg[12] | 0x0200); }
        if (a < b) { reg[12] = (ushort)(reg[12] | 0x0100); }
    }

    private void HDWR(ushort num, ushort msg)
    {
        device[num].signal(msg);
    }

    private void hdwrupdt()
    {
        ushort iqn;
        for (int i = 0; i < 65536; i++)
        {
            if (device[i] != null)
            {
                iqn = device[i].update();
                if(iqn > 0) QI(iqn);
            }
        }
    }

    public void QI(ushort i)
    {
        if (qp < 255 && (reg[12] & 0x0080) > 0)
        {
            qp++;
            queue[qp] = i;
        }
    }

    private void QC()
    {
        qp = 0;
    }

    private bool QA()
    {
        if (qp > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private ushort QN()
    {
        return queue[qp--];
    }
}
