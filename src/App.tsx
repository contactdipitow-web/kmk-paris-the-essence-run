import { useEffect, useMemo, useRef, useState } from 'react';
import GameScene from './GameScene';
import { audio } from './audio';
import { themes, useGame } from './store';

export default function App() {
  const phase = useGame((s) => s.phase);
  const start = useGame((s) => s.start);
  const shift = useGame((s) => s.shift);
  const jump = useGame((s) => s.jump);
  const slide = useGame((s) => s.slide);
  const score = useGame((s) => s.score);
  const essence = useGame((s) => s.essence);
  const combo = useGame((s) => s.combo);
  const best = useGame((s) => s.best);
  const lane = useGame((s) => s.lane);
  const [muted, setMuted] = useState(false);
  const pointer = useRef<{ x: number; y: number } | null>(null);
  const theme = useMemo(() => themes[Math.floor(score / 1300) % themes.length], [score]);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'ArrowLeft' || event.key.toLowerCase() === 'a') { shift(-1); audio.move(); }
      if (event.key === 'ArrowRight' || event.key.toLowerCase() === 'd') { shift(1); audio.move(); }
      if (event.key === 'ArrowUp' || event.key === ' ' || event.key.toLowerCase() === 'w') { jump(); audio.jump(); }
      if (event.key === 'ArrowDown' || event.key.toLowerCase() === 's') { slide(); audio.slide(); }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [jump, shift, slide]);

  const begin = () => {
    audio.start();
    start();
  };

  const onPointerDown = (event: React.PointerEvent) => {
    pointer.current = { x: event.clientX, y: event.clientY };
  };

  const onPointerUp = (event: React.PointerEvent) => {
    if (!pointer.current || phase !== 'running') return;
    const dx = event.clientX - pointer.current.x;
    const dy = event.clientY - pointer.current.y;
    pointer.current = null;
    if (Math.max(Math.abs(dx), Math.abs(dy)) < 28) return;
    if (Math.abs(dx) > Math.abs(dy)) {
      shift(dx > 0 ? 1 : -1);
      audio.move();
    } else if (dy < 0) {
      jump();
      audio.jump();
    } else {
      slide();
      audio.slide();
    }
  };

  return (
    <main className="app" onPointerDown={onPointerDown} onPointerUp={onPointerUp}>
      <section className="game-canvas"><GameScene /></section>

      <header className="top-brand">
        <div><div className="brand">KMK PARIS</div><div className="game-name">THE ESSENCE RUN</div></div>
        <button className="sound" onClick={(e) => { e.stopPropagation(); setMuted(audio.toggle()); }}>{muted ? 'SON OFF' : 'SON ON'}</button>
      </header>

      {phase === 'running' && (
        <>
          <div className="hud">
            <Hud label="SCORE" value={score.toString().padStart(5, '0')} />
            <Hud label="ESSENCE" value={String(essence).padStart(2, '0')} />
            <Hud label="COMBO" value={`x${Math.max(1, combo)}`} />
          </div>
          <div className="chapter" style={{ borderColor: theme.accent, color: theme.glow }}>{theme.name}</div>
          <div className="lane-indicator">{[0, 1, 2].map((i) => <span key={i} className={i === lane ? 'active' : ''} />)}</div>
          <div className="gesture-hint">SWIPE ← → · ↑ SAUT · ↓ GLISSADE</div>
        </>
      )}

      {phase === 'menu' && (
        <div className="overlay">
          <div className="hero-mark">K</div>
          <div className="eyebrow">PARIS · FRAGRANCE · MOUVEMENT</div>
          <h1>THE<br />ESSENCE RUN</h1>
          <p>Traverse un Paris nocturne en 3D. Collecte l’Essence, saute, glisse et évite le bruit.</p>
          <button className="play" onClick={begin}>COMMENCER LE RUN</button>
          <small>Prototype Web3D · aucune installation nécessaire</small>
        </div>
      )}

      {phase === 'gameover' && (
        <div className="overlay gameover">
          <div className="eyebrow">RUN TERMINÉ</div>
          <h1>{score.toString().padStart(5, '0')}</h1>
          <div className="results"><span>{essence} ESSENCES</span><span>BEST {best.toString().padStart(5, '0')}</span></div>
          <button className="play" onClick={begin}>REJOUER</button>
        </div>
      )}
    </main>
  );
}

function Hud({ label, value }: { label: string; value: string }) {
  return <div className="hud-cell"><span>{label}</span><strong>{value}</strong></div>;
}
