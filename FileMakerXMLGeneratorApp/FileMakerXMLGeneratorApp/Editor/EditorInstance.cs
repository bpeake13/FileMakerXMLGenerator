using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileMakerXMLGeneratorApp.FieldTree;
using FileMakerXMLGeneratorApp.UI;

namespace FileMakerXMLGeneratorApp.Editor
{
    public class EditorInstance
    {
        public RootNode Root
        {
            get { return m_root; }
        }

        public EditorInstance(MainForm winForm)
        {
            m_root = new RootNode();
            m_winForm = winForm;

            Setup();
        }

        public EditorInstance(string fileName, MainForm winForm)
        {
            using (FileStream fstream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(fstream))
                {
                    m_root = AbstractFieldNode.Deserialize(reader) as RootNode;
                }
            }

            m_winForm = winForm;

            Setup();
        }

        public void Save(string fileName)
        {
            using (FileStream fstream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (BinaryWriter writer = new BinaryWriter(fstream))
                {
                    m_root.Serialize(writer);
                }
            }
        }

        private void Setup()
        {
            GC.Collect();

            m_winForm.PreSetup();
            SetupNode(m_root);
            m_winForm.PostSetup();
        }

        private void SetupNode(AbstractFieldNode node)
        {
            node.OnChildAdded += OnNodeAdded;
            node.OnChanged += OnNodeChanged;
            node.OnChildRemoved += OnNodeRemoved;
            m_winForm.AddNode(node);

            int childCount = node.Count;
            for (int i = 0; i < childCount; i++)
            {
                SetupNode(node.GetChild(i));
            }
        }

        private void OnNodeChanged(AbstractFieldNode node)
        {
            m_winForm.NodeChanged(node);
        }

        private void OnNodeAdded(AbstractFieldNode parent, AbstractFieldNode child)
        {
            SetupNode(child);
        }

        private void OnNodeRemoved(AbstractFieldNode parent, AbstractFieldNode child)
        {
            m_winForm.ChildNodeRemoved(parent, child);
        }

        private RootNode m_root;

        private MainForm m_winForm;
    }
}
