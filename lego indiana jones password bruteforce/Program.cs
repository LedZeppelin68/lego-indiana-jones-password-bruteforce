using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace lego_indiana_jones_password_bruteforce
{
    class Program
    {
        static void Main(string[] args)
        {
            uint[] crc32table = new uint[256];

            using (BinaryReader br = new BinaryReader(new FileStream("crc32.bin", FileMode.Open)))
            {
                for (int i = 0; i < 256; i++)
                {
                    crc32table[i] = br.ReadUInt32();
                }
            }

            List<uint> pass_table = new List<uint>();

            using (BinaryReader br = new BinaryReader(new FileStream("pass.bin", FileMode.Open)))
            {
                while(br.BaseStream.Position != br.BaseStream.Length)
                {
                    pass_table.Add(br.ReadUInt32());
                }
            }

            string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code = "04EM94";

            List<string> log = new List<string>();

            byte[] code2 = Encoding.ASCII.GetBytes(code);
            byte[] alpha_array = Encoding.ASCII.GetBytes(alpha);

            long total_vars = (long)Math.Pow(alpha_array.Length, code2.Length);

            Parallel.For(0, total_vars, var =>
            {
                long sample = var;
                byte[] sample_code = new byte[6];

                for (int c = 0; c < sample_code.Length; c++)
                {
                    sample_code[c] = (byte)alpha[(byte)(sample % alpha.Length)];
                    sample /= alpha.Length;
                }


                uint crc = 0;

                for (int i = 0; i < sample_code.Length; i++)
                {
                    crc = (crc << 8) ^ crc32table[(byte)((crc >> 0x18) ^ sample_code[i])];
                }

                uint enc_password = (uint)crc;

                if (pass_table.Contains(enc_password))
                {
                    string text = string.Format("Found: {0:d2} {1}", pass_table.IndexOf(enc_password), Encoding.ASCII.GetString(sample_code));

                    log.Add(text);
                    Console.WriteLine(text);
                }
            });

            log.Sort();

            File.WriteAllLines("indiana.txt", log);
        }
    }
}
