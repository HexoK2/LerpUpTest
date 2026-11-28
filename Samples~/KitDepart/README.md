# Kit de départ LerpUp / LerpUp starter kit

Une petite salle iso toute prête pour faire les premières salles de LerpUp sans partir de zéro : un sol en losanges, Aster (face et dos) et le script de la première salle.

A ready-made small iso room so you can do LerpUp's first rooms without starting from scratch: a diamond floor, Aster (front and back) and the script of the first room.

## En 5 étapes

1. Crée un projet **Unity 6 « 2D (URP) »** (le modèle 2D contient déjà les paquets utiles : 2D Sprite et 2D Tilemap).
2. Ouvre **Window > Package Manager**, choisis **LerpUp Testeur**, onglet **Samples**, puis **Import** à côté de « Kit de départ ».
3. Ouvre le menu **LerpUp > Créer la salle de départ**. Unity règle les images (pixel art net), découpe les feuilles d'Aster et construit la scène `Assets/LerpUp/Salle/SalleDepart.unity`.
4. Appuie sur **Play** : Aster se déplace avec les flèches. Si la Console parle de `Input`, mets **Active Input Handling** sur « Both » (Edit > Project Settings > Player > Other Settings).
5. Tu es prêt pour la salle « Comprendre le code qui fait marcher ton perso ».

## In 5 steps

1. Create a **Unity 6 "2D (URP)"** project (the 2D template already has the packages we need: 2D Sprite and 2D Tilemap).
2. Open **Window > Package Manager**, select **LerpUp Testeur**, **Samples** tab, then **Import** next to "Kit de départ".
3. Run the menu **LerpUp > Créer la salle de départ**. Unity sets up the images (sharp pixel art), slices Aster's sheets and builds the scene `Assets/LerpUp/Salle/SalleDepart.unity`.
4. Press **Play**: Aster moves with the arrow keys. If the Console mentions `Input`, set **Active Input Handling** to "Both" (Edit > Project Settings > Player > Other Settings).
5. You are ready for the room "Understand the code that makes your character move".

## Ce que contient le kit / What is inside

| Dossier | Contenu |
|---|---|
| `Scripts/DeplacementJoueur.cs` | Le script de la première salle (déplacement avec `Translate`). Les scripts des salles suivantes sont dans leur page, avec un bouton « Télécharger le .cs ». |
| `Sprites/Aster/` | Aster : face (immobile, marche) et dos (immobile, marche). Toutes les images font 32 × 32. Le profil n'est pas inclus : voir « Ajouter le profil d'Aster » plus bas. |
| `Sprites/Decor/` | `sol-iso` (un losange de 64 × 32), `bloc-iso` (un cube pour passer devant / derrière) et `ombre` (pour le saut). |
| `Editor/CreerSalleDepart.cs` | Le menu qui crée la salle. |

Aster n'a volontairement **ni Rigidbody2D ni collider** : tu les ajoutes dans la salle « Faire avancer ton perso sans traverser les murs ». Les animations ne sont pas faites non plus : tu les crées dans la salle « Animer ton perso selon la direction où il va ».

Aster has **no Rigidbody2D and no collider** on purpose: you add them in the room "Make your character move without going through walls". The animations are not built either: you create them in the room "Animate your character by the direction it walks".

## Ajouter le profil d'Aster

Le kit contient Aster de face et de dos. Son profil (de côté) n'est pas dans le kit : la licence du pack ne permet pas de le redistribuer. Sans lui, tout marche quand même : Aster reste de face dans la scène. Tu en as besoin pour la salle « Animer ton perso selon la direction où il va » (états Droite, Haut-droite, Bas-droite).

1. Télécharge gratuitement le pack **Hooded Seeker** : https://primitive-pixels-games.itch.io/hooded-seeker (bouton « Download », tu peux mettre 0 $).
2. Dézippe-le. Les images sont des feuilles : plusieurs dessins côte à côte sur une seule ligne. Choisis celles du profil : immobile (idle), course (run) et saut (jump).
3. Si tu veux, recolore-les aux couleurs d'Aster dans un logiciel de pixel art (Aseprite, LibreSprite, Piskel…). Ce n'est pas obligatoire.
4. Crée le dossier `Assets/LerpUp/Aster/Profil` dans Unity et glisse tes images dedans.
5. Clique sur une image et, dans l'Inspector, règle : **Texture Type** = Sprite (2D and UI), **Sprite Mode** = Multiple, **Pixels Per Unit** = 32, **Filter Mode** = Point (no filter), **Compression** = None. Puis **Sprite Editor > Slice > Grid By Cell Size** avec la taille d'une case du pack (32 × 32 si ce sont des cases de 32), puis **Apply**.
6. Branche-les : dans la fenêtre Animation, crée un clip par feuille (par exemple `Droite_idle`, `Droite_course`) en glissant ses cases dedans, puis mets ces clips dans l'Animator d'Aster (états Droite, Haut-droite, Bas-droite).
7. Pour aller à gauche, ne dessine rien de plus : coche **Flip X** sur le **Sprite Renderer** d'Aster.

## Add Aster's side view

The kit has Aster from the front and from the back. The side view is not in the kit, because the pack's license does not allow redistributing it. Everything still works without it: Aster just stays front-facing in the scene. You need it for the room "Animate your character by the direction it walks" (states Right, Up-right, Down-right).

1. Download the free **Hooded Seeker** pack: https://primitive-pixels-games.itch.io/hooded-seeker (click "Download", you can pay $0).
2. Unzip it. The images are sheets: several drawings side by side on one row. Pick the side-view ones: idle, run and jump.
3. If you want, recolor them to Aster's colors in a pixel art tool (Aseprite, LibreSprite, Piskel…). Optional.
4. Create the folder `Assets/LerpUp/Aster/Profil` in Unity and drop your images in it.
5. Click an image and, in the Inspector, set: **Texture Type** = Sprite (2D and UI), **Sprite Mode** = Multiple, **Pixels Per Unit** = 32, **Filter Mode** = Point (no filter), **Compression** = None. Then **Sprite Editor > Slice > Grid By Cell Size** with the pack's frame size (32 × 32 if the frames are 32 px), then **Apply**.
6. Plug them in: in the Animation window, create one clip per sheet (for example `Droite_idle`, `Droite_course`) by dragging its frames in, then put those clips in Aster's Animator (states Right, Up-right, Down-right).
7. To go left, draw nothing more: tick **Flip X** on Aster's **Sprite Renderer**.

Crédits : voir `CREDITS.md`.
