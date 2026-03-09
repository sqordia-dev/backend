const http = require('http');

const TOKEN = process.argv[2];
const ORG_ID = 'd95e4a1d-6257-4b14-8a29-79c6af92182a';

function apiCall(method, path, body) {
  return new Promise((resolve, reject) => {
    const data = body ? JSON.stringify(body) : null;
    const opts = {
      hostname: 'localhost', port: 5241, path, method,
      headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + TOKEN }
    };
    if (data) opts.headers['Content-Length'] = Buffer.byteLength(data);
    const req = http.request(opts, res => {
      let b = '';
      res.on('data', c => b += c);
      res.on('end', () => {
        try { resolve({ status: res.statusCode, data: JSON.parse(b) }); }
        catch { resolve({ status: res.statusCode, data: b }); }
      });
    });
    req.on('error', reject);
    if (data) req.write(data);
    req.end();
  });
}

// Cafe Soleil answers for each question number
const ANSWERS = {
  1: "Cafe Soleil est un cafe artisanal specialise dans les cafes de specialite torrefies localement et les patisseries maison. Nous offrons une experience chaleureuse et authentique dans un cadre moderne et accueillant au coeur du Plateau Mont-Royal a Montreal.",
  2: "Notre mission est de democratiser l'acces aux cafes de specialite tout en soutenant les producteurs locaux et en creant un lieu de rassemblement communautaire. Nous croyons que chaque tasse de cafe raconte une histoire et merite d'etre savouree.",
  3: "Le probleme principal est le manque de cafes independants offrant des produits de qualite superieure a prix accessible dans le quartier. Les grandes chaines dominent le marche avec des produits standardises, et les residents cherchent une alternative locale authentique.",
  4: "Notre solution est un cafe artisanal qui combine torrefaction sur place, patisseries maison preparees quotidiennement, et un espace de travail collaboratif. Nous proposons un abonnement mensuel 'Club Soleil' offrant des rabais et des degustations exclusives.",
  5: "Nous operons dans l'industrie de la restauration rapide et des boissons de specialite (SCIAN 7225). Le marche canadien du cafe de specialite est evalue a 4.3 milliards CAD en 2025, avec une croissance annuelle de 8.2%. Le segment artisanal represente 23% du marche.",
  6: "Notre clientele cible principale est composee de jeunes professionnels ages de 25 a 45 ans, residents du Plateau Mont-Royal et des quartiers avoisinants. Ils sont soucieux de la qualite, valorisent les produits locaux et sont prets a payer un premium pour une experience superieure. Revenu moyen de 55 000$ a 85 000$.",
  7: "Nos principaux concurrents sont: 1) Starbucks (grande chaine, prix moyen, qualite standard), 2) Cafe Myriade (cafe de specialite haut de gamme), 3) Dispatch Coffee (torrefacteur local). Notre avantage: torrefaction sur place + patisseries maison + programme de fidelite innovant + espace coworking.",
  8: "Notre modele de revenus repose sur: 1) Ventes de boissons (60% du CA), 2) Patisseries et restauration legere (25%), 3) Vente de grains torrefies au detail (10%), 4) Abonnements Club Soleil et location d'espace evenementiel (5%). Prix moyen d'une boisson: 5.50$, ticket moyen: 12$.",
  9: "Notre strategie marketing comprend: presence active sur Instagram et TikTok avec contenu visuel de qualite, partenariats avec des influenceurs locaux, programme de parrainage, evenements de degustation mensuels, collaboration avec des commerces du quartier, et presence sur les plateformes de livraison (UberEats, DoorDash).",
  10: "L'equipe fondatrice comprend: Marie Dupont (CEO, 10 ans d'experience en restauration, diplome en gestion hoteliere de l'ITHQ), Jean-Francois Tremblay (Chef patissier, formation au Cordon Bleu), et Sophie Chen (Directrice marketing, ex-agence numerique). Nous prevoyons embaucher 4 baristas et 2 assistants cuisine.",
  11: "Les operations quotidiennes incluent: ouverture a 6h30, torrefaction des grains 3 fois par semaine, preparation des patisseries des 5h, service continu jusqu'a 21h. Nous utilisons un systeme de point de vente Square, gestion des stocks avec MarketMan, et comptabilite avec QuickBooks. Approvisionnement en grains verts via importateurs certifies equitables.",
  12: "Les jalons cles sont: Mois 1-3: Renovation du local et obtention des permis. Mois 4: Lancement soft opening. Mois 6: Lancement du Club Soleil. Mois 9: Introduction de la livraison. Mois 12: Atteinte du seuil de rentabilite. Mois 18: Exploration d'un deuxieme emplacement.",
  13: "Les risques principaux sont: 1) Hausse du prix des grains de cafe verts (mitigation: contrats a terme), 2) Concurrence accrue (mitigation: differenciation par la qualite), 3) Saisonnalite (mitigation: menu adapte aux saisons), 4) Penurie de main-d'oeuvre (mitigation: conditions de travail superieures et formation continue).",
  14: "Nous recherchons un financement total de 350 000$: 1) Investissement personnel des fondateurs: 100 000$, 2) Pret bancaire BDC: 150 000$, 3) Subvention Futurpreneur: 50 000$, 4) Investisseur providentiel: 50 000$. Les fonds serviront a la renovation (150K), l'equipement (120K), le fonds de roulement (50K) et le marketing de lancement (30K).",
  15: "Projections financieres sur 3 ans: An 1 - CA: 480 000$, Cout des ventes: 168 000$ (35%), Charges fixes: 264 000$, Resultat net: 48 000$. An 2 - CA: 624 000$ (+30%), Resultat net: 93 600$. An 3 - CA: 780 000$ (+25%), Resultat net: 148 200$. Seuil de rentabilite atteint au mois 10.",
  16: "Notre structure juridique est une societe par actions (SPA) incorporee au Quebec. Les trois cofondateurs detiennent respectivement 40%, 30% et 30% des actions. Nous avons retenu les services du cabinet juridique Fasken pour la constitution et les contrats commerciaux.",
  17: "Notre impact social et environnemental inclut: approvisionnement 100% equitable et biologique, utilisation de gobelets compostables, programme de compostage des dechets de cafe, partenariat avec la banque alimentaire locale pour redistribuer les invendus, et embauche prioritaire de personnes issues de l'immigration.",
  18: "La propriete intellectuelle comprend: la marque Cafe Soleil (enregistree aupres de l'OPIC), nos recettes de patisseries exclusives, notre melange de cafe signature 'Blend Soleil', et notre application mobile de fidelite developpee sur mesure.",
  19: "Notre strategie de sortie a long terme prevoit soit la franchise du concept Cafe Soleil apres 5 ans d'exploitation reussie avec 3 succursales, soit la vente de l'entreprise a un groupe de restauration etabli. Valorisation cible de 2.5M$ a 5 ans.",
  20: "En resume, Cafe Soleil represente une opportunite unique de capturer une part du marche croissant du cafe de specialite a Montreal. Avec une equipe experimentee, un modele d'affaires eprouve et un positionnement distinctif, nous sommes confiants d'atteindre nos objectifs de croissance et de devenir une reference incontournable du cafe artisanal au Quebec."
};

