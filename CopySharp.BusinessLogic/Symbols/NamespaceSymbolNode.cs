using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  class NamespaceSymbolNode : SymbolNode
  {
    protected uint m_hashValue = 0;

    public INamespaceSymbol InnerSymbol
    {
      get { return (INamespaceSymbol)base.InnerSymbol; }
    }

    internal NamespaceSymbolNode(INamespaceSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      if (symbol != null)
      {
        int hash = ((int)symbol.Kind << 24) + ((int)symbol.NamespaceKind << 20) + ((symbol.DeclaringSyntaxReferences.Length % 1024) << 10) + (symbol.Locations.Length % 1024);
        m_hashValue = (uint)hash;
      }
    }

    public override uint GetLabel()
    {
      return m_hashValue;
    }
  }
}
