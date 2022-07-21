using System.Xml;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EffectManager : MonoBehaviour
{
    public PostProcessVolume volume;

    public void setEffects(XmlNode xml)
    {
        clearEffects();
        addEffects(xml);
    }

    public void clearEffects()
    {
        foreach (var setting in volume.profile.settings)
        {
            volume.profile.RemoveSettings(setting.GetType());
        }
    }

    public void addEffects(XmlNode xml)
    {
        // loop through all xml effects
        foreach(XmlNode child in xml.ChildNodes)
        {
            // get effect
            var effect = getEffectFromXML(child);

            // if effect exists, add it to the profile, otherwise, throw an error
            if (effect != null)
            {
                volume.profile.AddSettings(effect);
                effect.enabled.value = true; // effect needs to be enabled too cause unity
            }
            else
                Debug.LogError("Unknow camera effect: " + child.OuterXml);
        }
    }

    private PostProcessEffectSettings getEffectFromXML(XmlNode xml)
    {
        switch (xml.Name)
        {
            case "bloom":
                Bloom bloom = new Bloom();
                bloom.intensity.value = XMLDecoder.decodeFloat(xml.Attributes["intensity"], 4f);
                bloom.color.value = XMLDecoder.decodeColor(xml.Attributes["color"], Color.white);
                bloom.intensity.overrideState = true;
                bloom.color.overrideState = true;
                return bloom;
            case "vignette":
                Vignette vignette = new Vignette();
                vignette.intensity.value = XMLDecoder.decodeFloat(xml.Attributes["intensity"], 0.25f);
                vignette.color.value = XMLDecoder.decodeColor(xml.Attributes["color"], Color.black);
                vignette.intensity.overrideState = true;
                vignette.color.overrideState = true;
                return vignette;
            default:
                return null;
        }
    }
}
