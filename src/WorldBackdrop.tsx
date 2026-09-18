import React, { useEffect, useRef } from 'react';
import { Animated, StyleSheet, View } from 'react-native';
import Svg, {
  Circle,
  Defs,
  Ellipse,
  G,
  LinearGradient,
  Line,
  Path,
  Polygon,
  Rect,
  Stop,
} from 'react-native-svg';

import type { World } from './gameData';

type Props = {
  world: World;
  width: number;
  height: number;
  running: boolean;
  slowed: boolean;
};

const VIEWBOX_WIDTH = 390;
const VIEWBOX_HEIGHT = 700;

export function WorldBackdrop({ world, width, height, running, slowed }: Props) {
  const scroll = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    scroll.setValue(0);
    if (!running || height <= 0) return undefined;

    const animation = Animated.loop(
      Animated.timing(scroll, {
        toValue: 1,
        duration: slowed ? 13_000 : 7_600,
        useNativeDriver: true,
        isInteraction: false,
      }),
    );
    animation.start();
    return () => animation.stop();
  }, [height, running, scroll, slowed, world.id]);

  const translateY = scroll.interpolate({
    inputRange: [0, 1],
    outputRange: [-height, 0],
  });

  return (
    <View pointerEvents="none" style={StyleSheet.absoluteFill}>
      <WorldIllustration world={world} width={width} height={height} />
      {height > 0 && (
        <Animated.View
          style={[
            styles.movingStrip,
            {
              width,
              height: height * 2,
              transform: [{ translateY }],
            },
          ]}
        >
          <MovingDetails world={world} width={width} height={height} />
          <MovingDetails world={world} width={width} height={height} />
        </Animated.View>
      )}
      <View style={styles.sunWash} />
    </View>
  );
}

function WorldIllustration({ world, width, height }: { world: World; width: number; height: number }) {
  return (
    <Svg
      width={width}
      height={height}
      viewBox={`0 0 ${VIEWBOX_WIDTH} ${VIEWBOX_HEIGHT}`}
      preserveAspectRatio="xMidYMid slice"
      style={StyleSheet.absoluteFill}
    >
      <Defs>
        <LinearGradient id="sky" x1="0" y1="0" x2="0" y2="1">
          <Stop offset="0" stopColor={world.palette.skyTop} />
          <Stop offset="1" stopColor={world.palette.skyBottom} />
        </LinearGradient>
        <LinearGradient id="trail" x1="0" y1="0" x2="0" y2="1">
          <Stop offset="0" stopColor={world.palette.path} stopOpacity="0.42" />
          <Stop offset="1" stopColor={world.palette.path} stopOpacity="0.94" />
        </LinearGradient>
      </Defs>
      <Rect width="390" height="700" fill="url(#sky)" />
      {world.id === 'liane' && <LianeLibreScene />}
      {world.id === 'palme' && <PalmeHiverScene />}
      {world.id === 'rivage' && <RivageCuivreScene />}
      <Path d="M136 700 L254 700 L224 156 L166 156 Z" fill="url(#trail)" />
      <Path d="M136 700 L166 156" stroke={world.palette.pathEdge} strokeWidth="5" strokeOpacity="0.34" />
      <Path d="M254 700 L224 156" stroke={world.palette.pathEdge} strokeWidth="5" strokeOpacity="0.34" />
      <Path d="M175 700 L186 156" stroke="#FFFFFF" strokeWidth="2" strokeDasharray="15 19" strokeOpacity="0.34" />
      <Path d="M215 700 L204 156" stroke="#FFFFFF" strokeWidth="2" strokeDasharray="15 19" strokeOpacity="0.34" />
    </Svg>
  );
}

