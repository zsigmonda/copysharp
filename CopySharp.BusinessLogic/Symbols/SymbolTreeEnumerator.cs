using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class SymbolTreeEnumerator : IEnumerator<SymbolNode>
  {
    private int m_index;
    private ImmutableArray<SymbolNode> m_cache;

    public SymbolNode Current
    {
      get
      {
        try
        {
          return m_cache[m_index];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }

    object IEnumerator.Current
    {
      get
      {
        return Current;
      }
    }

    public void Dispose()
    {
      return;
    }

    public bool MoveNext()
    {
      m_index++;
      return (m_index < m_cache.Length);
    }

    public void Reset()
    {
      m_index = -1;
    }

    public SymbolTreeEnumerator(ImmutableArray<SymbolNode> cache)
    {
      m_index = -1;
      m_cache = cache;
    }
  }
}
