using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileMakerXMLGeneratorApp.UI;

namespace FileMakerXMLGeneratorApp.FieldTree
{
    public class FieldNodeAttribute
    {
        [Browsable(true)]
        public string Key
        {
            get { return m_key; }
            set { m_key = value; }
        }

        [Browsable(true)]
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public FieldNodeAttribute()
        {
            m_value = "";
            m_key = "";
        }

        public FieldNodeAttribute(string key, string value)
        {
            m_key = key;
            m_value = value;
        }

        public override string ToString()
        {
            string value = ConvertValueString();
            return string.Format("{0}={1}", m_key, value);
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(m_key);
            writer.Write(m_value);
        }

        public void Deserialize(BinaryReader reader)
        {
            m_key = reader.ReadString();
            m_value = reader.ReadString();
        }

        private string ConvertValueString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\\\"");

            bool inVariableName = false;
            for (int i = 0; i < m_value.Length; i++)
            {
                char c = m_value[i];
                char last = i == 0 ? '\0' : m_value[i - 1];

                if (c == '$' && last != '\\')
                {
                    if (inVariableName)
                    {
                        inVariableName = false;

                        stringBuilder.Append(" & \"");
                    }
                    else
                    {
                        inVariableName = true;

                        stringBuilder.Append("\" & ");
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            stringBuilder.Append("\\\"");

            return stringBuilder.ToString();
        }

        private string m_key;

        private string m_value;
    }

    public class FieldNode : BranchingNode
    {
        public override Type[] AllowedChildTypes
        {
            get { return new[] {typeof(FieldNode)}; }
        }

        public override string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        [Browsable(true)]
        [Category("Attributes")]
        public FieldNodeAttribute[] Attributes
        {
            get { return m_attributes.ToArray(); }
            set { m_attributes = new List<FieldNodeAttribute>(value); }
        }

        [Browsable(true)]
        [Category("Attributes")]
        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public override string Compile()
        {
            if (Count == 0)
                return string.Format("XML_WriteField(\"{0}\" ; {1} ; {2})", m_name, m_value, GenerateAttributesString());
            else
            {
                string childString = "";
                for (int i = 0; i < Count; i++)
                {
                    childString += GetChild(i).Compile();
                    if (i < Count - 1)
                        childString += " &\n";
                }

                return string.Format("XML_WriteField(\"{0}\" ; {1}\n ; {2})", m_name, childString, GenerateAttributesString());
            }
        }

        public override AbstractFieldNodeControl CreateControl()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (Count == 0)
                return string.Format("XML_WriteField(\"{0}\" ; {1} ; {2})", m_name, m_value, GenerateAttributesString());
            else
                return string.Format("XML_WriteField(\"{0}\" ; ... ; {1})", m_name, GenerateAttributesString());
        }

        private string GenerateAttributesString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\"');

            foreach (FieldNodeAttribute attribute in m_attributes)
            {
                builder.Append(attribute);
                builder.Append(' ');
            }

            if(builder.Length > 1)
                builder.Remove(builder.Length - 1, 1);
            builder.Append('\"');

            return builder.ToString();
        }

        protected override void OnSerialize(BinaryWriter writer)
        {
            writer.Write(m_name);
            writer.Write(m_value);

            writer.Write(m_attributes.Count);
            foreach (FieldNodeAttribute attribute in m_attributes)
            {
                attribute.Serialize(writer);
            }
        }

        protected override void OnDeserialize(BinaryReader reader)
        {
            m_name = reader.ReadString();
            m_value = reader.ReadString();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                FieldNodeAttribute attribute = new FieldNodeAttribute();
                attribute.Deserialize(reader);
            }
        }

        private string m_name = "";

        private string m_value = "";

        private List<FieldNodeAttribute> m_attributes = new List<FieldNodeAttribute>(); 
    }
}
