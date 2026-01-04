 🧭 EPIC 1 : Parcours Utilisateur & Génération de Plan
User Story 1 : Choix du type de plan
 En tant qu’entrepreneur ou OBNL, je veux choisir le type de plan (affaires ou stratégique) afin d’obtenir un flux adapté.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un nouvel utilisateur accède à la page d’accueil,
 When il clique sur "Créer un plan",
 Then il peut choisir entre "Plan d’affaires" et "Plan stratégique".
 And le type sélectionné détermine la suite du parcours.

User Story 2 : Questionnaire guidé intelligent
 En tant qu’entrepreneur ou OBNL, je veux répondre à un questionnaire de 20 questions sans saisie complexe afin de générer mon plan.
 SP : 8 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur a choisi son type de plan,
 When il débute le questionnaire,
 Then il voit des questions une par une avec choix de réponses.
 And le système sauvegarde automatiquement les réponses à chaque étape.

User Story 3 : Suggestions IA contextuelles
 En tant qu’entrepreneur, je veux recevoir des suggestions automatiques selon mon secteur afin d’enrichir mes réponses.
 SP : 8 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur répond à une question,
 When il soumet sa réponse,
 Then deux suggestions contextuelles apparaissent.
 And une explication indique la logique IA utilisée.

User Story 4 : Compilation automatique du plan
 En tant qu’entrepreneur, je veux voir mon plan se compiler automatiquement afin de visualiser le résultat complet.
 SP : 5 | MoSCoW : M
 Critères d’acceptation :
 Given un questionnaire complété,
 When l’utilisateur clique sur "Voir mon plan",
 Then le plan complet se génère automatiquement.
 And toutes les sections sont structurées selon le type de plan.

User Story 5 : Édition visuelle et textuelle
 En tant qu’entrepreneur, je veux personnaliser le texte, les images et le branding afin d’adapter le plan à mon identité visuelle.
 SP : 8 | MoSCoW : S
 Critères d’acceptation :
 Given un plan généré,
 When l’utilisateur clique sur une section,
 Then il peut modifier le texte et les images.
 And l’historique des modifications est sauvegardé.

User Story 6 : Export multi-format
 En tant qu’entrepreneur, je veux exporter mon plan en PDF, Word, Excel ou PowerPoint afin de le partager facilement.
 SP : 5 | MoSCoW : M
 Critères d’acceptation :
 Given un plan finalisé,
 When l’utilisateur sélectionne “Exporter”,
 Then il peut choisir le format souhaité.
 And le design et la structure du plan sont conservés.

User Story 7 : Collaboration multi-utilisateurs
 En tant qu’entrepreneur, je veux inviter d’autres personnes à collaborer sur mon plan afin de travailler en équipe.
 SP : 8 | MoSCoW : S
 Critères d’acceptation :
 Given un utilisateur propriétaire d’un plan,
 When il invite un collaborateur,
 Then celui-ci reçoit un lien d’accès.
 And les rôles et permissions définissent l’accès aux sections.













🧠 EPIC 2 : Modules IA Intelligents
User Story 8 : StrategySuggestions IA
 En tant qu’entrepreneur, je veux obtenir des stratégies de croissance générées par l’IA afin d’améliorer mon plan.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given un plan en cours,
 When l’utilisateur demande “Idées de stratégies”,
 Then l’IA génère au moins trois recommandations cohérentes avec le secteur.

User Story 9 : Risk Mitigation IA
 En tant qu’entrepreneur, je veux détecter les risques et leurs plan de mitigation afin de réduire les menaces dans mon plan.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given les données du plan complètes,
 When l’utilisateur active l’analyse des risques,
 Then l’IA identifie au moins trois risques avec des mesures d’atténuation.

User Story 10 : Business Mentor IA
 En tant que coach, je veux que l’IA analyse les réponses pour détecter des opportunités cachées afin de guider mes clients.
 SP : 8 | MoSCoW : C
 Critères d’acceptation :
 Given un utilisateur souhaite un retour global,
 When il active “Analyse IA complète”,
 Then un rapport synthétique avec opportunités et faiblesses s’affiche.





📊 EPIC 3 : Business Planning – Contenu du Plan
User Story 11 : Génération automatique SWOT
 En tant qu’entrepreneur, je veux générer une analyse SWOT automatique afin d’avoir une vue synthétique de ma situation.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un plan généré,
 When l’utilisateur accède à la section SWOT,
 Then les forces, faiblesses, opportunités et menaces sont préremplies.

User Story 12 : Pricing & Market Analysis
 En tant qu’entrepreneur, je veux une simulation de prix et une analyse concurrentielle afin d’ajuster ma stratégie de marché.
 SP : 8 | MoSCoW : S
 Critères d’acceptation :
 Given des données d’entrée (produit, marché, coûts),
 When l’utilisateur valide,
 Then l’IA affiche une grille de prix et un rapport concurrentiel.

User Story 13 : Objectifs SMART
 En tant qu’entrepreneur, je veux générer des objectifs SMART afin de suivre la progression de mon plan.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given la stratégie choisie,
 When l’utilisateur clique “Générer mes objectifs”,
 Then des objectifs mesurables et temporels s’affichent.






