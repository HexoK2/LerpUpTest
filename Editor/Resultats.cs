using System.Collections.Generic;

namespace LerpUp.Testeur
{
    // La liste des lignes d'un test : une ligne par vérification, réussie ou non.
    public class Resultats
    {
        public readonly List<KeyValuePair<bool, string>> Lignes = new List<KeyValuePair<bool, string>>();

        public void Ajouter(bool ok, string texte) { Lignes.Add(new KeyValuePair<bool, string>(ok, texte)); }
        public void Ajouter(bool ok, string texteOk, string texteKo) { Ajouter(ok, ok ? texteOk : texteKo); }

        public bool Tout { get { foreach (var l in Lignes) if (!l.Key) return false; return true; } }
        public int Reussis { get { int n = 0; foreach (var l in Lignes) if (l.Key) n++; return n; } }

        // Pour garder le résultat pendant que Unity recharge ses scripts (entrée en mode Play)
        public string Serialiser()
        {
            var s = new System.Text.StringBuilder();
            foreach (var l in Lignes) s.Append(l.Key ? "1" : "0").Append(l.Value.Replace('\n', ' ')).Append('\n');
            return s.ToString();
        }

        public static Resultats Lire(string texte)
        {
            var r = new Resultats();
            foreach (var ligne in (texte ?? "").Split('\n'))
                if (ligne.Length > 1) r.Ajouter(ligne[0] == '1', ligne.Substring(1));
            return r;
        }
    }
}
