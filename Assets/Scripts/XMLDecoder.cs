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
                decodeScene(manager, document.FirstChild);
                break;
            case 1:
                decodeActions(manager, document.FirstChild);
                break;
            case 2:
                decodeInstructions(manager, document.FirstChild);
                break;
            case 3:
                decodeEntity(manager, document.FirstChild);
                break;
            default:
                Debug.LogWarning("No xml file type " + type + " was created!");
                break;
        }
    }

    public static void decodeScene(Manager manager, XmlNode xml)
    {
        // loop through children
        foreach (XmlNode child in xml.ChildNodes)
        {
            // if entities, loop through childs children and create the entities
            if (child.Name == "entities")
                foreach (XmlNode entity in child.ChildNodes)
                    decodeEntity(manager, entity);
            else if (child.Name == "instructionsets")
                foreach (XmlNode set in child.ChildNodes)
                    decodeInstructions(manager, set);
            else if (child.Name == "actionmaps")
                foreach (XmlNode map in child.ChildNodes)
                    decodeActions(manager, map);
        }
    }

    public static void decodeEntity(Manager manager, XmlNode xml)
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

    public static void decodeActions(Manager manager, XmlNode xmls)
    {
        // make sure actions are for our plaform
        string platform = xmls.Attributes["platform"].Value;
        if (platform != manager.platform && platform != "any") return;

        // loop through each action and decompile it
        foreach (XmlNode xml in xmls.ChildNodes)
        {
            // get xml for conditions and instructinos
            XmlNode conditionsXML = xml.FirstChild;
            XmlNode instructionsXML = xml.LastChild;

            manager.actionManager.addAction(
                new Action(
                    xml.Attributes["name"].Value,
                    decodeConditions(manager, conditionsXML),
                    decodeInstructionToArray(manager, instructionsXML)
                )
            );
        }
    }

    public static Condition[] decodeConditions(Manager manager, XmlNode xml)
    {
        // create condition array and return
        Condition[] conditions = new Condition[xml.ChildNodes.Count];
        for (int i = 0; i < conditions.Length; i++)
            conditions[i] = Condition.getCompiledCondition(manager, xml.ChildNodes.Item(i));
        return conditions;
    }

    public static Instruction[] decodeInstructionToArray(Manager manager, XmlNode xml)
    {
        // create instruction array
        Instruction[] instructions = new Instruction[xml.ChildNodes.Count];

        // loop through all instructions
        for (int i = 0; i < instructions.Length; i++)
            instructions[i] = decodeInstruction(manager, xml.ChildNodes.Item(i));

        return instructions;
    }

    public static void decodeInstructions(Manager manager, XmlNode xml) 
    {
        string run = xml.Attributes["run"].Value;
        Instruction[] instructions = decodeInstructionToArray(manager, xml);

        switch (run)
        {
            case "on_update":
                manager.actionManager.addAction(
                    new Action(
                        "#update",
                        new Condition[] { new OnUpdateCondition() },
                        instructions
                    )
                );
                break;
            case "now":
                Instruction.runInstructions(manager, instructions);
                break;
            default:
                Debug.Log("Unknow run time " + run);
                break;
        }
    }

    public static Instruction decodeInstruction(Manager manager, XmlNode xml)
    {
        return Instruction.getCompiledInstruction(manager, xml);
    }

    public static void decodeComponent(EntityManager entity, XmlNode xml)
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
            case "instructionset":
                XMLDecoder.decodeInstructions(entity.manager, xml);
                break;
            default:
                Debug.LogError("No component made for " + xml.Name);
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