function LianeLibreScene() {
  return (
    <G>
      <Circle cx="310" cy="86" r="39" fill="#FFE89B" opacity="0.9" />
      <Path d="M0 214 Q78 112 155 211 T310 202 T440 196 L440 360 L0 360 Z" fill="#2E9471" opacity="0.42" />
      <Path d="M0 258 Q92 149 184 252 T390 225 L390 404 L0 404 Z" fill="#126A4E" opacity="0.64" />
      <Path d="M0 322 Q92 235 177 321 T390 290 L390 450 L0 450 Z" fill="#0B553F" opacity="0.72" />
      <Path d="M26 700 C94 550 91 390 42 218" stroke="#6A4B2F" strokeWidth="24" opacity="0.85" />
      <Path d="M362 700 C305 534 310 350 355 184" stroke="#6A4B2F" strokeWidth="27" opacity="0.86" />
      <Path d="M38 240 C95 250 108 301 76 348 C42 322 28 283 38 240 Z" fill="#44B85B" />
      <Path d="M352 226 C296 236 275 286 302 338 C340 317 360 276 352 226 Z" fill="#73CA52" />
      <Path d="M3 402 C68 363 112 390 119 452 C67 463 25 447 3 402 Z" fill="#0C8B61" />
      <Path d="M387 400 C329 361 281 384 270 446 C322 462 365 447 387 400 Z" fill="#087454" />
      <Path d="M72 0 C82 109 46 159 82 232" stroke="#C3E86E" strokeWidth="7" fill="none" />
      <Path d="M314 0 C290 89 336 153 303 222" stroke="#96D65C" strokeWidth="6" fill="none" />
      <Path d="M0 584 C67 521 109 528 145 570 L133 700 L0 700 Z" fill="#137B4F" />
      <Path d="M390 570 C329 516 286 527 246 572 L255 700 L390 700 Z" fill="#0F6B48" />
      <Path d="M282 184 Q307 166 329 186 Q306 178 282 184 Z" fill="#E44350" />
      <Path d="M284 184 Q303 196 318 187" stroke="#263B2E" strokeWidth="3" fill="none" />
      <Spectators x={267} y={272} shirtA="#FFCA57" shirtB="#EF5A71" skinA="#704329" skinB="#A76A48" />
      <Circle cx="94" cy="179" r="3" fill="#FFF6A6" />
      <Circle cx="126" cy="225" r="2.5" fill="#FFF6A6" />
      <Circle cx="285" cy="238" r="3" fill="#FFF6A6" />
    </G>
  );
}

function PalmeHiverScene() {
  return (
    <G>
      <Circle cx="74" cy="92" r="42" fill="#FFF2AF" opacity="0.9" />
      <Path d="M-20 98 Q72 34 152 107 T324 88 T430 112" stroke="#7CE6CF" strokeWidth="24" opacity="0.42" fill="none" />
      <Path d="M-20 138 Q89 71 186 141 T420 124" stroke="#F8CE79" strokeWidth="16" opacity="0.35" fill="none" />
      <Polygon points="-20,333 76,150 166,333" fill="#E9FAFB" opacity="0.88" />
      <Polygon points="91,333 202,118 310,333" fill="#D9F3F5" opacity="0.92" />
      <Polygon points="222,333 332,158 420,333" fill="#F4FBF7" opacity="0.9" />
      <Polygon points="76,150 99,229 48,210" fill="#B7DFE6" />
      <Polygon points="202,118 229,228 166,207" fill="#A9D7E1" />
      <Path d="M22 700 Q84 535 140 574 L145 700 Z" fill="#D8F4F1" />
      <Path d="M368 700 Q303 523 244 568 L249 700 Z" fill="#C5E9EC" />
      <FrozenPalm x={63} y={315} scale={1.14} flip={false} />
      <FrozenPalm x={330} y={290} scale={1.05} flip />
      <FrozenPalm x={28} y={480} scale={0.76} flip />
      <FrozenPalm x={364} y={466} scale={0.7} flip={false} />
      <Spectators x={266} y={267} shirtA="#E68649" shirtB="#2D8FA0" skinA="#6D432E" skinB="#A96749" />
      <Path d="M270 202 Q285 191 299 203" stroke="#4B7480" strokeWidth="3" fill="none" />
      <Path d="M300 203 Q315 191 330 204" stroke="#4B7480" strokeWidth="3" fill="none" />
    </G>
  );
}