async function main() {
  // 1. Create business plan
  console.log('=== CREATING BUSINESS PLAN ===');
  const bp = await apiCall('POST', '/api/v1/business-plans', {
    title: 'Cafe Soleil - Plan d\'affaires',
    planType: 'BusinessPlan',
    organizationId: ORG_ID,
    persona: 'Entrepreneur',
    onboardingContext: {
      industry: 'Restauration / Cafe',
      businessStage: 'Startup',
      teamSize: '5-10',
      fundingStatus: 'Seeking',
      goals: ['Ouvrir un cafe artisanal', 'Atteindre rentabilite en 18 mois'],
      targetMarket: 'Jeunes professionnels 25-45 ans'
    }
  });

  if (bp.status !== 200 && bp.status !== 201) {
    console.log('Failed to create BP:', bp.status, JSON.stringify(bp.data).substring(0, 500));
    return;
  }

  const bpId = bp.data.id;
  console.log('Business Plan created! ID:', bpId);

  // 2. Get questionnaire
  console.log('\n=== FETCHING QUESTIONNAIRE ===');
  const q = await apiCall('GET', `/api/v1/business-plans/${bpId}/questionnaire`);

  if (q.status !== 200) {
    console.log('Failed to get questionnaire:', q.status, JSON.stringify(q.data).substring(0, 500));
    return;
  }

  const questions = q.data.questions || q.data;
  console.log('Total questions:', questions.length);

  // 3. Submit answers for each question
  console.log('\n=== SUBMITTING ANSWERS ===');
  for (const question of questions) {
    const qNum = question.questionNumber || question.order;
    const qId = question.questionTemplateId || question.id;
    const answer = ANSWERS[qNum];

    if (!answer) {
      console.log(`Q${qNum}: No answer prepared, skipping`);
      continue;
    }

    const resp = await apiCall('POST', `/api/v1/business-plans/${bpId}/questionnaire/responses`, {
      questionTemplateId: qId,
      responseText: answer
    });

    if (resp.status === 200 || resp.status === 201) {
      console.log(`Q${qNum}: OK - ${question.questionText ? question.questionText.substring(0, 60) : 'submitted'}...`);
    } else {
      console.log(`Q${qNum}: FAILED (${resp.status}) - ${JSON.stringify(resp.data).substring(0, 200)}`);
    }
  }

  console.log('\n=== DONE ===');
  console.log('Business Plan ID:', bpId);
  console.log('All answers submitted. You can now proceed in the UI.');
}

main().catch(console.error);
