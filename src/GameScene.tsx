import { useMemo, useRef, useState } from 'react';
import { Canvas, useFrame, useThree } from '@react-three/fiber';
import { Environment } from '@react-three/drei';
import * as THREE from 'three';
import { audio } from './audio';
import { laneX, themes, useGame } from './store';

const SEGMENT_LENGTH = 10;
const SEGMENT_COUNT = 18;

type Hazard = 'block' | 'jump' | 'slide' | 'essence' | 'none';

function Runner() {
  const group = useRef<THREE.Group>(null);
  const leftArm = useRef<THREE.Group>(null);
  const rightArm = useRef<THREE.Group>(null);
  const leftLeg = useRef<THREE.Group>(null);
  const rightLeg = useRef<THREE.Group>(null);
  const lane = useGame((s) => s.lane);
  const action = useGame((s) => s.action);
  const phase = useGame((s) => s.phase);
  const actionStart = useRef(0);
  const previousAction = useRef(action);

  useFrame(({ clock }, dt) => {
    if (!group.current) return;
    if (previousAction.current !== action) {
      previousAction.current = action;
      actionStart.current = clock.elapsedTime;
    }
    const targetX = laneX[lane];
    group.current.position.x = THREE.MathUtils.damp(group.current.position.x, targetX, 13, dt);
    const t = clock.elapsedTime;
    const run = phase === 'running' ? Math.sin(t * 13) : Math.sin(t * 2) * 0.15;
    if (leftArm.current) leftArm.current.rotation.x = run * 0.7;
    if (rightArm.current) rightArm.current.rotation.x = -run * 0.7;
    if (leftLeg.current) leftLeg.current.rotation.x = -run * 0.65;
    if (rightLeg.current) rightLeg.current.rotation.x = run * 0.65;

    let y = 0;
    let scaleY = 1;
    if (action === 'jump') {
      const p = Math.min(1, (t - actionStart.current) / 0.72);
      y = Math.sin(p * Math.PI) * 2.15;
    } else if (action === 'slide') {
      scaleY = 0.58;
      y = -0.35;
    }
    group.current.position.y = THREE.MathUtils.damp(group.current.position.y, y, 18, dt);
    group.current.scale.y = THREE.MathUtils.damp(group.current.scale.y, scaleY, 18, dt);
  });

  return (
    <group ref={group} position={[0, 0, 0]}>
      <mesh position={[0, 2.35, 0]} castShadow>
        <sphereGeometry args={[0.38, 20, 20]} />
        <meshStandardMaterial color="#8a5638" roughness={0.72} />
      </mesh>
      <mesh position={[0, 2.57, -0.05]} scale={[1.05, 0.48, 1.04]} castShadow>
        <sphereGeometry args={[0.38, 18, 18]} />
        <meshStandardMaterial color="#12100e" roughness={0.9} />
      </mesh>
      <mesh position={[0, 2.13, 0.25]} scale={[0.78, 0.32, 0.42]} castShadow>
        <sphereGeometry args={[0.35, 16, 16]} />
        <meshStandardMaterial color="#191511" />
      </mesh>
      <mesh position={[0, 1.45, 0]} castShadow>
        <capsuleGeometry args={[0.5, 0.85, 8, 16]} />
        <meshStandardMaterial color="#e8dfce" metalness={0.05} roughness={0.65} />
      </mesh>
      <group ref={leftArm} position={[-0.58, 1.76, 0]}>
        <mesh position={[0, -0.48, 0]} castShadow><capsuleGeometry args={[0.13, 0.72, 6, 12]} /><meshStandardMaterial color="#8a5638" /></mesh>
      </group>
      <group ref={rightArm} position={[0.58, 1.76, 0]}>
        <mesh position={[0, -0.48, 0]} castShadow><capsuleGeometry args={[0.13, 0.72, 6, 12]} /><meshStandardMaterial color="#8a5638" /></mesh>
      </group>
      <group ref={leftLeg} position={[-0.27, 0.83, 0]}>
        <mesh position={[0, -0.52, 0]} castShadow><capsuleGeometry args={[0.17, 0.8, 6, 12]} /><meshStandardMaterial color="#211e1a" /></mesh>
        <mesh position={[0, -1.03, 0.16]} scale={[1.2, 0.55, 1.7]} castShadow><boxGeometry args={[0.3, 0.24, 0.45]} /><meshStandardMaterial color="#efe8db" /></mesh>
      </group>
      <group ref={rightLeg} position={[0.27, 0.83, 0]}>
        <mesh position={[0, -0.52, 0]} castShadow><capsuleGeometry args={[0.17, 0.8, 6, 12]} /><meshStandardMaterial color="#211e1a" /></mesh>
        <mesh position={[0, -1.03, 0.16]} scale={[1.2, 0.55, 1.7]} castShadow><boxGeometry args={[0.3, 0.24, 0.45]} /><meshStandardMaterial color="#efe8db" /></mesh>
      </group>
    </group>
  );
}