function RivageCuivreScene() {
  return (
    <G>
      <Circle cx="286" cy="126" r="55" fill="#FFE09A" opacity="0.94" />
      <Rect x="0" y="245" width="390" height="455" fill="#4DB6C2" opacity="0.68" />
      <Path d="M0 290 Q48 267 96 290 T192 290 T288 290 T390 289" stroke="#DFF8F5" strokeWidth="7" fill="none" opacity="0.76" />
      <Path d="M0 354 Q55 327 110 354 T220 354 T330 354 T440 354" stroke="#F6F1D1" strokeWidth="5" fill="none" opacity="0.58" />
      <Path d="M0 700 L0 220 Q58 229 83 281 Q111 349 134 700 Z" fill="#A84E37" />
      <Path d="M390 700 L390 203 Q333 223 305 286 Q276 351 251 700 Z" fill="#C56643" />
      <Path d="M0 323 Q54 278 104 336 L131 700 L0 700 Z" fill="#E19458" opacity="0.72" />
      <Path d="M390 307 Q342 272 291 334 L253 700 L390 700 Z" fill="#EAA05C" opacity="0.65" />
      <Path d="M333 222 Q358 192 376 216" stroke="#6C3C35" strokeWidth="4" fill="none" />
      <Path d="M330 222 Q306 191 290 214" stroke="#6C3C35" strokeWidth="4" fill="none" />
      <Line x1="339" y1="218" x2="339" y2="324" stroke="#70422F" strokeWidth="7" />
      <Path d="M338 229 C308 200 284 208 274 239 C302 245 323 240 338 229 Z" fill="#47794D" />
      <Path d="M340 236 C369 201 393 208 404 240 C377 249 354 246 340 236 Z" fill="#3F704A" />
      <Surfer x={85} y={340} color="#F2C84B" />
      <Spectators x={270} y={272} shirtA="#1F7186" shirtB="#F3BD4D" skinA="#6E402B" skinB="#A66643" />
      <Path d="M70 184 Q81 173 93 184 Q104 171 116 183" stroke="#73453D" strokeWidth="3" fill="none" />
      <Path d="M150 143 Q160 134 170 143 Q180 132 190 142" stroke="#73453D" strokeWidth="3" fill="none" />
    </G>
  );
}

function Spectators({
  x,
  y,
  shirtA,
  shirtB,
  skinA,
  skinB,
}: {
  x: number;
  y: number;
  shirtA: string;
  shirtB: string;
  skinA: string;
  skinB: string;
}) {
  return (
    <G>
      <Circle cx={x} cy={y} r="7" fill={skinA} />
      <Rect x={x - 6} y={y + 8} width="12" height="22" rx="5" fill={shirtA} />
      <Line x1={x - 3} y1={y + 29} x2={x - 7} y2={y + 45} stroke="#314446" strokeWidth="4" />
      <Line x1={x + 3} y1={y + 29} x2={x + 7} y2={y + 45} stroke="#314446" strokeWidth="4" />
      <Circle cx={x + 22} cy={y + 4} r="7" fill={skinB} />
      <Rect x={x + 16} y={y + 12} width="12" height="21" rx="5" fill={shirtB} />
      <Line x1={x + 19} y1={y + 32} x2={x + 16} y2={y + 47} stroke="#4B3E46" strokeWidth="4" />
      <Line x1={x + 25} y1={y + 32} x2={x + 30} y2={y + 47} stroke="#4B3E46" strokeWidth="4" />
    </G>
  );
}

function FrozenPalm({ x, y, scale, flip }: { x: number; y: number; scale: number; flip: boolean }) {
  return (
    <G transform={`translate(${x} ${y}) scale(${flip ? -scale : scale} ${scale})`}>
      <Path d="M0 88 C6 59 4 31 0 0" stroke="#8B7057" strokeWidth="8" fill="none" />
      <Path d="M0 7 C-40 -4 -58 7 -70 25 C-37 27 -15 20 0 7 Z" fill="#71C8C5" />
      <Path d="M1 7 C32 -13 58 -7 73 10 C44 20 20 16 1 7 Z" fill="#8AD9D3" />
      <Path d="M0 8 C-26 -25 -46 -28 -62 -18 C-42 2 -22 10 0 8 Z" fill="#B9ECE4" />
      <Path d="M2 8 C23 -22 42 -27 58 -16 C40 2 22 10 2 8 Z" fill="#D9F7EF" />
      <Path d="M-62 -18 Q-32 -8 0 8" stroke="#FFFFFF" strokeWidth="2" fill="none" opacity="0.75" />
    </G>
  );
}

function Surfer({ x, y, color }: { x: number; y: number; color: string }) {
  return (
    <G>
      <Ellipse cx={x} cy={y + 30} rx="25" ry="5" fill="#F5E5B8" transform={`rotate(-12 ${x} ${y + 30})`} />
      <Circle cx={x} cy={y} r="6" fill="#73442E" />
      <Path d={`M${x} ${y + 7} L${x + 7} ${y + 20} L${x - 6} ${y + 26}`} stroke={color} strokeWidth="7" fill="none" />
      <Line x1={x + 2} y1={y + 11} x2={x + 17} y2={y + 2} stroke="#73442E" strokeWidth="4" />
    </G>
  );
}

