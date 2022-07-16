using System.Xml;
using UnityEngine;

public class XMLDecoder
{
    Manager manager;
    public XMLDecoder(Manager manager) { this.manager = manager; }

    public void decode(byte type, string text)
    {
        // load xml text as a document
        XmlDocument document = new XmlDocument();
        document.LoadXml(text);

        // for each possible type of xml document, call decode
        switch(type)
        {
            case 0:
                decodeScene(document.FirstChild);
                break;
            case 1:
                decodeInput(document.FirstChild);
                break;
            case 2:
                decodeInstructions(document.FirstChild);
                break;
            case 3:
                decodeEntity(document.FirstChild);
                break;
            default:
                Debug.LogWarning("No xml file type " + type + " was created!");
                break;
        }
    }

    private void decodeScene(XmlNode xml)
    {
        // loop through all entities in the scene and decode them
        XmlNodeList entities = xml.ChildNodes;
        foreach (XmlNode xmlEntity in entities)
        {
            decodeEntity(xmlEntity);
        }
    }

    private void decodeEntity(XmlNode xml)
    {
        // return if comment
        if (xml.OuterXml.StartsWith("<!--")) return;

        // get datafrom xml
        string name = xml.Attributes["name"].Value;
        Vector3 position = decodeVector(xml.Attributes["position"], new Vector3(0f, 0f, 0f));
        Vector3 rotation = decodeVector(xml.Attributes["rotation"], new Vector3(0f, 0f, 0f));
        Vector3 scale = decodeVector(xml.Attributes["scale"], new Vector3(1f, 1f, 1f));
        bool active = decodeBoolean(xml.Attributes["active"], true);

        // create entity
        GameObject entity = GameObject.Instantiate(manager.baseObject, position, Quaternion.Euler(rotation));
        entity.transform.localScale = scale;
        entity.active = active;
        entity.name = name;
        manager.entities[name] = entity;

        // update entity manager
        EntityManager entityManager = entity.GetComponent<EntityManager>();
        entityManager.manager = manager;
        entityManager.name = name;

        // find something todo with component here
        XmlNodeList componentNodes = xml.ChildNodes;
        foreach (XmlNode componentNode in componentNodes)
            decodeComponent(entity.GetComponent<EntityManager>(), componentNode);
    }

    private void decodeInput(XmlNode xml)
    {

    }

    private void decodeInstructions(XmlNode xml)
    {

    }

    private void decodeComponent(EntityManager entity, XmlNode xml)
    {
        switch(xml.Name)
        {
            case "collider":
                entity.setCollideable(true);
                break;
            case "simplemesh":
                entity.addMesh(xml.Attributes["mesh"].Value);
                break;
            case "simplematerial":
                entity.addMaterial(xml.Attributes["material"].Value);
                break;
            case "light":
                entity.setLight(
                    decodeString(xml.Attributes["type"], "directional"),
                    decodeFloat(xml.Attributes["intensity"], 1f),
                    decodeFloat(xml.Attributes["range"], 1f),
                    decodeFloat(xml.Attributes["angle"], 1f),
                    decodeColor(xml.Attributes["color"], Color.white)
                );
                break;
            default:
                Debug.LogWarning("No component made for " + xml.Name);
                break;
        }
    }

    private bool decodeBoolean(XmlAttribute attribute, bool def)
    {
        if (attribute == null)
            return def;
        else
            return attribute.Value == "true";
    }

    private float decodeFloat(XmlAttribute attribute, float def)
    {
        if (attribute == null)
            return def;
        else
            return float.Parse(attribute.Value);
    }

    private string decodeString(XmlAttribute attribute, string def)
    {
        if (attribute == null)
            return def;
        else
            return attribute.Value;
    }

    private Vector3 decodeVector(XmlAttribute attribute, Vector3 def)
    {
        if (attribute == null)
            return def;
        else
        {
            string[] tokens = attribute.Value.Split(";");
            if (tokens.Length != 3) return def;
            return new Vector3(
                float.Parse(tokens[0]),
                float.Parse(tokens[1]),
                float.Parse(tokens[2])
            );
        }
    }

    private Color decodeColor(XmlAttribute attribute, Color def)
    {
        if (attribute == null)
            return def;
        else
        {
            string[] tokens = attribute.Value.Split(";");
            if (tokens.Length != 3) return def;
            return new Color(
                int.Parse(tokens[0]) / 255f,
                int.Parse(tokens[1]) / 255f,
                int.Parse(tokens[2]) / 255f
            );
        }
    }
}
