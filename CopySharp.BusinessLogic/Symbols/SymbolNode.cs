using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CopySharp.BusinessLogic.Fingerprinting;

namespace CopySharp.BusinessLogic.Symbols
{
  public abstract class SymbolNode : IEnumerable<SymbolNode>, IFingerprintableVertex
  {
    protected ISymbol m_symbol;
    protected SymbolNode m_parent;
    protected List<SymbolNode> m_children;
    protected bool m_isDeclaration;

    public bool IsDeclaration
    {
      get { return m_isDeclaration; }
      internal set { m_isDeclaration = value; }
    }

    public ISymbol InnerSymbol
    {
      get { return m_symbol; }
    }

    public SymbolNode Parent
    {
      get { return m_parent; }
      internal set { m_parent = value; }
    }

    internal List<SymbolNode> Children
    {
      get { return m_children; }
    }

    public int GetDepth()
    {
      int d = 0;
      SymbolNode node = this;
      while (node.Parent != null)
      {
        node = node.Parent;
        d++;
      }
      return d;
    }

    public abstract uint GetLabel();

    public IEnumerable<SymbolNode> GetDescendants()
    {
      var builder = ImmutableArray.CreateBuilder<SymbolNode>();
      builder.AddRange(m_children);
      foreach (SymbolNode node in m_children)
      {
        builder.AddRange(node.GetDescendants());
      }
      return builder.MoveToImmutable();
    }

    public IEnumerable<SymbolNode> GetChildren()
    {
      return m_children.ToImmutableArray<SymbolNode>();
    }

    public IEnumerable<SymbolNode> GetChildrenAndSelf()
    {
      IEnumerable<SymbolNode> children = GetChildren();
      if (children == null)
      {
        return ImmutableArray.Create<SymbolNode>(this);
      }
      else
      {
        return children.Union(ImmutableArray.Create<SymbolNode>(this));
      }
    }

    public IEnumerable<SymbolNode> GetDescendantsAndSelf()
    {
      IEnumerable<SymbolNode> descendants = GetDescendants();
      if (descendants == null)
      {
        return ImmutableArray.Create<SymbolNode>(this);
      }
      else
      {
        return descendants.Union(ImmutableArray.Create<SymbolNode>(this));
      }
    }

    public IEnumerator<SymbolNode> GetEnumerator()
    {
      return new SymbolNodeEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    internal SymbolNode(ISymbol symbol, SymbolNode parent = null, IEnumerable<SymbolNode> children = null)
    {
      m_symbol = symbol;
      m_parent = parent;
      m_children = (children == null) ? new List<SymbolNode>() : children.ToList<SymbolNode>();

      if (m_parent != null)
        m_parent.Children.Add(this);

      foreach (SymbolNode c in m_children)
      {
        c.Parent = this;
      }
    }
  }
}
