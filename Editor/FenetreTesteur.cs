using UnityEditor;
using UnityEngine;

namespace LerpUp.Testeur
{
    // La fenêtre « LerpUp > Tester cette salle » : on choisit la salle, on colle son numéro (gardé dans EditorPrefs),
    // et on lance les tests. Le résultat s'affiche dans la Console, et ici aussi.
    public class FenetreTesteur : EditorWindow
    {
        int choix;
        Vector2 defilement;

        [MenuItem("LerpUp/Tester cette salle")]
        static void Ouvrir()
        {
            var f = GetWindow<FenetreTesteur>("Testeur LerpUp");
            f.minSize = new Vector2(380, 280);
        }

        void OnEnable() { Lanceur.Termine += Repaint; }
        void OnDisable() { Lanceur.Termine -= Repaint; }

        static string Cle(string id) { return "LerpUp.Numero." + id; }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Salle", EditorStyles.boldLabel);
            var noms = new string[TestsSalles.Liste.Length];
            for (int i = 0; i < noms.Length; i++) noms[i] = TestsSalles.Liste[i].Titre;
            choix = Mathf.Clamp(EditorGUILayout.Popup(choix, noms), 0, noms.Length - 1);
            var salle = TestsSalles.Liste[choix];

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Ton numéro pour cette salle (donné par le site LerpUp)", EditorStyles.boldLabel);
            var numero = EditorGUILayout.TextField(EditorPrefs.GetString(Cle(salle.Id), ""));
            EditorPrefs.SetString(Cle(salle.Id), numero.Trim());

            EditorGUILayout.Space(8);
            var numeroOk = System.Text.RegularExpressions.Regex.IsMatch(numero.Trim(), "^[0-9]{4}$");
            if (!numeroOk) EditorGUILayout.HelpBox("Colle le numéro à 4 chiffres affiché dans la tâche « Faire » de la salle.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!numeroOk || Lanceur.EnCours))
                if (GUILayout.Button("Lancer les tests", GUILayout.Height(32))) Lanceur.Lancer(salle.Id, numero.Trim());

            EditorGUILayout.Space(8);
            var dernier = Lanceur.DernierResultat;
            if (dernier != "")
            {
                defilement = EditorGUILayout.BeginScrollView(defilement);
                var style = new GUIStyle(EditorStyles.wordWrappedLabel) { richText = true };
                EditorGUILayout.LabelField(dernier, style);
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
