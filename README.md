# MatterCrafter
## Description de la structure : 
- Le dossier Prefabs contient tous les différents prefabs des fusions donc chaque objet fusionné
- Le dossier Script contient les scripts de fusions, suppression des items, gestion de la duplications et aussi les scripts liés au menu de chargement du jeu
- Tous les assets se trouves directement dans le dossier asset

## Codage intéressant : 
- Fusions :
  - Les fusions sont gérées grâce aux tags des éléments comme par exemple : DuplicateEau + DuplicateFeu = Vapeur
  - Pour voir si une fusions existe, je parcours une list des fusions disponibles pour chaque élément
  - Impossible de fusionner les éléments d'origine (dont le tag ne comprends pas Duplicate) afin de pouvoir continuer à faire des fusions avec les éléments de bases

- Duplication :
  - Quand un élément est dupliqué, il prend le tag "Duplicate" + le tag de l'objet d'origine
  - Quand un objet est laché après l'avoir récupérer, il disparait (fonction DestroyAfterDelay())
  - On peut dupliquer que 3 instances maximal par objet afin d'éviter les lags
  - On ne peux pas dupliquer les objets qui ont le tag Duplicate 

- Suppression :
  - Si il y a plus de trois objets ayant le même tag, alors, il supprime tous les objets en trop pour en n'avoir que trois

