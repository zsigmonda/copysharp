using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class FieldSymbolNode : SymbolNode
  {
    public IFieldSymbol InnerSymbol
    {
      get { return (IFieldSymbol)base.InnerSymbol; }
    }

    internal FieldSymbolNode(IFieldSymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null) : base(symbol, parent, children)
    {
      ;
    }

    public override uint GetLabel()
    {
      return (uint)this.InnerSymbol.Kind;
    }
  }
}
