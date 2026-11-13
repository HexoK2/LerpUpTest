using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LerpUp.Testeur
{
    // Lance le test d'une salle : vérifications dans la scène, puis 3 secondes de jeu en comptant les exceptions
    // (Application.logMessageReceived), puis affichage du résultat dans la Console, avec le code s'il est entièrement réussi.
    // Unity recharge ses scripts en entrant en mode Play : l'état est donc gardé dans SessionState entre les deux.
    public static class Lanceur
    {
        const string K = "LerpUp.Testeur.";
        const double DureeJeu = 3.0;
        static double debut = -1;

        public static event Action Termine;
        public static string DernierResultat { get { return SessionState.GetString(K + "dernier", ""); } }
        public static bool EnCours { get { return SessionState.GetString(K + "phase", "") != ""; } }

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged -= OnMode; EditorApplication.playModeStateChanged += OnMode;
            Application.logMessageReceived -= OnLog; Application.logMessageReceived += OnLog;
        }

        public static void Lancer(string idSalle, string numero)
        {
            var test = TestsSalles.Trouver(idSalle);
            if (test == null) { Debug.LogError("[LerpUp] Salle inconnue : " + idSalle); return; }
            if (EditorApplication.isPlaying) { Debug.LogWarning("[LerpUp] Arrête le mode Play avant de lancer le test."); return; }
            var r = new Resultats();
            test.Verifier(r);
            SessionState.SetString(K + "salle", idSalle);
            SessionState.SetString(K + "numero", numero);
            SessionState.SetString(K + "res", r.Serialiser());
            SessionState.SetInt(K + "exceptions", 0);
            SessionState.SetString(K + "premiere", "");
            if (!r.Tout) { Terminer(); return; }    // inutile de jouer si ça ne compile pas ou s'il manque un champ
            SessionState.SetString(K + "phase", "jeu");
            EditorApplication.EnterPlaymode();
        }

        // Pour la ligne de commande : -executeMethod LerpUp.Testeur.Lanceur.LancerDepuisLigneDeCommande -salle <id> -numero <n>
        public static void LancerDepuisLigneDeCommande()
        {
            var args = Environment.GetCommandLineArgs();
            string salle = "", numero = "0000";
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-salle") salle = args[i + 1];
                if (args[i] == "-numero") numero = args[i + 1];
            }
            Lancer(salle, numero);
        }

        static void OnLog(string message, string pile, LogType type)
        {
            if (SessionState.GetString(K + "phase", "") != "jeu" || type != LogType.Exception) return;
            if (!Application.isPlaying || !VientDuJeu(pile)) return;
            SessionState.SetInt(K + "exceptions", SessionState.GetInt(K + "exceptions", 0) + 1);
            if (SessionState.GetString(K + "premiere", "") == "") SessionState.SetString(K + "premiere", message.Split('\n')[0]);
        }

        // Une exception compte si l'une des lignes de sa pile vient d'un script du projet. Celles de l'éditeur lui-même
        // (par exemple l'indexation de la recherche au démarrage) ne sont pas celles du jeu. Sans pile lisible, on compte.
        static bool VientDuJeu(string texte)
        {
            bool uneLigne = false;
            foreach (var brut in texte.Split('\n'))
            {
                var l = brut.Trim();
                if (l.Length == 0 || l.StartsWith("(Filename")) continue;
                if (l.StartsWith("at ")) l = l.Substring(3);
                uneLigne = true;
                bool interne = l.StartsWith("System.") || l.StartsWith("UnityEditor") || l.StartsWith("UnityEngine") || l.StartsWith("Unity.")
                    || l.StartsWith("Mono.") || l.StartsWith("Microsoft.") || l.StartsWith("LerpUp.Testeur");
                if (!interne) return true;
            }
            return !uneLigne;
        }

        static void Tick()
        {
            if (SessionState.GetString(K + "phase", "") != "jeu" || !EditorApplication.isPlaying) return;
            if (debut < 0) debut = EditorApplication.timeSinceStartup;
            if (EditorApplication.timeSinceStartup - debut >= DureeJeu)
            {
                SessionState.SetString(K + "phase", "retour");
                debut = -1;
                EditorApplication.ExitPlaymode();
            }
        }

        static void OnMode(PlayModeStateChange e)
        {
            if (e != PlayModeStateChange.EnteredEditMode) return;
            var phase = SessionState.GetString(K + "phase", "");
            if (phase == "") return;
            var r = Resultats.Lire(SessionState.GetString(K + "res", ""));
            int n = SessionState.GetInt(K + "exceptions", 0);
            if (phase == "retour")
                r.Ajouter(n == 0, "3 secondes de jeu sans exception", "Une exception est arrivée pendant le jeu : " + SessionState.GetString(K + "premiere", ""));
            else
                r.Ajouter(false, "Le jeu a été arrêté avant la fin des 3 secondes");
            SessionState.SetString(K + "res", r.Serialiser());
            Terminer();
        }

        static void Terminer()
        {
            var test = TestsSalles.Trouver(SessionState.GetString(K + "salle", ""));
            var numero = SessionState.GetString(K + "numero", "0000");
            var r = Resultats.Lire(SessionState.GetString(K + "res", ""));
            var s = new StringBuilder();
            s.Append("[LerpUp] Test de la salle « ").Append(test != null ? test.Titre : "?").Append(" »\n");
            foreach (var l in r.Lignes)
                s.Append(l.Key ? "<color=#35D6C4>✓</color> " : "<color=#FF6B6B>✗</color> ").Append(l.Value).Append('\n');
            if (r.Tout && test != null)
                s.Append(r.Reussis).Append(" tests sur ").Append(r.Lignes.Count).Append(". Ton code : <b>").Append(CodeLerpUp.Calculer(test.Id, numero)).Append("</b>");
            else
                s.Append(r.Reussis).Append(" tests sur ").Append(r.Lignes.Count).Append(". Corrige ce qui est en rouge, puis relance le test.");
            SessionState.SetString(K + "dernier", s.ToString());
            SessionState.SetString(K + "phase", "");
            if (r.Tout) Debug.Log(s.ToString()); else Debug.LogWarning(s.ToString());
            if (Termine != null) Termine();
            if (Application.isBatchMode) EditorApplication.Exit(r.Tout ? 0 : 1);
        }
    }
}
