using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Extensions
{
  public static class BigIntegerExtension
  {
    public static bool IsProbablePrime(this BigInteger source, int certainty)
    {
      if (source == 2 || source == 3)
        return true;
      if (source < 2 || source % 2 == 0)
        return false;

      BigInteger d = source - 1;
      int s = 0;

      while (d % 2 == 0)
      {
        d /= 2;
        s += 1;
      }

      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
        byte[] bytes = new byte[source.ToByteArray().LongLength];
        BigInteger a;

        for (int i = 0; i < certainty; i++)
        {
          do
          {
            rng.GetBytes(bytes);
            a = new BigInteger(bytes);
          }
          while (a < 2 || a >= source - 2);

          BigInteger x = BigInteger.ModPow(a, d, source);
          if (x == 1 || x == source - 1)
            continue;

          for (int r = 1; r < s; r++)
          {
            x = BigInteger.ModPow(x, 2, source);
            if (x == 1)
              return false;
            if (x == source - 1)
              break;
          }

          if (x != source - 1)
            return false;
        }

        return true;
      }
    }

    public static ulong ToUInt64(this BigInteger source)
    {
      byte[] contents = source.ToByteArray();
      if (contents.Length > 8)
        throw new ArgumentException();

      ulong ret = 0;
      for(int i = 0; i < contents.Length; i++)
      {
        ret = ret | ((ulong)contents[i] << (8 * i));
      }

      return ret;
    }
  }
}