function MovingDetails({ world, width, height }: { world: World; width: number; height: number }) {
  return (
    <Svg width={width} height={height} viewBox={`0 0 ${VIEWBOX_WIDTH} ${VIEWBOX_HEIGHT}`} preserveAspectRatio="xMidYMid slice">
      {world.id === 'liane' && (
        <G opacity="0.92">
          <Leaf x={26} y={70} rotate={18} color="#41B95E" />
          <Leaf x={354} y={126} rotate={-34} color="#79CF55" />
          <Leaf x={34} y={326} rotate={42} color="#0B8E62" />
          <Leaf x={347} y={425} rotate={-26} color="#2BAE59" />
          <Leaf x={63} y={590} rotate={12} color="#65C94E" />
          <Circle cx="92" cy="199" r="3" fill="#FFF59A" />
          <Circle cx="315" cy="277" r="3.5" fill="#FFF59A" />
          <Circle cx="78" cy="487" r="2.5" fill="#FFF59A" />
          <Path d="M315 547 Q326 533 337 547 Q326 540 315 547 Z" fill="#F35F75" />
        </G>
      )}
      {world.id === 'palme' && (
        <G opacity="0.82">
          {([
            [38, 70, 5],
            [338, 135, 7],
            [73, 228, 6],
            [307, 302, 5],
            [42, 430, 7],
            [350, 520, 5],
            [103, 619, 6],
          ] as const).map(([x, y, size], index) => (
            <Snowflake key={index} x={x} y={y} size={size} />
          ))}
          <Path d="M18 346 C55 325 83 334 98 368 C66 376 38 369 18 346 Z" fill="#A7E2DB" />
          <Path d="M372 636 C338 610 306 617 289 650 C319 662 350 657 372 636 Z" fill="#C7EEE8" />
        </G>
      )}
      {world.id === 'rivage' && (
        <G opacity="0.76">
          <Path d="M10 103 Q48 81 83 106" stroke="#FDF4D0" strokeWidth="6" fill="none" />
          <Path d="M307 224 Q347 200 383 226" stroke="#FDF4D0" strokeWidth="6" fill="none" />
          <Path d="M17 402 Q53 378 91 404" stroke="#FFFFFF" strokeWidth="5" fill="none" />
          <Path d="M296 565 Q338 539 381 568" stroke="#FFFFFF" strokeWidth="5" fill="none" />
          <Circle cx="74" cy="174" r="4" fill="#FFF4D6" />
          <Circle cx="325" cy="352" r="5" fill="#FFF4D6" />
          <Circle cx="48" cy="615" r="4" fill="#FFF4D6" />
          <Path d="M329 89 Q341 76 353 89 Q365 75 377 88" stroke="#6B3E38" strokeWidth="3" fill="none" />
        </G>
      )}
    </Svg>
  );
}

function Leaf({ x, y, rotate, color }: { x: number; y: number; rotate: number; color: string }) {
  return (
    <G transform={`translate(${x} ${y}) rotate(${rotate})`}>
      <Path d="M0 0 C32 -18 56 -5 65 28 C31 34 9 24 0 0 Z" fill={color} />
      <Line x1="4" y1="3" x2="56" y2="25" stroke="#E3F69B" strokeWidth="2" opacity="0.6" />
    </G>
  );
}

function Snowflake({ x, y, size }: { x: number; y: number; size: number }) {
  return (
    <G stroke="#FFFFFF" strokeWidth="2" strokeLinecap="round">
      <Line x1={x - size} y1={y} x2={x + size} y2={y} />
      <Line x1={x} y1={y - size} x2={x} y2={y + size} />
      <Line x1={x - size * 0.7} y1={y - size * 0.7} x2={x + size * 0.7} y2={y + size * 0.7} />
      <Line x1={x - size * 0.7} y1={y + size * 0.7} x2={x + size * 0.7} y2={y - size * 0.7} />
    </G>
  );
}

const styles = StyleSheet.create({
  movingStrip: {
    position: 'absolute',
    left: 0,
    top: 0,
  },
  sunWash: {
    position: 'absolute',
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    backgroundColor: 'rgba(255,255,255,0.035)',
  },
});