function Bottle({ color }: { color: string }) {
  return (
    <group position={[0, 1.05, 0]}>
      <mesh castShadow><boxGeometry args={[0.62, 0.78, 0.34]} /><meshPhysicalMaterial color={color} transparent opacity={0.64} transmission={0.25} roughness={0.18} metalness={0.15} emissive={color} emissiveIntensity={0.25} /></mesh>
      <mesh position={[0, 0.52, 0]}><boxGeometry args={[0.28, 0.24, 0.24]} /><meshStandardMaterial color="#17130d" metalness={0.6} /></mesh>
      <pointLight color={color} intensity={0.55} distance={3} />
    </group>
  );
}

function HazardMesh({ type, color }: { type: Hazard; color: string }) {
  if (type === 'essence') return <Bottle color={color} />;
  if (type === 'block') return (
    <mesh position={[0, 0.72, 0]} castShadow><boxGeometry args={[1.45, 1.45, 0.7]} /><meshStandardMaterial color="#30241b" roughness={0.72} emissive={color} emissiveIntensity={0.05} /></mesh>
  );
  if (type === 'jump') return (
    <group>
      <mesh position={[0, 0.5, 0]} castShadow><boxGeometry args={[1.8, 0.7, 0.55]} /><meshStandardMaterial color="#503a29" /></mesh>
      <mesh position={[0, 0.5, 0.3]}><boxGeometry args={[1.85, 0.1, 0.08]} /><meshStandardMaterial color={color} emissive={color} emissiveIntensity={0.35} /></mesh>
    </group>
  );
  if (type === 'slide') return (
    <group>
      <mesh position={[-0.8, 1.35, 0]}><boxGeometry args={[0.16, 2.7, 0.35]} /><meshStandardMaterial color="#3d3025" /></mesh>
      <mesh position={[0.8, 1.35, 0]}><boxGeometry args={[0.16, 2.7, 0.35]} /><meshStandardMaterial color="#3d3025" /></mesh>
      <mesh position={[0, 2.18, 0]} castShadow><boxGeometry args={[1.8, 0.52, 0.45]} /><meshStandardMaterial color="#5d4631" emissive={color} emissiveIntensity={0.06} /></mesh>
    </group>
  );
  return null;
}

function Building({ x, z, height, themeIndex }: { x: number; z: number; height: number; themeIndex: number }) {
  const theme = themes[themeIndex];
  return (
    <group position={[x, 0, z]}>
      <mesh position={[0, height / 2, 0]} castShadow receiveShadow>
        <boxGeometry args={[3.4, height, 4.5]} />
        <meshStandardMaterial color={theme.building} roughness={0.88} />
      </mesh>
      {[1.2, 2.4, 3.6, 4.8, 6].filter((y) => y < height - 0.6).map((y, i) => (
        <mesh key={i} position={[x < 0 ? 1.72 : -1.72, y, 0.6]}>
          <boxGeometry args={[0.025, 0.42, 1.85]} />
          <meshBasicMaterial color={i % 3 === 0 ? theme.glow : '#423822'} toneMapped={false} />
        </mesh>
      ))}
    </group>
  );
}

