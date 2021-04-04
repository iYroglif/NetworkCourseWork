using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTokenRing
{
    abstract class CyclicCode
    {
        byte[] g_pol = new byte[4] { 1, 0, 1, 1 };


        public byte[] Coding(byte[] double_vec)
        {
            byte[] res = new byte[double_vec.Length * 8];
            for (int i = 0; i < double_vec.Length; ++i)
            {
                byte[] f_vec = new byte[4];
                byte[] s_vec = new byte[4];
                byte temp = double_vec[i];
                for (int k = 0; k < 4; ++k)
                {
                    s_vec[k] = (byte)(temp % 2);
                    temp /= 2;
                }
                for (int k = 0; k < 4; ++k)
                {
                    f_vec[k] = (byte)(temp % 2);
                    temp /= 2;
                }

                /*
                byte f_vec = (byte)(double_vec[i] & 0b11110000);
                f_vec = (byte)(f_vec >> 4);
                byte s_vec = (byte)(double_vec[i] & 0b00001111);
                */

                CyclicCoding(f_vec).CopyTo(res, i * 14);
                CyclicCoding(s_vec).CopyTo(res, i * 14 + 7);
            }
            return res;
        }

        public byte[] CyclicCoding(byte[] vec)
        {
            byte[] c_vec = new byte[7];
            for (int i = 3; i < 7; ++i)
                c_vec[i] = vec[i - 3];
            for (int i = 6; i > 2; --i)
            {
                if (c_vec[i] == 0)
                    continue;
                for (int k = 0; k < 4; ++k)
                    c_vec[i - k] = (byte)(c_vec[i - k] ^ g_pol[3 - k]);
            }
            for (int i = 3; i < 7; ++i)
                c_vec[i] = vec[i - 3];
            return c_vec;
        }

        // ниже не сделано!!!
        public int[] Error(int[] vec, int[] err_vec)
        {
            int[] a_vec = new int[15];
            for (int i = 0; i < 15; ++i)
                a_vec[i] = vec[i] ^ err_vec[i];
            return a_vec;
        }

        public int[] Decoding(int[] vec)
        {
            int[] tmp = (int[])vec.Clone();
            for (int i = 14; i > 3; --i)
            {
                if (tmp[i] == 0)
                    continue;
                for (int k = 0; k < 5; ++k)
                    tmp[i - k] = tmp[i - k] ^ g_pol[4 - k];
            }
            int[] synd = new int[4];
            for (int i = 0; i < 4; ++i)
                synd[i] = tmp[i];
            return synd;
        }
    }
}
