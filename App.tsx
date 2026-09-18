import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Animated,
  Dimensions,
  LayoutChangeEvent,
  PanResponder,
  Pressable,
  SafeAreaView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import * as Haptics from 'expo-haptics';
import { StatusBar } from 'expo-status-bar';

import {
  Entity,
  Lane,
  PowerType,
  WORLD_SCORE_SPAN,
  WORLDS,
  World,
  nextWorldForScore,
  worldForScore,
  worldProgress,
} from './src/gameData';
import { WorldBackdrop } from './src/WorldBackdrop';

type Phase = 'idle' | 'running' | 'paused' | 'ended';
type PowerDisplay = { type: PowerType; label: string; detail: string; remaining: number };
type ToastMessage = { title: string; detail: string };

const { width: SCREEN_WIDTH, height: SCREEN_HEIGHT } = Dimensions.get('window');
const TRACK_WIDTH = Math.min(SCREEN_WIDTH - 24, 430);
const LANE_WIDTH = TRACK_WIDTH / 3;
const PLAYER_BOTTOM = 66;
const PLAYER_WIDTH = 62;
const PLAYER_HEIGHT = 88;
const ENTITY_HIT_HEIGHT = 58;
const TICK_MS = 32;
const POWER_DURATION_MS = 7_000;
const BASE_SPEED = 215;
const MAX_SPEED = 410;

const laneX = (lane: Lane) => lane * LANE_WIDTH + LANE_WIDTH / 2 - PLAYER_WIDTH / 2;
const laneCenter = (lane: Lane) => lane * LANE_WIDTH + LANE_WIDTH / 2;