function TrackSlice({ index }: { index: number }) {
  const group = useRef<THREE.Group>(null);
  const handled = useRef(false);
  const [cycle, setCycle] = useState(0);
  const phase = useGame((s) => s.phase);
  const speed = useGame((s) => s.speed);
  const score = useGame((s) => s.score);
  const themeIndex = Math.floor(score / 1300) % themes.length;
  const theme = themes[themeIndex];
  const seed = index + cycle * SEGMENT_COUNT * 3;
  const lane = (seed * 7 + 1) % 3;
  const selector = (seed * 13 + 5) % 11;
  const type: Hazard = selector < 3 ? 'essence' : selector < 5 ? 'jump' : selector < 7 ? 'slide' : selector < 9 ? 'block' : 'none';
  const heightL = 5.5 + ((seed * 17) % 48) / 10;
  const heightR = 6.2 + ((seed * 23) % 42) / 10;

  useFrame((_, dt) => {
    if (!group.current || phase !== 'running') return;
    group.current.position.z += speed * dt;
    const z = group.current.position.z;
    if (!handled.current && z > -0.45 && z < 0.7 && type !== 'none') {
      handled.current = true;
      const state = useGame.getState();
      if (state.lane === lane) {
        if (type === 'essence') {
          state.collect();
          audio.collect();
        } else {
          const safe = (type === 'jump' && state.action === 'jump') || (type === 'slide' && state.action === 'slide');
          if (!safe) {
            audio.hit();
            state.hit();
          }
        }
      }
    }
    if (z > SEGMENT_LENGTH) {
      group.current.position.z -= SEGMENT_COUNT * SEGMENT_LENGTH;
      handled.current = false;
      setCycle((v) => v + 1);
    }
  });

  return (
    <group ref={group} position={[0, 0, -index * SEGMENT_LENGTH - 4]}>
      <mesh position={[0, -0.13, 0]} receiveShadow>
        <boxGeometry args={[8.5, 0.25, SEGMENT_LENGTH]} />
        <meshStandardMaterial color={theme.road} roughness={0.95} />
      </mesh>
      <mesh position={[-5.2, 0, 0]} receiveShadow><boxGeometry args={[1.8, 0.38, SEGMENT_LENGTH]} /><meshStandardMaterial color="#27241d" /></mesh>
      <mesh position={[5.2, 0, 0]} receiveShadow><boxGeometry args={[1.8, 0.38, SEGMENT_LENGTH]} /><meshStandardMaterial color="#27241d" /></mesh>
      {[-1.28, 1.28].map((x) => (
        <mesh key={x} position={[x, 0.02, 0]}><boxGeometry args={[0.045, 0.025, 5.6]} /><meshBasicMaterial color={theme.accent} transparent opacity={0.36} /></mesh>
      ))}
      <Building x={-8.0} z={0} height={heightL} themeIndex={themeIndex} />
      <Building x={8.0} z={0} height={heightR} themeIndex={themeIndex} />
      <group position={[laneX[lane as 0 | 1 | 2], 0, -1.8]}>
        <HazardMesh type={type} color={theme.accent} />
      </group>
      <mesh position={[-4.1, 1.25, -2.6]}><cylinderGeometry args={[0.06, 0.08, 2.5, 8]} /><meshStandardMaterial color="#54472f" metalness={0.4} /></mesh>
      <pointLight position={[-4.1, 2.55, -2.6]} color={theme.glow} intensity={0.35} distance={5} />
      <mesh position={[4.1, 1.25, 2.4]}><cylinderGeometry args={[0.06, 0.08, 2.5, 8]} /><meshStandardMaterial color="#54472f" metalness={0.4} /></mesh>
      <pointLight position={[4.1, 2.55, 2.4]} color={theme.glow} intensity={0.35} distance={5} />
    </group>
  );
}

function CameraRig() {
  const { camera } = useThree();
  const lane = useGame((s) => s.lane);
  const phase = useGame((s) => s.phase);
  useFrame(({ clock }, dt) => {
    const x = laneX[lane] * 0.16;
    const bob = phase === 'running' ? Math.sin(clock.elapsedTime * 10) * 0.035 : 0;
    camera.position.x = THREE.MathUtils.damp(camera.position.x, x, 4, dt);
    camera.position.y = THREE.MathUtils.damp(camera.position.y, 3.25 + bob, 6, dt);
    camera.position.z = 7.2;
    camera.lookAt(camera.position.x * 0.2, 1.05, -8.5);
  });
  return null;
}

function World() {
  const score = useGame((s) => s.score);
  const themeIndex = Math.floor(score / 1300) % themes.length;
  const theme = themes[themeIndex];
  const accumulator = useRef(0);
  const phase = useGame((s) => s.phase);
  useFrame((_, dt) => {
    if (phase !== 'running') return;
    accumulator.current += dt;
    if (accumulator.current >= 0.1) {
      useGame.getState().advance(accumulator.current);
      accumulator.current = 0;
    }
  });
  return (
    <>
      <color attach="background" args={[theme.bg]} />
      <fog attach="fog" args={[theme.fog, 12, 62]} />
      <ambientLight intensity={0.42} />
      <directionalLight position={[4, 9, 5]} intensity={1.15} color={theme.glow} castShadow shadow-mapSize-width={1024} shadow-mapSize-height={1024} />
      <pointLight position={[0, 4, -10]} color={theme.accent} intensity={2.2} distance={22} />
      {Array.from({ length: SEGMENT_COUNT }, (_, i) => <TrackSlice key={i} index={i} />)}
      <Runner />
      <CameraRig />
      <Environment preset="city" environmentIntensity={0.18} />
    </>
  );
}

export default function GameScene() {
  return (
    <Canvas
      shadows
      dpr={[1, 1.5]}
      camera={{ position: [0, 3.25, 7.2], fov: 54, near: 0.1, far: 100 }}
      gl={{ antialias: true, powerPreference: 'high-performance' }}
    >
      <World />
    </Canvas>
  );
}
