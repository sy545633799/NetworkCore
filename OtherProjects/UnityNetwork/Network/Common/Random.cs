using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 梅森旋转算法
public class MersenneTwister
{
    private const int DEFAULT_MT_SIZE = 634;

    private uint[] MT;
    private uint idx = 0;
    private uint size = 0;
    private bool isInitialized = false;

    /* Initialize the generator from a seed */
    private void msInit(int seed)
    {
        uint i;
        uint p;
        idx = 0;
        MT[0] = (uint)seed;
        for (i = 1; i < size; ++i)
        { /* loop over each other element */
            p = 1812433253 * (MT[i - 1] ^ (MT[i - 1] >> 30)) + i;
            MT[i] = (uint)(p & 0xffffffff); /* get last 32 bits of p */
        }
        isInitialized = true;
    }

    private uint msRand()
    {
        if (!isInitialized)
            return 0;

        if (idx == 0)
            msRenerate();

        uint y = MT[idx];
        y = y ^ (y >> 11);
        y = (y ^ ((y << 7) & 2636928640));
        y = (y ^ ((y << 15) & 4022730752));
        y = y ^ (y >> 18);

        idx = (idx + 1) % size; /* increment idx mod 624*/
        return y;
    }

    private void msRenerate()
    {
        uint i;
        uint y;
        var half = size / 2;
        for (i = 0; i < size; ++i)
        {
            y = (MT[i] & 0x80000000) + (MT[(i + 1) % size] & 0x7fffffff);
            MT[i] = MT[(i + half) % size] ^ (y >> 1);
            if (y % 2 != 0)
            { /* y is odd */
                MT[i] = (MT[i] ^ (2567483615));
            }
        }
    }

    public void Rseed(int seed)
    {
        msInit(seed);
    }

    public uint Rand()
    {
        return msRand();
    }

    public int Next(int min, int max)
    {
        var val = (uint)Rand();

        if (max <= 0) max = min;

        return (int)((val % (uint)(max - min + 1)) + min);
    }

    public MersenneTwister(int seed)
    {
        this.MT = new uint[DEFAULT_MT_SIZE];
        this.size = DEFAULT_MT_SIZE;

        msInit(seed);
    }

    public MersenneTwister(int seed, uint size)
    {
        this.MT = new uint[size];
        this.size = size;

        msInit(seed);
    }
}
