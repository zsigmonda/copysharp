using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopySharp.BusinessLogic.Symbols
{
  public class SymbolNodeEnumerator : IEnumerator<SymbolNode>
  {
    private SymbolNode m_root;
    private SymbolNode m_node;
    private Stack<int> m_position;
    private SymbolNode m_current;
    
    public SymbolNode Current
    {
      get
      {
        return m_current;
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
      //enumerálás:
      //while (en.MoveNext()) { yield return en.Current; }
      
      if (m_node == m_root.Parent)
        return false;

      m_current = m_node;

      //Amíg az utolsó gyermeken állunk, addig jövünk felfelé. Vége, hogyha kiléptünk a gyökérelemből.
      while (m_node != m_root.Parent && m_node.Children.Count == m_position.Peek()+1)
      {
        m_node = m_node.Parent;
        m_position.Pop();
      }

      //Lehet, hogy bejártuk az egész fát...
      if (m_node != m_root.Parent)
      {
        //Elmozdulunk oldalra - lehet hova, hiszen fentebb szűrtünk.
        int childIdx = m_position.Pop();
        m_position.Push(++childIdx);

        if (m_node.Children.Count > childIdx)
        {
          m_node = m_node.Children[childIdx];
          m_position.Push(-1);
        }
      }

      return true;
    }

    public void Reset()
    {
      m_current = null;
      m_node = m_root;
      m_position.Clear();
      m_position.Push(-1);
    }

    public SymbolNodeEnumerator(SymbolNode root)
    {
      if (root == null)
        throw new ArgumentException(nameof(root));

      m_root = root;
      m_position = new Stack<int>();

      Reset();
    }
  }
}
