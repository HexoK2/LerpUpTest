using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LerpUp.Testeur
{
    // Les outils des tests : on trouve les objets par leurs COMPOSANTS, jamais par leur nom.
    public static class Outils
    {
        // Un script écrit par l'élève (et non un composant d'Unity)
        public static bool EstScriptEleve(Component c)
        {
            if (!(c is MonoBehaviour)) return false;
            var nom = c.GetType().Assembly.GetName().Name;
            return !(nom.StartsWith("Unity") || nom.StartsWith("mscorlib") || nom.StartsWith("System") || nom.StartsWith("LerpUp") || nom.StartsWith("TMPro"));
        }

        public static List<MonoBehaviour> ScriptsEleve(GameObject go)
        {
            var l = new List<MonoBehaviour>();
            foreach (var c in go.GetComponents<MonoBehaviour>()) if (c != null && EstScriptEleve(c)) l.Add(c);
            return l;
        }

        // Les objets de la scène qui ont un Rigidbody2D ET un script de l'élève
        public static List<GameObject> CorpsAvecScript()
        {
            var l = new List<GameObject>();
            foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
                if (ScriptsEleve(rb.gameObject).Count > 0) l.Add(rb.gameObject);
            return l;
        }

        // Les champs de référence d'un script : nom, type (par ex. "PPtr<$SpriteRenderer>") et s'ils sont remplis
        public static List<KeyValuePair<string, bool>> ChampsReference(MonoBehaviour script, string typeUnity)
        {
            var l = new List<KeyValuePair<string, bool>>();
            var so = new SerializedObject(script);
            var p = so.GetIterator();
            for (bool entre = p.NextVisible(true); entre; entre = p.NextVisible(false))
            {
                if (p.propertyPath == "m_Script" || p.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (typeUnity != null && p.type != "PPtr<$" + typeUnity + ">") continue;
                l.Add(new KeyValuePair<string, bool>(p.displayName, p.objectReferenceValue != null));
            }
            return l;
        }

        public static bool ScriptsCompilent() { return !EditorUtility.scriptCompilationFailed; }
    }
}
