using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileMakerXMLGeneratorApp.UI;

namespace FileMakerXMLGeneratorApp.FieldTree
{
    public delegate void OnFieldNodeChanged(AbstractFieldNode node);

    public delegate void OnChildNodeAdded(AbstractFieldNode parent, AbstractFieldNode newChild);

    public delegate void OnChildNodeRemoved(AbstractFieldNode parent, AbstractFieldNode removedChild);

    public abstract class AbstractFieldNode
    {
        /// <summary>
        /// Gets the number of children to this node
        /// </summary>
        public abstract int Count { get; }

        public abstract Type[] AllowedChildTypes { get; }

        public abstract string Name { get; set; }

        public AbstractFieldNode Parent
        {
            get { return m_parent; }
        }

        public event OnFieldNodeChanged OnChanged;

        public event OnChildNodeAdded OnChildAdded;

        public event OnChildNodeRemoved OnChildRemoved;

        /// <summary>
        /// Gets a child at a specific index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public abstract AbstractFieldNode GetChild(int i);

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(GetType().AssemblyQualifiedName);

            OnSerialize(writer);

            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                GetChild(i).Serialize(writer);
            }
        }

        public static AbstractFieldNode Deserialize(BinaryReader reader)
        {
            string qualifiedName = reader.ReadString();
            Type type = Type.GetType(qualifiedName);

            AbstractFieldNode node = Activator.CreateInstance(type) as AbstractFieldNode;
            node.OnDeserialize(reader);

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                AbstractFieldNode child = Deserialize(reader);
                node.Add(i, child);
            }

            return node;
        }

        public bool Add(int index, AbstractFieldNode newChild)
        {
            bool isAllowed = false;
            foreach (Type type in AllowedChildTypes)
            {
                if (type.IsInstanceOfType(newChild))
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
                return false;

            if (AddChild(index, newChild))
            {
                newChild.m_parent = this;
                newChild.m_index = index;

                InvokeChildAdded(newChild);

                return true;
            }

            return false;
        }

        public bool Remove()
        {
            if (m_parent != null && m_parent.RemoveChild(m_index))
            {
                AbstractFieldNode parent = m_parent;

                m_parent = null;
                m_index = -1;

                parent.InvokeChildRemoved(this);

                return true;
            }

            return false;
        }

        public abstract bool RemoveChild(int index);

        public abstract AbstractFieldNodeControl CreateControl();

        public virtual void GetMenuCommands(LinkedList<MenuItem> items)
        {
            foreach (Type type in AllowedChildTypes)
            {
                MenuItem newMenuItem = new MenuItem(string.Format("Add {0}", type.Name), AddChildType);
                newMenuItem.Tag = type;
                items.AddLast(newMenuItem);
            }

            MenuItem removeItem = new MenuItem("Remove", OnRemove);
            items.AddLast(removeItem);
        }

        private void OnRemove(object sender, EventArgs eventArgs)
        {
            Remove();
        }

        public virtual void InvokeChange()
        {
            OnChanged?.Invoke(this);
        }

        protected virtual void InvokeChildAdded(AbstractFieldNode newchild)
        {
            OnChildAdded?.Invoke(this, newchild);
        }

        protected virtual void InvokeChildRemoved(AbstractFieldNode removedchild)
        {
            OnChildRemoved?.Invoke(this, removedchild);
        }

        public override string ToString()
        {
            return Name;
        }

        private void AddChildType(object sender, EventArgs eventArgs)
        {
            MenuItem menuItem = sender as MenuItem;
            Debug.Assert(menuItem != null, "menuItem != null");

            Type type = menuItem.Tag as Type;

            AbstractFieldNode filedNode = Activator.CreateInstance(type) as AbstractFieldNode;
            Add(Count, filedNode);
        }

        public abstract string Compile();

        protected abstract bool AddChild(int index, AbstractFieldNode newChild);

        protected abstract void OnSerialize(BinaryWriter writer);
        protected abstract void OnDeserialize(BinaryReader reader);

        private AbstractFieldNode m_parent;

        private int m_index = -1;
    }
}
