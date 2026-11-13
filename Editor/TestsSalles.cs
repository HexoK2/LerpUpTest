using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LerpUp.Testeur
{
    // Un test de salle : un identifiant (le même que sur le site), un titre, et les vérifications faites dans la scène ouverte.
    // Après ces vérifications, le Lanceur joue le jeu 3 secondes et compte les exceptions.
    public class TestSalle
    {
        public string Id;
        public string Titre;
        public Action<Resultats> Verifier;
    }

    // Les salles de l'unité 1 qui se testent par du code. Les autres (réglage « à l'œil », comparaison clavier / manette,
    // marche lente avec Shift qui demande une vraie touche…) se prouvent avec une explication sur le site :
    // un test approximatif serait pire que pas de test.
    public static class TestsSalles
    {
        public static readonly TestSalle[] Liste =
        {
            new TestSalle { Id = "iso-deb-co-2", Titre = "Passer de Translate au Rigidbody2D", Verifier = Rigidbody },
            new TestSalle { Id = "iso-deb-co-3", Titre = "Choisir l'animation selon la direction", Verifier = Direction8 },
            new TestSalle { Id = "iso-deb-co-7", Titre = "Lire la Console d'erreurs", Verifier = LireLaConsole },
        };

        public static TestSalle Trouver(string id)
        {
            foreach (var t in Liste) if (t.Id == id) return t;
            return null;
        }

        // ----- Lire la Console d'erreurs : le script compile, le sprite est assigné, le Rigidbody2D est trouvé -----
        static void LireLaConsole(Resultats r)
        {
            r.Ajouter(Outils.ScriptsCompilent(), "Le script compile", "Le script ne compile pas : une erreur rouge reste dans la Console");
            var corps = Outils.CorpsAvecScript();
            if (corps.Count == 0)
            {
                r.Ajouter(false, "Aucun objet de la scène n'a à la fois un Rigidbody2D et ton script : le Rigidbody2D est introuvable (NullReferenceException)");
                return;
            }
            // Le sprite : un champ SpriteRenderer du script doit être rempli
            bool spriteOk = false, spriteTrouve = false;
            foreach (var go in corps)
                foreach (var s in Outils.ScriptsEleve(go))
                    foreach (var champ in Outils.ChampsReference(s, "SpriteRenderer")) { spriteTrouve = true; spriteOk |= champ.Value; }
            r.Ajouter(spriteTrouve && spriteOk,
                "Le sprite est assigné : aucune UnassignedReferenceException",
                spriteTrouve ? "Le champ sprite du script est vide (UnassignedReferenceException) : glisse le SpriteRenderer dedans" : "Le script n'a plus de champ SpriteRenderer : remets le script fourni");
            r.Ajouter(true, "Le Rigidbody2D est trouvé : aucune NullReferenceException");
        }

        // ----- Rigidbody2D : réglé comme dans la leçon (Dynamic, gravité 0, rotation Z bloquée, interpolation, collider) -----
        static void Rigidbody(Resultats r)
        {
            r.Ajouter(Outils.ScriptsCompilent(), "Le script compile", "Le script ne compile pas : une erreur rouge reste dans la Console");
            var corps = Outils.CorpsAvecScript();
            if (corps.Count == 0)
            {
                r.Ajouter(false, "Aucun objet de la scène n'a à la fois un Rigidbody2D et ton script de déplacement");
                return;
            }
            var go = corps[0];
            var rb = go.GetComponent<Rigidbody2D>();
            r.Ajouter(rb.bodyType == RigidbodyType2D.Dynamic, "Le Rigidbody2D est Dynamic", "Le Rigidbody2D n'est pas Dynamic : un corps Kinematic n'est pas arrêté par les murs");
            r.Ajouter(Mathf.Approximately(rb.gravityScale, 0f), "La gravité est à 0 (vue de dessus)", "La gravité n'est pas à 0 : le personnage tomberait");
            r.Ajouter((rb.constraints & RigidbodyConstraints2D.FreezeRotation) != 0, "La rotation Z est bloquée", "La rotation Z n'est pas bloquée (Freeze Rotation Z)");
            r.Ajouter(rb.interpolation != RigidbodyInterpolation2D.None, "L'interpolation est activée", "L'interpolation est sur None : mets Interpolate");
            r.Ajouter(go.GetComponent<Collider2D>() != null, "Le personnage a un collider", "Le personnage n'a pas de Collider2D : les murs ne peuvent pas l'arrêter");
        }

        // ----- Direction8 : l'Animator a les 5 états, les champs du script sont remplis -----
        static void Direction8(Resultats r)
        {
            r.Ajouter(Outils.ScriptsCompilent(), "Le script compile", "Le script ne compile pas : une erreur rouge reste dans la Console");
            Animator anim = null;
            bool spriteOk = false, animChamp = false, spriteChamp = false;
            foreach (var a in UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsSortMode.None))
            {
                foreach (var s in Outils.ScriptsEleve(a.gameObject))
                {
                    foreach (var champ in Outils.ChampsReference(s, "Animator")) { animChamp = true; if (champ.Value) anim = a; }
                    foreach (var champ in Outils.ChampsReference(s, "SpriteRenderer")) { spriteChamp = true; spriteOk |= champ.Value; }
                }
            }
            r.Ajouter(anim != null, "Le champ Animator du script est rempli", animChamp ? "Le champ Animator du script est vide" : "Aucun script avec un champ Animator sur un objet qui a un Animator");
            r.Ajouter(spriteChamp && spriteOk, "Le champ SpriteRenderer du script est rempli", "Le champ SpriteRenderer du script est vide");
            if (anim == null) return;
            var controleur = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(anim.runtimeAnimatorController));
            var noms = new HashSet<string>();
            if (controleur != null && controleur.layers.Length > 0)
                foreach (var e in controleur.layers[0].stateMachine.states) noms.Add(e.state.name);
            var manquants = new List<string>();
            foreach (var n in new[] { "Droite", "HautDroite", "Haut", "Bas", "BasDroite" }) if (!noms.Contains(n)) manquants.Add(n);
            r.Ajouter(manquants.Count == 0, "L'Animator a les 5 états : Droite, HautDroite, Haut, Bas, BasDroite",
                "Il manque des états dans l'Animator (les noms doivent être exacts) : " + string.Join(", ", manquants));
        }
    }
}