export default function App() {
  const [phase, setPhase] = useState<Phase>('idle');
  const phaseRef = useRef<Phase>('idle');
  const running = phase === 'running';

  const [lane, setLane] = useState<Lane>(1);
  const laneRef = useRef<Lane>(1);
  const playerX = useRef(new Animated.Value(laneX(1))).current;
  const playerBob = useRef(new Animated.Value(0)).current;

  const [entities, setEntities] = useState<Entity[]>([]);
  const entitiesRef = useRef<Entity[]>([]);
  const nextEntityIdRef = useRef(1);
  const spawnAccumulatorRef = useRef(0);

  const [score, setScore] = useState(0);
  const scoreRef = useRef(0);
  const [essence, setEssence] = useState(0);
  const essenceRef = useRef(0);
  const [best, setBest] = useState(0);
  const [combo, setCombo] = useState(0);
  const comboRef = useRef(0);
  const lastCollectAtRef = useRef(0);
  const gameTimeRef = useRef(0);

  const [trackHeight, setTrackHeight] = useState(Math.min(610, SCREEN_HEIGHT * 0.68));
  const trackHeightRef = useRef(trackHeight);

  const [activePower, setActivePower] = useState<PowerDisplay | null>(null);
  const activePowerTypeRef = useRef<PowerType | null>(null);
  const powerRemainingRef = useRef(-1);
  const shieldChargesRef = useRef(0);
  const slowUntilRef = useRef(0);
  const magnetUntilRef = useRef(0);

  const [toast, setToast] = useState<ToastMessage | null>(null);
  const toastTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const previousWorldIdRef = useRef(worldForScore(0).id);

  const world = worldForScore(score);
  const nextWorld = nextWorldForScore(score);
  const progress = worldProgress(score);
  const level = Math.floor(score / WORLD_SCORE_SPAN) + 1;

  const changePhase = useCallback((next: Phase) => {
    phaseRef.current = next;
    setPhase(next);
  }, []);

  const showToast = useCallback((title: string, detail: string) => {
    if (toastTimerRef.current) clearTimeout(toastTimerRef.current);
    setToast({ title, detail });
    toastTimerRef.current = setTimeout(() => setToast(null), 2_350);
  }, []);

  useEffect(
    () => () => {
      if (toastTimerRef.current) clearTimeout(toastTimerRef.current);
    },
    [],
  );

  const clearPower = useCallback(() => {
    activePowerTypeRef.current = null;
    shieldChargesRef.current = 0;
    slowUntilRef.current = 0;
    magnetUntilRef.current = 0;
    powerRemainingRef.current = -1;
    setActivePower(null);
  }, []);

  const moveToLane = useCallback(
    (nextLane: Lane) => {
      if (phaseRef.current !== 'running') return;
      laneRef.current = nextLane;
      setLane(nextLane);
      void Haptics.selectionAsync().catch(() => undefined);
      Animated.spring(playerX, {
        toValue: laneX(nextLane),
        useNativeDriver: true,
        damping: 20,
        stiffness: 260,
        mass: 0.58,
      }).start();
    },
    [playerX],
  );

  const moveBy = useCallback(
    (delta: -1 | 1) => {
      const next = Math.max(0, Math.min(2, laneRef.current + delta)) as Lane;
      if (next !== laneRef.current) moveToLane(next);
    },
    [moveToLane],
  );

  const panResponder = useMemo(
    () =>
      PanResponder.create({
        onMoveShouldSetPanResponder: (_, gesture) => phaseRef.current === 'running' && Math.abs(gesture.dx) > 8,
        onPanResponderRelease: (_, gesture) => {
          if (gesture.dx > 24) moveBy(1);
          if (gesture.dx < -24) moveBy(-1);
        },
      }),
    [moveBy],
  );

  const endRun = useCallback(() => {
    if (phaseRef.current !== 'running') return;
    changePhase('ended');
    setBest((current) => Math.max(current, Math.floor(scoreRef.current)));
    void Haptics.notificationAsync(Haptics.NotificationFeedbackType.Error).catch(() => undefined);
  }, [changePhase]);

  const resetGame = useCallback(() => {
    entitiesRef.current = [];
    setEntities([]);
    nextEntityIdRef.current = 1;
    spawnAccumulatorRef.current = 0;
    scoreRef.current = 0;
    setScore(0);
    essenceRef.current = 0;
    setEssence(0);
    comboRef.current = 0;
    setCombo(0);
    lastCollectAtRef.current = 0;
    gameTimeRef.current = 0;
    previousWorldIdRef.current = WORLDS[0]!.id;
    clearPower();
    laneRef.current = 1;
    setLane(1);
    playerX.setValue(laneX(1));
    changePhase('running');
    showToast('CHAPITRE I · LIANE LIBRE', 'La forêt amazonienne s’éveille.');
    void Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium).catch(() => undefined);
  }, [changePhase, clearPower, playerX, showToast]);

  const togglePause = useCallback(() => {
    if (phaseRef.current === 'running') {
      changePhase('paused');
      return;
    }
    if (phaseRef.current === 'paused') changePhase('running');
  }, [changePhase]);

  const onTrackLayout = useCallback((event: LayoutChangeEvent) => {
    const nextHeight = event.nativeEvent.layout.height;
    if (Math.abs(nextHeight - trackHeightRef.current) > 2) {
      trackHeightRef.current = nextHeight;
      setTrackHeight(nextHeight);
    }
  }, []);

  useEffect(() => {
    playerBob.setValue(0);
    if (!running) return undefined;
    const animation = Animated.loop(
      Animated.sequence([
        Animated.timing(playerBob, { toValue: -5, duration: 180, useNativeDriver: true, isInteraction: false }),
        Animated.timing(playerBob, { toValue: 1, duration: 180, useNativeDriver: true, isInteraction: false }),
      ]),
    );
    animation.start();
    return () => animation.stop();
  }, [playerBob, running]);

  useEffect(() => {
    if (previousWorldIdRef.current === world.id) return;
    previousWorldIdRef.current = world.id;
    if (phaseRef.current === 'running') {
      showToast(`${world.chapter} · ${world.name}`, world.promise);
      void Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success).catch(() => undefined);
    }
  }, [showToast, world]);

  useEffect(() => {
    if (!running) return undefined;

    let previousFrame = Date.now();

    const timer = setInterval(() => {
      if (phaseRef.current !== 'running') return;
      const frame = Date.now();
      const delta = Math.min(64, frame - previousFrame);
      previousFrame = frame;
      gameTimeRef.current += delta;
      const gameTime = gameTimeRef.current;

      scoreRef.current += delta * 0.017;
      const currentScore = Math.floor(scoreRef.current);
      const currentWorld = worldForScore(currentScore);

      if (comboRef.current > 0 && gameTime - lastCollectAtRef.current > 2_800) {
        comboRef.current = 0;
        setCombo(0);
      }

      const activeType = activePowerTypeRef.current;
      if (activeType === 'slow' || activeType === 'magnet') {
        const expiresAt = activeType === 'slow' ? slowUntilRef.current : magnetUntilRef.current;
        const remaining = Math.max(0, Math.ceil((expiresAt - gameTime) / 1000));
        if (remaining === 0) {
          clearPower();
        } else if (remaining !== powerRemainingRef.current) {
          const powerWorld = WORLDS.find((item) => item.power === activeType) ?? currentWorld;
          powerRemainingRef.current = remaining;
          setActivePower({
            type: activeType,
            label: powerWorld.powerName,
            detail: powerWorld.powerEffect,
            remaining,
          });
        }
      }

      const slowed = gameTime < slowUntilRef.current;
      const magnetized = gameTime < magnetUntilRef.current;
      const speed = Math.min(MAX_SPEED, BASE_SPEED + currentScore / 16) * (slowed ? 0.58 : 1);
      const spawnInterval = Math.max(540, 875 - currentScore / 10);
      spawnAccumulatorRef.current += delta;

      if (spawnAccumulatorRef.current >= spawnInterval) {
        spawnAccumulatorRef.current = 0;
        const nextId = () => nextEntityIdRef.current++;
        const make = (kind: Entity['kind'], laneValue: Lane, y: number, power?: PowerType): Entity => ({
          id: nextId(),
          kind,
          lane: laneValue,
          y,
          worldId: currentWorld.id,
          power,
        });
        const roll = Math.random();
        const row: Entity[] = [];

        if (roll < 0.13) {
          const powerLane = Math.floor(Math.random() * 3) as Lane;
          row.push(make('powerup', powerLane, -106, currentWorld.power));
        } else if (roll < 0.48) {
          const trailLane = Math.floor(Math.random() * 3) as Lane;
          row.push(make('essence', trailLane, -70));
          row.push(make('essence', trailLane, -148));
          if (currentScore > 900 && Math.random() > 0.45) row.push(make('essence', trailLane, -226));
        } else {
          const safeLane = Math.floor(Math.random() * 3) as Lane;
          const twoObstacles = currentScore > 460 && Math.random() > 0.42;
          const blocked = ([0, 1, 2] as Lane[]).filter((laneValue) => laneValue !== safeLane);
          row.push(make('obstacle', blocked[0]!, -70));
          if (twoObstacles) row.push(make('obstacle', blocked[1]!, -70));
          row.push(make('essence', safeLane, -78));
        }

        entitiesRef.current = [...entitiesRef.current, ...row];
      }

      const playerTop = trackHeightRef.current - PLAYER_BOTTOM - PLAYER_HEIGHT + 11;
      const playerBottom = trackHeightRef.current - PLAYER_BOTTOM + 7;
      const nextEntities: Entity[] = [];
      let crashed = false;
      let collectedCount = 0;
      let pickedPower: PowerType | null = null;
      let shieldWasUsed = false;

      for (const entity of entitiesRef.current) {
        const moved = { ...entity, y: entity.y + (speed * delta) / 1000 };
        const inPlayerZone = moved.y + ENTITY_HIT_HEIGHT >= playerTop && moved.y <= playerBottom;
        const sameLane = moved.lane === laneRef.current;
        const magnetCollect =
          magnetized && moved.kind === 'essence' && moved.y + ENTITY_HIT_HEIGHT >= playerTop - 185 && moved.y <= playerBottom;
        const touched = (sameLane && inPlayerZone) || magnetCollect;

        if (touched) {
          if (moved.kind === 'obstacle') {
            if (shieldChargesRef.current > 0) {
              shieldChargesRef.current = 0;
              activePowerTypeRef.current = null;
              powerRemainingRef.current = -1;
              setActivePower(null);
              shieldWasUsed = true;
              scoreRef.current += 30;
            } else {
              crashed = true;
            }
          } else if (moved.kind === 'powerup' && moved.power) {
            pickedPower = moved.power;
            scoreRef.current += 110;
          } else {
            collectedCount += 1;
          }
          continue;
        }

        if (moved.y < trackHeightRef.current + 120) nextEntities.push(moved);
      }

      if (collectedCount > 0) {
        comboRef.current += collectedCount;
        lastCollectAtRef.current = gameTime;
        const multiplier = Math.min(4, 1 + Math.floor(comboRef.current / 4));
        essenceRef.current += collectedCount;
        scoreRef.current += collectedCount * 45 * multiplier;
        setEssence(essenceRef.current);
        setCombo(comboRef.current);
        void Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light).catch(() => undefined);
      }

      if (pickedPower) {
        const powerWorld = WORLDS.find((item) => item.power === pickedPower) ?? currentWorld;
        activePowerTypeRef.current = pickedPower;
        shieldChargesRef.current = pickedPower === 'shield' ? 1 : 0;
        slowUntilRef.current = pickedPower === 'slow' ? gameTime + POWER_DURATION_MS : 0;
        magnetUntilRef.current = pickedPower === 'magnet' ? gameTime + POWER_DURATION_MS : 0;
        const remaining = pickedPower === 'shield' ? 1 : POWER_DURATION_MS / 1000;
        powerRemainingRef.current = remaining;
        setActivePower({
          type: pickedPower,
          label: powerWorld.powerName,
          detail: powerWorld.powerEffect,
          remaining,
        });
        showToast(`POUVOIR · ${powerWorld.powerName}`, powerWorld.powerEffect);
        void Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success).catch(() => undefined);
      }

      if (shieldWasUsed) {
        showToast('LIANE PROTECTRICE', 'Obstacle absorbé. Continue ta course !');
        void Haptics.notificationAsync(Haptics.NotificationFeedbackType.Warning).catch(() => undefined);
      }

      entitiesRef.current = nextEntities;
      setEntities(nextEntities);
      setScore(Math.floor(scoreRef.current));

      if (crashed) endRun();
    }, TICK_MS);

    return () => clearInterval(timer);
  }, [clearPower, endRun, running, showToast]);

  const slowed = activePower?.type === 'slow';

  return (
    <SafeAreaView style={[styles.safe, { backgroundColor: world.palette.page }]}>
      <StatusBar style="dark" />
      <View style={[styles.page, { backgroundColor: world.palette.page }]}>
        <View style={styles.topBar}>
          <View>
            <Text style={[styles.kmk, { color: world.palette.accent }]}>KMK PARIS</Text>
            <Text style={[styles.title, { color: world.palette.ink }]}>THE ESSENCE RUN</Text>
          </View>
          <View style={styles.topActions}>
            <View style={[styles.levelPill, { borderColor: world.palette.accent, backgroundColor: world.palette.accentSoft }]}>
              <Text style={[styles.levelLabel, { color: world.palette.accent }]}>RUN</Text>
              <Text style={[styles.levelValue, { color: world.palette.ink }]}>{level}</Text>
            </View>
            {(phase === 'running' || phase === 'paused') && (
              <Pressable
                style={[styles.pauseButton, { backgroundColor: world.palette.accent }]}
                onPress={togglePause}
                accessibilityLabel={phase === 'paused' ? 'Reprendre la course' : 'Mettre la course en pause'}
              >
                <Text style={styles.pauseText}>{phase === 'paused' ? '▶' : 'Ⅱ'}</Text>
              </Pressable>
            )}
          </View>
        </View>

        <View style={styles.statsRow}>
          <Stat label="SCORE" value={score.toString().padStart(5, '0')} color={world.palette.ink} accent={world.palette.accent} />
          <Stat label="ESSENCE" value={essence.toString().padStart(2, '0')} color={world.palette.ink} accent={world.palette.accent} />
          <Stat label={combo >= 4 ? `COMBO ×${Math.min(4, 1 + Math.floor(combo / 4))}` : 'RECORD'} value={(combo >= 4 ? combo : best).toString().padStart(combo >= 4 ? 2 : 5, '0')} color={world.palette.ink} accent={world.palette.accent} />
        </View>

        <View style={styles.track} onLayout={onTrackLayout} {...panResponder.panHandlers}>
          <WorldBackdrop world={world} width={TRACK_WIDTH} height={trackHeight} running={running} slowed={slowed} />

          <View style={styles.worldRibbon}>
            <View style={styles.worldRibbonTop}>
              <View style={styles.worldTitleBlock}>
                <Text style={[styles.chapter, { color: world.palette.accent }]}>{world.chapter}</Text>
                <Text style={[styles.worldName, { color: world.palette.ink }]}>{world.name}</Text>
                <Text style={[styles.place, { color: world.palette.accent }]}>{world.place}</Text>
              </View>
              <View style={[styles.powerPreview, { backgroundColor: world.palette.accentSoft, borderColor: world.palette.accent }]}>
                <Text style={[styles.powerPreviewIcon, { color: world.palette.accent }]}>{world.icon}</Text>
                <View>
                  <Text style={[styles.powerPreviewLabel, { color: world.palette.accent }]}>POUVOIR DU MONDE</Text>
                  <Text style={[styles.powerPreviewName, { color: world.palette.ink }]}>{world.powerLabel}</Text>
                </View>
              </View>
            </View>
            <View style={styles.progressTrack}>
              <View
                style={[
                  styles.progressFill,
                  {
                    backgroundColor: world.palette.accent,
                    width: `${Math.max(4, progress * 100)}%` as `${number}%`,
                  },
                ]}
              />
            </View>
            <Text style={[styles.nextWorldText, { color: world.palette.ink }]}>PROCHAIN MONDE · {nextWorld.name}</Text>
          </View>

          {activePower && (
            <View style={[styles.activePower, { backgroundColor: world.palette.ink, borderColor: world.palette.accentSoft }]}>
              <Text style={styles.activePowerIcon}>{activePower.type === 'shield' ? '◉' : activePower.type === 'slow' ? '❄' : '≈'}</Text>
              <View style={styles.activePowerCopy}>
                <Text style={styles.activePowerLabel}>{activePower.label}</Text>
                <Text style={styles.activePowerDetail}>{activePower.detail}</Text>
              </View>
              <Text style={styles.activePowerTime}>{activePower.type === 'shield' ? '1×' : `${activePower.remaining}s`}</Text>
            </View>
          )}

          {toast && (
            <View style={[styles.toast, { borderColor: world.palette.accent }]}>
              <Text style={[styles.toastTitle, { color: world.palette.accent }]}>{toast.title}</Text>
              <Text style={[styles.toastDetail, { color: world.palette.ink }]}>{toast.detail}</Text>
            </View>
          )}

          {entities.map((entity) => {
            const entityWorld = WORLDS.find((item) => item.id === entity.worldId) ?? world;
            const entityWidth = entity.kind === 'powerup' ? 104 : 58;
            return (
              <View
                key={entity.id}
                style={[
                  styles.entity,
                  {
                    width: entityWidth,
                    left: laneCenter(entity.lane) - entityWidth / 2,
                    top: entity.y,
                  },
                ]}
              >
                {entity.kind === 'powerup' ? (
                  <PowerPickup world={entityWorld} />
                ) : entity.kind === 'essence' ? (
                  <EssenceBottle world={entityWorld} />
                ) : (
                  <Obstacle world={entityWorld} />
                )}
              </View>
            );
          })}

          <Animated.View
            style={[
              styles.playerWrap,
              {
                transform: [{ translateX: playerX }, { translateY: playerBob }],
              },
            ]}
          >
            <MiniTyson power={activePower?.type ?? null} accent={world.palette.accent} />
          </Animated.View>

          {phase === 'idle' && <StartOverlay world={world} onStart={resetGame} />}
          {phase === 'paused' && (
            <GameOverlay
              eyebrow="PAUSE"
              title="REPRENDS TON VOYAGE"
              text={`${world.name} t’attend. Tes pouvoirs sont conservés.`}
              button="CONTINUER"
              onPress={togglePause}
              world={world}
            />
          )}
          {phase === 'ended' && (
            <GameOverlay
              eyebrow="RUN TERMINÉ"
              title={`${score.toString().padStart(5, '0')} PTS`}
              text={`${essence} essences collectées · combo max ${Math.max(1, Math.min(4, 1 + Math.floor(combo / 4)))}×`}
              button="REJOUER LES 3 MONDES"
              onPress={resetGame}
              world={world}
            />
          )}
        </View>

        <View style={styles.controls}>
          <Pressable
            style={[styles.controlButton, { backgroundColor: '#FFFFFF', borderColor: world.palette.accentSoft }]}
            onPress={() => moveBy(-1)}
            accessibilityLabel="Aller à gauche"
          >
            <Text style={[styles.controlArrow, { color: world.palette.accent }]}>←</Text>
          </Pressable>
          <View style={styles.controlCenter}>
            <Text style={[styles.controlHint, { color: world.palette.ink }]}>{running ? 'GLISSE OU TOUCHE' : 'UNE BRISE D’ÉLÉGANCE'}</Text>
            <View style={styles.laneDots}>
              {[0, 1, 2].map((dot) => (
                <View
                  key={dot}
                  style={[
                    styles.dot,
                    { backgroundColor: dot === lane ? world.palette.accent : world.palette.accentSoft },
                    dot === lane && styles.dotActive,
                  ]}
                />
              ))}
            </View>
          </View>
          <Pressable
            style={[styles.controlButton, { backgroundColor: '#FFFFFF', borderColor: world.palette.accentSoft }]}
            onPress={() => moveBy(1)}
            accessibilityLabel="Aller à droite"
          >
            <Text style={[styles.controlArrow, { color: world.palette.accent }]}>→</Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}

