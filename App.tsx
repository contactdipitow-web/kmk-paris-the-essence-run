import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Animated,
  Dimensions,
  PanResponder,
  Pressable,
  SafeAreaView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { StatusBar } from 'expo-status-bar';

type Lane = 0 | 1 | 2;
type EntityKind = 'essence' | 'obstacle';

type Entity = {
  id: number;
  kind: EntityKind;
  lane: Lane;
  y: number;
};

const { width: SCREEN_WIDTH, height: SCREEN_HEIGHT } = Dimensions.get('window');
const TRACK_WIDTH = Math.min(SCREEN_WIDTH - 24, 430);
const LANE_WIDTH = TRACK_WIDTH / 3;
const PLAYER_BOTTOM = 96;
const PLAYER_SIZE = 58;
const COLLISION_TOP = SCREEN_HEIGHT - PLAYER_BOTTOM - PLAYER_SIZE - 34;
const COLLISION_BOTTOM = SCREEN_HEIGHT - PLAYER_BOTTOM + 6;

const laneX = (lane: Lane) => lane * LANE_WIDTH + LANE_WIDTH / 2 - PLAYER_SIZE / 2;

export default function App() {
  const [lane, setLane] = useState<Lane>(1);
  const laneRef = useRef<Lane>(1);
  const [entities, setEntities] = useState<Entity[]>([]);
  const entitiesRef = useRef<Entity[]>([]);
  const [score, setScore] = useState(0);
  const scoreRef = useRef(0);
  const [essence, setEssence] = useState(0);
  const essenceRef = useRef(0);
  const [best, setBest] = useState(0);
  const [running, setRunning] = useState(false);
  const runningRef = useRef(false);
  const [started, setStarted] = useState(false);
  const playerX = useRef(new Animated.Value(laneX(1))).current;

  const moveToLane = useCallback(
    (nextLane: Lane) => {
      laneRef.current = nextLane;
      setLane(nextLane);
      Animated.spring(playerX, {
        toValue: laneX(nextLane),
        useNativeDriver: true,
        damping: 18,
        stiffness: 220,
        mass: 0.6,
      }).start();
    },
    [playerX],
  );

  const moveBy = useCallback(
    (delta: -1 | 1) => {
      const next = Math.max(0, Math.min(2, laneRef.current + delta)) as Lane;
      moveToLane(next);
    },
    [moveToLane],
  );

  const panResponder = useMemo(
    () =>
      PanResponder.create({
        onMoveShouldSetPanResponder: (_, gesture) => Math.abs(gesture.dx) > 12,
        onPanResponderRelease: (_, gesture) => {
          if (gesture.dx > 34) moveBy(1);
          if (gesture.dx < -34) moveBy(-1);
        },
      }),
    [moveBy],
  );

  const endRun = useCallback(() => {
    runningRef.current = false;
    setRunning(false);
    setBest((current) => Math.max(current, scoreRef.current));
  }, []);

  const resetGame = useCallback(() => {
    entitiesRef.current = [];
    setEntities([]);
    scoreRef.current = 0;
    setScore(0);
    essenceRef.current = 0;
    setEssence(0);
    moveToLane(1);
    setStarted(true);
    runningRef.current = true;
    setRunning(true);
  }, [moveToLane]);

  useEffect(() => {
    if (!running) return;

    const spawnTimer = setInterval(() => {
      const roll = Math.random();
      const item: Entity = {
        id: Date.now() + Math.floor(Math.random() * 10000),
        kind: roll < 0.38 ? 'essence' : 'obstacle',
        lane: Math.floor(Math.random() * 3) as Lane,
        y: -72,
      };
      entitiesRef.current = [...entitiesRef.current, item];
      setEntities(entitiesRef.current);
    }, 720);

    const gameTimer = setInterval(() => {
      if (!runningRef.current) return;

      scoreRef.current += 1;
      setScore(scoreRef.current);
      const speed = Math.min(20, 7 + scoreRef.current / 240);
      let collided = false;
      let collected = 0;

      const next = entitiesRef.current
        .map((entity) => ({ ...entity, y: entity.y + speed }))
        .filter((entity) => {
          const inHitZone = entity.y >= COLLISION_TOP && entity.y <= COLLISION_BOTTOM;
          const sameLane = entity.lane === laneRef.current;

          if (inHitZone && sameLane) {
            if (entity.kind === 'obstacle') {
              collided = true;
            } else {
              collected += 1;
            }
            return false;
          }

          return entity.y < SCREEN_HEIGHT + 90;
        });

      if (collected > 0) {
        essenceRef.current += collected;
        scoreRef.current += collected * 35;
        setEssence(essenceRef.current);
        setScore(scoreRef.current);
      }

      entitiesRef.current = next;
      setEntities(next);

      if (collided) endRun();
    }, 50);

    return () => {
      clearInterval(spawnTimer);
      clearInterval(gameTimer);
    };
  }, [endRun, running]);

  const level = Math.min(9, Math.floor(score / 300) + 1);

  return (
    <SafeAreaView style={styles.safe}>
      <StatusBar style="light" />
      <View style={styles.page}>
        <View style={styles.topBar}>
          <View>
            <Text style={styles.kmk}>KMK PARIS</Text>
            <Text style={styles.title}>THE ESSENCE RUN</Text>
          </View>
          <View style={styles.levelPill}>
            <Text style={styles.levelLabel}>NIVEAU</Text>
            <Text style={styles.levelValue}>{level}</Text>
          </View>
        </View>

        <View style={styles.statsRow}>
          <Stat label="SCORE" value={score.toString().padStart(5, '0')} />
          <Stat label="ESSENCE" value={essence.toString().padStart(2, '0')} />
          <Stat label="BEST" value={best.toString().padStart(5, '0')} />
        </View>

        <View style={styles.track} {...panResponder.panHandlers}>
          <ParisSkyline />
          <View style={[styles.laneLine, { left: LANE_WIDTH }]} />
          <View style={[styles.laneLine, { left: LANE_WIDTH * 2 }]} />
          <View style={styles.horizonGlow} />

          {entities.map((entity) => (
            <View
              key={entity.id}
              style={[
                styles.entity,
                {
                  left: entity.lane * LANE_WIDTH + LANE_WIDTH / 2 - 23,
                  top: entity.y,
                },
              ]}
            >
              {entity.kind === 'essence' ? <EssenceBottle /> : <Obstacle />}
            </View>
          ))}

          <Animated.View
            style={[
              styles.playerWrap,
              {
                transform: [{ translateX: playerX }],
              },
            ]}
          >
            <MiniTyson />
          </Animated.View>

          {!started && (
            <View style={styles.overlay}>
              <Text style={styles.overlayEyebrow}>PARIS. UNE COURSE. UNE ESSENCE.</Text>
              <Text style={styles.overlayTitle}>ATTRAPE L’ESSENCE.{`\n`}ÉVITE LE BRUIT.</Text>
              <Text style={styles.overlayText}>Swipe à gauche ou à droite pour changer de voie.</Text>
              <Pressable style={styles.primaryButton} onPress={resetGame}>
                <Text style={styles.primaryButtonText}>COMMENCER</Text>
              </Pressable>
            </View>
          )}

          {started && !running && (
            <View style={styles.overlay}>
              <Text style={styles.overlayEyebrow}>RUN TERMINÉ</Text>
              <Text style={styles.overlayTitle}>{score.toString().padStart(5, '0')} PTS</Text>
              <Text style={styles.overlayText}>{essence} essences collectées. Paris t’attend encore.</Text>
              <Pressable style={styles.primaryButton} onPress={resetGame}>
                <Text style={styles.primaryButtonText}>REJOUER</Text>
              </Pressable>
            </View>
          )}
        </View>

        <View style={styles.controls}>
          <Pressable style={styles.controlButton} onPress={() => moveBy(-1)} accessibilityLabel="Aller à gauche">
            <Text style={styles.controlArrow}>←</Text>
          </Pressable>
          <View style={styles.controlCenter}>
            <Text style={styles.controlHint}>{running ? 'SWIPE / TOUCH' : 'KMK PARIS'}</Text>
            <View style={styles.laneDots}>
              {[0, 1, 2].map((dot) => (
                <View key={dot} style={[styles.dot, dot === lane && styles.dotActive]} />
              ))}
            </View>
          </View>
          <Pressable style={styles.controlButton} onPress={() => moveBy(1)} accessibilityLabel="Aller à droite">
            <Text style={styles.controlArrow}>→</Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.stat}>
      <Text style={styles.statLabel}>{label}</Text>
      <Text style={styles.statValue}>{value}</Text>
    </View>
  );
}

