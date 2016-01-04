using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FileMakerXMLGeneratorApp.Editor;
using FileMakerXMLGeneratorApp.FieldTree;

namespace FileMakerXMLGeneratorApp.UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void PreSetup()
        {
            m_nodeLookup.Clear();
            m_treeNodeLookup.Clear();
            m_treeView.Nodes.Clear();
        }

        public void PostSetup()
        {
            
        }

        public void AddNode(AbstractFieldNode node)
        {
            AbstractFieldNode parent = node.Parent;
            if (parent == null)
            {
                if (m_nodeLookup.Count != 0)
                    throw new InvalidOperationException("Cannot add a root node, a root already exists.");

                LinkedList<MenuItem> menuItems = new LinkedList<MenuItem>();
                node.GetMenuCommands(menuItems);

                TreeNode treeNode = new TreeNode(node.ToString());
                treeNode.ContextMenu = new ContextMenu(menuItems.ToArray());
                treeNode.Tag = node;

                m_treeView.Nodes.Add(treeNode);

                m_treeNodeLookup.Add(node, treeNode);
                m_nodeLookup.Add(treeNode, node);
            }
            else
            {
                LinkedList<MenuItem> menuItems = new LinkedList<MenuItem>();
                node.GetMenuCommands(menuItems);

                TreeNode treeNode = new TreeNode(node.ToString());
                treeNode.ContextMenu = new ContextMenu(menuItems.ToArray());
                treeNode.Tag = node;

                TreeNode parentTreeNode = GetTreeNode(parent);
                parentTreeNode.Nodes.Add(treeNode);
                parentTreeNode.Expand();

                m_treeNodeLookup.Add(node, treeNode);
                m_nodeLookup.Add(treeNode, node);
            }
        }

        public void ChildNodeRemoved(AbstractFieldNode parent, AbstractFieldNode child)
        {
            TreeNode childTreeNode = GetTreeNode(child);
            
            childTreeNode.Remove();
        }

        public void NodeChanged(AbstractFieldNode node)
        {
            TreeNode treeNode = GetTreeNode(node);
            if(treeNode == null)
                return;

            treeNode.Text = node.ToString();

            for (int i = 0; i < node.Count; i++)
            {
                NodeChanged(node.GetChild(i));
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            m_editorInstance = new EditorInstance(this);
        }

        private AbstractFieldNode GetNode(TreeNode treeNode)
        {
            AbstractFieldNode node;
            m_nodeLookup.TryGetValue(treeNode, out node);
            return node;
        }

        private TreeNode GetTreeNode(AbstractFieldNode node)
        {
            TreeNode treeNode;
            m_treeNodeLookup.TryGetValue(node, out treeNode);
            return treeNode;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AbstractFieldNodeControl control = GetNode(e.Node).CreateControl();

            control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            NodeChanged(m_editorInstance.Root);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastFileName))
            {
                if (!SaveDialog())
                    return;
            }

            m_editorInstance.Save(lastFileName);
        }

        private bool SaveDialog()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = @"File Maker XML (*.fmxml)|*.fmxml|All files (*.*)|*.*";
            saveDialog.DefaultExt = "fmxml";

            switch (saveDialog.ShowDialog())
            {
                case DialogResult.None:
                    return false;
                case DialogResult.OK:
                    lastFileName = saveDialog.FileName;
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.Abort:
                    return false;
                case DialogResult.Retry:
                    return false;
                case DialogResult.Ignore:
                    return false;
                case DialogResult.Yes:
                    lastFileName = saveDialog.FileName;
                    break;
                case DialogResult.No:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(SaveDialog())
            {
                m_editorInstance.Save(lastFileName);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = @"File Maker XML (*.fmxml)|*.fmxml|All files (*.*)|*.*";
            dialog.DefaultExt = "fmxml";

            switch (dialog.ShowDialog())
            {
                case DialogResult.None:
                    return;
                case DialogResult.OK:
                    lastFileName = dialog.FileName;
                    break;
                case DialogResult.Cancel:
                    return;
                case DialogResult.Abort:
                    return;
                case DialogResult.Retry:
                    return;
                case DialogResult.Ignore:
                    return;
                case DialogResult.Yes:
                    lastFileName = dialog.FileName;
                    break;
                case DialogResult.No:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            m_editorInstance = new EditorInstance(lastFileName, this);
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(m_editorInstance.Root.Compile());
        }

        private EditorInstance m_editorInstance;

        private Dictionary<TreeNode, AbstractFieldNode> m_nodeLookup = new Dictionary<TreeNode, AbstractFieldNode>();

        private Dictionary<AbstractFieldNode, TreeNode> m_treeNodeLookup = new Dictionary<AbstractFieldNode, TreeNode>();

        private string lastFileName;
    }
}