function Stat({ label, value, color, accent }: { label: string; value: string; color: string; accent: string }) {
  return (
    <View style={styles.stat}>
      <Text style={[styles.statLabel, { color: accent }]}>{label}</Text>
      <Text style={[styles.statValue, { color }]}>{value}</Text>
    </View>
  );
}

function StartOverlay({ world, onStart }: { world: World; onStart: () => void }) {
  return (
    <View style={styles.overlay}>
      <Text style={[styles.overlayEyebrow, { color: world.palette.accent }]}>KMK PARIS PRÉSENTE</Text>
      <Text style={[styles.overlayTitle, { color: world.palette.ink }]}>3 MONDES.{`\n`}3 POUVOIRS.</Text>
      <Text style={[styles.overlayText, { color: world.palette.ink }]}>Chaque fragrance transforme la course, le décor et ta façon de jouer.</Text>
      <View style={styles.worldCards}>
        {WORLDS.map((item) => (
          <View key={item.id} style={[styles.worldCard, { backgroundColor: item.palette.accentSoft, borderColor: item.palette.accent }]}>
            <Text style={[styles.worldCardIcon, { color: item.palette.accent }]}>{item.icon}</Text>
            <Text style={[styles.worldCardName, { color: item.palette.ink }]}>{item.name}</Text>
            <Text style={[styles.worldCardPower, { color: item.palette.accent }]}>{item.powerLabel}</Text>
          </View>
        ))}
      </View>
      <Text style={[styles.gestureHelp, { color: world.palette.ink }]}>Glisse à gauche ou à droite · une voie reste toujours libre</Text>
      <Pressable style={[styles.primaryButton, { backgroundColor: world.palette.accent }]} onPress={onStart}>
        <Text style={styles.primaryButtonText}>ENTRER DANS LIANE LIBRE</Text>
      </Pressable>
    </View>
  );
}

