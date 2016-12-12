using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Fingerprinting
{
  public interface IFingerprintableGraph
  {
    IEnumerable<IList<IFingerprintableVertex>> GetAllPathsWithMaximumLength(int maxLength);
  }
}
