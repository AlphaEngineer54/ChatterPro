import '@testing-library/jest-dom';
import { afterEach } from 'vitest';
import { cleanup } from '@testing-library/react';

// Polyfill localStorage en mémoire (jsdom ne l'expose pas de façon fiable).
// Défini via defineProperty pour survivre à vi.unstubAllGlobals() des tests.
class LocalStorageMock {
  constructor() {
    this.store = {};
  }
  clear() {
    this.store = {};
  }
  getItem(key) {
    return Object.prototype.hasOwnProperty.call(this.store, key) ? this.store[key] : null;
  }
  setItem(key, value) {
    this.store[key] = String(value);
  }
  removeItem(key) {
    delete this.store[key];
  }
}
Object.defineProperty(globalThis, 'localStorage', {
  value: new LocalStorageMock(),
  configurable: true,
  writable: true,
});

// Nettoyage du DOM et du localStorage entre chaque test.
afterEach(() => {
  cleanup();
  localStorage.clear();
});
