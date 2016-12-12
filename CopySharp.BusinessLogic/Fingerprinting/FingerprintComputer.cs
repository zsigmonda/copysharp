using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using CopySharp.BusinessLogic.Extensions;

namespace CopySharp.BusinessLogic.Fingerprinting
{
  public class FingerprintComputer
  {
    private uint[] intFuncCoefficients;
    private uint[] hashFuncCoefficientsA;
    private uint[] hashFuncCoefficientsB;
    private ulong bigPrimeM;
    private ulong bigPrimeP;
    private int hashCount;
    private int maxPathLength;

    public FingerprintComputer(int hashCount, int maxPathLength)
    {
      this.hashCount = hashCount;
      this.maxPathLength = maxPathLength;
      intFuncCoefficients = new uint[maxPathLength];
      hashFuncCoefficientsA = new uint[hashCount];
      hashFuncCoefficientsB = new uint[hashCount];
      bigPrimeP = FindPrime(65537, 1048576);
      bigPrimeM = FindPrime(bigPrimeP, 1048576);

      Random r = new Random();
      for(int i = 0; i < maxPathLength; i++)
      {
        intFuncCoefficients[i] = (uint)r.Next((int)bigPrimeP);
      }
      for (int i = 0; i < hashCount; i++)
      {
        hashFuncCoefficientsA[i] = (uint)r.Next((int)bigPrimeM);
        hashFuncCoefficientsB[i] = (uint)r.Next((int)bigPrimeM);
      }
    }

    public GraphFingerprint Compute(IFingerprintableGraph graph)
    {
      ulong[] fp = new ulong[hashCount];
      for (int i = 0; i < fp.Length; i++)
        fp[i] = ulong.MaxValue;

      HashSet<ulong> pathInt = new HashSet<ulong>();

      var paths = graph.GetAllPathsWithMaximumLength(maxPathLength);
      foreach (var path in paths)
      {
        pathInt.Add(MapPathToInteger(path));
      }
      
      for (int i = 0; i < hashCount; i++)
      {
        fp[i] = MinHashOfSet(pathInt, i);
      }

      return new GraphFingerprint()
      {
        AssociatedGraph = graph,
        FingerprintValues = fp,
        HashCount = hashCount,
        MaximumPathLength = maxPathLength
      };
    }

    private ulong MapPathToInteger(IList<IFingerprintableVertex> path)
    {
      ulong retVal = 0;
      for(int i = 0; i < path.Count; i++)
      {
        retVal += intFuncCoefficients[i] * path[i].GetLabel();
      }
      return retVal % bigPrimeP;
    }

    private ulong MinHashOfSet(HashSet<ulong> set, int functionId)
    {
      ulong min = ulong.MaxValue;
      foreach (ulong pathInt in set)
      {
        ulong tmp = ((hashFuncCoefficientsA[functionId] * pathInt + hashFuncCoefficientsB[functionId]) % bigPrimeM);
        if (min > tmp)
          min = tmp;
      }
      return min;
    }

    private ulong FindPrime(ulong min, ulong max)
    {
      if (min > max)
        throw new ArgumentException();

      byte[] bytes = new byte[8];
      BigInteger a;
      BigInteger bMax = new BigInteger(max);
      BigInteger bMin = new BigInteger(min);

      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
        do
        {
          rng.GetBytes(bytes);
          a = new BigInteger(bytes);
          if (a.Sign < 0)
            a = BigInteger.Negate(a);

          a = BigInteger.Remainder(a, BigInteger.Subtract(bMax, bMin));
          a = BigInteger.Add(a, bMin);
        }
        while (!a.IsProbablePrime(200));
      }

      return a.ToUInt64();
    }

  }
}