function GameOverlay({
  eyebrow,
  title,
  text,
  button,
  onPress,
  world,
}: {
  eyebrow: string;
  title: string;
  text: string;
  button: string;
  onPress: () => void;
  world: World;
}) {
  return (
    <View style={styles.overlay}>
      <Text style={[styles.overlayEyebrow, { color: world.palette.accent }]}>{eyebrow}</Text>
      <Text style={[styles.overlayTitle, { color: world.palette.ink }]}>{title}</Text>
      <Text style={[styles.overlayText, { color: world.palette.ink }]}>{text}</Text>
      <Pressable style={[styles.primaryButton, { backgroundColor: world.palette.accent }]} onPress={onPress}>
        <Text style={styles.primaryButtonText}>{button}</Text>
      </Pressable>
    </View>
  );
}

function MiniTyson({ power, accent }: { power: PowerType | null; accent: string }) {
  return (
    <View style={styles.characterStage}>
      {power && <View style={[styles.powerAura, { borderColor: power === 'slow' ? '#DFFFFF' : power === 'magnet' ? '#FFD2A4' : '#F6E784' }]} />}
      <View style={styles.characterShadow} />
      <View style={styles.character}>
        <View style={styles.hair} />
        <View style={styles.head}>
          <View style={styles.beard} />
        </View>
        <View style={styles.neck} />
        <View style={[styles.jacket, { borderColor: accent }]}>
          <Text style={[styles.jacketText, { color: accent }]}>K</Text>
        </View>
        <View style={styles.legs}>
          <View style={[styles.leg, { transform: [{ rotate: '7deg' }] }]} />
          <View style={[styles.leg, { transform: [{ rotate: '-7deg' }] }]} />
        </View>
      </View>
    </View>
  );
}

