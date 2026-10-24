# VR Archery Challenge

Jeu de tir à l'arc en réalité virtuelle réalisé avec **Unity 6** et le **XR Interaction Toolkit 3**, dans le cadre du module ST2OOS – UnityXR (Majeure T2IA, 2026-2027).

Le joueur saisit un arc et des flèches avec ses manettes, tend la corde et vise des cibles fixes ou mobiles. Chaque niveau fixe un objectif de points à atteindre avant la fin du chrono pour débloquer le suivant.

## Fonctionnalités

- **Tir naturel** : prendre une flèche dans le carquois, l'encocher, tendre la corde, relâcher
- **Physique** : trajectoire soumise à la gravité, vitesse selon la tension, flèche plantée dans la cible
- **Cibles** : 5 zones de score (10, 8, 6, 4, 2 points), cibles mobiles à vitesse aléatoire
- **4 niveaux** : Entraînement (temps libre, aide à la visée), Facile, Normal, Difficile
- **Progression** : le niveau suivant se débloque quand l'objectif est atteint, séparément pour chaque pseudo
- **Interface VR** : menu, HUD (niveau, temps, score, objectif), clavier virtuel pour le pseudo
- **Classement** : scores sauvegardés en JSON, un record par joueur et par niveau, trié du meilleur au moins bon
- **Sons 3D** : tension et relâchement de la corde, départ de la flèche, impacts, ambiance

Reste à faire : plusieurs arcs et flèches avec améliorations, réglage du volume dans le menu.

## Lancer le projet

1. Ouvrir le dossier avec **Unity 6000.0.48f1** (pipeline URP).
2. Ouvrir la scène `Assets/Scenes/Menu.unity` et lancer le Play.
3. Jouer avec un casque VR, ou avec le XR Device Simulator.

Scènes à inclure dans le build, dans cet ordre : `Menu` (0), `Niv1` (1).

## Organisation du code

| Domaine | Scripts |
| --- | --- |
| Gameplay | Arc, Fleche, Carquois, Cible, CibleMobile, AideVisee |
| Partie | PartieManager, ScoreManager, ParametresNiveau, DispositionNiveau, Progression |
| Interface | MenuPrincipal, BoutonNiveauMenu, ChoixPseudo, ClavierVirtuel, HUDPartie, ScoreAffichage, EnregistrementScore, AffichageClassement |
| Données | SauvegardeScores (scores.json dans `Application.persistentDataPath`) |
| Sons | ArcSons, FlecheSons, AmbianceSonore, ReglageVolume |

Les niveaux sont des ScriptableObjects (`Assets/Niveaux`) : durée, taille et vitesse des cibles, objectif et niveau suivant se règlent sans toucher au code. La scène `Niv1` sert aux quatre niveaux.

## Équipe et contributions

Une grande partie du travail a été faite à plusieurs sur un même poste : la plupart des commits sont donc sur un seul compte. Les commits faits à plusieurs portent désormais une ligne `Co-authored-by`.

| Membre | Travail réalisé |
| --- | --- |
| Raphaël Girard (comptes « Raphaël » et « Rolwen ») | Arc, flèches, niveaux, scores, classement, Git |
| Femi Oussou | Écran de fin de partie et HUD |
| Robinson Diallo | Menu principal VR |
| Yassine Hadded | Sons 3D, ambiance sonore, carquois |
| Orlane Ndjibou-Madoungou | Placement des cibles, tests en casque |

## Workflow Git

Une branche `feature/<nom>` par fonctionnalité, puis une pull request vers `main` :
`tir-naturel`, `sons`, `partie-difficulte`, `menu`, `fin-de-partie`, `classement` (7 pull requests fusionnées).
