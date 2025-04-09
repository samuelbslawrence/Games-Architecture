using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace MemoryLayout
{
    struct SampleStruct
    {
        public short s1;
        public int i1;
        public char c1;
        public int i2;
     }

    class Program
    {
        static void Main(string[] args)
        {
            SampleStruct sampleStruct = new SampleStruct();
            sampleStruct.s1 = 10;
            sampleStruct.i1 = 274854959;
            sampleStruct.c1 = 'd';
            sampleStruct.i2 = 38384995;

            Console.WriteLine("Size of short = " + Marshal.SizeOf(sampleStruct.s1) + " bytes");
            Console.WriteLine("Size of int = " + Marshal.SizeOf(sampleStruct.i1) + " bytes");
            Console.WriteLine("Size of char = " + Marshal.SizeOf(sampleStruct.c1) + " bytes");
            Console.WriteLine("Size of int = " + Marshal.SizeOf(sampleStruct.i2) + " bytes");
            Console.WriteLine("\nSize of struct = " + Marshal.SizeOf(sampleStruct) + " bytes");

            Console.WriteLine("\nPress any key to finish");
            Console.ReadKey();
        }
    }
}
