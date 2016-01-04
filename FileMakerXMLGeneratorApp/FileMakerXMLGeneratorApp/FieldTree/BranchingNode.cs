using System.Collections.Generic;

namespace FileMakerXMLGeneratorApp.FieldTree
{
    public abstract class BranchingNode : AbstractFieldNode
    {
        public override int Count
        {
            get { return m_children.Count; }
        }

        public override AbstractFieldNode GetChild(int i)
        {
            return m_children[i];
        }

        protected override bool AddChild(int index, AbstractFieldNode newChild)
        {
            if (index < 0 || index > m_children.Count)
                return false;

            m_children.Insert(index, newChild);

            return true;
        }

        public override bool RemoveChild(int index)
        {
            if (index < 0 || index >= m_children.Count)
                return false;

            m_children.RemoveAt(index);

            return true;
        }

        private List<AbstractFieldNode> m_children = new List<AbstractFieldNode>();
    }
}