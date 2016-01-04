using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileMakerXMLGeneratorApp.UI;

namespace FileMakerXMLGeneratorApp.FieldTree
{
    public class RootNode : BranchingNode
    {
        public override Type[] AllowedChildTypes
        {
            get { return new Type[] {typeof(FieldNode)}; }
        }

        public override string Name { get { return "_root"; } set {} }

        public override AbstractFieldNodeControl CreateControl()
        {
            throw new NotImplementedException();
        }

        public override string Compile()
        {
            string compiledChildren = "";
            for (int i = 0; i < Count; i++)
            {
                compiledChildren += GetChild(i).Compile();
            }

            return compiledChildren;
        }

        protected override void OnSerialize(BinaryWriter writer)
        {
        }

        protected override void OnDeserialize(BinaryReader reader)
        {
        }
    }
}