function EssenceBottle({ world, power = false }: { world: World; power?: boolean }) {
  const shortName = world.id === 'liane' ? 'LL' : world.id === 'palme' ? 'PH' : 'RC';
  return (
    <View style={[styles.bottleGlow, { backgroundColor: `${world.palette.bottle}55` }]}>
      <View style={[styles.bottleCap, { backgroundColor: world.palette.accent }]} />
      <View style={[styles.bottle, { borderColor: world.palette.accent, backgroundColor: power ? '#FFFFFF' : `${world.palette.bottle}CC` }]}>
        <Text style={[styles.bottleKmk, { color: world.palette.ink }]}>KMK</Text>
        <Text style={[styles.bottleText, { color: world.palette.accent }]}>{shortName}</Text>
      </View>
    </View>
  );
}

function PowerPickup({ world }: { world: World }) {
  return (
    <View style={styles.powerPickup}>
      <View style={[styles.powerPickupLabel, { backgroundColor: '#FFFFFF', borderColor: world.palette.accent }]}>
        <Text style={[styles.powerPickupName, { color: world.palette.accent }]}>{world.powerName}</Text>
        <Text style={[styles.powerPickupEffect, { color: world.palette.ink }]}>{world.powerLabel}</Text>
      </View>
      <View style={[styles.powerPickupHalo, { borderColor: world.palette.accent, backgroundColor: world.palette.accentSoft }]}>
        <EssenceBottle world={world} power />
      </View>
    </View>
  );
}

