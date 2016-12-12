using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  class TypeSymbolNode : SymbolNode
  {
    public ITypeSymbol InnerSymbol
    {
      get { return (ITypeSymbol)base.InnerSymbol; }
    }

    internal TypeSymbolNode(ITypeSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      ;
    }

    public override uint GetLabel()
    {
      return (uint)this.InnerSymbol.Kind;
    }
  }
}
