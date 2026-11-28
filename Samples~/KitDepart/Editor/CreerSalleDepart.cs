using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LerpUp.KitDepart
{
    // Menu LerpUp > Créer la salle de départ.
    // Il règle les images du kit (pixel art net), puis construit une petite salle iso : un sol en losanges (Grid + Tilemap),
    // Aster avec le script DeplacementJoueur, et la caméra. La scène est enregistrée dans Assets/LerpUp/Salle/SalleDepart.unity.
    // Volontairement, Aster n'a PAS de Rigidbody2D ni de collider : c'est ce que tu ajoutes dans la salle « Faire avancer ton perso sans traverser les murs ».
    public static class CreerSalleDepart
    {
        const string DossierSalle = "Assets/LerpUp/Salle";
        const string ScenePath = DossierSalle + "/SalleDepart.unity";
        const string TuilePath = DossierSalle + "/TuileSol.asset";

        // Les feuilles d'Aster : des images de 32 x 32 posées côte à côte (le profil d’Aster n’est pas dans le kit : voir README.md, section « Ajouter le profil d’Aster »)
        static readonly string[] FeuillesAster =
        {
            "aster_bas_idle", "aster_bas_marche", "aster_haut_idle", "aster_haut_marche"
        };

        [MenuItem("LerpUp/Créer la salle de départ")]
        public static void Creer()
        {
            try
            {
                // 1. Retrouver les images (le kit peut être dans n'importe quel dossier de Assets)
                var feuilles = new Dictionary<string, string>();
                foreach (var nom in FeuillesAster) feuilles[nom] = Trouver(nom);
                string sol = Trouver("sol-iso"), bloc = Trouver("bloc-iso"), ombre = Trouver("ombre");

                // 2. Régler les images : pixel art net, pas de compression, découpe des feuilles en cases de 32 x 32
                foreach (var kv in feuilles) ReglerFeuille(kv.Value, kv.Key, 32, 32, 32f);
                ReglerImage(sol, 64f);
                ReglerImage(bloc, 64f);
                ReglerImage(ombre, 32f);

                // 3. La tuile du sol
                var spriteSol = AssetDatabase.LoadAssetAtPath<Sprite>(sol);
                if (spriteSol == null) throw new Exception("Le sprite du sol est introuvable : " + sol);
                Directory.CreateDirectory(DossierSalle);
                AssetDatabase.DeleteAsset(TuilePath);
                var tuile = ScriptableObject.CreateInstance<Tile>();
                tuile.sprite = spriteSol;
                AssetDatabase.CreateAsset(tuile, TuilePath);

                // 4. La scène (si elle existe déjà, on demande avant de la remplacer)
                if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("LerpUp", "La salle de départ existe déjà (" + ScenePath + "). La refaire de zéro ?", "La refaire", "Annuler")) return;
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                CreerCamera();
                CreerSol(tuile);
                CreerPerso(feuilles["aster_bas_idle"]);

                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                Debug.Log("[LerpUp] Salle de départ créée : " + ScenePath + ". Lance le jeu (Play) et déplace Aster avec les flèches. "
                    + "Si la Console parle de « Input », mets Active Input Handling sur « Both » (Edit > Project Settings > Player > Other Settings).");
            }
            catch (Exception e)
            {
                Debug.LogError("[LerpUp] La salle de départ n'a pas pu être créée : " + e.Message);
            }
        }

        // ----- Retrouver un fichier du kit par son nom (sans extension) -----
        static string Trouver(string nom)
        {
            foreach (var guid in AssetDatabase.FindAssets(nom + " t:Texture2D"))
            {
                var chemin = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(chemin) == nom) return chemin;
            }
            throw new Exception("Il manque l'image « " + nom + " » : importe d'abord le Sample « Kit de départ » (Package Manager > LerpUp Testeur > Samples > Import).");
        }

        // ----- Une image seule : sprite, pixel art net -----
        static TextureImporter Importer(string chemin, SpriteImportMode mode, float pixelsParUnite)
        {
            var imp = AssetImporter.GetAtPath(chemin) as TextureImporter;
            if (imp == null) throw new Exception("Cette image n'est pas importable : " + chemin);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = mode;
            imp.spritePixelsPerUnit = pixelsParUnite;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            return imp;
        }

        static void ReglerImage(string chemin, float pixelsParUnite)
        {
            Importer(chemin, SpriteImportMode.Single, pixelsParUnite).SaveAndReimport();
        }

        // ----- Une feuille : des cases de largeur x hauteur, pivot aux pieds (en bas au centre) -----
        static void ReglerFeuille(string chemin, string nom, int largeur, int hauteur, float pixelsParUnite)
        {
            var imp = Importer(chemin, SpriteImportMode.Multiple, pixelsParUnite);
            imp.SaveAndReimport();   // d'abord les réglages : la découpe se fait ensuite sur l'image importée

            int largeurImage = LargeurPng(chemin);
            int cases = largeurImage / largeur;
            var rects = new List<SpriteRect>();
            var noms = new List<SpriteNameFileIdPair>();
            for (int i = 0; i < cases; i++)
            {
                var r = new SpriteRect
                {
                    name = nom + "_" + i,
                    rect = new Rect(i * largeur, 0, largeur, hauteur),
                    alignment = SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0f),
                    spriteID = GUID.Generate()
                };
                rects.Add(r);
                noms.Add(new SpriteNameFileIdPair(r.name, r.spriteID));
            }

            var usine = new SpriteDataProviderFactories();
            usine.Init();
            var fournisseur = usine.GetSpriteEditorDataProviderFromObject(imp);
            fournisseur.InitSpriteEditorDataProvider();
            fournisseur.SetSpriteRects(rects.ToArray());
            var fournisseurNoms = fournisseur.GetDataProvider<ISpriteNameFileIdDataProvider>();
            fournisseurNoms.SetNameFileIdPairs(noms);
            fournisseur.Apply();
            imp.SaveAndReimport();
        }

        // La largeur d'un PNG se lit dans son en-tête (4 octets, à partir de l'octet 16)
        static int LargeurPng(string chemin)
        {
            var octets = File.ReadAllBytes(chemin);
            return (octets[16] << 24) | (octets[17] << 16) | (octets[18] << 8) | octets[19];
        }

        // ----- La scène -----
        static void CreerCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
        }

        static void CreerSol(Tile tuile)
        {
            var grille = new GameObject("Grid");
            var g = grille.AddComponent<Grid>();
            g.cellLayout = GridLayout.CellLayout.Isometric;
            g.cellSize = new Vector3(1f, 0.5f, 1f);   // un losange de 64 x 32 pixels à 64 pixels par unité

            var sol = new GameObject("Sol");
            sol.transform.SetParent(grille.transform, false);
            var tilemap = sol.AddComponent<Tilemap>();
            sol.AddComponent<TilemapRenderer>();
            for (int x = -4; x < 4; x++)
                for (int y = -4; y < 4; y++)
                    tilemap.SetTile(new Vector3Int(x, y, 0), tuile);
        }

        static void CreerPerso(string cheminFeuille)
        {
            var perso = new GameObject("Perso");
            var sr = perso.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;   // devant le sol
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(cheminFeuille))
                if (o is Sprite s && s.name == "aster_bas_idle_0") { sr.sprite = s; break; }

            // Le script du kit (s'il a été renommé ou supprimé, la salle se crée quand même : tu le remettras sur Aster)
            var type = Type.GetType("DeplacementJoueur, Assembly-CSharp");
            if (type != null) perso.AddComponent(type);
            else Debug.LogWarning("[LerpUp] Le script DeplacementJoueur est introuvable : ajoute-le sur Perso à la main.");
        }
    }
}
