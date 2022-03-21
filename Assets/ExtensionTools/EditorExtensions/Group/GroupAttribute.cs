using System;

namespace ExtensionTools
{
    public class GroupItemAttribute : Attribute
    {
        string m_Groupname;

        public string groupname
        {
            get => m_Groupname;
        }

        public GroupItemAttribute(string groupname)
        {
            m_Groupname = groupname;
        }
    }
}
