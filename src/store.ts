import { create } from 'zustand';

export type Phase = 'menu' | 'running' | 'gameover';
export type RunnerAction = 'idle' | 'jump' | 'slide';

interface GameState {
  phase: Phase;
  lane: 0 | 1 | 2;
  action: RunnerAction;
  score: number;
  distance: number;
  essence: number;
  combo: number;
  best: number;
  speed: number;
  start: () => void;
  shift: (delta: -1 | 1) => void;
  jump: () => void;
  slide: () => void;
  advance: (dt: number) => void;
  collect: () => void;
  hit: () => void;
}

let actionTimer: number | undefined;

export const useGame = create<GameState>((set, get) => ({
  phase: 'menu',
  lane: 1,
  action: 'idle',
  score: 0,
  distance: 0,
  essence: 0,
  combo: 0,
  best: Number(localStorage.getItem('kmk-best') || 0),
  speed: 11.5,
  start: () => {
    if (actionTimer) window.clearTimeout(actionTimer);
    set({ phase: 'running', lane: 1, action: 'idle', score: 0, distance: 0, essence: 0, combo: 0, speed: 11.5 });
  },
  shift: (delta) => {
    if (get().phase !== 'running') return;
    set((state) => ({ lane: Math.max(0, Math.min(2, state.lane + delta)) as 0 | 1 | 2 }));
  },
  jump: () => {
    if (get().phase !== 'running' || get().action !== 'idle') return;
    set({ action: 'jump' });
    actionTimer = window.setTimeout(() => set({ action: 'idle' }), 720);
  },
  slide: () => {
    if (get().phase !== 'running' || get().action !== 'idle') return;
    set({ action: 'slide' });
    actionTimer = window.setTimeout(() => set({ action: 'idle' }), 650);
  },
  advance: (dt) => set((state) => {
    if (state.phase !== 'running') return state;
    const nextDistance = state.distance + state.speed * dt;
    const nextSpeed = Math.min(23, 11.5 + nextDistance / 180);
    return {
      distance: nextDistance,
      score: Math.floor(nextDistance * 7 + state.essence * 55),
      speed: nextSpeed,
    };
  }),
  collect: () => set((state) => {
    if (state.phase !== 'running') return state;
    return { essence: state.essence + 1, combo: Math.min(9, state.combo + 1) };
  }),
  hit: () => {
    const state = get();
    if (state.phase !== 'running') return;
    const best = Math.max(state.best, state.score);
    localStorage.setItem('kmk-best', String(best));
    set({ phase: 'gameover', best, combo: 0, action: 'idle' });
  },
}));

export const laneX = [-2.55, 0, 2.55] as const;

export const themes = [
  { name: 'LIANE LIBRE', bg: '#090906', fog: '#151109', road: '#171510', accent: '#d1b36c', glow: '#f0d89b', building: '#242019' },
  { name: "PALME D’HIVER", bg: '#070a0c', fog: '#111b20', road: '#131a1c', accent: '#9fc9d4', glow: '#d6f3f7', building: '#1c2528' },
  { name: 'RIVAGE CUIVRÉ', bg: '#0c0705', fog: '#21110b', road: '#1b120e', accent: '#c77c4d', glow: '#ffbd7b', building: '#2b1912' },
] as const;
