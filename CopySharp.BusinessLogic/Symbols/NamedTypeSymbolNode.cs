using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class NamedTypeSymbolNode : SymbolNode
  {
    protected uint m_hashValue = 0;

    public INamedTypeSymbol InnerSymbol
    {
      get { return (INamedTypeSymbol)base.InnerSymbol; }
    }

    internal NamedTypeSymbolNode(INamedTypeSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      if (symbol != null)
      {
        int hash = ((int)symbol.Kind << 24) +
          (symbol.IsAbstract ? (1 << 23) : 0) +
          (symbol.IsScriptClass ? (1 << 23) : 0) +
          (symbol.IsSealed ? (1 << 22) : 0) +
          (symbol.IsStatic ? (1 << 21) : 0) +
          (symbol.IsVirtual ? (1 << 20) : 0) +
          (symbol.MightContainExtensionMethods ? (1 << 19) : 0) +
          (symbol.IsGenericType ? (1 << 18) : 0) +
          ((symbol.Arity % 32) << 13);
        m_hashValue = (uint)hash;
      }
    }

    public override uint GetLabel()
    {
      return m_hashValue;
    }
  }
}
