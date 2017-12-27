using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace PhotonControl.Helper
{
    public class PhotonConfiguration
    {
        // Fields
        private readonly List<Instance> instanceList = new List<Instance>();

        // Methods
        private PhotonConfiguration()
        {
        }

        public static PhotonConfiguration LoadConfiguration(string fileName)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileName);
                PhotonConfiguration configuration = new PhotonConfiguration();
                if ((document.DocumentElement != null) && (document.DocumentElement.Name == "Configuration"))
                {
                    for (int i = 0; i < document.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = document.DocumentElement.ChildNodes[i];
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            configuration.Instances.Add(new Instance((XmlElement)node));
                        }
                    }
                }
                return configuration;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("{0} {1} \r\n\r\n{2}", Program.ResourceManager.GetString("configFileNotFoundMsg"), fileName, exception.Message), Program.ResourceManager.GetString("configFileNotFoundCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return null;
            }
        }

        // Properties
        public List<Instance> Instances
        {
            get
            {
                return this.instanceList;
            }
        }

        // Nested Types
        public class Application
        {
            // Fields
            private readonly XmlElement xmlElement;

            // Methods
            public Application(XmlElement xmlElement)
            {
                this.xmlElement = xmlElement;
            }

            // Properties
            public string BaseDirectory
            {
                get
                {
                    return this.xmlElement.GetAttribute("BaseDirectory");
                }
            }

            public string Name
            {
                get
                {
                    return this.xmlElement.GetAttribute("Name");
                }
            }
        }

        public class Instance
        {
            // Fields
            private readonly List<PhotonConfiguration.Application> applicationList = new List<PhotonConfiguration.Application>();
            private readonly XmlElement xmlElement;

            // Methods
            public Instance(XmlElement element)
            {
                this.xmlElement = element;
                XmlNode node = element.SelectSingleNode("Applications");
                if (node != null)
                {
                    XmlNodeList list = node.SelectNodes("Application");
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            PhotonConfiguration.Application item = new PhotonConfiguration.Application((XmlElement)list[i]);
                            this.applicationList.Add(item);
                        }
                    }
                }
            }

            // Properties
            public List<PhotonConfiguration.Application> Applications
            {
                get
                {
                    return this.applicationList;
                }
            }

            public string DisplayName
            {
                get
                {
                    string attribute = this.xmlElement.GetAttribute("DisplayName");
                    if (!string.IsNullOrEmpty(attribute))
                    {
                        return attribute;
                    }
                    return this.Name;
                }
            }

            public string Name
            {
                get
                {
                    return this.xmlElement.Name;
                }
            }
        }
    }

}