function MiniTyson() {
  return (
    <View style={styles.character}>
      <View style={styles.hair} />
      <View style={styles.head}>
        <View style={styles.beard} />
      </View>
      <View style={styles.neck} />
      <View style={styles.jacket}>
        <Text style={styles.jacketText}>K</Text>
      </View>
      <View style={styles.legs}>
        <View style={styles.leg} />
        <View style={styles.leg} />
      </View>
    </View>
  );
}

function EssenceBottle() {
  return (
    <View style={styles.bottleWrap}>
      <View style={styles.bottleCap} />
      <View style={styles.bottle}>
        <Text style={styles.bottleText}>KMK</Text>
      </View>
    </View>
  );
}

function Obstacle() {
  return (
    <View style={styles.obstacle}>
      <View style={styles.obstacleStripe} />
      <View style={styles.obstacleStripe} />
    </View>
  );
}

function ParisSkyline() {
  return (
    <View style={styles.skyline} pointerEvents="none">
      {[54, 38, 72, 46, 62, 34, 80, 50, 68, 42].map((height, index) => (
        <View key={index} style={[styles.building, { height, width: 22 + (index % 3) * 8 }]} />
      ))}
      <View style={styles.eiffel}>
        <View style={styles.eiffelTop} />
        <View style={styles.eiffelBody} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: '#090907' },
  page: { flex: 1, alignItems: 'center', backgroundColor: '#090907', paddingHorizontal: 12 },
  topBar: {
    width: TRACK_WIDTH,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingTop: 8,
    paddingBottom: 10,
  },
  kmk: { color: '#EEE6D4', fontSize: 12, letterSpacing: 3.4, fontWeight: '700' },
  title: { color: '#F7F1E4', fontSize: 22, letterSpacing: -0.5, fontWeight: '900', marginTop: 2 },
  levelPill: {
    minWidth: 64,
    borderWidth: 1,
    borderColor: '#B79A5C',
    borderRadius: 14,
    paddingVertical: 6,
    paddingHorizontal: 10,
    alignItems: 'center',
  },
  levelLabel: { color: '#8E846F', fontSize: 7, letterSpacing: 1.4, fontWeight: '800' },
  levelValue: { color: '#D5B76F', fontSize: 16, fontWeight: '900' },
  statsRow: { width: TRACK_WIDTH, flexDirection: 'row', gap: 8, marginBottom: 8 },
  stat: {
    flex: 1,
    backgroundColor: '#12120F',
    borderWidth: 1,
    borderColor: '#222018',
    borderRadius: 10,
    paddingVertical: 7,
    paddingHorizontal: 9,
  },
  statLabel: { color: '#736C5C', fontSize: 7, letterSpacing: 1.5, fontWeight: '800' },
  statValue: { color: '#E5D9BD', fontSize: 15, fontWeight: '800', marginTop: 1 },
  track: {
    width: TRACK_WIDTH,
    flex: 1,
    minHeight: 480,
    maxHeight: 720,
    overflow: 'hidden',
    borderRadius: 22,
    backgroundColor: '#11110E',
    borderWidth: 1,
    borderColor: '#29261D',
  },
  laneLine: {
    position: 'absolute',
    top: 130,
    bottom: 0,
    width: 1,
    backgroundColor: 'rgba(213,183,111,0.14)',
  },
  horizonGlow: {
    position: 'absolute',
    top: 110,
    left: 0,
    right: 0,
    height: 110,
    backgroundColor: 'rgba(141,105,47,0.07)',
  },
  skyline: {
    position: 'absolute',
    top: 36,
    left: 12,
    right: 12,
    height: 94,
    flexDirection: 'row',
    alignItems: 'flex-end',
    opacity: 0.46,
  },
  building: { backgroundColor: '#27251E', marginRight: 4, borderTopLeftRadius: 2, borderTopRightRadius: 2 },
  eiffel: { position: 'absolute', right: 30, bottom: 0, width: 45, height: 92, alignItems: 'center' },
  eiffelTop: { width: 3, height: 28, backgroundColor: '#6C5A36' },
  eiffelBody: {
    width: 38,
    height: 64,
    borderLeftWidth: 5,
    borderRightWidth: 5,
    borderColor: '#6C5A36',
    transform: [{ perspective: 80 }, { rotateX: '8deg' }],
  },
  entity: { position: 'absolute', width: 46, height: 58, alignItems: 'center', justifyContent: 'center' },
  bottleWrap: { width: 34, height: 50, alignItems: 'center' },
  bottleCap: { width: 16, height: 8, borderRadius: 2, backgroundColor: '#C7A65E' },
  bottle: {
    width: 34,
    height: 41,
    borderWidth: 1,
    borderColor: '#D9C184',
    borderRadius: 7,
    backgroundColor: 'rgba(217,193,132,0.16)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  bottleText: { color: '#E8D69D', fontSize: 8, fontWeight: '900', letterSpacing: 1 },
  obstacle: {
    width: 42,
    height: 42,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#694F3B',
    backgroundColor: '#332820',
    padding: 6,
    justifyContent: 'space-evenly',
  },
  obstacleStripe: { height: 4, backgroundColor: '#A1744F', transform: [{ rotate: '-16deg' }] },
  playerWrap: {
    position: 'absolute',
    left: 0,
    bottom: PLAYER_BOTTOM,
    width: PLAYER_SIZE,
    height: 82,
    alignItems: 'center',
    justifyContent: 'flex-end',
  },
  character: { width: 58, height: 82, alignItems: 'center' },
  hair: { width: 33, height: 11, borderTopLeftRadius: 15, borderTopRightRadius: 15, backgroundColor: '#10100F', zIndex: 3 },
  head: {
    width: 31,
    height: 31,
    marginTop: -2,
    borderRadius: 16,
    backgroundColor: '#8E5A3B',
    alignItems: 'center',
    justifyContent: 'flex-end',
    zIndex: 2,
  },
  beard: { width: 22, height: 8, borderBottomLeftRadius: 10, borderBottomRightRadius: 10, backgroundColor: '#1A1715' },
  neck: { width: 12, height: 5, backgroundColor: '#7D4F35' },
  jacket: {
    width: 44,
    height: 29,
    borderTopLeftRadius: 8,
    borderTopRightRadius: 8,
    backgroundColor: '#E9E1D2',
    borderWidth: 1,
    borderColor: '#BFAF91',
    alignItems: 'center',
    justifyContent: 'center',
  },
  jacketText: { color: '#141310', fontSize: 13, fontWeight: '900' },
  legs: { width: 31, flexDirection: 'row', justifyContent: 'space-between' },
  leg: { width: 12, height: 17, borderBottomLeftRadius: 4, borderBottomRightRadius: 4, backgroundColor: '#23211D' },
  overlay: {
    position: 'absolute',
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    backgroundColor: 'rgba(8,8,6,0.88)',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 30,
    zIndex: 20,
  },
  overlayEyebrow: { color: '#B3985A', fontSize: 9, fontWeight: '800', letterSpacing: 2, textAlign: 'center', marginBottom: 12 },
  overlayTitle: { color: '#F5EEDC', fontSize: 30, lineHeight: 31, fontWeight: '900', textAlign: 'center', letterSpacing: -1.3 },
  overlayText: { color: '#AFA694', fontSize: 13, lineHeight: 19, textAlign: 'center', marginTop: 13, maxWidth: 260 },
  primaryButton: {
    marginTop: 24,
    backgroundColor: '#D1B36C',
    borderRadius: 999,
    paddingVertical: 13,
    paddingHorizontal: 28,
  },
  primaryButtonText: { color: '#11100D', fontSize: 12, fontWeight: '900', letterSpacing: 1.6 },
  controls: { width: TRACK_WIDTH, height: 74, flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingTop: 8 },
  controlButton: {
    width: 58,
    height: 48,
    borderRadius: 15,
    backgroundColor: '#14130F',
    borderWidth: 1,
    borderColor: '#2B281F',
    alignItems: 'center',
    justifyContent: 'center',
  },
  controlArrow: { color: '#D7BF83', fontSize: 24, fontWeight: '700' },
  controlCenter: { alignItems: 'center', gap: 7 },
  controlHint: { color: '#6F6858', fontSize: 8, fontWeight: '800', letterSpacing: 1.7 },
  laneDots: { flexDirection: 'row', gap: 7 },
  dot: { width: 5, height: 5, borderRadius: 3, backgroundColor: '#302D25' },
  dotActive: { width: 18, backgroundColor: '#C9AA64' },
});
