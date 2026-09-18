export type Lane = 0 | 1 | 2;
export type WorldId = 'liane' | 'palme' | 'rivage';
export type PowerType = 'shield' | 'slow' | 'magnet';
export type EntityKind = 'essence' | 'powerup' | 'obstacle';

export type Entity = {
  id: number;
  kind: EntityKind;
  lane: Lane;
  y: number;
  worldId: WorldId;
  power?: PowerType;
};

export type World = {
  id: WorldId;
  name: string;
  chapter: string;
  place: string;
  promise: string;
  power: PowerType;
  powerName: string;
  powerLabel: string;
  powerEffect: string;
  icon: string;
  palette: {
    page: string;
    skyTop: string;
    skyBottom: string;
    accent: string;
    accentSoft: string;
    ink: string;
    path: string;
    pathEdge: string;
    bottle: string;
  };
};

export const WORLD_SCORE_SPAN = 1200;

export const WORLDS: readonly World[] = [
  {
    id: 'liane',
    name: 'LIANE LIBRE',
    chapter: 'CHAPITRE I',
    place: 'FORÊT AMAZONIENNE',
    promise: 'Cours sous la canopée, entre lianes, oiseaux et rivières émeraude.',
    power: 'shield',
    powerName: 'LIANE PROTECTRICE',
    powerLabel: 'BOUCLIER · 1 CHOC',
    powerEffect: 'Absorbe le prochain obstacle.',
    icon: '✦',
    palette: {
      page: '#DDF6E8',
      skyTop: '#7BE4C2',
      skyBottom: '#F4D986',
      accent: '#087B58',
      accentSoft: '#B9EBCB',
      ink: '#123F34',
      path: '#D9B37C',
      pathEdge: '#7FB978',
      bottle: '#F3C94F',
    },
  },
  {
    id: 'palme',
    name: 'PALME D’HIVER',
    chapter: 'CHAPITRE II',
    place: 'OASIS POLAIRE',
    promise: 'Traverse un jardin de palmes givrées sous une aurore turquoise et dorée.',
    power: 'slow',
    powerName: 'SOUFFLE POLAIRE',
    powerLabel: 'RALENTI · 7 SEC',
    powerEffect: 'Ralentit le monde pendant 7 secondes.',
    icon: '❄',
    palette: {
      page: '#E5F7FA',
      skyTop: '#8DE7E1',
      skyBottom: '#FFF0B6',
      accent: '#137F91',
      accentSoft: '#C8F0EE',
      ink: '#164D5B',
      path: '#F7EAD5',
      pathEdge: '#A2DDE1',
      bottle: '#8BDDE8',
    },
  },
  {
    id: 'rivage',
    name: 'RIVAGE CUIVRÉ',
    chapter: 'CHAPITRE III',
    place: 'CÔTE DE TAGHAZOUT',
    promise: 'File entre falaises cuivrées, vagues atlantiques et silhouettes au couchant.',
    power: 'magnet',
    powerName: 'APPEL DU RIVAGE',
    powerLabel: 'AIMANT · 7 SEC',
    powerEffect: 'Attire toutes les essences proches.',
    icon: '≈',
    palette: {
      page: '#FFE8D5',
      skyTop: '#F78D72',
      skyBottom: '#FFD78E',
      accent: '#A64631',
      accentSoft: '#FFD2BB',
      ink: '#5B2C2A',
      path: '#CF8056',
      pathEdge: '#F0B46D',
      bottle: '#E9935F',
    },
  },
] as const;

export function worldForScore(score: number): World {
  const index = Math.floor(Math.max(0, score) / WORLD_SCORE_SPAN) % WORLDS.length;
  return WORLDS[index]!;
}

export function worldProgress(score: number): number {
  return (Math.max(0, score) % WORLD_SCORE_SPAN) / WORLD_SCORE_SPAN;
}

export function nextWorldForScore(score: number): World {
  const index = (Math.floor(Math.max(0, score) / WORLD_SCORE_SPAN) + 1) % WORLDS.length;
  return WORLDS[index]!;
}
