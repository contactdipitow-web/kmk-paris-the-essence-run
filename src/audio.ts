class KmkAudio {
  private ctx?: AudioContext;
  private master?: GainNode;
  private timer?: number;
  private beat = 0;
  private muted = false;

  start() {
    if (this.ctx) {
      void this.ctx.resume();
      return;
    }
    const AudioContextCtor = window.AudioContext || (window as typeof window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
    if (!AudioContextCtor) return;
    this.ctx = new AudioContextCtor();
    this.master = this.ctx.createGain();
    this.master.gain.value = 0.22;
    this.master.connect(this.ctx.destination);
    this.timer = window.setInterval(() => this.sequence(), 260);
  }

  toggle() {
    this.muted = !this.muted;
    if (this.master && this.ctx) this.master.gain.setTargetAtTime(this.muted ? 0 : 0.22, this.ctx.currentTime, 0.03);
    return this.muted;
  }

  private tone(frequency: number, duration: number, volume: number, type: OscillatorType = 'sine') {
    if (!this.ctx || !this.master || this.muted) return;
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    osc.type = type;
    osc.frequency.value = frequency;
    gain.gain.setValueAtTime(0.0001, this.ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(volume, this.ctx.currentTime + 0.012);
    gain.gain.exponentialRampToValueAtTime(0.0001, this.ctx.currentTime + duration);
    osc.connect(gain);
    gain.connect(this.master);
    osc.start();
    osc.stop(this.ctx.currentTime + duration + 0.02);
  }

  private sequence() {
    const bass = [73.42, 82.41, 98, 110];
    const note = bass[Math.floor(this.beat / 4) % bass.length];
    if (this.beat % 2 === 0) this.tone(note, 0.22, 0.09, 'triangle');
    if (this.beat % 4 === 0) this.tone(49, 0.16, 0.13, 'sine');
    if (this.beat % 4 === 2) this.tone(note * 4, 0.08, 0.025, 'square');
    this.beat += 1;
  }

  collect() { this.tone(660, 0.08, 0.09, 'sine'); window.setTimeout(() => this.tone(880, 0.08, 0.06, 'sine'), 45); }
  move() { this.tone(180, 0.055, 0.035, 'triangle'); }
  jump() { this.tone(260, 0.12, 0.05, 'sine'); }
  slide() { this.tone(120, 0.1, 0.045, 'triangle'); }
  hit() { this.tone(58, 0.3, 0.17, 'sawtooth'); }
}

export const audio = new KmkAudio();