function Obstacle({ world }: { world: World }) {
  if (world.id === 'liane') {
    return (
      <View style={styles.logObstacle}>
        <View style={styles.logRing} />
        <View style={styles.logVine} />
      </View>
    );
  }
  if (world.id === 'palme') {
    return (
      <View style={styles.iceObstacle}>
        <View style={[styles.iceFacet, { transform: [{ rotate: '24deg' }] }]} />
        <View style={[styles.iceFacet, { transform: [{ rotate: '-25deg' }] }]} />
      </View>
    );
  }
  return (
    <View style={styles.rockObstacle}>
      <View style={styles.rockShine} />
      <Text style={styles.rockMark}>≈</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1 },
  page: { flex: 1, alignItems: 'center', paddingHorizontal: 12 },
  topBar: {
    width: TRACK_WIDTH,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingTop: 6,
    paddingBottom: 8,
  },
  kmk: { fontSize: 11, letterSpacing: 3.5, fontWeight: '900' },
  title: { fontSize: 21, letterSpacing: -0.7, fontWeight: '900', marginTop: 1 },
  topActions: { flexDirection: 'row', alignItems: 'center', gap: 7 },
  levelPill: { minWidth: 59, borderWidth: 1, borderRadius: 15, paddingVertical: 5, paddingHorizontal: 9, alignItems: 'center' },
  levelLabel: { fontSize: 7, letterSpacing: 1.4, fontWeight: '900' },
  levelValue: { fontSize: 15, lineHeight: 17, fontWeight: '900' },
  pauseButton: { width: 38, height: 38, borderRadius: 14, alignItems: 'center', justifyContent: 'center' },
  pauseText: { color: '#FFFFFF', fontSize: 14, fontWeight: '900' },
  statsRow: { width: TRACK_WIDTH, flexDirection: 'row', gap: 7, marginBottom: 7 },
  stat: {
    flex: 1,
    backgroundColor: 'rgba(255,255,255,0.86)',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.96)',
    borderRadius: 12,
    paddingVertical: 6,
    paddingHorizontal: 9,
    shadowColor: '#255A4C',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.08,
    shadowRadius: 8,
  },
  statLabel: { fontSize: 7, letterSpacing: 1.15, fontWeight: '900' },
  statValue: { fontSize: 15, fontWeight: '900', marginTop: 1 },
  track: {
    width: TRACK_WIDTH,
    flex: 1,
    minHeight: 455,
    maxHeight: 740,
    overflow: 'hidden',
    borderRadius: 25,
    backgroundColor: '#9DDFC8',
    borderWidth: 2,
    borderColor: 'rgba(255,255,255,0.92)',
    shadowColor: '#284E48',
    shadowOffset: { width: 0, height: 12 },
    shadowOpacity: 0.18,
    shadowRadius: 20,
    elevation: 5,
  },
  worldRibbon: {
    position: 'absolute',
    top: 10,
    left: 10,
    right: 10,
    borderRadius: 17,
    backgroundColor: 'rgba(255,255,255,0.90)',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.98)',
    paddingHorizontal: 12,
    paddingVertical: 9,
    zIndex: 8,
    shadowColor: '#193E34',
    shadowOffset: { width: 0, height: 5 },
    shadowOpacity: 0.1,
    shadowRadius: 10,
  },
  worldRibbonTop: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', gap: 8 },
  worldTitleBlock: { flex: 1 },
  chapter: { fontSize: 7, letterSpacing: 1.7, fontWeight: '900' },
  worldName: { fontSize: 17, lineHeight: 19, fontWeight: '900', letterSpacing: -0.5 },
  place: { fontSize: 7, letterSpacing: 1.1, fontWeight: '800', marginTop: 1 },
  powerPreview: { maxWidth: 151, flexDirection: 'row', alignItems: 'center', gap: 6, borderWidth: 1, borderRadius: 12, padding: 7 },
  powerPreviewIcon: { fontSize: 17, fontWeight: '900' },
  powerPreviewLabel: { fontSize: 5.5, letterSpacing: 0.7, fontWeight: '900' },
  powerPreviewName: { fontSize: 8, lineHeight: 10, fontWeight: '900', marginTop: 1 },
  progressTrack: { height: 4, backgroundColor: 'rgba(30,55,48,0.10)', borderRadius: 3, marginTop: 7, overflow: 'hidden' },
  progressFill: { height: 4, borderRadius: 3 },
  nextWorldText: { fontSize: 6, letterSpacing: 0.9, fontWeight: '800', textAlign: 'right', marginTop: 3, opacity: 0.72 },
  activePower: {
    position: 'absolute',
    top: 105,
    left: 14,
    right: 14,
    zIndex: 10,
    borderWidth: 1,
    borderRadius: 14,
    paddingVertical: 7,
    paddingHorizontal: 10,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  activePowerIcon: { color: '#FFFFFF', fontSize: 18, fontWeight: '900' },
  activePowerCopy: { flex: 1 },
  activePowerLabel: { color: '#FFFFFF', fontSize: 9, fontWeight: '900', letterSpacing: 0.7 },
  activePowerDetail: { color: 'rgba(255,255,255,0.76)', fontSize: 7, marginTop: 1 },
  activePowerTime: { color: '#FFFFFF', fontSize: 14, fontWeight: '900' },
  toast: {
    position: 'absolute',
    top: 163,
    left: 34,
    right: 34,
    zIndex: 19,
    backgroundColor: 'rgba(255,255,255,0.96)',
    borderWidth: 2,
    borderRadius: 16,
    paddingVertical: 10,
    paddingHorizontal: 13,
    alignItems: 'center',
    shadowColor: '#152F2A',
    shadowOffset: { width: 0, height: 7 },
    shadowOpacity: 0.18,
    shadowRadius: 13,
  },
  toastTitle: { fontSize: 10, fontWeight: '900', letterSpacing: 1, textAlign: 'center' },
  toastDetail: { fontSize: 9, lineHeight: 13, fontWeight: '700', textAlign: 'center', marginTop: 3 },
  entity: { position: 'absolute', height: 84, alignItems: 'center', justifyContent: 'flex-end', zIndex: 5 },
  bottleGlow: { width: 47, height: 58, borderRadius: 24, alignItems: 'center', justifyContent: 'flex-end', paddingBottom: 3 },
  bottleCap: { width: 18, height: 8, borderTopLeftRadius: 4, borderTopRightRadius: 4 },
  bottle: { width: 38, height: 43, borderWidth: 2, borderRadius: 9, alignItems: 'center', justifyContent: 'center' },
  bottleKmk: { fontSize: 7, lineHeight: 8, fontWeight: '900', letterSpacing: 0.8 },
  bottleText: { fontSize: 10, lineHeight: 12, fontWeight: '900', letterSpacing: 1 },
  powerPickup: { width: 104, alignItems: 'center', justifyContent: 'flex-end' },
  powerPickupLabel: { minWidth: 102, borderWidth: 1.5, borderRadius: 9, paddingVertical: 4, paddingHorizontal: 5, alignItems: 'center', marginBottom: 3 },
  powerPickupName: { fontSize: 6.5, lineHeight: 8, fontWeight: '900', letterSpacing: 0.45, textAlign: 'center' },
  powerPickupEffect: { fontSize: 7.5, lineHeight: 9, fontWeight: '900', textAlign: 'center' },
  powerPickupHalo: { width: 62, height: 62, borderRadius: 31, borderWidth: 2, alignItems: 'center', justifyContent: 'center' },
  logObstacle: { width: 58, height: 39, borderRadius: 20, backgroundColor: '#6E4127', borderWidth: 4, borderColor: '#925F34', justifyContent: 'center' },
  logRing: { position: 'absolute', right: -2, width: 34, height: 34, borderRadius: 18, borderWidth: 4, borderColor: '#D39A57', backgroundColor: '#A56C39' },
  logVine: { width: 45, height: 7, borderRadius: 4, backgroundColor: '#3F9147', transform: [{ rotate: '-10deg' }] },
  iceObstacle: { width: 53, height: 53, alignItems: 'center', justifyContent: 'center' },
  iceFacet: { position: 'absolute', width: 32, height: 45, borderRadius: 8, backgroundColor: 'rgba(224,253,255,0.92)', borderWidth: 2, borderColor: '#79CAD7' },
  rockObstacle: { width: 55, height: 48, borderRadius: 16, borderTopLeftRadius: 25, backgroundColor: '#A84F38', borderWidth: 3, borderColor: '#E6A269', alignItems: 'center', justifyContent: 'center' },
  rockShine: { position: 'absolute', top: 8, left: 10, width: 20, height: 6, borderRadius: 5, backgroundColor: '#F2BF82', transform: [{ rotate: '-19deg' }] },
  rockMark: { color: '#FFD39A', fontSize: 21, fontWeight: '900' },
  playerWrap: { position: 'absolute', left: 0, bottom: PLAYER_BOTTOM, width: PLAYER_WIDTH, height: PLAYER_HEIGHT, alignItems: 'center', justifyContent: 'flex-end', zIndex: 12 },
  characterStage: { width: 76, height: 96, alignItems: 'center', justifyContent: 'flex-end' },
  powerAura: { position: 'absolute', bottom: 0, width: 74, height: 88, borderRadius: 38, borderWidth: 4, backgroundColor: 'rgba(255,255,255,0.24)' },
  characterShadow: { position: 'absolute', bottom: 0, width: 49, height: 10, borderRadius: 25, backgroundColor: 'rgba(24,44,37,0.28)' },
  character: { width: 58, height: 87, alignItems: 'center' },
  hair: { width: 35, height: 12, borderTopLeftRadius: 17, borderTopRightRadius: 17, backgroundColor: '#171412', zIndex: 3 },
  head: { width: 33, height: 32, marginTop: -2, borderRadius: 17, backgroundColor: '#8E5A3B', alignItems: 'center', justifyContent: 'flex-end', zIndex: 2 },
  beard: { width: 23, height: 9, borderBottomLeftRadius: 11, borderBottomRightRadius: 11, backgroundColor: '#211A17' },
  neck: { width: 13, height: 5, backgroundColor: '#7D4F35' },
  jacket: { width: 46, height: 30, borderTopLeftRadius: 9, borderTopRightRadius: 9, backgroundColor: '#FFFDF5', borderWidth: 2, alignItems: 'center', justifyContent: 'center' },
  jacketText: { fontSize: 14, fontWeight: '900' },
  legs: { width: 34, flexDirection: 'row', justifyContent: 'space-between' },
  leg: { width: 13, height: 18, borderBottomLeftRadius: 5, borderBottomRightRadius: 5, backgroundColor: '#2C2A29' },
  overlay: {
    position: 'absolute',
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    backgroundColor: 'rgba(255,252,243,0.94)',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 20,
    zIndex: 20,
  },
  overlayEyebrow: { fontSize: 9, fontWeight: '900', letterSpacing: 2, textAlign: 'center', marginBottom: 9 },
  overlayTitle: { fontSize: 29, lineHeight: 30, fontWeight: '900', textAlign: 'center', letterSpacing: -1.2 },
  overlayText: { fontSize: 12, lineHeight: 17, fontWeight: '600', textAlign: 'center', marginTop: 10, maxWidth: 285, opacity: 0.82 },
  worldCards: { width: '100%', flexDirection: 'row', gap: 6, marginTop: 17 },
  worldCard: { flex: 1, minHeight: 91, borderWidth: 1, borderRadius: 13, paddingVertical: 8, paddingHorizontal: 5, alignItems: 'center' },
  worldCardIcon: { fontSize: 18, fontWeight: '900' },
  worldCardName: { fontSize: 8, lineHeight: 10, fontWeight: '900', textAlign: 'center', marginTop: 4 },
  worldCardPower: { fontSize: 6.5, lineHeight: 9, fontWeight: '900', textAlign: 'center', marginTop: 5 },
  gestureHelp: { fontSize: 8.5, fontWeight: '700', textAlign: 'center', marginTop: 13, opacity: 0.72 },
  primaryButton: { marginTop: 18, borderRadius: 999, paddingVertical: 13, paddingHorizontal: 24, shadowColor: '#173C32', shadowOffset: { width: 0, height: 7 }, shadowOpacity: 0.18, shadowRadius: 10 },
  primaryButtonText: { color: '#FFFFFF', fontSize: 10, fontWeight: '900', letterSpacing: 1.2, textAlign: 'center' },
  controls: { width: TRACK_WIDTH, height: 67, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingTop: 7 },
  controlButton: { width: 58, height: 47, borderRadius: 16, borderWidth: 1, alignItems: 'center', justifyContent: 'center', shadowColor: '#21483E', shadowOffset: { width: 0, height: 4 }, shadowOpacity: 0.08, shadowRadius: 8 },
  controlArrow: { fontSize: 24, fontWeight: '900' },
  controlCenter: { alignItems: 'center', gap: 7 },
  controlHint: { fontSize: 7.5, fontWeight: '900', letterSpacing: 1.4 },
  laneDots: { flexDirection: 'row', gap: 7 },
  dot: { width: 6, height: 6, borderRadius: 4 },
  dotActive: { width: 20 },
});
