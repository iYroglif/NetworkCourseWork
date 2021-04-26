using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTokenRing
{
    abstract class CyclicCode
    {
        static int g_vect = 0b1011;

        static public byte[] Coding(byte[] inputVect)
        {
            byte[] res = new byte[inputVect.Length * 2];

            for (int i = 0; i < inputVect.Length; i++)
            {
                int left_vect = (inputVect[i] & 0b11110000) >> 4;
                int right_vect = inputVect[i] & 0b00001111;

                res[i * 2] = (byte)CyclicCoding(left_vect);
                res[i * 2 + 1] = (byte)CyclicCoding(right_vect);
            }
            return res;
        }
        static public (byte[], bool) Decoding(byte[] inputVect)
        {
            byte[] res = new byte[inputVect.Length / 2];
            bool hasError = false;

            for (int i = 0; i < inputVect.Length; i += 2)
            {
                int left_vect = inputVect[i];
                int right_vect = inputVect[i + 1];
                if (!hasError)
                {
                    if (!ErrorCheck(left_vect) || !ErrorCheck(right_vect))
                    {
                        hasError = true;
                    }
                }

                res[i / 2] = (byte)(((left_vect & 0b01111000) << 1) | ((right_vect & 0b01111000) >> 3));
            }
            return (res, hasError);
        }

        static int CyclicCoding(int info_vect)
        {
            int coded_vect = info_vect << 3;

            coded_vect = coded_vect | division(coded_vect);

            return coded_vect;
        }

        static bool ErrorCheck(int coded_vect)
        {
            if (division(coded_vect) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static int division(int vect)
        {
            for (int i = 6; i > 2; i--)
            {
                if (vect >> i == 1)
                {
                    vect = vect ^ (g_vect << (i - 3));
                }
            }
            return vect;
        }
    }
}
