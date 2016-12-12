using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class EventSymbolNode : SymbolNode
  {
    public IEventSymbol InnerSymbol
    {
      get { return (IEventSymbol)base.InnerSymbol; }
    }

    internal EventSymbolNode(IEventSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      ;
    }

    public override uint GetLabel()
    {
      return (uint)this.InnerSymbol.Kind;
    }
  }
}
