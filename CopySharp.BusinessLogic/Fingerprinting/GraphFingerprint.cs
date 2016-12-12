using System;

namespace CopySharp.BusinessLogic.Fingerprinting
{
  public struct GraphFingerprint
  {
    public ulong[] FingerprintValues { get; set; }
    public int HashCount { get; set; }
    public int MaximumPathLength { get; set; }
    public IFingerprintableGraph AssociatedGraph { get; set; }

    public double CompareTo(GraphFingerprint another)
    {
      if (another.HashCount != this.HashCount || this.FingerprintValues == null || another.FingerprintValues == null || this.FingerprintValues.Length != another.FingerprintValues.Length)
        throw new ArgumentException();

      int cnt = 0;
      for(int i = 0; i < this.FingerprintValues.Length; i++)
      {
        if (this.FingerprintValues[i] == another.FingerprintValues[i])
          cnt++;
      }
      return ((double)cnt / this.FingerprintValues.Length);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;

      GraphFingerprint fp = (GraphFingerprint)obj;
      return this.CompareTo(fp) == 1.0;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
  }
}