💰 EPIC 4 : Financial Projections
User Story 14 : Projections financières automatiques
 En tant qu’entrepreneur, je veux générer automatiquement mes projections financières afin de gagner du temps et éviter les erreurs.
 SP : 8 | MoSCoW : M
 Critères d’acceptation :
 Given les données de ventes et dépenses saisies,
 When l’utilisateur clique “Générer mes projections”,
 Then le système calcule revenus, marges et prévisions sur 3 ans.

User Story 15 : Scénarios What-if
 En tant qu’entrepreneur, je veux simuler plusieurs scénarios financiers afin d’évaluer mes options stratégiques.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given des projections générées,
 When l’utilisateur modifie des hypothèses,
 Then trois scénarios (optimiste, réaliste, pessimiste) sont affichés.

User Story 16 : Multi-devises et fiscalité locale
 En tant qu’entrepreneur international, je veux gérer mes projections multi-devises afin d’adapter mon plan selon le pays.
 SP : 8 | MoSCoW : C
 Critères d’acceptation :
 Given un utilisateur international,
 When il sélectionne une devise,
 Then tous les montants du plan s’adaptent automatiquement.






🤝 EPIC 5 : Collaboration & Expérience Utilisateur
User Story 17 : Commentaires collaboratifs
 En tant qu’entrepreneur, je veux commenter les sections du plan afin d’échanger avec mes collaborateurs.
 SP : 5 | MoSCoW : C
 Critères d’acceptation :
 Given plusieurs collaborateurs sur un plan,
 When un utilisateur sélectionne une section,
 Then il peut laisser un commentaire visible par tous.

User Story 18 : Sauvegarde et navigation fluide
 En tant qu’utilisateur, je veux que mes données soient sauvegardées automatiquement afin d’éviter toute perte d’information.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur répond à une question,
 When il quitte ou change d’écran,
 Then ses données sont sauvegardées automatiquement.

User Story 19 : Transparence IA
 En tant qu’entrepreneur, je veux comprendre la logique derrière les suggestions IA afin de valider leur pertinence.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given une suggestion IA,
 When l’utilisateur clique sur “Pourquoi cette suggestion ?”,
 Then une explication synthétique apparaît.





🌐 EPIC 6 : Site vitrine, Authentification & Gestion Administrative
User Story 20 : Page d’accueil du site vitrine
 En tant qu’entrepreneur ou OBNL, je veux consulter une page présentant la plateforme afin de comprendre son utilité.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un visiteur,
 When il accède au site,
 Then il voit la présentation du produit et le CTA “Créer mon plan”.

User Story 21 : Parcours d’inscription
 En tant qu’entrepreneur, je veux créer un compte facilement afin d’accéder à mon espace personnel.
 SP : 5 | MoSCoW : M
 Critères d’acceptation :
 Given un visiteur,
 When il clique sur “S’inscrire”,
 Then il peut créer un compte via e-mail ou Google/Microsoft.

User Story 22 : Connexion sécurisée
 En tant qu’utilisateur, je veux me connecter à mon compte afin d’accéder à mes plans.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur enregistré,
 When il saisit ses identifiants,
 Then il est redirigé vers son tableau de bord.

User Story 23 : Réinitialisation de mot de passe
 En tant qu’utilisateur, je veux réinitialiser mon mot de passe afin de regagner l’accès à mon compte.
 SP : 3 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur ayant oublié son mot de passe,
 When il demande une réinitialisation,
 Then il reçoit un courriel sécurisé.

User Story 24 : Tableau de bord utilisateur
 En tant qu’entrepreneur, je veux accéder à un tableau de bord regroupant mes plans afin de suivre leur avancement.
 SP : 5 | MoSCoW : M
 Critères d’acceptation :
 Given un utilisateur connecté,
 When il accède à son espace,
 Then il voit la liste de ses plans et leur statut.

User Story 25 : Gestion des abonnements et paiements
 En tant qu’entrepreneur, je veux gérer mes abonnements afin d’adapter mon plan tarifaire selon mes besoins.
 SP : 8 | MoSCoW : S
 Critères d’acceptation :
 Given un utilisateur connecté,
 When il choisit un plan d’abonnement,
 Then le paiement est sécurisé et un reçu est envoyé.

User Story 26 : Espace administrateur – gestion des comptes
 En tant qu’administrateur, je veux gérer les utilisateurs afin de contrôler l’accès à la plateforme.
 SP : 8 | MoSCoW : M
 Critères d’acceptation :
 Given un administrateur,
 When il se connecte à l’espace admin,
 Then il peut voir, bloquer ou supprimer des utilisateurs.

User Story 27 : Espace administrateur – gestion du contenu
 En tant qu’administrateur, je veux modifier le contenu du site vitrine afin de mettre à jour les informations publiques.
 SP : 5 | MoSCoW : S
 Critères d’acceptation :
 Given un administrateur connecté,
 When il édite le contenu,
 Then les changements s’affichent immédiatement.

User Story 28 : Journalisation et sécurité
 En tant qu’administrateur, je veux enregistrer les actions critiques afin d’assurer la traçabilité et la sécurité de la plateforme.
 SP : 8 | MoSCoW : S
 Critères d’acceptation :
 Given un événement important (connexion, suppression, paiement),
 When il survient,
 Then il est enregistré dans un journal d’audit consultable par les administrateurs.
