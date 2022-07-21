using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class XMLDecoder
{
    Manager manager;
    public XMLDecoder(Manager manager) { this.manager = manager; }

    string onUpdateInstruction;

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
                decodeActions(document.FirstChild);
                break;
            case 2:
                InstructionContainer instructions = decodeInstructions(document.FirstChild);
                switch(instructions.executeTime)
                {
                    case "on_update":
                        manager.actionManager.addAction(
                            new Action(
                                "#update",
                                new Condition[] { new OnUpdateCondition() },
                                instructions.instructions
                            )
                        );
                        break;
                    default:
                        Debug.Log("Unknow execute time " + instructions.executeTime);
                        break;
                }
                break;
            case 3:
                decodeEntity(document.FirstChild);
                break;
            default:
                Debug.LogWarning("No xml file type " + type + " was created!");
                break;
        }
    }

    public void decodeScene(XmlNode xml)
    {
        // loop through all entities in the scene and decode them
        XmlNodeList entities = xml.ChildNodes;
        foreach (XmlNode xmlEntity in entities)
        {
            decodeEntity(xmlEntity);
        }
    }

    public void decodeEntity(XmlNode xml)
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

    public void decodeActions(XmlNode xmls)
    {
        // make sure actions are for our plaform
        string platform = xmls.Attributes["type"].Value;
        if (platform != manager.platform) return;

        // loop through each action and decompile it
        foreach (XmlNode xml in xmls.ChildNodes)
        {
            // get xml for conditions and instructinos
            XmlNode conditionsXML = xml.FirstChild;
            XmlNode instructionsXML = xml.LastChild;

            manager.actionManager.addAction(
                new Action(
                    xml.Attributes["name"].Value,
                    decodeConditions(conditionsXML),
                    decodeInstructionToArray(instructionsXML)
                )
            );
        }
    }

    public Condition[] decodeConditions(XmlNode xml)
    {
        // create condition array and return
        Condition[] conditions = new Condition[xml.ChildNodes.Count];
        for (int i = 0; i < conditions.Length; i++)
            conditions[i] = Condition.getCompiledCondition(manager, xml.ChildNodes.Item(i));
        return conditions;
    }

    public Instruction[] decodeInstructionToArray(XmlNode xml)
    {
        // create instruction array
        Instruction[] instructions = new Instruction[xml.ChildNodes.Count];

        // loop through all instructions
        for (int i = 0; i < instructions.Length; i++)
            instructions[i] = decodeInstruction(xml.ChildNodes.Item(i));

        return instructions;
    }

    public InstructionContainer decodeInstructions(XmlNode xml) 
    {
        return new InstructionContainer(xml.Attributes["run"].Value, decodeInstructionToArray(xml));
    }

    public Instruction decodeInstruction(XmlNode xml)
    {
        return Instruction.getCompiledInstruction(manager, xml);
    }

    public void decodeComponent(EntityManager entity, XmlNode xml)
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
                    true,
                    decodeString(xml.Attributes["type"], "directional"),
                    decodeFloat(xml.Attributes["intensity"], 1f),
                    decodeFloat(xml.Attributes["range"], 1f),
                    decodeFloat(xml.Attributes["angle"], 1f),
                    decodeColor(xml.Attributes["color"], Color.white)
                );
                break;
            case "rigidbody":
                entity.setRigidbody(
                    true,
                    decodeFloat(xml.Attributes["mass"], 1f),
                    decodeFloat(xml.Attributes["drag"], 0f),
                    decodeFloat(xml.Attributes["angularDrag"], 0.05f),
                    decodeBoolean(xml.Attributes["isKinematic"], false),
                    decodeBoolean(xml.Attributes["useGravity"], true)
                );
                break;
            case "particleemitter":
                entity.setParticleEmitter(
                    true,
                    decodeVector(xml.Attributes["directionScale"], new Vector3(1f, 1f, 1f)),
                    decodeString(xml.Attributes["texture"], ""),
                    decodeFloat(xml.Attributes["lifetime"], 1f),
                    decodeFloat(xml.Attributes["duration"], 1f),
                    decodeFloat(xml.Attributes["speed"], 1f),
                    decodeFloat(xml.Attributes["size"], 1f),
                    decodeFloat(xml.Attributes["rate"], 1f)
                );
                break;
            default:
                Debug.LogWarning("No component made for " + xml.Name);
                break;
        }
    }

    public static GameObject getEntityWithKeywords(Manager manager, string entity)
    {
        if (entity == "camera") return Camera.main.gameObject;
        else if (!manager.entities.ContainsKey("entity")) return null;
        else return manager.entities[entity];
    }

    public static bool decodeBoolean(XmlAttribute attribute, bool def)
    {
        if (attribute == null)
            return def;
        else
            return attribute.Value == "true";
    }

    public static float decodeFloat(XmlAttribute attribute, float def)
    {
        if (attribute == null)
            return def;
        else
            return float.Parse(attribute.Value);
    }

    public static string decodeString(XmlAttribute attribute, string def)
    {
        if (attribute == null)
            return def;
        else
            return attribute.Value;
    }

    public static Vector3 decodeVector(XmlAttribute attribute, Vector3 def)
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

    public static Vector2 decodeVector(XmlAttribute attribute, Vector2 def)
    {
        if (attribute == null)
            return def;
        else
        {
            string[] tokens = attribute.Value.Split(";");
            if (tokens.Length != 2) return def;
            return new Vector2(
                float.Parse(tokens[0]),
                float.Parse(tokens[1])
            );
        }
    }

    public static Color decodeColor(XmlAttribute attribute, Color def)
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
